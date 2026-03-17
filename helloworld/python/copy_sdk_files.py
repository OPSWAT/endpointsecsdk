#!/usr/bin/env python3
###############################################################################################
##  Copy SDK Files
##  Copies the correct OPSWAT SDK binaries and license files into the local sdk/ directory.
##  Cross-platform equivalent of copysdkfiles.ps1
##
##  Usage:
##      python3 copy_sdk_files.py
##
##  The script walks up from its own location looking for an 'sdkroot' marker file
##  to locate the repo root, then copies:
##      OPSWAT-SDK/client/<os>/<arch>/                              -->  ./sdk/
##      eval-license/                                               -->  ./sdk/
##      OPSWAT-SDK/extract/analog/client/compliance/windows/docs/moby  -->  ./analog/
###############################################################################################

import os
import platform
import shutil
import sys


# ---------------------------------------------------------------------------
# Platform / architecture mapping
# ---------------------------------------------------------------------------

def get_os_dir():
    p = sys.platform
    if p == "win32":
        return "windows"
    elif p == "darwin":
        return "mac"
    elif p.startswith("linux"):
        return "linux"
    else:
        raise RuntimeError(f"Unsupported platform: {p}")


def get_arch_dir():
    # Maps Python architecture strings to OPSWAT SDK subdirectory names
    machine = platform.machine().lower()
    os_dir  = get_os_dir()

    if machine in ("x86_64", "amd64"):
        return None if os_dir == "mac" else "x64"   # mac is flat, no arch subdir
    elif machine in ("i386", "i686", "x86"):
        return None if os_dir == "mac" else ("win32" if os_dir == "windows" else "x86")
    elif machine in ("arm64", "aarch64"):
        return None if os_dir == "mac" else "arm64"
    else:
        raise RuntimeError(f"Unsupported architecture: {machine}")


# ---------------------------------------------------------------------------
# Repo root discovery via 'sdkroot' marker file
# ---------------------------------------------------------------------------

def find_repo_root(start_path):
    # Walk up from start_path until we find a directory containing 'sdkroot'
    current = os.path.abspath(start_path)
    while True:
        if os.path.isfile(os.path.join(current, "sdkroot")):
            return current
        parent = os.path.dirname(current)
        if parent == current:
            break
        current = parent

    raise FileNotFoundError(
        "Could not find 'sdkroot' marker file in any parent directory. "
        "Make sure the repo root contains a file named 'sdkroot'."
    )


# ---------------------------------------------------------------------------
# Copy helpers
# ---------------------------------------------------------------------------

def copy_files(src_dir, dst_dir, label):
    if not os.path.isdir(src_dir):
        raise FileNotFoundError(f"Source directory not found: {src_dir}")

    os.makedirs(dst_dir, exist_ok=True)
    print(f"Copying {label}:\n  {src_dir}\n  -> {dst_dir}")

    for filename in os.listdir(src_dir):
        src = os.path.join(src_dir, filename)
        dst = os.path.join(dst_dir, filename)
        if os.path.isfile(src):
            shutil.copy2(src, dst)
            print(f"  Copied {filename}")


def copy_analog_files(src_dir, dst_dir):
    """Recursively copy the analog moby directory tree into dst_dir."""
    if not os.path.isdir(src_dir):
        raise FileNotFoundError(f"Analog source directory not found: {src_dir}")

    print(f"Copying analog moby files:\n  {src_dir}\n  -> {dst_dir}")

    if os.path.exists(dst_dir):
        shutil.rmtree(dst_dir)

    shutil.copytree(src_dir, dst_dir)
    print(f"  Analog moby directory copied successfully.")


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    script_dir = os.path.dirname(os.path.abspath(__file__))
    sdk_dst    = os.path.join(script_dir, "sdk")
    analog_dst = os.path.join(script_dir, "analog")

    # Locate repo root via sdkroot marker
    try:
        repo_root = find_repo_root(script_dir)
    except FileNotFoundError as e:
        print(f"ERROR: {e}")
        sys.exit(1)

    print(f"Repo root: {repo_root}")

    # Build source path: OPSWAT-SDK/client/<os>/[<arch>/]
    os_dir   = get_os_dir()
    arch_dir = get_arch_dir()

    if arch_dir:
        sdk_src = os.path.join(repo_root, "OPSWAT-SDK", "client", os_dir, arch_dir)
    else:
        # macOS is flat — no architecture subdirectory
        sdk_src = os.path.join(repo_root, "OPSWAT-SDK", "client", os_dir)

    license_src = os.path.join(repo_root, "eval-license")

    # Analog moby source path (Windows-only, path is fixed per spec)
    analog_src = os.path.join(
        repo_root, "OPSWAT-SDK", "extract", "analog", "client",
        "compliance", "windows", "docs", "moby"
    )

    # Validate license files exist before copying anything
    for required in ("license.cfg", "pass_key.txt"):
        if not os.path.isfile(os.path.join(license_src, required)):
            print(f"ERROR: {required} not found in {license_src}")
            sys.exit(1)

    # Copy SDK binaries then license files into sdk/
    try:
        copy_files(sdk_src,     sdk_dst, "SDK binaries")
        copy_files(license_src, sdk_dst, "license files")
        print(f"\nDone. SDK ready at: {sdk_dst}")
    except FileNotFoundError as e:
        print(f"ERROR: {e}")
        sys.exit(1)

    # Copy analog moby directory into analog/
    try:
        copy_analog_files(analog_src, analog_dst)
        print(f"Done. Analog moby files ready at: {analog_dst}")
    except FileNotFoundError as e:
        print(f"ERROR: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()
