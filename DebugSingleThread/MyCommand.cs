using System;
using System.ComponentModel.Design;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using Thread = EnvDTE.Thread;

namespace ErwinMayerLabs.DebugSingleThread {
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class MyCommand {
        private static EnvDTE.DTE dte;
        private static OleMenuCommand FocusCmd;
        private static OleMenuCommand SwitchCmd;
        
        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            //Add our command handlers for menu (commands must exist in the .vsct file)
            if (await package.GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService commandService) {
                // Create the command for the menu item.
                var menuCommandID1 = new CommandID(GuidList.guidDebugSingleThreadCmdSet, (int)PkgCmdIDList.FocusOnCurrentThreadCmd);
                var menuCommandID2 = new CommandID(GuidList.guidDebugSingleThreadCmdSet, (int)PkgCmdIDList.SwitchToNextThreadCmd);
                FocusCmd = new OleMenuCommand(FocusOnCurrentThread, menuCommandID1);
                SwitchCmd = new OleMenuCommand(SwitchToNextThread, menuCommandID2);
                FocusCmd.BeforeQueryStatus += OnBeforeQueryStatus;
                SwitchCmd.BeforeQueryStatus += OnBeforeQueryStatus;

                commandService.AddCommand(FocusCmd);
                commandService.AddCommand(SwitchCmd);

                dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
                OnBeforeQueryStatus(FocusCmd, null);
                OnBeforeQueryStatus(SwitchCmd, null);
            }
        }
        
        //List<int> PreviouslyFrozenThreads = new List<int>(); // Could serve to remember threads that were frozen prior to Focusing, but it is likely that users will want to discard previous freezes.
        private static bool IsFocused = false;
        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private static void FocusOnCurrentThread(object sender, EventArgs e) {
            try {
                if (dte.Debugger.CurrentMode == EnvDTE.dbgDebugMode.dbgRunMode) dte.Debugger.Break();
                foreach (EnvDTE.Thread thread in dte.Debugger.CurrentProgram.Threads) {
                    if (!IsFocused) {
                        if (thread.ID == dte.Debugger.CurrentThread.ID) {
                            if (thread.IsFrozen) {
                                thread.Thaw();
                            }
                        }
                        else if (thread.IsAlive) {
                            thread.Freeze();
                        }
                    }
                    else {
                        if (thread.IsFrozen) {
                            thread.Thaw();
                        }
                    }
                }
                if (!IsFocused) {
                    IsFocused = true;
                    FocusCmd.Checked = true;
                    SwitchCmd.Enabled = true;
                }
                else {
                    IsFocused = false;
                    FocusCmd.Checked = false;
                    SwitchCmd.Enabled = false;
                }
            }
            catch (Exception) {
                // ignored
            }
        }

        private static void SwitchToNextThread(object sender, EventArgs e) {
            try {
                if (dte.Debugger.CurrentMode != EnvDTE.dbgDebugMode.dbgBreakMode) return;
                IsFocused = true;
                var liveThreads = dte.Debugger.CurrentProgram.Threads.Cast<Thread>().Where(thread => thread.IsAlive).ToList();
                // It is still to be found how to match an EnvDTE thread to a System.Threading.Thread to sort based on the ManagedThreadID, if possible and desirable at all.
                if (!liveThreads.Any()) return;
                liveThreads = liveThreads.Where(t => t.IsAlive).OrderBy(t => t.ID).ToList();
                var nextThread = liveThreads.FirstOrDefault(t => t.ID > dte.Debugger.CurrentThread.ID) ?? liveThreads.First();
                foreach (var thread in liveThreads.Where(thread => !thread.IsFrozen)) {
                    thread.Freeze();
                }
                nextThread.Thaw();
                dte.Debugger.CurrentThread = nextThread;
            }
            catch (Exception) {
                // ignored
            }
        }

        private static void OnBeforeQueryStatus(object sender, EventArgs e) {
            if (sender is OleMenuCommand myCommand) {
                if (dte.Mode == EnvDTE.vsIDEMode.vsIDEModeDebug) {
                    if (myCommand.CommandID.Guid != SwitchCmd.CommandID.Guid || dte.Debugger.CurrentMode == EnvDTE.dbgDebugMode.dbgBreakMode) {
                        if (myCommand.CommandID.Guid != SwitchCmd.CommandID.Guid && IsFocused) {
                            myCommand.Checked = true;
                        }
                        myCommand.Enabled = true;
                        myCommand.Visible = true;
                    }
                    else {
                        myCommand.Enabled = false;
                        myCommand.Visible = false;
                    }
                }
                else {
                    IsFocused = false;
                    myCommand.Checked = false;
                    myCommand.Enabled = false;
                    myCommand.Visible = false;
                }
            }
        }
    }
}
