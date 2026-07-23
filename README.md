# Wireless Scrcpy

Wireless Scrcpy is a Windows desktop helper that starts a `scrcpy` session over Android Debug Bridge (ADB) wireless debugging. It discovers the required command-line tools, prepares a single connected USB Android device for TCP/IP mode, discovers the phone IP address, prompts the user to unplug USB, connects to the device wirelessly, and launches `scrcpy` with audio disabled.

## Version

Release target: **1.0**

## Features

- WinForms status window with workflow status, device identity, IP address, details, and user actions.
- Connect, disconnect, exit, minimize-to-tray, and restore-from-tray support.
- System tray icon with status-aware Connect and Disconnect actions.
- Windows tray balloon notifications for running, reconnecting, restored, and error states.
- ADB and scrcpy tool discovery from saved settings, application-local paths, and `PATH`.
- Automatic ADB server startup, USB device validation, TCP/IP enablement, phone IP discovery, wireless ADB connection, and scrcpy launch.
- Reconnect supervision when the wireless connection is interrupted.
- JSON settings stored under `%LOCALAPPDATA%\WirelessScrcpy`.
- Session logs with retention stored under `%LOCALAPPDATA%\WirelessScrcpy\Logs`.

## Requirements

- Windows 10 or later.
- .NET 8 Desktop Runtime to run published framework-dependent builds, or the .NET 8 SDK to build from source.
- `adb.exe` from Android Platform Tools.
- `scrcpy.exe`.
- One authorized Android device connected over USB for session setup.
- The Windows PC and Android device on the same network after USB is disconnected.

## Usage

1. Install Android Platform Tools and scrcpy, or place `adb.exe` and `scrcpy.exe` next to the application executable.
2. Enable USB debugging on the Android device and authorize the PC.
3. Connect exactly one Android device over USB.
4. Start Wireless Scrcpy.
5. Click **Connect**.
6. When prompted, disconnect the USB cable and click **OK**.
7. Use **Disconnect** to stop the active workflow/session, or **Exit** to close the application.

Closing or minimizing the main window keeps the application available in the system tray. Double-click the tray icon or choose **Open Wireless Scrcpy** from the tray menu to restore it.

## Build

From a Windows machine with the .NET 8 SDK installed:

```powershell
dotnet restore WirelessScrcpy.sln
dotnet build WirelessScrcpy.sln -c Release
```

Publish the WinForms application:

```powershell
dotnet publish src/WirelessScrcpy.App/WirelessScrcpy.App.csproj -c Release -r win-x64 --self-contained false
```

## Project structure

```text
src/WirelessScrcpy.App   WinForms host, dependency injection, status window, tray UI, notifications
src/WirelessScrcpy.Core  Workflow, ADB, device discovery, scrcpy launch, settings, logging, diagnostics
docs/architecture.md     Architecture and workflow notes
```

## Runtime data

Wireless Scrcpy writes runtime files under the current user's local application data folder:

- Settings: `%LOCALAPPDATA%\WirelessScrcpy\settings.json`
- Logs: `%LOCALAPPDATA%\WirelessScrcpy\Logs\session-*.log`

Logs are retained for 14 days.

## Release verification checklist

Before shipping Version 1.0, verify the following on Windows with the .NET 8 SDK and the required Android tooling installed:

- `dotnet restore WirelessScrcpy.sln`
- `dotnet build WirelessScrcpy.sln -c Release`
- Launch the application and verify the status window and tray icon.
- Connect an authorized Android device over USB and complete the wireless workflow.
- Verify reconnect behavior by temporarily interrupting network connectivity.
- Verify Disconnect, Exit, close-to-tray, minimize-to-tray, and restore-from-tray behavior.
- Verify settings and logs are written to `%LOCALAPPDATA%\WirelessScrcpy`.

## License

No license file is currently included. Add one before distributing binaries outside the owning organization.
