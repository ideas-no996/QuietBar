# QuietBar

QuietBar is a low-distraction Windows desktop status bar for Windows 10 and Windows 11. It runs quietly in the background and shows a small transparent WPF window near the bottom-left taskbar area, without trying to inject itself into the real Windows taskbar.

The first version is intentionally simple: it displays CPU usage, memory usage, GPU usage, VRAM usage, and GPU temperature when available. If a GPU value cannot be read on the current machine or driver, QuietBar shows `N/A` instead of exiting or showing a warning.

## Features

- Borderless transparent always-on-top WPF window.
- Collapsed mode: low opacity and only CPU/GPU summary in the taskbar area.
- Hover mode: full opacity and horizontal CPU/RAM/GPU/VRAM/temperature details.
- Auto-collapses shortly after the mouse leaves.
- System tray icon with show/hide, reload config, startup toggle, and exit.
- Read-only hardware monitoring through `LibreHardwareMonitorLib`.

## Requirements

- Windows 10 or Windows 11.
- .NET 8 SDK to build from source.

## Build and Run

From this directory:

```powershell
dotnet restore
dotnet build
dotnet run --project .\QuietBar.csproj
```

For a portable release build that does not require the user to install .NET:

```powershell
dotnet publish .\QuietBar.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
```

Run the generated `QuietBar.exe` from the publish folder. The release build is self-contained so ordinary Windows users do not need to install the .NET Desktop Runtime separately.

## Configuration

QuietBar reads `config.json` from the application directory. The file is copied to the output folder during build.

```json
{
  "refreshIntervalMs": 1000,
  "collapsedOpacity": 0.55,
  "expandedOpacity": 1.0,
  "collapseDelayMs": 800,
  "fontSize": 12,
  "position": "bottom-left",
  "alwaysOnTop": true
}
```

Settings:

- `refreshIntervalMs`: hardware refresh interval in milliseconds. Minimum normalized value is 250.
- `collapsedOpacity`: opacity when semi-hidden.
- `expandedOpacity`: opacity when hovered.
- `collapseDelayMs`: delay before returning to semi-hidden mode after the mouse leaves.
- `fontSize`: status text size.
- `position`: currently supports `bottom-left`.
- `alwaysOnTop`: keeps QuietBar above normal windows.

Use the tray menu item `重新加载配置` to reload the file without restarting the app.

## Known Limitations

- GPU, VRAM, and temperature sensor availability depends on the GPU vendor, driver, and whether LibreHardwareMonitor can read the sensor on that machine.
- The first version only supports the primary screen bottom-left taskbar placement.
- No graphs, history, alerts, cleanup, optimization, or system modification features are included.
- Some hardware sensors may require elevated permissions on certain devices, but QuietBar is designed to degrade to `N/A`.

## Why Not Embed Into the Windows Taskbar

True taskbar embedding relies on private shell behavior, fragile window parenting tricks, or unsupported integration points that can break across Windows updates. QuietBar avoids that risk by using a normal borderless transparent top-level window placed near the taskbar. This keeps the app simpler, safer, easier to exit, and less likely to interfere with Explorer.
