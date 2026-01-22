# DESIGN DOCUMENT

## Mục Đích

Tài liệu thiết kế kiến trúc của **MagicKeyboardUtilities Reimplementation**, bao gồm:
- Lý do chọn công nghệ
- Sơ đồ luồng chạy (Mermaid diagrams)
- Kiến trúc component
- State machines

## Công Nghệ & Lý Do

### .NET 8 + WinForms

**Lý do chọn:**
- ✅ Native support cho NotifyIcon (tray icon)
- ✅ NativeWindow cho message-only windows (WM_DEVICECHANGE, WM_HOTKEY)
- ✅ P/Invoke user32.dll dễ dàng (hooks, hotkeys, SendInput)
- ✅ Không cần visible window → WinForms ApplicationContext phù hợp
- ✅ .NET logging framework built-in
- ✅ System.Text.Json cho config

**So với WPF:**
- WPF phức tạp hơn cho background app
- NotifyIcon trong WPF cần Forms compatibility
- WinForms đơn giản hơn cho app không có UI

### Architecture Pattern: Service-based

**Lý do:**
- Mỗi chức năng = 1 service (Hook, Hotkey, Device, Config, Tray)
- AppHost orchestrates lifecycle
- Dễ test (dependency injection friendly)
- Dễ toggle on/off từng feature

## Kiến Trúc Tổng Thể

### Layer Diagram

```mermaid
graph TB
    subgraph "Entry Layer"
        A[Program.cs]
    end
    
    subgraph "Orchestration Layer"
        B[AppHost]
    end
    
    subgraph "Service Layer"
        C1[TrayIconController]
        C2[KeyboardHookService]
        C3[HotkeyService]
        C4[DeviceMonitor]
        C5[ConfigStore]
    end
    
    subgraph "Core Layer"
        D1[RemappingEngine]
        D2[ActionDispatcher]
    end
    
    subgraph "Infrastructure Layer"
        E1[HiddenMessageWindow]
        E2[NativeMethods P/Invoke]
        E3[FileLogger]
    end
    
    subgraph "External"
        F1[config.json]
        F2[Windows API]
    end
    
    A --> B
    B --> C1
    B --> C2
    B --> C3
    B --> C4
    B --> C5
    
    C1 --> D2
    C2 --> D1
    C3 --> D2
    C4 --> E1
    C5 --> F1
    
    E1 --> C3
    E1 --> C4
    
    C2 --> E2
    C3 --> E2
    E2 --> F2
    
    style A fill:#e1f5ff
    style B fill:#fff4e1
    style C1 fill:#e8f5e9
    style C2 fill:#e8f5e9
    style C3 fill:#e8f5e9
    style C4 fill:#e8f5e9
    style C5 fill:#e8f5e9
    style D1 fill:#f3e5f5
    style D2 fill:#f3e5f5
    style E1 fill:#fce4ec
    style E2 fill:#fce4ec
    style E3 fill:#fce4ec
```

### Component Diagram

```mermaid
graph LR
    subgraph "MagicKeyboardUtilities.App"
        direction TB
        
        subgraph "Entry"
            P[Program.cs<br/>Single Instance<br/>Setup Logging]
        end
        
        subgraph "Host"
            H[AppHost<br/>Lifecycle Manager]
        end
        
        subgraph "Tray"
            T[TrayIconController<br/>NotifyIcon<br/>Context Menu]
        end
        
        subgraph "Input"
            KH[KeyboardHookService<br/>WH_KEYBOARD_LL<br/>Hook Callback]
            HK[HotkeyService<br/>RegisterHotKey<br/>WM_HOTKEY Handler]
        end
        
        subgraph "Core"
            RE[RemappingEngine<br/>VK Mapping Table]
            AD[ActionDispatcher<br/>Command Executor]
        end
        
        subgraph "Device"
            DM[DeviceMonitor<br/>WM_DEVICECHANGE<br/>Apple VID Check]
        end
        
        subgraph "Config"
            CS[ConfigStore<br/>JSON Load/Save]
            CF[AppConfig<br/>Data Model]
        end
        
        subgraph "Messaging"
            MW[HiddenMessageWindow<br/>NativeWindow<br/>WndProc]
        end
        
        P --> H
        H --> T
        H --> KH
        H --> HK
        H --> DM
        H --> CS
        
        T --> AD
        KH --> RE
        HK --> AD
        
        MW --> HK
        MW --> DM
        
        CS --> CF
    end
    
    subgraph "External"
        JSON[config.json]
        WIN[Windows API<br/>user32.dll<br/>kernel32.dll]
    end
    
    CS --> JSON
    KH --> WIN
    HK --> WIN
    MW --> WIN
    
    style P fill:#bbdefb
    style H fill:#fff9c4
    style T fill:#c8e6c9
    style KH fill:#c8e6c9
    style HK fill:#c8e6c9
    style DM fill:#c8e6c9
    style CS fill:#c8e6c9
    style RE fill:#f8bbd0
    style AD fill:#f8bbd0
    style MW fill:#d1c4e9
    style CF fill:#ffccbc
```

