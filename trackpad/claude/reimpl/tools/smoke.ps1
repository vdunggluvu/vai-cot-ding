Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Magic Trackpad Clone - Smoke Test ===" -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$logsDir = Join-Path $rootDir "logs"
$smokeLogPath = Join-Path $logsDir "smoke.log"
$exePath = Join-Path $rootDir "src\MagicTrackpadClone\bin\Release\net8.0-windows\MagicTrackpadClone.exe"

if (-not (Test-Path $logsDir)) {
    New-Item -ItemType Directory -Path $logsDir | Out-Null
}

if (Test-Path $smokeLogPath) {
    Remove-Item $smokeLogPath
}

function Write-SmokeLog {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logLine = "[$timestamp] $Message"
    Add-Content -Path $smokeLogPath -Value $logLine
    Write-Host $logLine -ForegroundColor Gray
}

Write-Host "Checking for executable..." -ForegroundColor Yellow
Write-SmokeLog "Smoke test started"
Write-SmokeLog "Executable path: $exePath"

if (-not (Test-Path $exePath)) {
    Write-Host "ERROR: Executable not found: $exePath" -ForegroundColor Red
    Write-Host "Please run build.ps1 first" -ForegroundColor Yellow
    Write-SmokeLog "ERROR: Executable not found"
    exit 1
}

Write-Host "V Executable found" -ForegroundColor Green
Write-SmokeLog "Executable found"

Write-Host "`nChecking for existing instances..." -ForegroundColor Yellow
$existingProcess = Get-Process -Name "MagicTrackpadClone" -ErrorAction SilentlyContinue
if ($existingProcess) {
    Write-Host "WARNING: Application is already running. Terminating..." -ForegroundColor Yellow
    Write-SmokeLog "Terminating existing instance (PID: $($existingProcess.Id))"
    Stop-Process -Name "MagicTrackpadClone" -Force
    Start-Sleep -Seconds 2
}

Write-Host "`nLaunching application..." -ForegroundColor Yellow
Write-SmokeLog "Launching application"

try {
    $process = Start-Process -FilePath $exePath -PassThru -WindowStyle Hidden
    Write-SmokeLog "Process started (PID: $($process.Id))"
    Write-Host "V Application started (PID: $($process.Id))" -ForegroundColor Green
    
    Write-Host "`nWaiting 5 seconds..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
    
    $runningProcess = Get-Process -Id $process.Id -ErrorAction SilentlyContinue
    if ($runningProcess) {
        Write-Host "V Application is running" -ForegroundColor Green
        Write-SmokeLog "Application running (Memory: $($runningProcess.WorkingSet64 / 1MB) MB)"
        
        $appLogPattern = Join-Path $logsDir "app_*.log"
        $appLogs = Get-ChildItem -Path $appLogPattern -ErrorAction SilentlyContinue
        if ($appLogs) {
            Write-Host "V Application logs found" -ForegroundColor Green
            Write-SmokeLog "Application logs found: $($appLogs.Count) file(s)"
            
            $latestLog = $appLogs | Sort-Object LastWriteTime -Descending | Select-Object -First 1
            $logContent = Get-Content $latestLog.FullName -Tail 10
            Write-Host "`nRecent log entries:" -ForegroundColor Cyan
            $logContent | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
        }
        else {
            Write-Host "NOTE: No application logs found yet" -ForegroundColor Yellow
            Write-SmokeLog "No application logs found"
        }
        
        Write-Host "`nWaiting 5 more seconds..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
        
        Write-Host "`nTerminating application..." -ForegroundColor Yellow
        Write-SmokeLog "Terminating application"
        Stop-Process -Id $process.Id -Force
        Start-Sleep -Seconds 2
        
        $stillRunning = Get-Process -Id $process.Id -ErrorAction SilentlyContinue
        if ($stillRunning) {
            Write-Host "WARNING: Application did not terminate cleanly" -ForegroundColor Yellow
            Write-SmokeLog "WARNING: Application did not terminate cleanly"
            Stop-Process -Id $process.Id -Force
        }
        else {
            Write-Host "V Application terminated cleanly" -ForegroundColor Green
            Write-SmokeLog "Application terminated cleanly"
        }
        
        Write-Host "`n=== Smoke Test PASSED ===" -ForegroundColor Green
        Write-SmokeLog "Smoke test PASSED"
        exit 0
    }
    else {
        Write-Host "ERROR: Application crashed or exited immediately" -ForegroundColor Red
        Write-SmokeLog "ERROR: Application not running after launch"
        
        $appLogPattern = Join-Path $logsDir "app_*.log"
        $appLogs = Get-ChildItem -Path $appLogPattern -ErrorAction SilentlyContinue | 
                   Sort-Object LastWriteTime -Descending | Select-Object -First 1
        
        if ($appLogs) {
            Write-Host "`nRecent application log:" -ForegroundColor Yellow
            Get-Content $appLogs.FullName -Tail 20 | ForEach-Object { 
                Write-Host "  $_" -ForegroundColor Gray 
            }
        }
        
        Write-Host "`n=== Smoke Test FAILED ===" -ForegroundColor Red
        Write-SmokeLog "Smoke test FAILED"
        exit 1
    }
}
catch {
    Write-Host "ERROR: Failed to launch application: $_" -ForegroundColor Red
    Write-SmokeLog "ERROR: Exception during smoke test: $_"
    Write-Host "`n=== Smoke Test FAILED ===" -ForegroundColor Red
    exit 1
}
