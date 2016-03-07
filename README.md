# Debug Single Thread - Visual Studio Extension

This is the official repository for:  
http://visualstudiogallery.msdn.microsoft.com/54ef0f07-ed1d-4b89-b4ae-6506b196f843

Initial release page here:  
http://erwinmayer.com/labs/visual-studio-2010-extension-debug-single-thread

**Description**

This Visual Studio extension adds two shortcuts and toolbar buttons to allow developers to easily focus on single threads while debugging multi-threaded applications.  
It dramatically reduces the need to manually go into the Threads window to freeze/thaw all threads but the one that needs to be followed, and therefore helps improve productivity.

**Features**
- Restrict further execution to the current thread only. Will freeze all other threads. Shortcut: `CTRL+T+T` or `snowflake` button.  
- Switch to the next single thread (based on ID). Will change current thread and freeze all other threads. Shortcut: `CTRL+T+J` or `Next` button.

**Supported editions**

Visual Studio 2010, 2012, 2013, 2015+.

You are welcome to contribute to this project!

This project is being supported by [Jetbrains][1]' excellent ReSharper, DotTrace and DotMemory extensions.

[1]: http://www.jetbrains.com
