@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "& '%~dp0build.ps1' %*"
exit /B %errorlevel%
