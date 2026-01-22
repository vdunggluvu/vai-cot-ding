# 📚 ASSUMPTIONS & DESIGN DECISIONS

## Core Assumptions

### 1. **Data Format & Structure**
- CSV files use UTF-8 encoding
- CSV header is always the first line
- Column separator is comma (`,`)
- Text fields are quoted when containing special characters
- Date format: `yyyy-MM-dd HH:mm:ss` (ISO 8601)
- Decimal separator: `.` (dot, culture-invariant)
- Maximum file size: ~100MB for reasonable performance

### 2. **Business Domain**
- Application processes "data records" - generic term allowing customization
- Each record has: ID, Name, Category, Value, Date, Status, Description
- Categories are predefined: "Type A", "Type B", "Type C", "Other"
- Valid statuses: "Pending", "Processed", "Completed", "Failed"
- Processing applies category-based multipliers (example transformation)
- Validation rules are customizable business logic

### 3. **Runtime Environment**
- **Operating System**: Windows 10 (1607+) or Windows 11
- **Framework**: .NET 8.0 LTS (Long Term Support)
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Single User**: No concurrent access or multi-user scenarios
- **Desktop Application**: Not a web app or service
- **Local Execution**: Runs on user's machine, not deployed remotely

### 4. **Data Persistence**
- **Primary Storage**: In-memory during runtime
- **File Format**: CSV for import/export
- **No Database**: Uses file system only (easily extendable to DB)
- **Configuration**: JSON file (`appsettings.json`)
- **Logs**: Text files with daily rotation
- **State**: Not persisted between sessions (can be added)

### 5. **Performance & Scale**
- Maximum records in memory: 10,000 (configurable)
- UI remains responsive for up to ~50,000 records
- Import/export operations are async to prevent UI freezing
- No pagination - all records loaded at once
- Suitable for datasets < 10MB for optimal UX

### 6. **User Interface**
- **Language**: English (UI, messages, logs)
- **Screen Resolution**: Minimum 1024x768, optimal 1920x1080
- **Theme**: Light theme (dark mode not implemented)
- **Accessibility**: Basic keyboard navigation, no screen reader support yet
- **User Skill Level**: Technical users comfortable with CSV files

### 7. **Error Handling**
- User-friendly error messages in dialog boxes
- Technical details logged to file
- Application does not crash on validation errors
- Invalid records are marked but not deleted
- Failed operations provide actionable feedback

### 8. **Security & Access**
- **No Authentication**: Single-user desktop app
- **File Access**: Requires read/write permissions on selected folders
- **Data Validation**: Input sanitization for CSV parsing
- **No Encryption**: Data stored in plain text CSV/JSON
- **Trust Model**: User is trusted to select legitimate files

### 9. **Logging**
- File-based logging to `Logs/` folder
- Log files named: `app_yyyyMMdd_HHmmss.log`
- Levels: DEBUG, INFO, WARN, ERROR
- Console output mirrored for debugging
- No log rotation (manual cleanup required)

### 10. **Testing**
- Unit tests for business logic (use cases, validators)
- No integration tests or UI tests included
- Test framework: xUnit
- Code coverage not measured (can be added)
- Tests use temp files, cleaned up after execution

## Design Decisions

### Architecture Choices

**Clean Architecture**
- **Why**: Separation of concerns, testability, maintainability
- **Tradeoff**: More files/folders, initial complexity
- **Benefit**: Easy to swap implementations (e.g., CSV → Database)

**MVVM Pattern**
- **Why**: WPF best practice, separation of UI and logic
- **Tradeoff**: More boilerplate (ViewModels, Commands)
- **Benefit**: Testable logic, data binding, UI updates

**Dependency Injection (Manual)**
- **Why**: Decoupling, testability
- **Why Not DI Container**: Keep scaffold simple and educational
- **Future**: Can add Microsoft.Extensions.DependencyInjection

**Async/Await**
- **Why**: Responsive UI during I/O operations
- **Tradeoff**: Slightly more complex code
- **Benefit**: No UI freezing, better UX

### Technology Choices

**WPF vs WinUI 3**
- **Chose WPF**: Mature, stable, wide documentation
- **WinUI 3**: Modern but newer, less Stack Overflow answers
- **Future**: Can migrate to WinUI 3 or Avalonia for cross-platform

**.NET 8**
- **Why**: Latest LTS, performance, modern C# features
- **Tradeoff**: Requires newer SDK
- **Benefit**: Long-term support until 2026+

**xUnit vs MSTest**
- **Chose xUnit**: Modern, widely adopted, better assertions
- **Alternative**: MSTest works equally well
- **Benefit**: Community support, extensions

