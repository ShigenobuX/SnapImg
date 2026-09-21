@echo off
setlocal

echo Publishing SnapImg for distribution...
dotnet publish .\imgconv.csproj -c Release -r win-x64 --self-contained true -o .\publish-folder
if errorlevel 1 (
  echo Publish failed.
  exit /b %errorlevel%
)

echo Complete: publish-folder\snapimg.exe
