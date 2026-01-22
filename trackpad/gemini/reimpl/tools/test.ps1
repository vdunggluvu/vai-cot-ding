Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Join-Path $scriptDir ".."
$trxPath = Join-Path $rootDir "test_results.trx"
$summaryPath = Join-Path $rootDir "test_summary.md"
$testerGuidePath = Join-Path $rootDir "TESTER_GUIDE.md"

Write-Host "Running Tests..."

$testCmd = "dotnet test `"$rootDir\MagicTrackpadUtilities.slnx`" --configuration Release --logger `"trx;LogFileName=$trxPath`""
Invoke-Expression $testCmd

# Parse TRX to get results (simple regex or xml)
# For simplicity, we assume generic pass/fail check from exit code first, then parse XML for details if needed.
# But dotnet test returns 1 if fails.

if ($LASTEXITCODE -eq 0) {
    Write-Host "Tests PASSED."
    "
# Test Summary
Result: PASS
Date: $(Get-Date)
" | Out-File $summaryPath -Force

    # Generate TESTER_GUIDE.md if not exists (or always if requirement says so)
    # Requirement: "CHỈ được tạo ... khi PASS 100%"
    
    $guideContent = "
# TESTER GUIDE - MagicTrackpadUtilities Clone

## Pre-requisites
- Windows 10 or Windows 11
- .NET 8 Runtime (or Desktop Runtime)

## Installation / Run
1. Open PowerShell
2. Run: `.\tools\build.ps1`
3. Run: `.\tools\smoke.ps1`

## Test Checklist
- [ ] Startup (System Tray Icon appears)
- [ ] Singleton check (Launch 2nd instance, it should exit)
- [ ] Settings (Open config from Tray)
- [ ] Gestures (Mock Mode):
    - Verify logs in `./logs/debug.log` show gesture detection
    - Verify `SendInput` simulation (e.g. key presses)

## Reporting Bugs
Attach `test_results.trx` and `logs/*.log`.
"
    $guideContent | Out-File $testerGuidePath -Force
    Write-Host "Generated TESTER_GUIDE.md"
    exit 0
} else {
    Write-Host "Tests FAILED."
    "
# Test Summary
Result: FAIL
Date: $(Get-Date)
" | Out-File $summaryPath -Force
    exit 1
}
