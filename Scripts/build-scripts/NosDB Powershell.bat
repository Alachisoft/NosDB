@echo off

echo BUILDING NoSDBPS.sln
echo ======================
@%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe ..\..\Tool\NoSDBPSProvider\NoSDBPS.sln /t:Rebuild /p:Configuration=Release /p:platform="Any CPU"
set BUILD_STATUS=%ERRORLEVEL%
if not %BUILD_STATUS%==0 goto NoSDBPS
echo NoSDBPS BUILD ALL SUCCEEDED
pause
exit /b 0

:NoSDBPS
echo FAILED TO BUILD NoSDBPS.sln
echo =============================
pause
exit /b 1