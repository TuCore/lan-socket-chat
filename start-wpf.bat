@echo off
chcp 65001 >nul
echo ============================================
echo   ChatDesktop Launcher (WPF)
echo ============================================
echo.

echo Building ChatDesktop...
dotnet build ChatDesktop\ChatDesktop.csproj -c Debug -q
if %errorlevel% neq 0 (
    echo [ERROR] Build failed!
    pause
    exit /b 1
)

echo.
echo Starting ChatDesktop (WPF)...
echo ============================================
echo.

start "" dotnet run --project ChatDesktop --no-build

echo ChatDesktop started in a new window.
timeout /t 2 /nobreak >nul
