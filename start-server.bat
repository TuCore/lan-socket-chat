@echo off
chcp 65001 >nul
echo ============================================
echo   ChatServer Launcher
echo ============================================
echo.

set PORT=9000
if not "%~1"=="" set PORT=%~1

echo Building ChatServer...
dotnet build ChatServer\ChatServer.csproj -c Debug -q
if %errorlevel% neq 0 (
    echo [ERROR] Build failed!
    pause
    exit /b 1
)

echo.
echo Starting ChatServer on port %PORT%...
echo Press Ctrl+C to stop the server.
echo ============================================
echo.

dotnet run --project ChatServer --no-build -- %PORT%

pause
