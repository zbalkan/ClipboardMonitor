# ClipboardMonitor

## Overview
ClipboardMonitor is a lightweight background utility that tracks **clipboard** usage.
It

* filters text for high-risk tokens (`POWERSHELL`, `MSHTA`, `CMD`, `MSIEXEC`, etc.) copied **from well-known browsers**, and submits matches to **AMSI** for antivirus verdicts;
* detects **payment-card PANs**, masks them, and scrubs the clipboard (sample DLP);
* warns the user if they press **Win + R** within 30 s of a risky copy;  
* logs every incident asynchronously to the Windows Event Log.
* uses a toast notification to inform the user.

## Installation
Logs are written to the Windows Event Log. Creating (or removing) the log source requires elevation.

1. Open an **elevated** PowerShell / CMD window and run  
   `ClipboardMonitor -i` (or `/i`, `--install`) to register the event-log source.  
2. Launch `ClipboardMonitor.exe` normally (or place it in Startup / Task Scheduler).  
3. Done.

### Uninstallation
Run **as Administrator**:  
`ClipboardMonitor -u` (or `/u`, `--uninstall`) to remove the event-log source.

## Usage
```
USAGE: ClipboardMonitor [ARGUMENTS]
-i,/i,--install      Registers the Windows-Event-Log source (Admin required).
-u,/u,--uninstall    Removes   the Windows-Event-Log source (Admin required).
-?, -h, /h, --help   Displays this message box.

```

### WARNING
ClipboardMonitor itself runs fine under a standard user account.  
An optional `ENABLE_CRITICAL_PROCESS` block (currently **commented-out** for safety) can mark the process as critical; if re-enabled and the process is forcibly terminated, Windows will bug-check with **CRITICAL_PROCESS_DIED**. Enable only in hardened production builds—**never during normal development**.

## Development
The application s built with **.NET Framework 4.8.1** and WPF/C# 7.x and using Winforms components when needed.
Test project uses .NET 9.0.
Open the solution in Visual Studio (with *.NET Desktop Development* workload).

## Thanks
Thanks to **Gérald Barré, aka. Meziantou** for the [AMSI in .NET article](https://www.meziantou.net/using-windows-antimalware-scan-interface-in-dotnet.htm) and **Eric Lawrence** for ClipShield and his [attack-techniques article](https://textslashplain.com/2024/06/04/attack-techniques-trojaned-clipboard/).

## Icon
[Monitoring icons created by smashingstocks - Flaticon](https://www.flaticon.com/free-icons/monitoring)
