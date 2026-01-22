Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Join-Path $scriptDir ".."
$binPath = Join-Path $rootDir "src\MagicTrackpad.UI\bin\Release\net10.0-windows\MagicTrackpad.UI.exe"
$logDir = Join-Path $rootDir "logs"

if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir | Out-Null }
$smokeLog = Join-Path $logDir "smoke.log"

"Smoke Test Started: $(Get-Date)" | Add-Content $smokeLog

# Check binary exists
if (-not (Test-Path $binPath)) {
    "Binary not found at $binPath" | Add-Content $smokeLog
    Write-Error "Binary not found. Run build.ps1 first."
}

# Start Process
$proc = Start-Process -FilePath $binPath -PassThru
"Process Started: ID $($proc.Id)" | Add-Content $smokeLog
Write-Host "App launched. Waiting 5s..."

Start-Sleep -Seconds 5

if ($proc.HasExited) {
    "Process exited prematurely with code $($proc.ExitCode)" | Add-Content $smokeLog
    Write-Error "Smoke Test Failed: Process exited too early."
} else {
    "Process still running (OK)." | Add-Content $smokeLog
    Write-Host "Process running. Killing process to finish smoke test..."
    Stop-Process -Id $proc.Id -Force
    "Smoke Test PASSED." | Add-Content $smokeLog
    exit 0
}
