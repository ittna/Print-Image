#!/usr/local/bin/pwsh

Import-Module ./bin/Release/net6.0/publish/Print-Image.dll
Import-Module ./bin/Release/net6.0/publish/SixLabors.ImageSharp.dll

clear
Show-Image -Width 80 -ImageFile chilicorn-running.gif
