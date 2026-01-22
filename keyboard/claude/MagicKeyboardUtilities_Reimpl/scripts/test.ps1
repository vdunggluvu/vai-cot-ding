# Test script for MagicKeyboardUtilities Reimplementation
# Traceability: SCRIPTS section - test command

Write-Host "=== Running Tests for MagicKeyboardUtilities Reimplementation ===" -ForegroundColor Cyan

$SolutionPath = Join-Path $PSScriptRoot "..\src\MagicKeyboardUtilities.Reimpl.sln"

if (-not (Test-Path $SolutionPath)) {
    Write-Host "ERROR: Solution not found at $SolutionPath" -ForegroundColor Red
    exit 1
}

Write-Host "Running tests..." -ForegroundColor Yellow
dotnet test $SolutionPath -c Release --verbosity normal

if ($LASTEXITCODE -eq 0) {
    Write-Host "=== All tests passed ===" -ForegroundColor Green
} else {
    Write-Host "=== Some tests failed ===" -ForegroundColor Red
    exit $LASTEXITCODE
}
