@echo off
REM ============================================================
REM Bizflow Desktop - Dev Runner + Log Tail
REM ============================================================
REM 1. Launches `dotnet run` in a separate background window
REM    (window title: "Bizflow-Backend")
REM 2. Tails the rolling Serilog file in the current window
REM    (handles bizflow-YYYY-MM-DD.log daily rollover)
REM
REM Usage:   run-dev.bat
REM Stop:    Ctrl+C stops the tail; close "Bizflow-Backend" to kill app
REM ============================================================

setlocal

REM -- Config (Serilog writes here via RollingInterval.Day) --
set "LOG_DIR=%LOCALAPPDATA%\Bizflow\Logs"
set "PROJECT_DIR=%~dp0"

echo ============================================================
echo  Bizflow Desktop - Dev Runner
echo ============================================================
echo  Project : %PROJECT_DIR%
echo  Log dir : %LOG_DIR%
echo ============================================================
echo.

REM --- 1. Launch dotnet run in its own background window ---
echo [1/2] Starting "dotnet run" in background window "Bizflow-Backend"...
start "Bizflow-Backend" /D "%PROJECT_DIR%" cmd /k "title Bizflow-Backend && echo Starting dotnet run... && dotnet run"

echo.

REM --- 2. Wait for the first log file to appear ---
echo [2/2] Waiting for log file at %LOG_DIR%\bizflow-*.log ...
echo       (Serilog creates the file on the first log statement)
echo.

REM --- 3. Tail the most recent log file (re-evaluates on rollover) ---
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$logDir = '%LOG_DIR%';" ^
    "$pattern = Join-Path $logDir 'bizflow-*.log';" ^
    "" ^
    "Write-Host '' -NoNewline;" ^
    "while ($true) {" ^
    "    $log = Get-ChildItem -Path $pattern -ErrorAction SilentlyContinue |" ^
    "           Sort-Object LastWriteTime -Descending |" ^
    "           Select-Object -First 1;" ^
    "    if ($log) { break }" ^
    "    Write-Host '.' -NoNewline -ForegroundColor Yellow;" ^
    "    Start-Sleep -Seconds 1" ^
    "};" ^
    "" ^
    "Write-Host '';" ^
    "Write-Host ('=== Tailing: ' + $log.FullName + ' ===') -ForegroundColor Cyan;" ^
    "Write-Host '(Ctrl+C to stop tailing; close Bizflow-Backend window to kill app)' -ForegroundColor DarkGray;" ^
    "Write-Host '';" ^
    "Get-Content -Path $log.FullName -Wait"

endlocal
