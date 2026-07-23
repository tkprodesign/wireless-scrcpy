# Wireless Scrcpy 1.0 Architecture

## 1. Scope and product constraints

Wireless Scrcpy 1.0 is a lightweight Windows 10+ WinForms application for automating the complete wireless scrcpy setup and launch workflow. Version 1.0 is feature-frozen and contains only the following capabilities:

- One-click launch.
- Automatic ADB detection.
- Automatic scrcpy detection.
- Automatic USB device detection.
- Automatic TCP/IP enablement on port 5555.
- Automatic phone IP discovery.
- Prompt the user to disconnect USB.
- Automatic wireless ADB connection.
- Launch scrcpy using `--no-audio`.
- Status window.
- System tray icon.
- Windows notifications.
- Persistent settings.
- Session logging.
- Automatic reconnection after brief network interruption.
- Clean shutdown.
- Graceful error handling.

No additional product features are included in the 1.0 architecture.

## 2. Architectural principles

- **Specification-first:** implementation begins only after this architecture is reviewed and approved.
- **Single responsibility:** each class owns one reason to change.
- **Deterministic workflow:** all launch steps are represented by an explicit state machine.
- **Small surface area:** prefer .NET 8 and WinForms platform APIs over external packages.
- **Asynchronous process execution:** ADB and scrcpy interactions must not block the UI thread.
- **Dependency injection:** services are composed centrally and depend on abstractions where it materially improves testability and separation.
- **Low resource usage:** no busy polling; timers use conservative intervals and are active only when required.
- **Clean shutdown:** every process, stream, timer, notify icon, cancellation token, and file handle is disposed deterministically.
- **Graceful failures:** errors are surfaced through controlled status messages, notifications, and session logs.

## 3. Proposed solution layout

```text
wireless-scrcpy/
  WirelessScrcpy.sln
  src/
    WirelessScrcpy.App/
      WirelessScrcpy.App.csproj
      Program.cs
      Composition/
      UI/
      app.manifest
    WirelessScrcpy.Core/
      WirelessScrcpy.Core.csproj
      Abstractions/
      ADB/
      Scrcpy/
      Devices/
      Workflow/
      Settings/
      Logging/
      Notifications/
      Diagnostics/
      Common/
```

### Project responsibilities

- `WirelessScrcpy.App`: WinForms entry point, UI forms, tray icon integration, tray notification adapter, application composition, and application lifetime wiring.
- `WirelessScrcpy.Core`: platform-neutral workflow orchestration, process abstractions, tool discovery, device parsing, settings contracts, logging contracts, and domain models.

## 4. Namespaces

- `WirelessScrcpy.App`
- `WirelessScrcpy.App.Composition`
- `WirelessScrcpy.App.UI`
- `WirelessScrcpy.App.UI.Tray`
- `WirelessScrcpy.App.UI.Notifications`
- `WirelessScrcpy.Core`
- `WirelessScrcpy.Core.Abstractions`
- `WirelessScrcpy.Core.Adb`
- `WirelessScrcpy.Core.Scrcpy`
- `WirelessScrcpy.Core.Devices`
- `WirelessScrcpy.Core.Workflow`
- `WirelessScrcpy.Core.Settings`
- `WirelessScrcpy.Core.Logging`
- `WirelessScrcpy.Core.Notifications`
- `WirelessScrcpy.Core.Diagnostics`
- `WirelessScrcpy.Core.Common`

## 5. Class inventory and responsibilities

### 5.1 Application composition and lifetime

| Class | Namespace | Responsibility |
| --- | --- | --- |
| `Program` | `WirelessScrcpy.App` | Configures WinForms high DPI settings, builds the application host, starts the application context, and ensures final disposal. |
| `ApplicationBootstrapper` | `WirelessScrcpy.App.Composition` | Creates the dependency graph for the application using built-in dependency injection patterns. |
| `ApplicationLifetime` | `WirelessScrcpy.App.Composition` | Coordinates startup and shutdown events between WinForms, tray UI, workflow services, and cancellation tokens. |
| `WindowsFormsSynchronizationContextDispatcher` | `WirelessScrcpy.App.UI` | Marshals callbacks from background services onto the WinForms UI thread. |
| `WirelessScrcpyApplicationContext` | `WirelessScrcpy.App.UI` | Owns the tray icon and status window lifecycle without forcing the main window to remain visible. |

