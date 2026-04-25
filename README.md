# MCP OS Doctor

A local, read-only [Model Context Protocol](https://modelcontextprotocol.io/) (MCP) server that exposes Windows system
diagnostics to AI assistants. It lets AI clients like Claude Code inspect event logs, services, processes, hardware 
info, and boot history, all without granting script execution, write access, or network capabilities.

## Features

| Tool                      | Description                                                                                     |
|---------------------------|-------------------------------------------------------------------------------------------------|
| `get_capabilities`        | Reports available tools, platform, elevation status, and parameter hints                        |
| `query_system_log`        | Search Windows Event Log entries by time, severity, source, and keywords                        |
| `list_log_sources`        | List available event log sources                                                                |
| `get_service_status`      | Query Windows services by name, pattern, or status                                              |
| `list_top_processes`      | List top processes sorted by CPU or memory usage                                                |
| `get_system_info`         | Hardware and OS snapshot (hostname, CPU, memory, disks, uptime)                                 |
| `get_boot_history`        | Boot, shutdown, crash, and sleep/wake events with timestamps                                    |
| `get_gpu_info`            | NVIDIA GPU info: model, driver, VRAM usage, temperature, utilization, power draw via nvidia-smi |
| `get_directx_info`        | DirectX version, display adapters (VRAM, drivers, feature levels), and sound devices via dxdiag |
| `start_sensor_monitoring` | Start background hardware sensor polling (temperature, fan, voltage, clock, load, power)        |
| `stop_sensor_monitoring`  | Stop background sensor monitoring; collected data remains available via `get_sensor_data`       |
| `get_sensor_data`         | Retrieve sensor monitoring results with min/max/average/current statistics per sensor           |


Sensor monitoring data is persisted to disk (`%TEMP%/mcp-os-doctor/`), so collected readings survive server crashes and can be retrieved on restart via `get_sensor_data`.

## Security

MCP OS Doctor is **read-only by design**:

- **No write operations**: the server never modifies system state
- **No command execution**: no `Process.Start()`, no PowerShell, no WMI method invocations
- **No network I/O**: stdio transport only, no network listener
- **No credential handling**: never accepts, stores, or transmits credentials
- **Output sanitization**: redacts passwords, tokens, API keys, and connection strings from all output. This is a best-effort attempt, but may not catch all sensitive information due to limitations in parsing and redaction.

## Requirements

- Windows 10 or later (might work on previous versions, but not tested)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (for building from source)

### Optional

- **nvidia-smi** -- required by `get_gpu_info`. Included with NVIDIA GPU drivers; the tool reports unavailable if not found.
- **dxdiag** -- required by `get_directx_info`. Ships with Windows; the tool reports unavailable if not found.
- **Administrator privileges** -- sensor monitoring (`start_sensor_monitoring`) benefits from elevation for full hardware access. Some event log sources (Security, Setup) also require elevation. Can work fine without elevation.

> Tip: You can use `gsudo` (https://github.com/gerardog/gsudo) to run MCP OS Doctor as an administrator without elevating the terminal, which would also grant the AI agent more privileges than it should ever have.

## Building

```bash
dotnet build
```

To publish a self-contained single-file executable:

```bash
dotnet publish src/McpOsDoctor.Server -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
```

## Running tests

```bash
dotnet test
```

## Using with Claude Code

Add an MCP server entry to one of these locations:

| Scope                      | File                     | When to use                          |
|----------------------------|--------------------------|--------------------------------------|
| Global (all projects)      | `~/.claude.json`         | Personal system-wide diagnostics     |
| Project (shared with team) | `.mcp.json` in repo root | Ship the MCP config with the project |

### Option 1: Published executable

Build the project, then add to your config file of choice:

```json
{
  "mcpServers": {
    "os-doctor": {
      "command": "C:/path/to/published/McpOsDoctor.exe"
    }
  }
}
```

### Option 2: Published executable with elevated privileges using `gsudo`

```json
{
  "mcpServers": {
    "os-doctor": {
      "command": "gsudo",
      "args": [
        "C:/path/to/published/McpOsDoctor.exe"
      ]
    }
  }
}
```

### Option 3: Run via `dotnet run`

Point Claude Code at the project directly without publishing first:

```json
{
  "mcpServers": {
    "os-doctor": {
      "command": "dotnet",
      "args": ["run", "--project", "C:/path/to/mcp-os-doctor/src/McpOsDoctor.Server"]
    }
  }
}
```

### Verifying the connection

Once configured, restart Claude Code (or run `/mcp` to check status). You should see `os-doctor` listed with 12 tools available. You can verify by asking Claude:

> "What diagnostic tools do you have available?"

Claude will call `get_capabilities` and report the available tools.

### Example prompts

Once connected, you can ask Claude things like:

- **"Why is my computer running slow?"**: Claude will check processes, system info, and event logs
- **"Show me any recent system errors"** queries the event log for Error/Critical entries
- **"Is the Windows Update service running?**": checks service status
- **"Has my computer crashed recently?"**: inspects boot history for unexpected shutdowns
- **"What's using all my memory?"**: lists top processes by memory consumption
- **"Monitor my CPU and GPU temperatures"**: starts sensor monitoring and reports thermal data

### Elevation

Some diagnostics (e.g., Security event log) require administrator privileges. If you run Claude Code from an elevated 
terminal (or use something like `gsudo`), MCP OS Doctor will automatically detect elevation and unlock additional 
capabilities. The `get_capabilities` tool reports the current elevation status.

## Architecture

| Project                        | Description                                       |
|--------------------------------|---------------------------------------------------|
| `McpOsDoctor.Core`             | Interfaces, DTOs, enums                           |
| `McpOsDoctor.Tools`            | MCP tool implementations                          |
| `McpOsDoctor.Platform.Windows` | Windows-specific implementations                  |
| `McpOsDoctor.Server`           | Entry point, DI composition (composes all layers) |

## Logs

Internal server logs are written to `%LOCALAPPDATA%/McpOsDoctor/logs/`. 
Logs never go to stdout/stderr (those are reserved for MCP transport).

## Roadmap

The current architecture facilitates adding a new platform by simply implementing the necessary interfaces. 
New platforms should be added by writing a new `McpOsDoctor.Platform.<OS>` project that implements the interfaces for 
each tool.

Below are some high-level ideas for future platform support.

### Linux support

Create a `McpOsDoctor.Platform.Linux` project (targeting `net10.0`) implementing all provider interfaces with Linux-native APIs:

- **System logs** (`ISystemLogReader`): read from journald via `journalctl` output parsing, with fallback to syslog files (`/var/log/syslog`, `/var/log/messages`).
- **Boot history** (`IBootHistoryProvider`): parse `journalctl --list-boots` and shutdown events, or use `last reboot`/`last shutdown` from wtmp records.
- **Processes** (`IProcessInspector`): enumerate via `/proc` filesystem or `Process.GetProcesses()` (.NET cross-platform API); read CPU/memory from `/proc/[pid]/stat` and `/proc/[pid]/status`.
- **Services** (`IServiceInspector`): query systemd via `systemctl` output parsing or D-Bus API. Map systemd unit states to the existing `ServiceRunState` enum.
- **System info** (`ISystemInfoProvider`): read `/proc/cpuinfo`, `/proc/meminfo`, `/etc/os-release`, and `lsblk` or `/proc/diskstats` for disk volumes.
- **Performance** (`IPerformanceReader`): read CPU and memory metrics from `/proc/stat` and `/proc/meminfo`.
- **Privilege checking** (`IPrivilegeChecker`): check effective UID (`geteuid() == 0`) or sudo/capability detection.
- **GPU info** (`IGpuInfoProvider`): nvidia-smi works on Linux as-is; add AMD support via `rocm-smi` parsing.
- **DirectX info** (`IDirectXInfoProvider`): not applicable on Linux. Could repurpose as a graphics API info tool reporting Vulkan capabilities (`vulkaninfo`) or OpenGL details (`glxinfo`).
- **Sensor monitoring** (`ISensorMonitor`): read from `lm-sensors` / hwmon sysfs (`/sys/class/hwmon/*/`) for temperature, fan, and voltage data.
- **Output sanitization** (`IOutputSanitizer`): largely reusable; add Linux-specific path patterns if needed.

Update `Program.cs` platform detection to register Linux providers when running on Linux.

### macOS support

Create a `McpOsDoctor.Platform.MacOS` project (targeting `net10.0`) implementing all provider interfaces with macOS-native APIs:

- **System logs** (`ISystemLogReader`): query Apple's Unified Logging via `log show` with predicate-based filtering.
- **Boot history** (`IBootHistoryProvider`): use `last reboot` and `log show --predicate` filtering for shutdown/sleep/wake events.
- **Processes** (`IProcessInspector`): enumerate via `Process.GetProcesses()` (.NET cross-platform API); supplement with `ps` for CPU and memory details.
- **Services** (`IServiceInspector`): query launchd via `launchctl list` and `launchctl print` for service metadata, state, and dependencies.
- **System info** (`ISystemInfoProvider`): use `sysctl` for hardware info (CPU, memory), `sw_vers` for OS version, and `diskutil list` for disk volumes.
- **Performance** (`IPerformanceReader`): use `sysctl` and `vm_stat` for CPU and memory metrics.
- **Privilege checking** (`IPrivilegeChecker`): check effective UID (`geteuid() == 0`).
- **GPU info** (`IGpuInfoProvider`): parse `system_profiler SPDisplaysDataType` for GPU model, VRAM, and Metal support details.
- **DirectX info** (`IDirectXInfoProvider`): not applicable on macOS. Could repurpose to report Metal capabilities via `system_profiler`.
- **Sensor monitoring** (`ISensorMonitor`): read from the System Management Controller (SMC) via IOKit framework for temperature, fan speed, and power data. May require a native interop layer or a third-party library.
- **Output sanitization** (`IOutputSanitizer`): largely reusable; add macOS-specific path patterns if needed (e.g., Keychain paths).

Update `Program.cs` platform detection to register macOS providers when running on macOS.

## License

See [LICENSE](LICENSE.md) for details.
