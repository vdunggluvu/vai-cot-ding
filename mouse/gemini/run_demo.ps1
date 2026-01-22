# Run Demo Script
$ErrorActionPreference = "Stop"

$appPath = ".\MagicMouseClone\src\MagicMouseClone.App\bin\Release\net8.0-windows\MagicMouseClone.App.exe"

if (-not (Test-Path $appPath)) {
    Write-Error "App not found! Please run build.ps1 first."
}

Write-Host "Starting Magic Mouse Clone..." -ForegroundColor Cyan
Start-Process $appPath

Write-Host "App started in Background (Tray). Check your system tray!" -ForegroundColor Green
