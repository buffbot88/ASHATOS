@echo off
REM Startup script for ASHAT GoddessQ CLI (Windows)

REM Get the directory where this script is located
set SCRIPT_DIR=%~dp0

REM Change to the AshatGoddessQ directory
cd /d "%SCRIPT_DIR%"

REM Check if dotnet is installed
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Error: .NET runtime not found. Please install .NET 9.0 or later.
    pause
    exit /b 1
)

REM Run the CLI
dotnet run -- %*
