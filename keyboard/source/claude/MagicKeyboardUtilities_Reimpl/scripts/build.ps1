# Build script for MagicKeyboardUtilities Reimplementation
# Traceability: SCRIPTS section - build command

Write-Host "=== Building MagicKeyboardUtilities Reimplementation ===" -ForegroundColor Cyan

$SolutionPath = Join-Path $PSScriptRoot "..\src\MagicKeyboardUtilities.Reimpl.sln"

if (-not (Test-Path $SolutionPath)) {
    Write-Host "ERROR: Solution not found at $SolutionPath" -ForegroundColor Red
    exit 1
}

Write-Host "Building solution: $SolutionPath" -ForegroundColor Yellow
dotnet build $SolutionPath -c Release

if ($LASTEXITCODE -eq 0) {
    Write-Host "=== Build completed successfully ===" -ForegroundColor Green
} else {
    Write-Host "=== Build failed ===" -ForegroundColor Red
    exit $LASTEXITCODE
}
