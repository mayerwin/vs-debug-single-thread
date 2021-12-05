// Guids.cs
// MUST match guids.h
using System;

namespace ErwinMayerLabs.DebugSingleThread
{
    static class GuidList
    {
        public const string guidDebugSingleThreadPkgString = "9fb8ed8b-b44a-4076-b677-cbeb7b834afb";
        public const string guidDebugSingleThreadCmdSetString = "2d00031b-eb83-4527-a3d0-9e1c14c1be97";

        public static readonly Guid guidDebugSingleThreadCmdSet = new Guid(guidDebugSingleThreadCmdSetString);
    };
}