### 5.2 User interface

| Class | Namespace | Responsibility |
| --- | --- | --- |
| `StatusForm` | `WirelessScrcpy.App.UI` | Displays current workflow state, current detail message, and launch/shutdown controls. |
| `StatusViewModel` | `WirelessScrcpy.App.UI` | Converts workflow state updates into simple bindable UI text, enabled states, and severity indicators. |
| `TrayIconController` | `WirelessScrcpy.App.UI.Tray` | Owns `NotifyIcon`, context menu items, show/hide behavior, and tray disposal. |
| `TrayMenuFactory` | `WirelessScrcpy.App.UI.Tray` | Builds the fixed tray menu for opening the window, connecting, disconnecting, and exiting. |
| `UsbDisconnectPrompt` | `WirelessScrcpy.App.UI` | Presents the blocking user prompt to disconnect USB at the correct workflow state. |

### 5.3 Notifications

| Class | Namespace | Responsibility |
| --- | --- | --- |
| `IUserNotifier` | `WirelessScrcpy.Core.Notifications` | Abstraction for user-facing notifications. |
| `TrayBalloonNotifier` | `WirelessScrcpy.App.UI.Notifications` | Notification implementation using Windows tray balloon tips. |
| `NotificationMessage` | `WirelessScrcpy.Core.Notifications` | Immutable notification title, body, and severity model. |

### 5.4 Workflow orchestration

| Class | Namespace | Responsibility |
| --- | --- | --- |
| `WirelessScrcpyController` | `WirelessScrcpy.Core.Workflow` | Public service used by the UI to start, observe, and stop a session. |
| `WirelessScrcpyWorkflow` | `WirelessScrcpy.Core.Workflow` | Executes the ordered one-click launch workflow using dependencies and the state machine. |
| `WorkflowStateMachine` | `WirelessScrcpy.Core.Workflow` | Validates legal state transitions and emits immutable workflow snapshots. |
| `WorkflowState` | `WirelessScrcpy.Core.Workflow` | Enumeration of all lifecycle states. |
| `WorkflowEvent` | `WirelessScrcpy.Core.Workflow` | Enumeration of events that trigger state transitions. |
| `WorkflowSnapshot` | `WirelessScrcpy.Core.Workflow` | Immutable current state, detail message, timestamp, device identity, and severity. |
| `WorkflowOptions` | `WirelessScrcpy.Core.Workflow` | Immutable runtime constants such as TCP port, reconnect delay, and process timeouts. |
| `ReconnectSupervisor` | `WirelessScrcpy.Core.Workflow` | Handles brief wireless interruptions after scrcpy launch and attempts deterministic reconnection. |
| `SessionHandle` | `WirelessScrcpy.Core.Workflow` | Owns the active cancellation scope and disposable runtime resources for one session. |

### 5.5 ADB integration

| Class | Namespace | Responsibility |
| --- | --- | --- |
| `AdbClient` | `WirelessScrcpy.Core.Adb` | High-level asynchronous API for required ADB operations. |
| `AdbCommandBuilder` | `WirelessScrcpy.Core.Adb` | Builds argument lists for supported ADB commands without string duplication. |
| `AdbOutputParser` | `WirelessScrcpy.Core.Adb` | Parses `adb devices`, `ip route`, and command output into typed results. |
| `AdbDeviceSelector` | `WirelessScrcpy.Core.Adb` | Selects exactly one eligible USB device or returns a controlled error when none or multiple exist. |
| `AdbTcpIpService` | `WirelessScrcpy.Core.Adb` | Enables TCP/IP mode on port 5555 for the selected USB device. |
| `AdbWirelessConnector` | `WirelessScrcpy.Core.Adb` | Connects to the discovered device IP and verifies the wireless ADB target. |
| `AdbServerManager` | `WirelessScrcpy.Core.Adb` | Starts or restarts the ADB server only when required by the workflow. |
| `AdbCommandResult` | `WirelessScrcpy.Core.Adb` | Immutable exit code, standard output, standard error, and duration. |

