@echo off
chcp 65001 >nul
echo ============================================
echo   ChatConsole Launcher (Console)
echo ============================================
echo.

echo Building ChatConsole...
dotnet build ChatConsole\ChatConsole.csproj -c Debug -q
if %errorlevel% neq 0 (
    echo [ERROR] Build failed!
    pause
    exit /b 1
)

echo.
echo Starting ChatConsole...
echo ============================================
echo.

dotnet run --project ChatConsole --no-build

pause
