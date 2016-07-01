 Import-Module Y:\Tool\NoSDBPSProvider\NoSDBPS\bin\Debug\NoSDBPS.dll -Verbose

cd NoSDB:

#Connect-ConfigManager -name nosconfig -server 20.200.20.24

new-ConfigManager -name nosconfig -server 20.200.20.24


import-module NoSDBPS; cd nosdb:;Connect-ConfigManager 20.200.20.24