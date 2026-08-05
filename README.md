# Project Launcher

Project Launcher is a local-first desktop dashboard for organizing development projects, inspecting local Git state, linking GitHub repositories, and tracking commit streaks.

## Quick installation

The installers download the latest self-contained release. You do not need .NET or administrator access.

### Linux x64

Open a terminal and run:

```bash
curl -fsSL https://raw.githubusercontent.com/tashley46/Project-Launcher/main/scripts/install-linux.sh | bash
```

Then open **Project Launcher** from the application menu or run:

```bash
project-launcher
```

### Windows x64

Open PowerShell and run:

```powershell
irm https://raw.githubusercontent.com/tashley46/Project-Launcher/main/scripts/install-windows.ps1 | iex
```

Then open **Project Launcher** from the Start menu.

> The scripts must come from a branch you trust. You can download and inspect them before running them if preferred. Windows may show a SmartScreen warning while development releases remain unsigned.

Git is optional for project organization. Install Git and make it available on `PATH` to enable repository detection, GitHub linking, and streak calculation.

## Updates and application data

Run the same installation command again to update Project Launcher. Saved data remains separate from the executable:

- Linux: `~/.local/share/ProjectLauncher/project-launcher.db`
- Windows: `%LOCALAPPDATA%\ProjectLauncher\project-launcher.db`

Database migrations run automatically when the application starts.

## Uninstall

Linux:

```bash
rm -f "$HOME/.local/bin/project-launcher" \
      "$HOME/.local/share/applications/project-launcher.desktop"
rm -rf "$HOME/.local/share/project-launcher/app"
```

Windows PowerShell:

```powershell
Remove-Item "$env:LOCALAPPDATA\Programs\ProjectLauncher" -Recurse -Force
Remove-Item "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Project Launcher.lnk" -Force
```

These commands preserve the database. Delete the application-data directory separately only if you also want to erase saved projects.

## Build release packages

Maintainers with the .NET 10 SDK, Bash, `tar`, and `zip` can build both platforms with:

```bash
./scripts/package-release.sh 0.1.0
```

Output is written to `artifacts/`. Upload these stable-name assets to a GitHub release so the quick installers can always target the latest release:

```text
ProjectLauncher-linux-x64.tar.gz
ProjectLauncher-win-x64.zip
```

Versioned archives and SHA-256 checksums are produced alongside them.

## Run from source

```bash
dotnet run --project src/ProjectLauncher.UI.Avalonia/ProjectLauncher.UI.Avalonia.csproj
```
