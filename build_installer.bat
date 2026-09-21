@echo off
setlocal

set "ISCC=%ProgramFiles%\Inno Setup 7\ISCC.exe"
if not exist "%ISCC%" set "ISCC=%ProgramFiles(x86)%\Inno Setup 7\ISCC.exe"
if not exist "%ISCC%" set "ISCC=%LocalAppData%\Programs\Inno Setup 7\ISCC.exe"

if not exist "publish-folder\snapimg.exe" (
  echo Error: publish-folder\snapimg.exe was not found.
  echo Run dotnet publish first.
  exit /b 1
)

if not exist "%ISCC%" (
  echo Error: Inno Setup 7 was not found.
  echo Install it from https://jrsoftware.org/isdl.php, then run this script again.
  exit /b 1
)

"%ISCC%" installer.iss
if errorlevel 1 exit /b %errorlevel%

echo Installer created in InstallerOutput.
