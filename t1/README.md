## DesktopScaffold (WPF MVVM .NET 8)

Mục tiêu repo này là một **scaffold Desktop App mẫu** có cấu trúc giống ứng dụng thực tế: startup/load config + state, UI thao tác người dùng, chạy use cases, logging, export dữ liệu, và shutdown có persist state.

Tài liệu kiến trúc/flow/mapping: xem `docs/ARCHITECTURE.md`.

### Quick start

Yêu cầu:
- Windows 10+ (WPF)
- .NET SDK 8.x (LTS)
- Visual Studio 2022 (khuyến nghị) hoặc `dotnet` CLI

Chạy:
1. Mở `DesktopScaffold.sln` bằng Visual Studio 2022 và Run project `DesktopScaffold.App`
2. Hoặc dùng CLI:
   - `dotnet restore`
   - `dotnet build -c Debug`
   - `dotnet test -c Debug`
   - `dotnet run --project .\src\DesktopScaffold.App\DesktopScaffold.App.csproj`

File cấu hình và log sẽ nằm trong thư mục output của app:
- `appsettings.json`
- `state.json`
- `logs\app-YYYYMMDD.log`

