<a align="center" href="http://sync.theater"><img width="75%" src="https://alecchan.org/assets/img/sync.theater.png" alt="sync.theater"></a>
# Sync.Theater

## Sync.Theater is a rework of [alec-chan/VideoSync](https://github.com/alec-chan/VideoSync)
The purpose of this new revision is mainly to offload all of the logic from the frontend to the backend.  
In the previous version, most, if not all of the logic was handled on the frontend in JavaScript.  This was a terrible mistake and left the app prone to tons of synchronization errors and became a mess to add features to.  

This new version moves the app towards more of an MVC design, handling all of the logic and database interaction on the C# backend and only updating the frontend with the information that it needs.  The frontend will just handle displaying the data that the backend gives it, like it should.

There are 5 projects in this solution:

`Sync.Theater`: is the core of the Sync Theater backend. It is a class library project that gets compiled to a dll for portability.

`Sync.Theater.Console` is the Console App version of Sync Theater, useful for checking output while debugging.

`Sync.Theater.Service` is the Windows Service version of Sync Theater, for deploying to a server so it can be automatically restarted when a crash or reboot happens.

`Sync.Theater.Web` is a git submodule pointing to the web frontend for Sync Theater.

`STServiceSetup` is an installer project that builds the installer for `Sync.Theater.Service`

## Usage
- Open Sync.Theater.sln in Visual Studio 2017.
- To build/debug the console app:
  - Change the startup project in the startup project switcher to `Sync.Theater.Console` and click Start or F5 to build and run.
- To build/debug the Windows service:
  - For first time build:
    - Change startup project to `Sync.Theater.Service`
    - Open Solution Explorer and right click on STServiceSetup project and click build. This will build the Sync.Theater library, the Sync.Theater.Service executable and the STServiceSetup installer.
    - Once the build is complete right click on STServiceSetup project again and click install.
    - Open services.msc and find STService and start it.
    - To debug:
      - Open Visual Studio while STService is running and go to Debug->Attach To Process, check Show processes from all users box, select Sync.Theater.Service.exe and click Attach.
  - To rebuild the service:
    - Before building the STServiceSetup project, right click on it and click uninstall to uninstall the older build. If you do this step after building the new STServiceSetup, the uninstall will fail and you will have to uninstall through control panel.
    - Follow the rest of the steps for first time build.