# Run script for MagicKeyboardUtilities Reimplementation
# Traceability: SCRIPTS section - run command

Write-Host "=== Running MagicKeyboardUtilities Reimplementation ===" -ForegroundColor Cyan

$ProjectPath = Join-Path $PSScriptRoot "..\src\MagicKeyboardUtilities.App\MagicKeyboardUtilities.App.csproj"

if (-not (Test-Path $ProjectPath)) {
    Write-Host "ERROR: Project not found at $ProjectPath" -ForegroundColor Red
    exit 1
}

Write-Host "Running project: $ProjectPath" -ForegroundColor Yellow
Write-Host "NOTE: Application will run in background with tray icon" -ForegroundColor Yellow
Write-Host "      Right-click tray icon to Exit" -ForegroundColor Yellow
Write-Host ""

dotnet run --project $ProjectPath -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "=== Application exited with error ===" -ForegroundColor Red
    exit $LASTEXITCODE
}
