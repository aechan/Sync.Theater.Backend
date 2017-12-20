#!/bin/bash
cd Sync.Theater.Web
npm install
grunt
cd ..
nuget restore
xbuild
