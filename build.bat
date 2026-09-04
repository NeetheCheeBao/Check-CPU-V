@echo off
setlocal EnableExtensions EnableDelayedExpansion
cd /d "%~dp0"

where dotnet >nul 2>&1
if errorlevel 1 (
  echo ERROR: dotnet CLI not found on PATH.
  echo Install .NET SDK and add it to PATH, or check your Environment Variables.
  echo.
  pause
  exit /b 1
)

set "CONFIG=Release"
set "OUTDIR=dist"
set "BAR_WIDTH=30"
set /a TOTAL_STEPS=5
set /a CURRENT_STEP=0

cls

call :progress "Cleaning output..."
if exist "%OUTDIR%" rmdir /s /q "%OUTDIR%" >nul 2>&1
mkdir "%OUTDIR%" >nul 2>&1

call :progress "Restoring packages..."
dotnet restore --nologo -v q >nul 2>&1
if errorlevel 1 goto :fail

call :progress "Building project..."
dotnet build -c %CONFIG% --no-restore --nologo -v q >nul 2>&1
if errorlevel 1 goto :fail

call :progress "Publishing to %OUTDIR%..."
dotnet publish -c %CONFIG% -o %OUTDIR% --no-build --nologo -v q >nul 2>&1
if errorlevel 1 goto :fail

call :progress "Finalizing..."

echo.
echo BUILD OK  -^> %OUTDIR%\
echo.

timeout /t 1 /nobreak >nul
exit /b 0

:fail
echo.
echo BUILD FAILED
echo.
pause
exit /b 1

:progress
set /a CURRENT_STEP+=1
set /a "pct=CURRENT_STEP*100/TOTAL_STEPS"
set /a "filled=CURRENT_STEP*BAR_WIDTH/TOTAL_STEPS"
if !filled! gtr %BAR_WIDTH% set "filled=%BAR_WIDTH%"

set "bar="
for /L %%i in (1,1,%BAR_WIDTH%) do (
    if %%i LEQ !filled! (set "bar=!bar!=") else (set "bar=!bar! ")
)

echo [!bar!] !pct!%%  %~1
goto :eof