# DataFlow Desktop App

A production-ready desktop application scaffold built with C# .NET 8 and WPF, following Clean Architecture and MVVM patterns.

## 🎯 Overview

**DataFlow Desktop App** is a complete, working example of a data processing desktop application. This scaffold demonstrates:
- Clean separation of concerns (Domain, Application, Infrastructure, UI)
- MVVM pattern implementation
- CSV data import/export
- Data validation and transformation
- Configuration management
- File-based logging
- Comprehensive error handling
- Unit testing foundation

## 🏗️ Architecture

The project follows **Clean Architecture** principles with clear layer separation:

```
┌─────────────────┐
│   Presentation  │  (WPF UI + ViewModels)
├─────────────────┤
│   Application   │  (Use Cases / Business Logic)
├─────────────────┤
│     Domain      │  (Models + Interfaces)
├─────────────────┤
│ Infrastructure  │  (Services, Config, Logging)
└─────────────────┘
```

### Project Structure

- **DataFlowApp** - WPF UI layer with XAML views and ViewModels
- **DataFlowApp.Application** - Business logic and use cases
- **DataFlowApp.Domain** - Core domain models and interfaces (no dependencies)
- **DataFlowApp.Infrastructure** - External concerns (file I/O, logging, config)
- **DataFlowApp.Tests** - Unit tests using xUnit

## ✨ Features

### Core Functionality
- ✅ **Import CSV** - Load data from CSV files with validation
- ✅ **Process Data** - Transform and validate records with business rules
- ✅ **Export CSV** - Save processed data to CSV files
- ✅ **Configuration** - JSON-based app configuration
- ✅ **Logging** - File-based logging with rotation
- ✅ **Error Handling** - Comprehensive error handling and user feedback

### Technical Features
- Clean Architecture with dependency inversion
- MVVM pattern with INotifyPropertyChanged
- Command pattern for user interactions
- Async/await for I/O operations
- Unit tests with xUnit
- Type-safe configuration management

## 🚀 Getting Started

### Prerequisites

- **.NET 8 SDK** or later ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Visual Studio 2022** (17.8+) or **Visual Studio Code** with C# extension
- **Windows 10/11** (for WPF)

### Build & Run

1. **Clone or navigate to the project directory:**
   ```powershell
   cd D:\CTF\tocdai2
   ```

2. **Restore dependencies:**
   ```powershell
   dotnet restore DataFlowApp.sln
   ```

3. **Build the solution:**
   ```powershell
   dotnet build DataFlowApp.sln --configuration Release
   ```

4. **Run the application:**
   ```powershell
   dotnet run --project src\DataFlowApp\DataFlowApp.csproj
   ```

   Or open in Visual Studio and press F5.

### Run Tests

```powershell
dotnet test tests\DataFlowApp.Tests\DataFlowApp.Tests.csproj
```

## 📖 Usage

1. **Import Data**
   - Click "📂 Import CSV"
   - Select a CSV file (use `sample_data.csv` for testing)
   - Data is loaded and validated automatically

2. **Process Data**
   - Click "⚙️ Process Data"
   - Records are validated and transformed
   - Invalid records are marked with error messages

3. **Export Results**
   - Click "💾 Export CSV"
   - Choose output location
   - Processed data is saved to CSV

4. **View Logs**
   - Check the `Logs` folder in the application directory
   - Each session creates a timestamped log file

## 🔧 Customization Guide

### Extending the Data Model

Edit [`DataRecord.cs`](src/DataFlowApp.Domain/Models/DataRecord.cs):
```csharp
public class DataRecord
{
    // Add your custom properties here
    public string CustomField { get; set; }
}
```

### Customizing Validation Rules

Edit [`DataValidator.cs`](src/DataFlowApp.Infrastructure/Services/DataValidator.cs):
```csharp
public ValidationResult ValidateRecord(DataRecord record)
{
    // Add your custom validation logic
    if (record.CustomField == "invalid")
    {
        result.AddError("Custom validation failed");
    }
    return result;
}
```

### Modifying Processing Logic

Edit [`ProcessDataUseCase.cs`](src/DataFlowApp.Application/UseCases/ProcessDataUseCase.cs):
```csharp
private void TransformRecord(DataRecord record)
{
    // Implement your custom transformation
    record.ProcessedValue = YourCustomCalculation(record.Value);
}
```

