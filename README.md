<a align="center" href="http://sync.theater"><img width="75%" src="https://alecchan.org/assets/img/sync.theater.png" alt="sync.theater"></a>
# Sync.Theater

## Sync.Theater is a rework of [alec-chan/VideoSync](https://github.com/alec-chan/VideoSync)
The purpose of this new revision is mainly to offload all of the logic from the frontend to the backend.  
In the previous version, most, if not all of the logic was handled on the frontend in JavaScript.  This was a mistake and left the app prone to tons of synchronization errors and became a mess to add features to.  

This new version moves the app towards more of an MVC design, handling all of the logic and database interaction on the C# backend and only updating the frontend with the information that it needs.  The frontend will just handle displaying the data that the backend gives it, like it should.

There are 3 projects in this solution:

`Sync.Theater`: is the core of the Sync Theater backend. It is a class library project that gets compiled to a dll for portability.

`Sync.Theater.Console` is the Console App version of Sync Theater, useful for checking output while debugging.

`Sync.Theater.Web` is a git submodule pointing to the web frontend for Sync Theater.


## Usage
The build process for Sync.Theater is intended to be run on a Unix system, so it may no work exactly right on Windows, but with some small modifications or by running it from a Cygwin or Mintty terminal it should work. The program itself should work on any program that supports Mono.

1. Make sure the git submodules have been initialized and cloned. 
    - `git submodule init && git submodule update`
2. Run `build.sh`
3. Run `run.sh` as root - necessary to bind to port 80.
4. To stop run `stop.sh` - warning: this will kill all mono processes.
