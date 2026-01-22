# BUILD INSTRUCTIONS

## Prerequisites

### Required Software

1. **.NET 8.0 SDK** (or higher)
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Choose "SDK" (not Runtime)
   - Install and verify:
     ```powershell
     dotnet --version
     ```
   - Should output: `8.0.x` or higher

2. **PowerShell 5.1+** (included in Windows 10/11)

3. **Visual Studio 2022** (optional, for IDE development)
   - Workload: ".NET desktop development"
   - Alternative: VS Code with C# extension

## Build Steps

### Method 1: Using Build Script (Recommended)

```powershell
cd MagicKeyboardUtilities_Reimpl
.\scripts\build.ps1
```

### Method 2: Direct dotnet Command

```powershell
cd MagicKeyboardUtilities_Reimpl
dotnet build src\MagicKeyboardUtilities.Reimpl.sln -c Release
```

### Method 3: Visual Studio

1. Open `src\MagicKeyboardUtilities.Reimpl.sln`
2. Select "Release" configuration
3. Build → Build Solution (Ctrl+Shift+B)

## Build Output

Compiled binary location:
```
src\MagicKeyboardUtilities.App\bin\Release\net8.0-windows\MagicKeyboardUtilities.exe
```

## Run

### Method 1: Using Run Script

```powershell
.\scripts\run.ps1
```

### Method 2: Direct Execution

```powershell
dotnet run --project src\MagicKeyboardUtilities.App -c Release
```

### Method 3: Run Binary Directly

```powershell
cd src\MagicKeyboardUtilities.App\bin\Release\net8.0-windows
.\MagicKeyboardUtilities.exe
```

## Test

```powershell
.\scripts\test.ps1
```

Or:

```powershell
dotnet test src\MagicKeyboardUtilities.Reimpl.sln -c Release
```

## Troubleshooting

### Error: "dotnet not found"
- Install .NET 8.0 SDK from https://dotnet.microsoft.com/download
- Restart terminal after installation

### Error: "Cannot load config.json"
- Ensure `config.json` is copied to output directory
- Check project file has: `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>`

### Error: "Hook installation failed"
- May require admin privileges on some systems
- Try running with elevated permissions
- Or keep hooks disabled (default)

### Error: "Assembly not found"
- Ensure all NuGet packages are restored:
  ```powershell
  dotnet restore src\MagicKeyboardUtilities.Reimpl.sln
  ```

## Clean Build

```powershell
dotnet clean src\MagicKeyboardUtilities.Reimpl.sln
dotnet build src\MagicKeyboardUtilities.Reimpl.sln -c Release
```

## Publish Standalone

To create self-contained executable (no .NET runtime required):

```powershell
dotnet publish src\MagicKeyboardUtilities.App -c Release -r win-x64 --self-contained
```

Output: `src\MagicKeyboardUtilities.App\bin\Release\net8.0-windows\win-x64\publish\`

## Build Configuration

- **Target Framework**: net8.0-windows
- **Platform**: x64
- **Output Type**: WinExe (Windows GUI app)
- **Allow Unsafe**: No
- **Nullable**: Enabled

## Dependencies

All managed by NuGet:
- Microsoft.Extensions.Logging (8.0.0)
- Microsoft.Extensions.Logging.Console (8.0.0)
- System.Text.Json (8.0.0)
- xunit (2.6.2) - Test project only
- Microsoft.NET.Test.Sdk (17.8.0) - Test project only

No native dependencies required.
