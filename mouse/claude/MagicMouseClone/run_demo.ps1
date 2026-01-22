# Run demo with fake device backend
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " MagicMouseClone Demo" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This demo uses FakeDeviceBackend to simulate" -ForegroundColor Yellow
Write-Host "a Magic Mouse without real hardware." -ForegroundColor Yellow
Write-Host ""
Write-Host "Features demonstrated:" -ForegroundColor Green
Write-Host "  - System tray integration" -ForegroundColor Gray
Write-Host "  - Device connection simulation" -ForegroundColor Gray
Write-Host "  - Gesture detection (from simulated touches)" -ForegroundColor Gray
Write-Host "  - Action mapping framework" -ForegroundColor Gray
Write-Host ""
Write-Host "Press Ctrl+C to stop." -ForegroundColor Yellow
Write-Host ""

dotnet run --project src\MagicMouseClone.App -c Release
