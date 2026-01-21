# PowerShell Build Script for DataFlow Desktop App
# Run this script to build the application: .\build.ps1

param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [Parameter()]
    [switch]$RunTests,
    
    [Parameter()]
    [switch]$Launch
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " DataFlow Desktop App - Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET SDK is installed
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = & dotnet --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: .NET SDK not found!" -ForegroundColor Red
    Write-Host "Please install .NET 8 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Red
    exit 1
}
Write-Host "✓ .NET SDK version: $dotnetVersion" -ForegroundColor Green
Write-Host ""

# Navigate to solution directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
& dotnet clean DataFlowApp.sln --configuration $Configuration --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Clean failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Clean completed" -ForegroundColor Green
Write-Host ""

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
& dotnet restore DataFlowApp.sln --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Restore failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Restore completed" -ForegroundColor Green
Write-Host ""

# Build solution
Write-Host "Building solution ($Configuration)..." -ForegroundColor Yellow
& dotnet build DataFlowApp.sln --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Build completed successfully" -ForegroundColor Green
Write-Host ""

# Run tests if requested
if ($RunTests) {
    Write-Host "Running unit tests..." -ForegroundColor Yellow
    & dotnet test tests\DataFlowApp.Tests\DataFlowApp.Tests.csproj --configuration $Configuration --no-build --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "WARNING: Some tests failed!" -ForegroundColor Red
    } else {
        Write-Host "✓ All tests passed" -ForegroundColor Green
    }
    Write-Host ""
}

# Show build output location
$exePath = "src\DataFlowApp\bin\$Configuration\net8.0-windows\DataFlowApp.exe"
if (Test-Path $exePath) {
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host " Build Summary" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Configuration : $Configuration" -ForegroundColor White
    Write-Host "Executable    : $exePath" -ForegroundColor White
    Write-Host "Size          : $([math]::Round((Get-Item $exePath).Length / 1KB, 2)) KB" -ForegroundColor White
    Write-Host ""
    
    if ($Launch) {
        Write-Host "Launching application..." -ForegroundColor Yellow
        & $exePath
    } else {
        Write-Host "To run the application:" -ForegroundColor Yellow
        Write-Host "  .\$exePath" -ForegroundColor White
        Write-Host ""
        Write-Host "Or use this command:" -ForegroundColor Yellow
        Write-Host "  dotnet run --project src\DataFlowApp\DataFlowApp.csproj" -ForegroundColor White
    }
} else {
    Write-Host "ERROR: Executable not found at expected location!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Build completed successfully! ✓" -ForegroundColor Green
