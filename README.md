# ClipboardMonitor

## Overview
ClipboardMonitor is an application running in the background that tracks clipboard usage to detect PAN data.

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

## Development
ClipboardMonitor is built with .NET 6 and WPF. Therefore you need Visual Studio with .NET Desktop Development features for development.