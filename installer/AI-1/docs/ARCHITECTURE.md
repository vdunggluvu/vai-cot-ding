## Assumptions

- Chạy trên Windows 10+ với .NET 8 SDK (LTS) và Visual Studio 2022 (hoặc `dotnet` CLI).
- App mẫu xử lý dữ liệu dạng bảng đơn giản (CSV: `Id,Name,Value`) để minh họa luồng Import → Transform/Compute → Export.
- Không dùng DI container hay logging framework ngoài; chỉ dùng built-in + một logger ghi file đơn giản.
- Config và state được lưu ở thư mục output của app (cùng cấp với exe) để dễ quan sát khi scaffold.

## 1) Ứng dụng mẫu được “clone” (mô tả ngắn)

DesktopScaffold là một **desktop app dữ liệu** kiểu “tool nội bộ”: người dùng chọn file CSV, app đọc/validate, hiển thị dữ liệu, chạy một bước xử lý (transform/compute) và xuất ra CSV mới. App có startup/load config+state, ghi log ra file, và lưu state khi shutdown.

Scaffold này dùng làm **khung thay thế**: bạn có thể giữ nguyên cấu trúc (UI/UseCases/Domain/Infrastructure) và thay phần logic của use cases + model/IO để clone app thật.

## 2) Flow list (luồng hoạt động)

### Flow A — Startup / Initialize
- **Mục tiêu**: Load config, load state, set title/defaults, sẵn sàng UI.
- **Bắt đầu**: `App.OnStartup` → `Bootstrapper.BuildMainWindow`.
- **Kết thúc**: `MainViewModel.InitializeAsync` hoàn tất (UI hiển thị Ready).

### Flow B — Import CSV
- **Mục tiêu**: Người dùng chọn CSV → validate → parse → render lên grid → update recent files.
- **Bắt đầu**: `MainWindow` → `MainViewModel.ImportCommand`.
- **Kết thúc**: `MainViewModel` cập nhật `CurrentRows` + `RecentFiles`.

### Flow C — Transform/Compute
- **Mục tiêu**: Chạy transform (multiply factor, filter) và tính summary.
- **Bắt đầu**: `MainViewModel.TransformCommand`.
- **Kết thúc**: `CurrentRows` + `SummaryText` cập nhật.

### Flow D — Export CSV
- **Mục tiêu**: Validate output path → serialize data → ghi file.
- **Bắt đầu**: `MainViewModel.ExportCommand`.
- **Kết thúc**: File CSV output được tạo, UI log message.

### Flow E — Shutdown / Persist State
- **Mục tiêu**: Lưu `state.json` (recent files, last opened).
- **Bắt đầu**: `MainWindow.Closing` event.
- **Kết thúc**: `IStateStore.SaveAsync` best-effort xong.

## 3) Biểu đồ

### 3.1 Module graph (architecture text)

```
DesktopScaffold.App (WPF MVVM)
  ├─ View (XAML) + ViewModel (Commands/State)
  ├─ calls UseCases (Core)
  └─ wires Infrastructure implementations (no external DI)

DesktopScaffold.Core
  ├─ Domain (Models, Validation)
  ├─ Application (UseCases)
  └─ Abstractions (IFileSystem, IConfigStore, IStateStore, IAppLogger)

DesktopScaffold.Infrastructure
  ├─ PhysicalFileSystem : IFileSystem
  ├─ JsonConfigStore    : IConfigStore
  ├─ JsonStateStore     : IStateStore
  └─ FileAppLogger      : IAppLogger

DesktopScaffold.Tests
  └─ Unit tests for Core use cases (xUnit)
```

### 3.2 Sequence (ví dụ luồng Import → Transform → Export)

```
User            MainWindow      MainViewModel         UseCase(Core)          Infrastructure
 | click Import     |                |                      |                      |
 |----------------->|  command       |                      |                      |
 |                  |--------------->| ImportAsync()        |                      |
 |                  |                | validate input       |                      |
 |                  |                |--------------------->| ImportCsvUseCase     |
 |                  |                |                      | read/parse CSV       |
 |                  |                |                      |---------> IFileSystem(Read) -> PhysicalFileSystem
 |                  |                |                      |<-------- rows
 |                  |                |<---------------------| result + validation  |
 |                  |                | update grid + recent |                      |
 | click Transform  |                |                      |                      |
 |----------------->|--------------->| TransformAsync()     |--------------------->| TransformUseCase
 |                  |                |<---------------------| output + summary     |
 | click Export     |                |                      |                      |
 |----------------->|--------------->| ExportAsync()        |--------------------->| ExportCsvUseCase
 |                  |                |                      |---------> IFileSystem(Write) -> PhysicalFileSystem
 |                  |                |<---------------------| ok                   |
 |                  |                | update UI message    |                      |
```

## 4) Kiến trúc dự án

### 4.1 Cây thư mục

```
/
  DesktopScaffold.sln
  README.md
  docs/
    ARCHITECTURE.md
  src/
    DesktopScaffold.App/                (WPF UI)
    DesktopScaffold.Core/               (Domain + Application)
    DesktopScaffold.Infrastructure/      (Config/State/FS/Logging)
  tests/
    DesktopScaffold.Tests/              (xUnit)
```

### 4.2 Mapping “Flow → File/Class/Method”

- **Flow A (Startup)**:
  - `src/DesktopScaffold.App/App.xaml.cs` → `App.OnStartup`
  - `src/DesktopScaffold.App/Bootstrapper.cs` → `BuildMainWindow`
  - `src/DesktopScaffold.App/ViewModels/MainViewModel.cs` → `InitializeAsync`
  - `src/DesktopScaffold.Infrastructure/Config/JsonConfigStore.cs` → `LoadConfigAsync`
  - `src/DesktopScaffold.Infrastructure/State/JsonStateStore.cs` → `LoadAsync`

- **Flow B (Import)**:
  - `MainViewModel.ImportAsync` → `ImportCsvUseCase.ExecuteAsync`
  - `ImportCsvUseCase` → `IFileSystem.ReadAllTextAsync`

- **Flow C (Transform)**:
  - `MainViewModel.TransformAsync` → `TransformUseCase.ExecuteAsync`

- **Flow D (Export)**:
  - `MainViewModel.ExportAsync` → `ExportCsvUseCase.ExecuteAsync`
  - `ExportCsvUseCase` → `IFileSystem.WriteAllTextAsync`

- **Flow E (Shutdown)**:
  - `Bootstrapper` hook `Window.Closing` → `MainViewModel.OnClosingAsync` → `IStateStore.SaveAsync`

## 6) Build/Run (cụ thể)

1. Cài **.NET SDK 8.x** (LTS).
2. Cài **Visual Studio 2022** với workload “.NET desktop development” (WPF).
3. Mở `DesktopScaffold.sln`.
4. Set startup project: `DesktopScaffold.App`.
5. Build `Debug|Any CPU`.
6. Run.
7. Trong app: Browse chọn CSV → Import → Transform → Browse Output → Export.
8. Kiểm tra output folder: `appsettings.json`, `state.json`, `logs\app-YYYYMMDD.log`.