### 5.6 scrcpy integration

| Class | Namespace | Responsibility |
| --- | --- | --- |
| `ScrcpyLauncher` | `WirelessScrcpy.Core.Scrcpy` | Starts scrcpy with the selected wireless target and the required `--no-audio` argument. |
| `ScrcpyCommandBuilder` | `WirelessScrcpy.Core.Scrcpy` | Builds the exact scrcpy argument list for version 1.0. |
| `ScrcpySession` | `WirelessScrcpy.Core.Scrcpy` | Owns the running scrcpy process and exposes asynchronous stop and disposal. |

### 5.7 Tool detection

| Class | Namespace | Responsibility |
| --- | --- | --- |
| `ToolDiscoveryService` | `WirelessScrcpy.Core.Diagnostics` | Finds `adb.exe` and `scrcpy.exe` using settings, application-local paths, PATH lookup, and sensible defaults. |
| `ExecutablePathValidator` | `WirelessScrcpy.Core.Diagnostics` | Validates that a discovered path exists, is a file, and has the expected executable name. |
| `EnvironmentPathScanner` | `WirelessScrcpy.Core.Diagnostics` | Searches PATH entries without executing unknown files. |
| `ToolLocation` | `WirelessScrcpy.Core.Diagnostics` | Immutable executable path and discovery source. |
| `ToolDiscoveryResult` | `WirelessScrcpy.Core.Diagnostics` | Immutable success or failure result for required tools. |

### 5.8 Device models and discovery

| Class | Namespace | Responsibility |
| --- | --- | --- |
| `AndroidDevice` | `WirelessScrcpy.Core.Devices` | Immutable ADB serial, transport type, state, and optional IP address. |
| `DeviceConnectionType` | `WirelessScrcpy.Core.Devices` | Enumeration for USB and wireless device transports. |
| `DeviceState` | `WirelessScrcpy.Core.Devices` | Enumeration for authorized, unauthorized, offline, and unknown ADB states. |
| `PhoneIpDiscoveryService` | `WirelessScrcpy.Core.Devices` | Discovers the phone IP address through ADB shell network commands. |
| `IpAddressCandidateSelector` | `WirelessScrcpy.Core.Devices` | Selects the most appropriate private IPv4 address from parsed shell output. |
| `NetworkEndpoint` | `WirelessScrcpy.Core.Devices` | Immutable IP address and port value object. |

### 5.9 Process execution

| Class | Namespace | Responsibility |
| --- | --- | --- |
| `IProcessRunner` | `WirelessScrcpy.Core.Abstractions` | Abstraction for starting processes and collecting asynchronous output. |
| `ProcessRunner` | `WirelessScrcpy.Core.Diagnostics` | Production implementation based on `System.Diagnostics.Process`. |
| `ProcessStartRequest` | `WirelessScrcpy.Core.Diagnostics` | Immutable executable path, arguments, working directory, timeout, and environment options. |
| `ProcessRunResult` | `WirelessScrcpy.Core.Diagnostics` | Immutable process result including output, error output, exit code, and timeout status. |
| `ProcessHandle` | `WirelessScrcpy.Core.Diagnostics` | Disposable wrapper for long-running processes such as scrcpy. |

### 5.10 Settings

| Class | Namespace | Responsibility |
| --- | --- | --- |
| `ISettingsStore` | `WirelessScrcpy.Core.Settings` | Abstraction for loading and saving persistent application settings. |
| `JsonSettingsStore` | `WirelessScrcpy.Core.Settings` | Stores settings as JSON under the user application data folder. |
| `ApplicationSettings` | `WirelessScrcpy.Core.Settings` | Immutable persisted settings such as last known tool paths and window visibility preference. |
| `SettingsFileLocator` | `WirelessScrcpy.Core.Settings` | Resolves the per-user settings file path. |
| `SettingsSerializer` | `WirelessScrcpy.Core.Settings` | Serializes and deserializes settings using `System.Text.Json`. |

