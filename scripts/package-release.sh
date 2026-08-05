#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -- "$SCRIPT_DIR/.." && pwd)"
PROJECT="$REPO_ROOT/src/ProjectLauncher.UI.Avalonia/ProjectLauncher.UI.Avalonia.csproj"
ARTIFACTS_DIR="$REPO_ROOT/artifacts"
PUBLISH_ROOT="$ARTIFACTS_DIR/publish"
VERSION="${1:-0.1.0}"

if [[ ! "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+([.-][0-9A-Za-z.-]+)?$ ]]; then
    echo "Version must resemble 1.2.3 or 1.2.3-preview.1" >&2
    exit 1
fi

if [[ "$PUBLISH_ROOT" != "$REPO_ROOT/artifacts/publish" ]]; then
    echo "Refusing to clean an unexpected publish directory." >&2
    exit 1
fi

rm -rf -- "$PUBLISH_ROOT"
mkdir -p -- "$PUBLISH_ROOT/linux-x64" "$PUBLISH_ROOT/win-x64"

publish_runtime() {
    local runtime="$1"
    local output="$PUBLISH_ROOT/$runtime"

    dotnet publish "$PROJECT" \
        --configuration Release \
        --runtime "$runtime" \
        --self-contained true \
        --output "$output" \
        -p:Version="$VERSION" \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:EnableCompressionInSingleFile=true \
        -p:DebugType=None \
        -p:DebugSymbols=false
}

publish_runtime linux-x64
publish_runtime win-x64

chmod +x "$PUBLISH_ROOT/linux-x64/ProjectLauncher"

tar -C "$PUBLISH_ROOT/linux-x64" \
    -czf "$ARTIFACTS_DIR/ProjectLauncher-$VERSION-linux-x64.tar.gz" \
    ProjectLauncher

(
    cd "$PUBLISH_ROOT/win-x64"
    zip -q -9 "$ARTIFACTS_DIR/ProjectLauncher-$VERSION-win-x64.zip" ProjectLauncher.exe
)

cp -- "$ARTIFACTS_DIR/ProjectLauncher-$VERSION-linux-x64.tar.gz" \
    "$ARTIFACTS_DIR/ProjectLauncher-linux-x64.tar.gz"
cp -- "$ARTIFACTS_DIR/ProjectLauncher-$VERSION-win-x64.zip" \
    "$ARTIFACTS_DIR/ProjectLauncher-win-x64.zip"

(
    cd "$ARTIFACTS_DIR"
    sha256sum \
        "ProjectLauncher-$VERSION-linux-x64.tar.gz" \
        "ProjectLauncher-$VERSION-win-x64.zip" \
        > "ProjectLauncher-$VERSION-SHA256SUMS.txt"
)

echo
echo "Packages created in $ARTIFACTS_DIR"
ls -lh \
    "$ARTIFACTS_DIR/ProjectLauncher-$VERSION-linux-x64.tar.gz" \
    "$ARTIFACTS_DIR/ProjectLauncher-$VERSION-win-x64.zip" \
    "$ARTIFACTS_DIR/ProjectLauncher-linux-x64.tar.gz" \
    "$ARTIFACTS_DIR/ProjectLauncher-win-x64.zip" \
    "$ARTIFACTS_DIR/ProjectLauncher-$VERSION-SHA256SUMS.txt"
