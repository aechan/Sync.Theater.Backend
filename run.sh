#!/bin/bash
python2.7 LivestreamExtractor/LivestreamExtractionServer.py alec:vakama000 &
cd Sync.Theater.Console/bin/Debug
sudo nohup mono Sync.Theater.Console.exe &

