// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace ErwinMayerLabs.DebugSingleThread
{
    static class PkgCmdIDList
    {
        public const uint FocusOnCurrentThreadCmd = 0x100;
        public const uint SwitchToNextThreadCmd = 0x101;
    };
}