### 5.11 Logging

| Class | Namespace | Responsibility |
| --- | --- | --- |
| `ISessionLogger` | `WirelessScrcpy.Core.Logging` | Abstraction for writing structured session events. |
| `FileSessionLogger` | `WirelessScrcpy.Core.Logging` | Writes append-only session logs under the user application data folder. |
| `LogFileLocator` | `WirelessScrcpy.Core.Logging` | Resolves the active session log file location. |
| `LogRetentionPolicy` | `WirelessScrcpy.Core.Logging` | Applies conservative cleanup to old session logs at startup. |

### 5.12 Common utilities

| Class | Namespace | Responsibility |
| --- | --- | --- |
| `Result<T>` | `WirelessScrcpy.Core.Common` | Represents success or controlled failure without using exceptions for expected workflow outcomes. |
| `AppError` | `WirelessScrcpy.Core.Common` | Immutable error code, user-safe message, diagnostic message, and severity. |
| `ErrorCode` | `WirelessScrcpy.Core.Common` | Enumeration of expected failures such as missing ADB, missing scrcpy, no USB device, unauthorized device, IP discovery failure, connection failure, and scrcpy launch failure. |
| `SystemClock` | `WirelessScrcpy.Core.Common` | Provides current time for logging and timeout calculations. |
| `IClock` | `WirelessScrcpy.Core.Abstractions` | Clock abstraction for deterministic tests. |

## 6. Application lifecycle

1. `Program` initializes WinForms configuration.
2. `ApplicationBootstrapper` creates settings, logging, process, detection, workflow, notification, tray, and UI services.
3. `JsonSettingsStore` loads settings or returns defaults when no settings file exists.
4. `LogRetentionPolicy` cleans old logs within a small bounded startup budget.
5. `WirelessScrcpyApplicationContext` creates `TrayIconController` and `StatusForm`.
6. The status window starts in an idle state and exposes the one-click launch action.
7. On launch, `WirelessScrcpyController` creates a `SessionHandle` and starts `WirelessScrcpyWorkflow` asynchronously.
8. The workflow emits `WorkflowSnapshot` updates to the UI, tray, notifications, and session log.
9. When scrcpy is running, `ReconnectSupervisor` watches brief interruptions without consuming CPU while idle.
10. On user exit or Windows shutdown, `ApplicationLifetime` cancels the active session, stops scrcpy if running, saves settings, flushes logs, disposes tray resources, and exits WinForms.

## 7. State machine

### States

- `Idle`
- `Starting`
- `DetectingAdb`
- `DetectingScrcpy`
- `StartingAdbServer`
- `DetectingUsbDevice`
- `EnablingTcpIp`
- `DiscoveringPhoneIp`
- `PromptingUsbDisconnect`
- `ConnectingWirelessAdb`
- `LaunchingScrcpy`
- `Running`
- `Reconnecting`
- `Stopping`
- `Completed`
- `Failed`

### Primary transitions

