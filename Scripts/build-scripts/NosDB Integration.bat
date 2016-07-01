@ECHO OFF

::_________________________________________::
::______BUILDING ADO.NETProvider______::
::_________________________________________::

ECHO ================================================
if exist %windir%\Microsoft.NET\Framework\v4.0.30319\ (
	@%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe ..\..\Integrations\ADO.NETProvider\ADO.NETProvider.sln /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU"
)

ECHO INTEGRATION BUILD SUCCESSFULL
pause
