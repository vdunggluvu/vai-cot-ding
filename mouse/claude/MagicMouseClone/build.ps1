# Build and run script for MagicMouseClone
param(
    [switch]$Clean,
    [switch]$Test,
    [switch]$Run
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host " MagicMouseClone Build Script" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

# Clean
if ($Clean)
{
    Write-Host "[1/4] Cleaning..." -ForegroundColor Yellow
    dotnet clean -c Release
    if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
    if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
}
else
{
    Write-Host "[1/4] Skipping clean" -ForegroundColor Gray
}

# Restore
Write-Host "[2/4] Restoring packages..." -ForegroundColor Yellow
dotnet restore

# Build
Write-Host "[3/4] Building (Release)..." -ForegroundColor Yellow
dotnet build -c Release --no-restore

if ($LASTEXITCODE -ne 0)
{
    Write-Host ""
    Write-Host "❌ Build FAILED!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build succeeded" -ForegroundColor Green

# Test
if ($Test)
{
    Write-Host "[4/4] Running tests..." -ForegroundColor Yellow
    dotnet test -c Release --no-build --verbosity normal

    if ($LASTEXITCODE -ne 0)
    {
        Write-Host ""
        Write-Host "❌ Tests FAILED!" -ForegroundColor Red
        exit 1
    }

    Write-Host "✅ All tests passed" -ForegroundColor Green
}
else
{
    Write-Host "[4/4] Skipping tests" -ForegroundColor Gray
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "✅ Build completed successfully!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan

# Run
if ($Run)
{
    Write-Host ""
    Write-Host "Starting application..." -ForegroundColor Yellow
    dotnet run --project src\MagicMouseClone.App -c Release --no-build
}