**File-based Logging**
- **Why**: Simple, no dependencies, persistent
- **Tradeoff**: Manual log management
- **Alternative**: Could use Serilog, NLog
- **Benefit**: Zero configuration, works out-of-box

### Implementation Choices

**ObservableCollection**
- **Why**: Built-in INotifyCollectionChanged for WPF binding
- **Tradeoff**: Not thread-safe
- **Benefit**: Auto UI updates on collection changes

**RelayCommand Implementation**
- **Why**: Lightweight, no external dependencies
- **Alternative**: Community toolkits (MVVM Light, CommunityToolkit.Mvvm)
- **Benefit**: Educational, transparent, customizable

**Reflection in ConfigurationManager**
- **Why**: Generic key-based access
- **Tradeoff**: Less type-safe
- **Alternative**: Strongly-typed configuration (recommended for production)
- **Benefit**: Flexible configuration system

**CSV Parsing (Custom)**
- **Why**: Educational, no dependencies
- **Alternative**: CsvHelper library (recommended for production)
- **Benefit**: Full control, transparent logic

## Extensibility Points

### Easy to Extend

1. **Data Model**: Add fields to `DataRecord`
2. **Validation Rules**: Modify `DataValidator.ValidateRecord()`
3. **Processing Logic**: Change `ProcessDataUseCase.TransformRecord()`
4. **UI**: Add buttons/views in XAML
5. **Configuration**: Add properties to `AppConfiguration`

### Requires More Work

1. **Database Support**: Implement new `IDataService` (SQL, EF Core)
2. **Multiple File Formats**: Add Excel, JSON importers
3. **Charts/Visualization**: Integrate charting library
4. **Multi-language**: Implement localization (resx files)
5. **Authentication**: Add user login system

### Architectural Extensions

1. **Dependency Injection Container**: Replace manual DI
2. **Unit of Work Pattern**: For transaction management
3. **CQRS**: Separate read/write models
4. **Event Sourcing**: Audit trail of changes
5. **Microservices**: Extract processing to separate service

## Not Included (Out of Scope)

- ❌ Network/cloud features
- ❌ Real-time collaboration
- ❌ Database integration
- ❌ Advanced reporting
- ❌ Data visualization/charts
- ❌ Undo/redo functionality
- ❌ Search/filtering UI
- ❌ Sorting/grouping UI
- ❌ Export to other formats (Excel, PDF)
- ❌ Scheduled processing
- ❌ Email notifications
- ❌ Plugin system
- ❌ Auto-updates

## Migration Path to Real Application

### Phase 1: Replace Scaffold Logic (1-2 days)
1. Update `DataRecord` with real domain model
2. Implement actual validation rules
3. Add real business logic to use cases
4. Test with real data files

### Phase 2: Enhance UI (2-3 days)
1. Add search/filter controls
2. Improve data grid (custom columns, formatting)
3. Add progress indicators
4. Implement settings dialog

### Phase 3: Add Database (3-5 days)
1. Add Entity Framework Core
2. Implement `IDataService` for SQL
3. Add migrations
4. Maintain CSV import/export

### Phase 4: Production Readiness (5-7 days)
1. Add comprehensive logging (Serilog)
2. Implement error reporting
3. Add telemetry/analytics
4. Installer/deployment (ClickOnce, MSI)
5. User documentation

## Known Limitations

1. **No pagination**: All records loaded in memory
2. **No background processing**: UI thread does work
3. **Simple validation**: Only basic rules
4. **No caching**: Re-loads data on each import
5. **Limited error recovery**: Some operations cannot be undone
6. **Single window**: No multi-window support
7. **No drag-drop**: Must use file dialogs
8. **Basic CSV parsing**: Doesn't handle all edge cases
9. **No data compression**: Large files consume memory
10. **Windows-only**: WPF not cross-platform

## Recommendations for Production Use

### Must Have
- ✅ Add DI container (Microsoft.Extensions.DependencyInjection)
- ✅ Use CsvHelper library for robust CSV parsing
- ✅ Implement proper exception handling strategy
- ✅ Add comprehensive logging (Serilog with sinks)
- ✅ Implement configuration validation on startup
- ✅ Add integration tests

### Should Have
- ✅ Implement background task processing
- ✅ Add progress reporting for long operations
- ✅ Implement undo/redo for data modifications
- ✅ Add data export to multiple formats
- ✅ Implement search and filtering
- ✅ Add telemetry and crash reporting

### Nice to Have
- ✅ Implement dark theme
- ✅ Add keyboard shortcuts
- ✅ Implement recent files list
- ✅ Add data visualization
- ✅ Localization support
- ✅ Plugin architecture

---

**Document Version**: 1.0  
**Last Updated**: January 2026  
**Maintained By**: Development Team
