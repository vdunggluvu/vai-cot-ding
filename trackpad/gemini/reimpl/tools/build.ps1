Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Join-Path $scriptDir ".."
$flowFile = Join-Path $rootDir "..\flow.md"
$logDir = Join-Path $rootDir "logs"
$reportFile = Join-Path $rootDir "docs\validation_report.md"

if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir | Out-Null }
if (-not (Test-Path $rootDir\docs)) { New-Item -ItemType Directory -Path $rootDir\docs | Out-Null }

function Log-Message {
    param([string]$msg)
    Write-Host "[BUILD] $msg"
    Add-Content -Path $reportFile -Value "- [BUILD] $msg" -Force
}

# Initialize Report
"
# Validation Report
Date: $(Get-Date)
" | Out-File -FilePath $reportFile -Force

Log-Message "Checking prerequisites..."

# 1. Check flow.md
if (-not (Test-Path $flowFile)) {
    $altFlow = Join-Path $rootDir "..\claude-flow.md"
    if (Test-Path $altFlow) {
        $flowFile = $altFlow
        Log-Message "Found flow spec at $flowFile"
    } else {
        Log-Message "ERROR: flow.md not found at $flowFile"
        exit 1
    }
}

# 2. Check dotnet
try {
    $dotnetInfo = dotnet --info
    Log-Message "Dotnet found: $($dotnetInfo | Select-Object -First 1)"
} catch {
    Log-Message "ERROR: dotnet SDK not found. Please install .NET 8 SDK."
    exit 1
}

# 3. Build
Log-Message "Restoring solution..."
dotnet restore "$rootDir\MagicTrackpadUtilities.slnx"
if ($LASTEXITCODE -ne 0) { Log-Message "Restore failed"; exit 1 }

Log-Message "Building solution..."
dotnet build "$rootDir\MagicTrackpadUtilities.slnx" --no-restore --configuration Release
if ($LASTEXITCODE -ne 0) { Log-Message "Build failed"; exit 1 }

Log-Message "Build Success!"
exit 0
