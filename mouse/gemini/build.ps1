# Build and Test Script
$ErrorActionPreference = "Stop"

Write-Host "Restoring..." -ForegroundColor Cyan
dotnet restore MagicMouseClone/MagicMouseClone.sln

Write-Host "Building (Release)..." -ForegroundColor Cyan
dotnet build MagicMouseClone/MagicMouseClone.sln -c Release

Write-Host "Running Tests..." -ForegroundColor Cyan
dotnet test MagicMouseClone/MagicMouseClone.sln -c Release --no-build

Write-Host "BUILD AND TEST SUCCESSFUL!" -ForegroundColor Green
