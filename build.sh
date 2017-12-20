git submodule init
git submodule update
cd Sync.Theater.Web
npm install
grunt
cd ..
nuget restore
xbuild
