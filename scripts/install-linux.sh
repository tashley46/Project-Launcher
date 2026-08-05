#!/usr/bin/env bash

set -euo pipefail

REPOSITORY_URL="https://github.com/tashley46/Project-Launcher"
DOWNLOAD_URL="${PROJECT_LAUNCHER_DOWNLOAD_URL:-$REPOSITORY_URL/releases/latest/download/ProjectLauncher-linux-x64.tar.gz}"
INSTALL_DIR="${PROJECT_LAUNCHER_INSTALL_DIR:-${XDG_DATA_HOME:-$HOME/.local/share}/project-launcher/app}"
BIN_DIR="${PROJECT_LAUNCHER_BIN_DIR:-$HOME/.local/bin}"
DESKTOP_DIR="${PROJECT_LAUNCHER_DESKTOP_DIR:-${XDG_DATA_HOME:-$HOME/.local/share}/applications}"
TEMP_DIR="$(mktemp -d)"
trap 'rm -rf -- "$TEMP_DIR"' EXIT

command -v curl >/dev/null || { echo "Project Launcher requires curl." >&2; exit 1; }
command -v tar >/dev/null || { echo "Project Launcher requires tar." >&2; exit 1; }

echo "Downloading Project Launcher…"
curl --fail --location --silent --show-error \
    "$DOWNLOAD_URL" \
    --output "$TEMP_DIR/ProjectLauncher.tar.gz"

mkdir -p -- "$INSTALL_DIR" "$BIN_DIR" "$DESKTOP_DIR"
tar -xzf "$TEMP_DIR/ProjectLauncher.tar.gz" -C "$INSTALL_DIR"
chmod +x "$INSTALL_DIR/ProjectLauncher"
ln -sfn "$INSTALL_DIR/ProjectLauncher" "$BIN_DIR/project-launcher"

cat > "$DESKTOP_DIR/project-launcher.desktop" <<EOF
[Desktop Entry]
Name=Project Launcher
Comment=Local-first developer project dashboard
Exec=$INSTALL_DIR/ProjectLauncher
Terminal=false
Type=Application
Categories=Development;
EOF
chmod +x "$DESKTOP_DIR/project-launcher.desktop"

echo "Project Launcher is installed. Open it from your application menu"
echo "or run: $BIN_DIR/project-launcher"