| From | Event | To |
| --- | --- | --- |
| `Idle` | `LaunchRequested` | `Starting` |
| `Starting` | `WorkflowInitialized` | `DetectingAdb` |
| `DetectingAdb` | `AdbFound` | `DetectingScrcpy` |
| `DetectingAdb` | `AdbMissing` | `Failed` |
| `DetectingScrcpy` | `ScrcpyFound` | `StartingAdbServer` |
| `DetectingScrcpy` | `ScrcpyMissing` | `Failed` |
| `StartingAdbServer` | `AdbServerReady` | `DetectingUsbDevice` |
| `DetectingUsbDevice` | `SingleUsbDeviceFound` | `EnablingTcpIp` |
| `DetectingUsbDevice` | `NoUsbDeviceFound` | `Failed` |
| `DetectingUsbDevice` | `MultipleUsbDevicesFound` | `Failed` |
| `DetectingUsbDevice` | `UsbDeviceUnauthorized` | `Failed` |
| `EnablingTcpIp` | `TcpIpEnabled` | `DiscoveringPhoneIp` |
| `EnablingTcpIp` | `TcpIpFailed` | `Failed` |
| `DiscoveringPhoneIp` | `PhoneIpDiscovered` | `PromptingUsbDisconnect` |
| `DiscoveringPhoneIp` | `PhoneIpMissing` | `Failed` |
| `PromptingUsbDisconnect` | `UserConfirmedUsbDisconnected` | `ConnectingWirelessAdb` |
| `PromptingUsbDisconnect` | `UserCancelled` | `Stopping` |
| `ConnectingWirelessAdb` | `WirelessAdbConnected` | `LaunchingScrcpy` |
| `ConnectingWirelessAdb` | `WirelessAdbFailed` | `Failed` |
| `LaunchingScrcpy` | `ScrcpyStarted` | `Running` |
| `LaunchingScrcpy` | `ScrcpyLaunchFailed` | `Failed` |
| `Running` | `NetworkInterrupted` | `Reconnecting` |
| `Reconnecting` | `ReconnectSucceeded` | `Running` |
| `Reconnecting` | `ReconnectExpired` | `Failed` |
| Any active state | `StopRequested` | `Stopping` |
| `Stopping` | `Stopped` | `Completed` |

### State machine rules

- Invalid transitions are rejected and logged as diagnostics.
- Expected workflow failures transition to `Failed` with a user-safe `AppError`.
- Cancellation transitions through `Stopping` to `Completed` unless cleanup itself fails, in which case the cleanup failure is logged but shutdown continues.
- Only one active session is allowed. A second launch request while active is ignored with a status update.

## 8. Feature implementation plan

### One-click launch

The status window and tray menu call `WirelessScrcpyController.StartAsync`. The controller prevents concurrent sessions, creates a new session scope, and starts the workflow.

### Automatic ADB detection

`ToolDiscoveryService` checks the persisted ADB path first, then application-local tool locations, then PATH, and finally common Android platform-tools locations under user and system program directories. `ExecutablePathValidator` confirms the executable name and file existence.

### Automatic scrcpy detection

`ToolDiscoveryService` performs the same deterministic discovery process for `scrcpy.exe`, favoring persisted and application-local locations before PATH and sensible defaults.

### Automatic USB device detection

`AdbClient` runs `adb devices -l`. `AdbOutputParser` converts output to device models, and `AdbDeviceSelector` accepts exactly one authorized USB device.

### Automatic TCP/IP enablement on port 5555

`AdbTcpIpService` runs the equivalent of `adb -s <usb-serial> tcpip 5555` through `AdbClient` and validates success output or exit status.

### Automatic phone IP discovery

`PhoneIpDiscoveryService` queries network state through ADB shell commands. `IpAddressCandidateSelector` chooses a valid private IPv4 address and rejects loopback, link-local, multicast, and empty values.

### Prompt user to disconnect USB

`UsbDisconnectPrompt` is shown only after TCP/IP mode is enabled and an IP address is known. The workflow proceeds only after the user confirms disconnection or cancels the launch.

### Automatic wireless ADB connection

`AdbWirelessConnector` runs the equivalent of `adb connect <ip>:5555`, verifies the target appears in `adb devices`, and returns the wireless endpoint.

### Launch scrcpy using `--no-audio`

`ScrcpyCommandBuilder` always includes `--no-audio` for 1.0. `ScrcpyLauncher` starts the process against the wireless serial or endpoint and returns a `ScrcpySession`.

### Status window

`StatusForm` subscribes to workflow snapshots and displays current state, detail message, and failure details. It does not execute workflow logic directly.

### System tray icon

`TrayIconController` owns the Windows tray icon, mirrors high-level state, and exposes fixed launch, show status, and exit actions.

### Windows notifications

`TrayBalloonNotifier` sends notifications for important transitions: ready/running, reconnecting, reconnection failure, and unrecoverable errors.

### Persistent settings

