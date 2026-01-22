Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Magic Trackpad Clone - Test Script ===" -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$testResultsPath = Join-Path $rootDir "test_results.trx"
$testSummaryPath = Join-Path $rootDir "test_summary.md"
$logsDir = Join-Path $rootDir "logs"

if (-not (Test-Path $logsDir)) {
    New-Item -ItemType Directory -Path $logsDir | Out-Null
}

Write-Host "Running all tests..." -ForegroundColor Yellow

Push-Location $rootDir
try {
    $testOutput = dotnet test --configuration Release --logger "trx;LogFileName=$testResultsPath" --no-build 2>&1 | Out-String
    $testExitCode = $LASTEXITCODE
    
    Write-Host $testOutput
    
    if ($testExitCode -ne 0) {
        Write-Host "`nERROR: Tests failed with exit code $testExitCode" -ForegroundColor Red
    }
}
catch {
    Write-Host "ERROR: Test execution failed: $_" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}

Write-Host "`nParsing test results..." -ForegroundColor Yellow

if (Test-Path $testResultsPath) {
    try {
        [xml]$trxContent = Get-Content $testResultsPath
        $resultSummary = $trxContent.TestRun.ResultSummary
        
        $total = [int]$resultSummary.Counters.total
        $passed = [int]$resultSummary.Counters.passed
        $failed = [int]$resultSummary.Counters.failed
        $skipped = [int]$resultSummary.Counters.inconclusive + [int]$resultSummary.Counters.notExecuted
        
        $executed = $total - $skipped
        $passRate = if ($executed -gt 0) { [math]::Round(($passed / $executed) * 100, 2) } else { 0 }
        
        Write-Host "`n=== Test Results ===" -ForegroundColor Cyan
        Write-Host "Total:   $total" -ForegroundColor White
        Write-Host "Passed:  $passed" -ForegroundColor Green
        Write-Host "Failed:  $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "Green" })
        Write-Host "Skipped: $skipped" -ForegroundColor Yellow
        Write-Host "Pass Rate: $passRate%" -ForegroundColor $(if ($passRate -eq 100) { "Green" } else { "Yellow" })
        
        $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        $summaryContent = "# Test Summary`n"
        $summaryContent += "Generated: $timestamp`n`n"
        $summaryContent += "## Results Overview`n"
        $summaryContent += "- **Total Tests**: $total`n"
        $summaryContent += "- **Passed**: $passed`n"
        $summaryContent += "- **Failed**: $failed`n"
        $summaryContent += "- **Skipped**: $skipped`n"
        $summaryContent += "- **Pass Rate**: $passRate%`n`n"
        $summaryContent += "## Status`n"

        if ($failed -eq 0 -and $executed -gt 0) {
            $summaryContent += "**ALL TESTS PASSED** (excluding skipped tests)`n"
            $allPassed = $true
        }
        else {
            $summaryContent += "**TESTS FAILED**`n"
            $allPassed = $false
        }
        
        if ($failed -gt 0) {
            $summaryContent += "`n## Failed Tests`n"
            $failedTests = $trxContent.TestRun.Results.UnitTestResult | Where-Object { $_.outcome -eq "Failed" }
            foreach ($test in $failedTests) {
                $testName = $test.testName
                $message = $test.Output.ErrorInfo.Message
                $summaryContent += "- **$testName**`n"
                if ($message) {
                    $summaryContent += "  Error: $message`n`n"
                }
            }
        }
        
        $summaryContent += "`n## Test Details`n"
        $summaryContent += "- Test Results File: test_results.trx`n"
        $summaryContent += "- Configuration: Release`n"
        
        $startTime = [DateTime]::Parse($trxContent.TestRun.Times.start)
        $finishTime = [DateTime]::Parse($trxContent.TestRun.Times.finish)
        $duration = $finishTime - $startTime
        $summaryContent += "- Execution Time: $($duration.TotalSeconds) seconds`n`n"
        
        $summaryContent += "## Test Categories`n"

        $testsByNamespace = $trxContent.TestRun.Results.UnitTestResult | Group-Object { 
            $parts = $_.testName -split '\.'
            $parts[0..($parts.Count - 2)] -join '.'
        }
        
        foreach ($group in $testsByNamespace) {
            $summaryContent += "`n### $($group.Name)`n"
            $groupPassed = ($group.Group | Where-Object { $_.outcome -eq "Passed" }).Count
            $groupTotal = $group.Count
            $summaryContent += "- Tests: $groupTotal`n"
            $summaryContent += "- Passed: $groupPassed`n"
        }
        
        $summaryContent | Out-File -FilePath $testSummaryPath -Encoding UTF8
        Write-Host "`nV Test summary created: $testSummaryPath" -ForegroundColor Green
        
        if ($allPassed) {
            Write-Host "`n=== Generating TESTER_GUIDE.md ===" -ForegroundColor Cyan
            & "$scriptDir\generate_tester_guide.ps1"
            if ($LASTEXITCODE -eq 0) {
                Write-Host "V TESTER_GUIDE.md generated" -ForegroundColor Green
            }
        }
        else {
            Write-Host "`nNOTE: TESTER_GUIDE.md will not be generated until all tests pass" -ForegroundColor Yellow
        }
        
        if ($testExitCode -ne 0) {
            Write-Host "`n=== Tests Failed ===" -ForegroundColor Red
            exit 1
        }
        
        Write-Host "`n=== All Tests Passed ===" -ForegroundColor Green
        exit 0
        
    }
    catch {
        Write-Host "ERROR: Failed to parse test results: $_" -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "ERROR: Test results file not found: $testResultsPath" -ForegroundColor Red
    exit 1
}
