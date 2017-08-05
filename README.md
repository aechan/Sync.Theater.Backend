# Sync.Theater

## Sync.Theater is a rework of [alec-chan/VideoSync](https://github.com/alec-chan/VideoSync)
The purpose of this new revision is mainly to offload all of the logic from the frontend to the backend.  
In the previous version, most, if not all of the logic was handled on the frontend in JavaScript.  This was a terrible mistake and left the app prone to tons of synchronization errors and became a mess to add features to.  

This new version moves the app towards more of an MVC design, handling all of the logic and database interaction on the C# backend and only updating the frontend with the information that it needs.  The frontend will just handle displaying the data that the backend gives it, like it should.

## Usage
The Sync.Theater backend can be tested and built in Visual Studio. Currently, for debugging it runs as a console app, but in the future I will get it running as a Windows service so it can be installed as a Windows service on a remote server.