## Luồng Chạy Chi Tiết

### 1. Overall Flow (Startup → Running → Shutdown)

```mermaid
flowchart TD
    START([Windows Starts Process]) --> MUTEX{Single Instance<br/>Mutex Check}
    MUTEX -->|Already Running| SHOW_MSG[Show Already Running]
    SHOW_MSG --> END1([Exit])
    
    MUTEX -->|First Instance| INIT_LOG[Initialize Logging]
    INIT_LOG --> LOAD_CFG[Load config.json]
    LOAD_CFG --> CREATE_SVC[Create Services]
    
    CREATE_SVC --> HOST_START[AppHost.Start]
    HOST_START --> LOAD_MAP[Load Remapping Table]
    LOAD_MAP --> CREATE_WIN[Create Hidden Message Window]
    CREATE_WIN --> CREATE_TRAY[Create Tray Icon]
    CREATE_TRAY --> APPLY_CFG{Config:<br/>enableHooks?}
    
    APPLY_CFG -->|Yes| INSTALL_HOOK[Install Keyboard Hook]
    APPLY_CFG -->|No| SKIP_HOOK[Skip Hook Install]
    INSTALL_HOOK --> CHECK_HK{Config:<br/>enableHotkeys?}
    SKIP_HOOK --> CHECK_HK
    
    CHECK_HK -->|Yes| REG_HK[Register Hotkeys]
    CHECK_HK -->|No| SKIP_HK[Skip Hotkeys]
    REG_HK --> RUN_LOOP
    SKIP_HK --> RUN_LOOP
    
    RUN_LOOP[Enter Message Loop<br/>Application.Run] --> WAIT{Wait for Event}
    
    WAIT -->|Key Event| HOOK_CB[Hook Callback]
    WAIT -->|Hotkey| HK_CB[WM_HOTKEY Handler]
    WAIT -->|Device Change| DEV_CB[WM_DEVICECHANGE]
    WAIT -->|Tray Click| TRAY_CB[Tray Menu Handler]
    WAIT -->|Exit Command| EXIT
    
    HOOK_CB --> REMAP[Check Remapping]
    REMAP -->|Match| SEND[SendInput New Key]
    REMAP -->|No Match| PASS[Pass Through]
    SEND --> WAIT
    PASS --> WAIT
    
    HK_CB --> DISPATCH[Action Dispatcher]
    DISPATCH --> WAIT
    
    DEV_CB --> LOG_DEV[Log Device Event]
    LOG_DEV --> WAIT
    
    TRAY_CB --> MENU{Menu Item}
    MENU -->|Enable| ENABLE[Enable Features]
    MENU -->|Disable| DISABLE[Disable Features]
    MENU -->|Settings| OPEN_CFG[Open config.json]
    MENU -->|Exit| EXIT
    ENABLE --> WAIT
    DISABLE --> WAIT
    OPEN_CFG --> WAIT
    
    EXIT[Exit Command] --> UNHOOK[Uninstall Hooks]
    UNHOOK --> UNREG_HK[Unregister Hotkeys]
    UNREG_HK --> SAVE{AutoSave?}
    SAVE -->|Yes| SAVE_CFG[Save config.json]
    SAVE -->|No| SKIP_SAVE
    SAVE_CFG --> REMOVE_TRAY
    SKIP_SAVE[Skip Save] --> REMOVE_TRAY
    REMOVE_TRAY[Remove Tray Icon] --> DESTROY_WIN[Destroy Message Window]
    DESTROY_WIN --> END2([Exit Process])
    
    style START fill:#4caf50
    style END1 fill:#f44336
    style END2 fill:#f44336
    style RUN_LOOP fill:#2196f3
    style HOOK_CB fill:#ff9800
    style HK_CB fill:#ff9800
    style DEV_CB fill:#ff9800
    style TRAY_CB fill:#ff9800
```

### 2. Hook Callback Sequence

