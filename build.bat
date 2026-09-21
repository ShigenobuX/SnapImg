@echo off
setlocal

echo Building SnapImg in Release configuration...
dotnet build .\imgconv.csproj -c Release
if errorlevel 1 (
  echo Build failed.
  exit /b %errorlevel%
)

echo Complete: bin\Release\net8.0-windows\snapimg.exe