`JsonSettingsStore` persists only settings needed by 1.0, such as last validated tool paths and window visibility preference, using atomic file replacement where possible.

### Session logging

`FileSessionLogger` records launch steps, process commands without sensitive data, state transitions, controlled failures, reconnect attempts, and shutdown events.

### Automatic reconnection after brief network interruption

`ReconnectSupervisor` reacts to scrcpy exit or wireless ADB interruption after the session reaches `Running`. It attempts reconnection for a bounded brief window using conservative delays from `WorkflowOptions`, then relaunches scrcpy or fails gracefully.

### Clean shutdown

`ApplicationLifetime` cancels active operations, asks `SessionHandle` to stop scrcpy, optionally disconnects transient wireless ADB session state if owned by this run, flushes logs, saves settings, disposes UI resources, and exits.

### Graceful error handling

Expected operational failures return `Result<T>` with `AppError`. Unexpected exceptions are caught at workflow boundaries, logged with diagnostics, and converted to user-safe errors.

## 9. Risks and mitigations

| Risk | Impact | Mitigation |
| --- | --- | --- |
| Different Android devices expose IP information differently. | IP discovery can fail on valid devices. | Support a small deterministic sequence of common ADB shell network commands and parse multiple known output formats. |
| Multiple connected USB devices. | The app could configure the wrong phone. | Reject multiple eligible USB devices in 1.0 and show a clear error. |
| Unauthorized USB debugging. | Workflow cannot proceed. | Detect unauthorized state and instruct the user to authorize debugging on the phone. |
| ADB server stale state. | Device detection or wireless connection may be unreliable. | Start the server explicitly and restart only after a controlled command failure pattern. |
| Windows notification APIs vary by environment. | Toast notification delivery may fail. | Fallback to tray balloon notifications. |
| scrcpy process exits for user-driven reasons. | Reconnection might relaunch when user intended to close. | Treat explicit app shutdown as stop, but treat brief unexpected exit during active session as reconnectable. |
| Antivirus or policy blocks process execution. | Tools may exist but fail to start. | Capture process start failures and surface actionable errors. |
| Settings or logs path unavailable. | Startup or logging can fail. | Use per-user application data paths and continue with in-memory defaults if non-critical persistence fails. |

## 10. Implementation roadmap

### Phase 1: Repository foundation

- Create the .NET 8 solution and projects.
- Add nullable reference types, implicit usings, analyzers, and consistent build settings.
- Add the initial test project.

### Phase 2: Core process and diagnostics infrastructure

- Implement process request/result models.
- Implement asynchronous process execution.
- Implement executable discovery and validation.
- Unit test PATH scanning and path validation.

### Phase 3: Settings and logging

- Implement JSON settings storage.
- Implement session log file creation and retention.
- Unit test default settings, malformed settings recovery, and log path resolution.

### Phase 4: ADB domain services

- Implement ADB command building.
- Implement device and network output parsing.
- Implement USB device selection and TCP/IP enablement services.
- Unit test parser fixtures and selection edge cases.

### Phase 5: scrcpy domain services

- Implement scrcpy command building.
- Implement scrcpy launch and process monitoring.
- Unit test argument generation and process lifecycle behavior through abstractions.

### Phase 6: Workflow and state machine

- Implement workflow states, events, snapshots, and transition validation.
- Implement one-session controller and cancellation handling.
- Implement reconnect supervisor.
- Unit test success path, controlled failures, cancellation, and reconnect decisions.

### Phase 7: WinForms shell

- Implement application context, status form, tray icon, prompt, and UI dispatcher.
- Wire UI actions to controller methods.
- Ensure UI remains responsive throughout workflow execution.

### Phase 8: Notifications and polish

- Implement Windows notification adapter and tray fallback.
- Add user-safe messages for every expected `ErrorCode`.
- Validate clean shutdown and disposal paths.

### Phase 9: Acceptance validation

- Build in Release configuration.
- Run all automated tests.
- Manually validate the complete workflow on Windows 10+ with a supported Android device, ADB, and scrcpy.
- Verify startup time, idle CPU behavior, logging output, and graceful error scenarios.