```mermaid
sequenceDiagram
    participant User
    participant Windows
    participant Hook as KeyboardHookService
    participant Engine as RemappingEngine
    participant WinAPI as Windows API

    User->>Windows: Press Key (F13)
    Windows->>Hook: LowLevelKeyboardProc(nCode, wParam, lParam)
    
    Hook->>Hook: Check nCode >= 0?
    alt nCode < 0
        Hook->>WinAPI: CallNextHookEx()
        WinAPI-->>Windows: Pass Through
    else nCode >= 0
        Hook->>Hook: Check IsEnabled?
        alt Not Enabled
            Hook->>WinAPI: CallNextHookEx()
            WinAPI-->>Windows: Pass Through
        else Enabled
            Hook->>Hook: Parse KBDLLHOOKSTRUCT
            Hook->>Hook: Extract vkCode = 124 (F13)
            Hook->>Engine: TryRemap(124)
            Engine->>Engine: Lookup in mapping table
            
            alt Mapping Found
                Engine-->>Hook: remappedVk = 175 (Volume Up)
                Hook->>Hook: Check SendInputEnabled?
                alt SendInput Enabled
                    Hook->>WinAPI: SendInput(VK_VOLUME_UP down)
                    Hook->>WinAPI: SendInput(VK_VOLUME_UP up)
                    WinAPI-->>Windows: Inject New Key
                    Windows-->>User: Volume Up Action
                else SendInput Disabled
                    Hook->>Hook: Log only (safe mode)
                end
                Hook-->>Windows: Return 1 (block original)
            else No Mapping
                Hook->>WinAPI: CallNextHookEx()
                WinAPI-->>Windows: Pass Through
                Windows-->>User: Original Key (F13)
            end
        end
    end
```

### 3. State Machine

```mermaid
stateDiagram-v2
    [*] --> NotStarted
    
    NotStarted --> Initializing: Program.Main()
    Initializing --> ConfigLoading: Create Services
    ConfigLoading --> ServicesCreated: ConfigStore.Load()
    
    ServicesCreated --> Disabled: AppHost.Start()<br/>(Hooks OFF by default)
    
    Disabled --> Enabled: User clicks "Enable"<br/>or<br/>Config enableHooks=true
    Enabled --> Disabled: User clicks "Disable"
    
    Enabled --> HooksInstalled: Install Hooks
    HooksInstalled --> HooksActive: Hook Callback Running
    HooksActive --> HooksInstalled: Process Events
    HooksInstalled --> Disabled: Uninstall Hooks
    
    Disabled --> ShuttingDown: User clicks "Exit"
    Enabled --> ShuttingDown: User clicks "Exit"
    HooksActive --> ShuttingDown: User clicks "Exit"
    
    ShuttingDown --> Cleanup: Unhook, Unregister
    Cleanup --> ConfigSaving: AutoSave check
    ConfigSaving --> [*]: Process.Exit()
    
    note right of Disabled
        - Tray icon visible
        - No hooks installed
        - Hotkeys may be active
    end note
    
    note right of Enabled
        - Hooks installed
        - Remapping active
        - SendInput depends on config
    end note
    
    note right of ShuttingDown
        - Graceful cleanup
        - Save config if AutoSave
        - Remove tray icon
    end note
```

### 4. Configuration Flow

```mermaid
flowchart LR
    START([App Start]) --> CHECK{config.json<br/>exists?}
    
    CHECK -->|No| DEFAULT[Create Default Config]
    CHECK -->|Yes| LOAD[Load from File]
    
    DEFAULT --> PARSE
    LOAD --> PARSE[Parse JSON]
    
    PARSE -->|Success| APPLY[Apply to Services]
    PARSE -->|Error| DEFAULT
    
    APPLY --> REMAP[Load Remapping Table]
    APPLY --> FEAT[Apply Feature Flags]
    APPLY --> DEV[Configure Device Monitor]
    
    REMAP --> RUNNING[App Running]
    FEAT --> RUNNING
    DEV --> RUNNING
    
    RUNNING --> CHANGE{User Changes<br/>Settings}
    CHANGE -->|Via Tray Menu| ACTION[Execute Action]
    CHANGE -->|Edit JSON| RELOAD[Restart App]
    
    ACTION --> DIRTY[Mark Config Dirty]
    RELOAD --> CHECK
    
    RUNNING --> EXIT{Exit Signal}
    DIRTY --> EXIT
    
    EXIT -->|AutoSave=true| SAVE[Save config.json]
    EXIT -->|AutoSave=false| SKIP
    
    SAVE --> END([Exit])
    SKIP[Skip Save] --> END
    
    style DEFAULT fill:#fff9c4
    style LOAD fill:#c8e6c9
    style APPLY fill:#bbdefb
    style SAVE fill:#f8bbd0
```

## Design Decisions

### 1. Tại Sao Dùng WinForms ApplicationContext?

**Lý do:**
- App chạy nền, không cần visible window
- ApplicationContext.Run() vẫn có message loop
- NotifyIcon tự động hoạt động trong context
- Không cần Form.ShowInTaskbar = false tricks

**Code:**
```csharp
Application.Run(); // Runs message loop without form
```

**Alternative (không chọn):**
- Form với Visible=false → Phức tạp, không cần UI
- Console app với manual message loop → Mất WinForms benefits

