# ClipboardMonitor

## Overview
ClipboardMonitor is an application running in the background that tracks clipboard usage. The clipboard is checked against data copied from well-known browsers, run against AMSI for prevention of attacks asking users running commands.
By default, every text copied into cliploard is also scanned to detect PAN data, as a sample and simple DLP-like tool.

## Installation
The logs are written into event log. In order to add or remove the log source, you need administration rights during installation and uninstallation.

1. In an elevated terminal/cmd/powershell session, run `ClipboardMonitor -i` (or `/i` or `--install`).
2. Run the `ClipboardMonitor`.
3. Success.

### Uninstallation
In an elevated terminal/cmd/powershell session, run `ClipboardMonitor -u` (or `/u` or `--uninstall`).

## Usage
```
USAGE: ClipboardMonitor [ARGUMENTS]
-i,/i,--install			Installs the application (Needs Admin rights).
-u,/u,--uninstall		Installs the application (Needs Admin rights).
-?, -h, /h, --help		Displays this message box.
```

### WARNING
ClipboardMonitor requires Administrator (`SeDebugPrivilege`) privileges as of latest release.

In case of interruption, such as running `taskkill` command or killing the process using Task Manager, user will get a BSOD `CRITICAL_PROCESS_DIED`.

## Development
ClipboardMonitor is built with .NET 4.8.1 and WPF. Therefore, you need Visual Studio with .NET Desktop Development features for development.

## Thanks
Thanks Meziantou for [AMSI usage in .NET article](https://www.meziantou.net/using-windows-antimalware-scan-interface-in-dotnet.htm). Also, thanks to the Eric Lawrence for his ClipShield project and the [great article](https://textslashplain.com/2024/06/04/attack-techniques-trojaned-clipboard/) explaining it.
