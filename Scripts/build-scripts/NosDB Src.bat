@echo off

echo BUILDING NCacheDB.sln
echo ======================
@%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe ..\..\Src\NosDB.sln /t:Rebuild /p:Configuration=Release /p:platform="Any CPU"
set BUILD_STATUS=%ERRORLEVEL%
if not %BUILD_STATUS%==0 goto failNosDB
echo SRC BUILD ALL SUCCEEDED
pause
exit /b 0

:failNosDB
echo FAILED TO BUILD NosDB.sln
echo =============================
pause
exit /b 1
