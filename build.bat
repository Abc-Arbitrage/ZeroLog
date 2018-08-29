@echo off
powershell -ExecutionPolicy Bypass build\build.ps1 -Script build\build.cake -Target NuGet
pause
