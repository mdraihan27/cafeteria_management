@echo off
REM Builds and launches the desktop app. Make sure start-database.bat
REM and start-backend.bat are already running in their own windows
REM before you use this one.

REM If dotnet was installed after this terminal type opened, PATH
REM might not know about it yet - fall back to the usual install
REM location instead of making you reopen a terminal.
where dotnet >nul 2>nul
if errorlevel 1 (
    if exist "C:\Program Files\dotnet\dotnet.exe" (
        set "PATH=%PATH%;C:\Program Files\dotnet"
    ) else (
        echo Could not find dotnet. Install the .NET SDK and try again.
        pause
        exit /b 1
    )
)

cd /d "%~dp0..\Frontend"
dotnet run
pause
