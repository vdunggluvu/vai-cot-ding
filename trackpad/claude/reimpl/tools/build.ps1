Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Magic Trackpad Clone - Build Script ===" -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$flowSpecPath = Join-Path (Split-Path -Parent $rootDir) "claude-flow.md"
$validationReportPath = Join-Path $rootDir "docs\validation_report.md"

$docsDir = Join-Path $rootDir "docs"
if (-not (Test-Path $docsDir)) {
    New-Item -ItemType Directory -Path $docsDir | Out-Null
}

Write-Host "Checking for flow specification..." -ForegroundColor Yellow
if (-not (Test-Path $flowSpecPath)) {
    Write-Host "ERROR: Flow specification not found at: $flowSpecPath" -ForegroundColor Red
    Write-Host "Expected location: ../claude-flow.md (relative to reimpl folder)" -ForegroundColor Red
    exit 1
}
Write-Host "V Flow specification found" -ForegroundColor Green

Write-Host "`nChecking for .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "V .NET SDK found: $dotnetVersion" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: .NET SDK not found!" -ForegroundColor Red
    Write-Host "`nPlease install .NET SDK:" -ForegroundColor Yellow
    Write-Host "  1. Visit: https://dotnet.microsoft.com/download" -ForegroundColor White
    Write-Host "  2. Download .NET 8 SDK (or .NET 6 SDK minimum)" -ForegroundColor White
    Write-Host "  3. Run the installer and restart your terminal" -ForegroundColor White
    exit 1
}

Write-Host "`nGathering environment information..." -ForegroundColor Yellow
$dotnetInfo = dotnet --info | Out-String
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

$validationContent = "# Validation Report`n"
$validationContent += "Generated: $timestamp`n`n"
$validationContent += "## Environment Information`n`n"
$validationContent += "### .NET SDK`n"
$validationContent += "````````n"
$validationContent += $dotnetInfo
$validationContent += "````````n`n"
$validationContent += "### Operating System`n"
$validationContent += "- OS: " + [System.Environment]::OSVersion.ToString() + "`n"
$validationContent += "- Version: " + [System.Environment]::OSVersion.Version.ToString() + "`n"
$validationContent += "- Platform: " + [System.Environment]::OSVersion.Platform.ToString() + "`n"
$validationContent += "- Machine Name: " + [System.Environment]::MachineName + "`n"
$validationContent += "- User: " + [System.Environment]::UserName + "`n"
$validationContent += "- 64-bit OS: " + [System.Environment]::Is64BitOperatingSystem + "`n"
$validationContent += "- 64-bit Process: " + [System.Environment]::Is64BitProcess + "`n`n"
$validationContent += "### Build Configuration`n"
$validationContent += "- Root Directory: $rootDir`n"
$validationContent += "- Flow Spec: $flowSpecPath`n"
$validationContent += "- Flow Spec Exists: " + (Test-Path $flowSpecPath) + "`n"

$validationContent | Out-File -FilePath $validationReportPath -Encoding UTF8
Write-Host "V Environment information captured" -ForegroundColor Green

Write-Host "`nRestoring NuGet packages..." -ForegroundColor Yellow
Push-Location $rootDir
try {
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: dotnet restore failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
    Write-Host "V Packages restored" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: Failed to restore packages: $_" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}

Write-Host "`nBuilding solution..." -ForegroundColor Yellow
Push-Location $rootDir
try {
    dotnet build --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
    Write-Host "V Build successful" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: Build failed: $_" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}

$buildTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$buildResults = "`n## Build Results`n"
$buildResults += "- Status: SUCCESS`n"
$buildResults += "- Configuration: Release`n"
$buildResults += "- Build Time: $buildTime`n"

$buildResults | Out-File -FilePath $validationReportPath -Append -Encoding UTF8

Write-Host "`n=== Build Complete ===" -ForegroundColor Green
Write-Host "Validation report: $validationReportPath" -ForegroundColor Cyan
