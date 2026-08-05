$ErrorActionPreference = "Stop"

$RepositoryUrl = "https://github.com/tashley46/Project-Launcher"
$DownloadUrl = "$RepositoryUrl/releases/latest/download/ProjectLauncher-win-x64.zip"
$InstallDirectory = Join-Path $env:LOCALAPPDATA "Programs\ProjectLauncher"
$TemporaryDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("ProjectLauncher-" + [guid]::NewGuid())
$ArchivePath = Join-Path $TemporaryDirectory "ProjectLauncher.zip"

try {
    Write-Host "Downloading Project Launcher..."
    New-Item -ItemType Directory -Path $TemporaryDirectory -Force | Out-Null
    Invoke-WebRequest -Uri $DownloadUrl -OutFile $ArchivePath

    New-Item -ItemType Directory -Path $InstallDirectory -Force | Out-Null
    Expand-Archive -Path $ArchivePath -DestinationPath $InstallDirectory -Force

    $ExecutablePath = Join-Path $InstallDirectory "ProjectLauncher.exe"
    $StartMenuDirectory = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs"
    $ShortcutPath = Join-Path $StartMenuDirectory "Project Launcher.lnk"
    $Shell = New-Object -ComObject WScript.Shell
    $Shortcut = $Shell.CreateShortcut($ShortcutPath)
    $Shortcut.TargetPath = $ExecutablePath
    $Shortcut.WorkingDirectory = $InstallDirectory
    $Shortcut.Description = "Local-first developer project dashboard"
    $Shortcut.Save()

    Write-Host "Project Launcher is installed. Open it from the Start menu."
}
finally {
    if (Test-Path $TemporaryDirectory) {
        Remove-Item -Path $TemporaryDirectory -Recurse -Force
    }
}
