
#!/bin/bash
rm -rf Sync.Theater.Web
git clone https://github.com/sync-theater/Sync.Theater.Web
cd Sync.Theater.Web
npm install
grunt
cd ..
nuget restore
xbuild
