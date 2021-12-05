# Debug Single Thread - Visual Studio Extension
![Screenshot](/DebugSingleThreadSharedFolder/Screenshot.png?raw=true "Screenshot")

This is the official repository for:  
https://marketplace.visualstudio.com/items?itemName=mayerwin.DebugSingleThread

Initial release page here:  
http://erwinmayer.com/labs/visual-studio-2010-extension-debug-single-thread

**Description**

This Visual Studio extension adds two shortcuts and toolbar buttons to allow developers to easily focus on single threads while debugging multi-threaded applications.  
It dramatically reduces the need to manually go into the Threads window to freeze/thaw all threads but the one that needs to be followed, and therefore helps improve productivity.

**Features**
- Restrict further execution to the current thread only. Will freeze all other threads. Shortcut: `CTRL+T+T` or `snowflake` button. Click button again to thaw all other threads. Known frozen threads before the command runs (e.g. due to a breakpoint) will be remembered and not thawed. If this is not what you expect, please let me know so I can add an option to customize behavior.
- Switch to the next single thread (based on ID). Will change current thread and freeze all other threads. Shortcut: `CTRL+T+J` or `Next` button.

**Supported editions**

Visual Studio 2015, 2017, 2019, 2022+.

Visual Studio 2022 support is currently available via the beta release which can be downloaded [here](https://github.com/mayerwin/vs-debug-single-thread/releases/tag/4.0.0). The extension available on the Visual Studio Marketplace will be updated to support Visual Studio 2022 when Microsoft will have actually implemented the following key important missing feature:

> In the future, the Marketplace will allow you to upload multiple VSIXs to just one Marketplace listing, allowing you to upload your Visual Studio 2022-targeted VSIX and a pre-Visual Studio 2022 VSIX. Your users will automatically get the right VSIX for the VS version they have installed, when using the VS extension manager.
https://docs.microsoft.com/en-us/visualstudio/extensibility/migration/update-visual-studio-extension?view=vs-2022#visual-studio-marketplace

Visual Studio 2010 support has been dropped due to limitations imposed by Microsoft's VSIX format to support Visual Studio 2017. The last stable release supporting Visual Studio 2010 can however be downloaded [here](https://github.com/mayerwin/vs-debug-single-thread/releases/tag/1.1.3).

Visual Studio 2012 and 2013 support has been dropped due to new requirements imposed by Microsoft to speed up loading time with AsyncPackage. The last stable release supporting Visual Studio 2012 and 2013 can however be downloaded [here](https://github.com/mayerwin/vs-debug-single-thread/releases/tag/1.1.3_2012-2017).

You are welcome to contribute to this project!