### 2. Tại Sao Dùng NativeWindow Thay Vì Form?

**Lý do:**
- Cần nhận WM_DEVICECHANGE và WM_HOTKEY
- Form không cần thiết (no UI)
- NativeWindow nhẹ hơn, chỉ có WndProc
- Message-only window (HWND_MESSAGE parent)

**Code:**
```csharp
CreateHandle(new CreateParams
{
    Parent = new IntPtr(-3) // HWND_MESSAGE
});
```

### 3. Tại Sao Config JSON External Thay Vì Embedded?

**Lý do:**
- ✅ Transparency: User dễ xem/sửa
- ✅ No recompile: Thay đổi config không cần rebuild
- ✅ Debugging: Dễ test các scenarios
- ❌ Original binary: Config embedded/encrypted (không thể reverse)

**Trade-off:**
- Mất obfuscation → Chấp nhận vì mục đích học tập

### 4. Tại Sao Default OFF Cho Hooks?

**Lý do:**
- ⚠️ Safety: Hooks có thể block input nếu config sai
- ⚠️ Evidence: Flow report chỉ INFER hooks, không confirm
- ✅ Testing: User phải enable explicitly sau khi review config

**Migration path:**
1. User review remapping table
2. Enable hooks với SendInput=false (log only)
3. Test mapping logic
4. Enable SendInput=true (live)

### 5. Tại Sao Không Implement Full Device Detection?

**Lý do:**
- ❌ Evidence: Flow report không có chi tiết SetupAPI calls
- ❌ Complexity: SetupDiEnumDeviceInfo, device path parsing phức tạp
- ✅ Stub đủ: Log WM_DEVICECHANGE events
- ⚠️ Future: Có thể hoàn thiện nếu có thêm evidence

**Current implementation:**
- Nhận WM_DEVICECHANGE
- Log device arrival/removal
- Không parse device path → Không check VID/PID

## P/Invoke Signatures

### Critical Win32 APIs

```csharp
// Hook
SetWindowsHookEx(WH_KEYBOARD_LL, callback, hModule, 0)
UnhookWindowsHookEx(hhk)
CallNextHookEx(hhk, nCode, wParam, lParam)

// Hotkey
RegisterHotKey(hWnd, id, modifiers, vk)
UnregisterHotKey(hWnd, id)

// Input
SendInput(nInputs, pInputs, cbSize)

// Message Loop
GetMessage(out MSG, hWnd, min, max)
TranslateMessage(ref MSG)
DispatchMessage(ref MSG)
```

### Safety Considerations

**GC Protection:**
```csharp
// Keep delegate reference to prevent GC
private NativeMethods.LowLevelKeyboardProc? _hookProc;
_hookProc = HookCallback; // Store before SetWindowsHookEx
```

**Exception Handling:**
```csharp
try {
    // Hook callback logic
} catch (Exception ex) {
    _logger.LogError(ex, "Hook error");
    // ALWAYS call CallNextHookEx on exception
    return CallNextHookEx(...);
}
```

## Testing Strategy

### Unit Tests
- ✅ ConfigStore load/save
- ✅ RemappingEngine lookup
- ✅ ActionDispatcher execute

### Manual Tests (Required)
- ⚠️ Hook installation (requires admin on some systems)
- ⚠️ Remapping (safe mode → live mode)
- ⚠️ Hotkey registration
- ⚠️ Device plug/unplug

### No Integration Tests
- ❌ Hooks require real keyboard input (hard to automate)
- ❌ Tray icon requires interactive UI

## Performance Considerations

### Hook Callback
- **Critical**: Must return fast (< 1ms)
- **Avoid**: Heavy processing, I/O, logging in production
- **Current**: Log only in Debug build

### Remapping Lookup
- **Data structure**: Dictionary<int, int> → O(1) lookup
- **Alternative**: Array[256] for VK codes → Faster but wastes memory

### Memory Footprint
- **Target**: < 50 MB (original ~105 MB)
- **Actual**: ~20 MB (unpacked .NET)

## Future Improvements

### If More Evidence Available

1. **Full Device Detection**
   - Implement SetupDiGetClassDevs
   - Parse device interface path
   - Extract VID/PID from path string
   - See Section 8.3 in flow report

2. **Custom Icon**
   - Extract MAINICON from original binary
   - Add to project resources
   - Update NotifyIcon.Icon

3. **Settings UI**
   - WinForms dialog for config editing
   - No need to open JSON manually
   - Validation for VK codes

4. **Installer**
   - MSI package
   - Optional: Auto-start registry entry
   - Digital signature

---

**Version:** 1.0.0  
**Last Updated:** 22/01/2026  
**Traceability:** Flow Report Section 3 (Bản Đồ Luồng), Section 8 (Implementation Checklist)
