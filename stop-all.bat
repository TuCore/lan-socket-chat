@echo off
chcp 65001 >nul
echo ============================================
echo   Stop All Chat Processes
echo ============================================
echo.

echo Stopping ChatServer processes...
taskkill /f /fi "WINDOWTITLE eq ChatServer*" >nul 2>&1

echo Stopping ChatConsole processes...
taskkill /f /fi "WINDOWTITLE eq ChatConsole*" >nul 2>&1

echo Stopping ChatDesktop processes...
taskkill /f /im ChatDesktop.exe >nul 2>&1

REM Also kill any dotnet processes running our projects
echo Cleaning up remaining dotnet processes...
for /f "tokens=2" %%i in ('wmic process where "commandline like '%%ChatServer%%' and name='dotnet.exe'" get processid 2^>nul ^| findstr /r "[0-9]"') do (
    taskkill /f /pid %%i >nul 2>&1
)
for /f "tokens=2" %%i in ('wmic process where "commandline like '%%ChatConsole%%' and name='dotnet.exe'" get processid 2^>nul ^| findstr /r "[0-9]"') do (
    taskkill /f /pid %%i >nul 2>&1
)
for /f "tokens=2" %%i in ('wmic process where "commandline like '%%ChatDesktop%%' and name='dotnet.exe'" get processid 2^>nul ^| findstr /r "[0-9]"') do (
    taskkill /f /pid %%i >nul 2>&1
)

echo.
echo All chat processes stopped!
echo ============================================
echo.
pause
