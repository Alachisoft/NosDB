ECHO OFF
	   
ECHO Buil Source
CALL ".\NosDB Src.bat"

ECHO Buil Powershell Tool
CALL ".\NosDB Powershell.bat"

ECHO Buil Integerations
CALL ".\NosDB Integration.bat"
