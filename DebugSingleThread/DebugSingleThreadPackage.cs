/* 
 * TODO:
 * - Add context menu item in the Threads window (Freeze all but this thread)
 * Sources:
 * http://msdn.microsoft.com/en-us/library/cc138589.aspx
 * http://msdn.microsoft.com/en-us/library/bb164715.aspx
 * http://findicons.com/icon/137367/flake?width=256#
 * http://www.iconspedia.com/icon/snow-flake.html
 * http://www.iconspedia.com/search/next/
 */
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ErwinMayerLabs.DebugSingleThread {
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.Debugging_string)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidDebugSingleThreadPkgString)]
    public sealed class DebugSingleThreadPackage : Package {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public DebugSingleThreadPackage() {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members
        private EnvDTE.DTE dte;
        OleMenuCommand FocusCmd;
        OleMenuCommand SwitchCmd;

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize() {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            //Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs) {
                // Create the command for the menu item.
                var menuCommandID = new CommandID(GuidList.guidDebugSingleThreadCmdSet, (int)PkgCmdIDList.FocusOnCurrentThreadCmd);
                var menuCommandID2 = new CommandID(GuidList.guidDebugSingleThreadCmdSet, (int)PkgCmdIDList.SwitchToNextThreadCmd);
                this.FocusCmd = new OleMenuCommand(FocusOnCurrentThread, menuCommandID);
                this.SwitchCmd = new OleMenuCommand(SwitchToNextThread, menuCommandID2);
                this.FocusCmd.BeforeQueryStatus += new EventHandler(OnBeforeQueryStatus);
                this.SwitchCmd.BeforeQueryStatus += new EventHandler(OnBeforeQueryStatus);

                mcs.AddCommand(this.FocusCmd);
                mcs.AddCommand(this.SwitchCmd);
            }
            this.dte = (EnvDTE.DTE)GetGlobalService(typeof(EnvDTE.DTE));
            this.OnBeforeQueryStatus(this.FocusCmd, null);
            this.OnBeforeQueryStatus(this.SwitchCmd, null);
        }

        //protected OptionPageGrid Settings {
        //    get { return (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid)); }
        //}
        #endregion

        //List<int> PreviouslyFrozenThreads = new List<int>(); // Could serve to remember threads that were frozen prior to Focusing, but it is likely that users will want to discard previous freezes.
        bool IsFocused = false;
        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void FocusOnCurrentThread(object sender, EventArgs e) {
            try{
                if (this.dte.Debugger.CurrentMode == EnvDTE.dbgDebugMode.dbgRunMode) this.dte.Debugger.Break();
                foreach (EnvDTE.Thread thread in this.dte.Debugger.CurrentProgram.Threads) {
                    if (!this.IsFocused) {
                        if (thread.IsAlive && thread.ID != this.dte.Debugger.CurrentThread.ID) {
                            thread.Freeze();
                        }
                    }
                    else {
                        if (thread.IsFrozen) {
                            thread.Thaw();
                        }
                    }
                }
                if (!this.IsFocused) {
                    this.IsFocused = true;
                    this.FocusCmd.Checked = true;
                    this.SwitchCmd.Enabled = true;
                }
                else { 
                    this.IsFocused = false;
                    this.FocusCmd.Checked = false;
                    this.SwitchCmd.Enabled = false;
                }
            }
            catch (Exception) {}
        }

        private void SwitchToNextThread(object sender, EventArgs e) {
            try {
                if (this.dte.Debugger.CurrentMode == EnvDTE.dbgDebugMode.dbgRunMode) this.dte.Debugger.Break();
                if (!this.IsFocused) return;
                var threads = new List<EnvDTE.Thread>();
                foreach (EnvDTE.Thread thread in this.dte.Debugger.CurrentProgram.Threads) {
                    threads.Add(thread);
                }
                var liveThreads = threads.Where(t => t.IsAlive).OrderBy(t => t.ID).ToList();
                if (!liveThreads.Any()) return;
                var nextThread = liveThreads.Where(t => t.ID > this.dte.Debugger.CurrentThread.ID).FirstOrDefault();
                if (nextThread == null) nextThread = liveThreads.First();
                foreach (EnvDTE.Thread thread in liveThreads) {
                    if (!thread.IsFrozen) thread.Freeze();
                }
                nextThread.Thaw();
                this.dte.Debugger.CurrentThread = nextThread;
            }
            catch (Exception) {}
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e) {
            var myCommand = sender as OleMenuCommand;
            if (null != myCommand) {
                if (this.dte.Mode == EnvDTE.vsIDEMode.vsIDEModeDebug) {
                    if (myCommand.CommandID != this.SwitchCmd.CommandID || this.IsFocused) {
                        myCommand.Enabled = true;
                    }
                    else {
                        myCommand.Enabled = false;
                    }
                    myCommand.Visible = true;
                }
                else {
                    myCommand.Enabled = false;
                    myCommand.Visible = false;
                }
            }
        }
    }
}
