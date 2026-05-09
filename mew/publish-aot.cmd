@echo off
setlocal

set VSDEVCMD=D:\Scoop\apps\vsbuildtools2022\current\vs\Common7\Tools\VsDevCmd.bat
if not exist "%VSDEVCMD%" (
  echo [ERROR] VsDevCmd not found: %VSDEVCMD%
  echo Please install vsbuildtools2022 or update this path.
  exit /b 1
)

call "%VSDEVCMD%" -arch=amd64 -host_arch=amd64 >nul 2>&1
if errorlevel 1 (
  echo [ERROR] Failed to initialize Visual C++ build environment.
  exit /b 1
)

dotnet publish "%~dp0src\MewPad.Hosting\MewPad.Hosting.csproj" /p:PublishProfile=win-x64-aot /p:IlcUseEnvironmentalTools=true %*
if errorlevel 1 exit /b %errorlevel%

echo.
echo [OK] NativeAOT publish completed.
exit /b 0
