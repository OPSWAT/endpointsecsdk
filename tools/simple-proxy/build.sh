#!/usr/bin/env bash
#
# build.sh - build simple-proxy on macOS or Linux and stage the artifacts into bin/.
#
# Produces (release):
#     bin/simple-proxy-demo       demo executable
#     bin/libsimple_proxy.so      (Linux) dynamic library (C ABI) for embedding
#     bin/libsimple_proxy.dylib   (macOS) dynamic library (C ABI) for embedding
#     bin/simple_proxy.h          C header (copied from include/)
#
# Requires the Rust toolchain (https://rustup.rs) and a C linker (build-essential / Xcode CLT).
#
# Created by Chris Seiler - OPSWAT OEM Field CTO
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$root"

echo "Building simple-proxy (release)..."
cargo build --release

bin="$root/bin"
mkdir -p "$bin"
rel="$root/target/release"

copy() {
    if [ -f "$rel/$1" ]; then
        cp -f "$rel/$1" "$bin/"
        echo "  copied $1"
    else
        echo "  missing $1" >&2
    fi
}

case "$(uname -s)" in
    Darwin)
        copy "simple-proxy-demo"
        copy "libsimple_proxy.dylib"
        ;;
    Linux)
        copy "simple-proxy-demo"
        copy "libsimple_proxy.so"
        ;;
    *)
        echo "  unknown platform '$(uname -s)', attempting Linux-style names" >&2
        copy "simple-proxy-demo"
        copy "libsimple_proxy.so"
        ;;
esac

if [ -f "$root/include/simple_proxy.h" ]; then
    cp -f "$root/include/simple_proxy.h" "$bin/"
    echo "  copied simple_proxy.h"
fi

echo "Done. Artifacts in $bin"
