@echo off
chcp 65001 >nul
echo ============================================
echo   ChatSystem - Demo Launcher
echo ============================================
echo.
REM ---- Step 0: Cleanup ----
echo [*] Cleaning up previous background processes...
REM Kill cmd windows by title (from previous run)
taskkill /f /fi "WINDOWTITLE eq ChatServer*" >nul 2>&1
taskkill /f /fi "WINDOWTITLE eq ChatConsole*" >nul 2>&1
taskkill /f /fi "WINDOWTITLE eq ChatDesktop*" >nul 2>&1
REM Kill compiled executables (if any)
taskkill /f /im ChatServer.exe >nul 2>&1
taskkill /f /im ChatConsole.exe >nul 2>&1
taskkill /f /im ChatDesktop.exe >nul 2>&1
REM Kill dotnet.exe processes running our projects (the actual processes from "dotnet run")
for /f "tokens=2" %%i in ('wmic process where "commandline like '%%ChatServer%%' and name='dotnet.exe'" get processid 2^>nul ^| findstr /r "[0-9]"') do (
    taskkill /f /pid %%i >nul 2>&1
)
for /f "tokens=2" %%i in ('wmic process where "commandline like '%%ChatConsole%%' and name='dotnet.exe'" get processid 2^>nul ^| findstr /r "[0-9]"') do (
    taskkill /f /pid %%i >nul 2>&1
)
for /f "tokens=2" %%i in ('wmic process where "commandline like '%%ChatDesktop%%' and name='dotnet.exe'" get processid 2^>nul ^| findstr /r "[0-9]"') do (
    taskkill /f /pid %%i >nul 2>&1
)
REM Shutdown dotnet build servers (MSBuild, VBCSCompiler, etc.) that lock obj files
dotnet build-server shutdown >nul 2>&1
timeout /t 2 /nobreak >nul
echo.

REM ---- Step 1: Build ----
echo [1/3] Building solution...
dotnet build ChatSystem.slnx -c Debug -q
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Build failed! Fix errors before running.
    pause
    exit /b 1
)
echo       Build succeeded!
echo.

REM ---- Step 2: Start Server ----
echo [2/3] Starting ChatServer on port 9000...
start "ChatServer - Port 9000" cmd /k "title ChatServer - Port 9000 && dotnet run --project ChatServer --no-build"

echo       Waiting for server to start...
timeout /t 3 /nobreak >nul
echo       Server started!
echo.

REM ---- Step 3: Start 5 Clients ----
echo [3/3] Starting 5 clients (3 Console + 2 WPF)...
echo.

echo       Starting Console Client 1...
start "ChatConsole 1" cmd /k "title ChatConsole 1 && dotnet run --project ChatConsole --no-build"
timeout /t 1 /nobreak >nul

echo       Starting Console Client 2...
start "ChatConsole 2" cmd /k "title ChatConsole 2 && dotnet run --project ChatConsole --no-build"
timeout /t 1 /nobreak >nul

echo       Starting Console Client 3...
start "ChatConsole 3" cmd /k "title ChatConsole 3 && dotnet run --project ChatConsole --no-build"
timeout /t 1 /nobreak >nul

echo       Starting WPF Client 1...
start "ChatDesktop 1" cmd /c "dotnet run --project ChatDesktop --no-build"
timeout /t 1 /nobreak >nul

echo       Starting WPF Client 2...
start "ChatDesktop 2" cmd /c "dotnet run --project ChatDesktop --no-build"

echo.
echo ============================================
echo   All processes started!
echo.
echo   Server : 1  (port 9000)
echo   Console: 3  (ChatConsole)
echo   WPF    : 2  (ChatDesktop)
echo   Total  : 5 clients
echo.
echo   To stop all: run stop-all.bat
echo ============================================
echo.
pause
