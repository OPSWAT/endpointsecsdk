#!/bin/bash

# Ensure the script exits if any command fails
set -e

# ---------------------------------------------------------------------------
# macOS SDK setup.
#
# SDK files come from the cross-platform SDK downloader at the repo root.
# IMPORTANT: the SDK downloader MUST be run before these samples can build.
# This script runs it for you below (it resolves the repo root via the sdkroot
# marker and uses eval-license/download_token.txt), then stages the macOS
# headers and libraries into src/sdk where the sample Makefiles expect them.
# ---------------------------------------------------------------------------

# This script is expected to run from helloworld/mac-cpp/src
REPO_ROOT="../../.."
OPSWAT_SDK="$REPO_ROOT/OPSWAT-SDK"
MAC_LIB_SRC="$OPSWAT_SDK/client/mac"      # flat dir of universal .dylib + data files
MAC_INC_SRC="$OPSWAT_SDK/extract/mac/inc"
MAC_DOCS_SRC="$OPSWAT_SDK/extract/mac/docs"

echo "Running the SDK downloader at the repo root..."
( cd "$REPO_ROOT/sdk-downloader/script/src" && python3 main.py )

# Guard: the SDK downloader must have produced the macOS SDK files. If it was
# not run (or failed, e.g. missing/invalid eval-license/download_token.txt),
# stop here with a clear, actionable error instead of failing later in the build.
if [ ! -e "$MAC_LIB_SRC/libwaapi.dylib" ] || [ ! -d "$MAC_INC_SRC" ]; then
    echo "ERROR: macOS SDK files were not found under OPSWAT-SDK."
    echo "       Expected: $MAC_LIB_SRC/libwaapi.dylib and $MAC_INC_SRC/"
    echo "       The SDK downloader must be run first. It uses the download token in"
    echo "       eval-license/download_token.txt at the repo root. From the repo root run:"
    echo "           cd sdk-downloader/script/src && python3 main.py"
    echo "       Then re-run this script."
    exit 2
fi

echo "Cleaning up old SDK files..."
rm -rf sdk
mkdir -p sdk/lib/x64 sdk/lib/arm64 sdk/inc sdk/docs

# macOS dylibs are universal binaries, so the same set serves both architectures.
# Stage them under both lib/x64 and lib/arm64 so the sample Makefiles (which pick
# the dir from `uname -m`) build on either Intel or Apple Silicon.
echo "Staging SDK libraries into sdk/lib/{x64,arm64}..."
cp -a "$MAC_LIB_SRC/." sdk/lib/x64/
cp -a "$MAC_LIB_SRC/." sdk/lib/arm64/

echo "Staging SDK headers into sdk/inc..."
cp -a "$MAC_INC_SRC/." sdk/inc/

if [ -d "$MAC_DOCS_SRC" ]; then
    echo "Staging SDK docs into sdk/docs..."
    cp -a "$MAC_DOCS_SRC/." sdk/docs/
fi

# Copy the vulnerability data files used by the GetProductVulnerability sample.
echo "Copying vulnerability data files to GetProductVulnerability directory..."
mkdir -p "GetProductVulnerability"
for f in mav.dat v2mod.dat; do
    if [ -e "$MAC_LIB_SRC/$f" ]; then
        cp -f "$MAC_LIB_SRC/$f" "GetProductVulnerability/"
    else
        echo "Warning: $f not found in $MAC_LIB_SRC; GetProductVulnerability example may not work."
    fi
done

echo "SDK download and setup complete!"
echo "SDK files are located in: $(pwd)/sdk"
echo "  - Library files: sdk/lib/{x64,arm64}"
echo "  - Include files: sdk/inc"
echo "  - Documentation: sdk/docs"

exit 0
