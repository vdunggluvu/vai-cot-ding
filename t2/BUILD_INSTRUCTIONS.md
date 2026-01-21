# DataFlow Desktop App - Build and Run Guide

## Prerequisites Installation

### 1. Install .NET 8 SDK

**Download and install .NET 8 SDK:**
- Visit: https://dotnet.microsoft.com/download/dotnet/8.0
- Download "SDK 8.0.x" for Windows x64
- Run the installer and follow prompts
- Restart terminal/PowerShell after installation

**Verify installation:**
```powershell
dotnet --version
# Should output: 8.0.x or higher
```

### 2. Install Visual Studio (Optional but Recommended)

**Option A: Visual Studio 2022 Community (Recommended)**
- Download: https://visualstudio.microsoft.com/downloads/
- During installation, select:
  - ".NET desktop development" workload
  - Individual components: .NET 8.0 Runtime
- This provides IDE with debugging, IntelliSense, and GUI designer

**Option B: Visual Studio Code (Lightweight)**
- Download: https://code.visualstudio.com/
- Install C# extension (ms-dotnettools.csharp)
- Install .NET Extension Pack

## Build Instructions

### Method 1: Using Visual Studio 2022

1. **Open Solution**
   ```
   Double-click DataFlowApp.sln
   ```

2. **Restore NuGet Packages**
   - Right-click solution → "Restore NuGet Packages"
   - Or: Build → Rebuild Solution (automatic restore)

3. **Build**
   - Press `Ctrl+Shift+B` or
   - Build → Build Solution

4. **Run**
   - Press `F5` (Debug) or `Ctrl+F5` (Run without debugging)
   - Or: Debug → Start Debugging

### Method 2: Using Command Line

1. **Navigate to project directory**
   ```powershell
   cd D:\CTF\tocdai2
   ```

2. **Restore dependencies**
   ```powershell
   dotnet restore DataFlowApp.sln
   ```

3. **Build Release version**
   ```powershell
   dotnet build DataFlowApp.sln --configuration Release
   ```

4. **Run the application**
   ```powershell
   dotnet run --project src\DataFlowApp\DataFlowApp.csproj
   ```

   Or run the executable directly:
   ```powershell
   .\src\DataFlowApp\bin\Release\net8.0-windows\DataFlowApp.exe
   ```

### Method 3: Using Build Script

Run the provided PowerShell script:
```powershell
.\build.ps1
```

## Run Tests

```powershell
# Run all tests
dotnet test tests\DataFlowApp.Tests\DataFlowApp.Tests.csproj

# Run with detailed output
dotnet test tests\DataFlowApp.Tests\DataFlowApp.Tests.csproj --logger "console;verbosity=detailed"

# Run with code coverage
dotnet test tests\DataFlowApp.Tests\DataFlowApp.Tests.csproj --collect:"XPlat Code Coverage"
```

## Quick Start After Build

1. **Launch the application**
   - Run from Visual Studio (F5) or use command line

2. **Import sample data**
   - Click "📂 Import CSV" button
   - Navigate to project root
   - Select `sample_data.csv`
   - Click Open

3. **Process the data**
   - Click "⚙️ Process Data" button
   - View results in the grid
   - Check validation messages

4. **Export results**
   - Click "💾 Export CSV" button
   - Choose save location
   - Enter filename (e.g., `output.csv`)
   - Click Save

5. **Check logs**
   - Navigate to `Logs` folder in project directory
   - Open the latest `.log` file to see execution details

## Troubleshooting

### "No .NET SDKs were found"
**Solution:** Install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0

### "Could not load file or assembly"
**Solution:** 
```powershell
dotnet clean
dotnet restore
dotnet build
```

### "Access denied" errors
**Solution:** Run Visual Studio or PowerShell as Administrator

### Tests failing
**Solution:**
- Ensure test project references are correct
- Check that `TestLogs` folder can be created
- Run from project root directory

### Application crashes on startup
**Solution:**
- Check `Logs` folder for error details
- Ensure `config` folder exists
- Verify write permissions on application directory

## Build Outputs

After successful build:
- **Executable**: `src\DataFlowApp\bin\Release\net8.0-windows\DataFlowApp.exe`
- **Config**: `config\appsettings.json` (copied to output)
- **Logs**: Created in `Logs\` folder at runtime
- **Test Results**: `tests\DataFlowApp.Tests\bin\Release\net8.0\`

## Performance Notes

- Application loads up to 10,000 records by default (configurable)
- First run may be slower due to JIT compilation
- Log files are auto-flushed for reliability
- CSV parsing handles files up to ~100MB efficiently

## Environment Requirements

| Requirement | Minimum | Recommended |
|-------------|---------|-------------|
| OS | Windows 10 1607+ | Windows 11 |
| .NET | 8.0 | 8.0 LTS |
| RAM | 2 GB | 4 GB+ |
| Disk | 500 MB | 1 GB+ |
| Screen | 1024x768 | 1920x1080 |

## Development Tips

### Hot Reload (Visual Studio 2022)
Enable Hot Reload for faster development:
- Debug → Options → .NET/C++ Hot Reload → Enable Hot Reload

### XAML Designer
View UI while editing:
- Double-click `.xaml` files in Solution Explorer
- Designer shows above/below code editor

### Debugging
- Set breakpoints: Click left margin in code editor (F9)
- Step through: F10 (step over), F11 (step into)
- Watch variables: Debug → Windows → Watch

### Code Cleanup
Format code automatically:
```
Edit → Advanced → Format Document (Ctrl+K, Ctrl+D)
```

## Next Steps

See [README.md](README.md) for:
- Architecture overview
- Customization guide
- Feature documentation
- Testing strategies