### Adding New Use Cases

1. Create new use case class in `DataFlowApp.Application/UseCases`
2. Inject dependencies in constructor
3. Implement `ExecuteAsync()` method
4. Add command to `MainViewModel`
5. Add UI button in `MainWindow.xaml`

## 📁 Key Files

| File | Purpose |
|------|---------|
| [`App.xaml.cs`](src/DataFlowApp/App.xaml.cs) | Application startup and dependency setup |
| [`MainViewModel.cs`](src/DataFlowApp/ViewModels/MainViewModel.cs) | Main UI logic and commands |
| [`ImportDataUseCase.cs`](src/DataFlowApp.Application/UseCases/ImportDataUseCase.cs) | Import business logic |
| [`ProcessDataUseCase.cs`](src/DataFlowApp.Application/UseCases/ProcessDataUseCase.cs) | Processing business logic |
| [`CsvDataService.cs`](src/DataFlowApp.Infrastructure/Services/CsvDataService.cs) | CSV file operations |
| [`DataValidator.cs`](src/DataFlowApp.Infrastructure/Services/DataValidator.cs) | Validation rules |
| [`appsettings.json`](config/appsettings.json) | Application configuration |

## 🧪 Testing

Unit tests are located in `tests/DataFlowApp.Tests`. Examples:

- **ProcessDataUseCaseTests** - Tests data processing logic
- **DataValidatorTests** - Tests validation rules

Add your own tests following the same pattern:
```csharp
[Fact]
public async Task YourTest_WithScenario_ExpectedResult()
{
    // Arrange
    // Act
    // Assert
}
```

## 📝 Configuration

Edit `config/appsettings.json` to customize:

```json
{
  "Logging": {
    "LogDirectory": "Logs",
    "LogLevel": "Info"
  },
  "Data": {
    "MaxRecordsToProcess": 10000,
    "ValidateOnImport": true
  },
  "Ui": {
    "WindowWidth": 1200,
    "WindowHeight": 800
  }
}
```

## 🔍 Troubleshooting

**Build errors:**
- Ensure .NET 8 SDK is installed: `dotnet --version`
- Clean and rebuild: `dotnet clean && dotnet build`

**Runtime errors:**
- Check log files in the `Logs` directory
- Ensure CSV files match expected format (see `sample_data.csv`)

**UI not displaying data:**
- Verify records are loaded (check status bar)
- Look for validation errors in logs

## 🎓 Learning Resources

This scaffold demonstrates:
- **Clean Architecture** - Separation of concerns
- **MVVM Pattern** - UI/Logic separation
- **Dependency Injection** - Constructor injection (manual, can use DI container)
- **Command Pattern** - ICommand for user actions
- **Repository Pattern** - IDataService abstraction
- **Unit Testing** - xUnit with AAA pattern

## 📋 Assumptions

1. **Data Format**: CSV files with specific columns (Id, Name, Category, Value, etc.)
2. **Single User**: No multi-user or concurrent access handling
3. **File System**: Application has read/write access to local file system
4. **Windows Platform**: WPF requires Windows OS
5. **Memory Limits**: Up to 10,000 records loaded in memory by default
6. **.NET 8**: Uses latest LTS version of .NET
7. **No External DB**: Uses in-memory storage and CSV files only
8. **English Language**: UI and messages in English
9. **Date Format**: ISO 8601 date format (yyyy-MM-dd HH:mm:ss)
10. **UTF-8 Encoding**: CSV files use UTF-8 encoding

## 🔄 Next Steps

To clone your real application:

1. **Replace Domain Models** - Update `DataRecord` with your actual data structure
2. **Implement Real Validation** - Customize rules in `DataValidator`
3. **Add Business Logic** - Modify `ProcessDataUseCase` transformation
4. **Extend Data Sources** - Add database support, APIs, etc.
5. **Enhance UI** - Add more views, dialogs, visualizations
6. **Add Features** - Search, filtering, sorting, charts, etc.
7. **Implement DI Container** - Use Microsoft.Extensions.DependencyInjection
8. **Add More Tests** - Increase test coverage

## 📄 License

This is a scaffold/template for learning and development purposes. Customize as needed for your project.

---

**Version:** 1.0.0  
**Framework:** .NET 8 / WPF  
**Pattern:** Clean Architecture + MVVM  
**Last Updated:** January 2026
