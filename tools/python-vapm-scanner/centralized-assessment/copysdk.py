#!/usr/bin/env python3
###############################################################################################
##  Copy SDK Files
##  Stages the correct OPSWAT SDK client binaries, data files, and license files into the
##  local sdk/ directory for this VAPM scanner sample. Mirrors the helloworld/python approach.
##
##  Usage:
##      python3 copysdk.py
##
##  Walks up from this script to find the 'sdkroot' marker (repo root), then copies:
##      OPSWAT-SDK/client/<os>/[<arch>/]   -->  ./sdk
##      eval-license/                      -->  ./sdk
##
##  Run the SDK downloader (sdk-downloader) first so OPSWAT-SDK/ exists.
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import os
import platform
import shutil
import sys


def get_os_dir():
    p = sys.platform
    if p == "win32":
        return "windows"
    elif p == "darwin":
        return "mac"
    elif p.startswith("linux"):
        return "linux"
    raise RuntimeError(f"Unsupported platform: {p}")


def get_arch_dir():
    # Maps the current CPU to the OPSWAT-SDK client subdirectory name.
    # macOS is flat (universal binaries) — no architecture subfolder.
    machine = platform.machine().lower()
    os_dir = get_os_dir()
    if machine in ("x86_64", "amd64"):
        return None if os_dir == "mac" else "x64"
    elif machine in ("i386", "i686", "x86"):
        return None if os_dir == "mac" else ("win32" if os_dir == "windows" else "x86")
    elif machine in ("arm64", "aarch64"):
        return None if os_dir == "mac" else "arm64"
    raise RuntimeError(f"Unsupported architecture: {machine}")


def find_repo_root(start_path):
    # Walk up until a directory containing the 'sdkroot' marker file is found.
    current = os.path.abspath(start_path)
    while True:
        if os.path.isfile(os.path.join(current, "sdkroot")):
            return current
        parent = os.path.dirname(current)
        if parent == current:
            break
        current = parent
    raise FileNotFoundError(
        "Could not find the 'sdkroot' marker file in any parent directory."
    )


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


def main():
    script_dir = os.path.dirname(os.path.abspath(__file__))
    sdk_dst = os.path.join(script_dir, "sdk")

    try:
        repo_root = find_repo_root(script_dir)
    except FileNotFoundError as e:
        print(f"ERROR: {e}")
        sys.exit(1)

    print(f"Repo root: {repo_root}")

    os_dir = get_os_dir()
    arch_dir = get_arch_dir()
    if arch_dir:
        sdk_src = os.path.join(repo_root, "OPSWAT-SDK", "client", os_dir, arch_dir)
    else:
        sdk_src = os.path.join(repo_root, "OPSWAT-SDK", "client", os_dir)

    license_src = os.path.join(repo_root, "eval-license")

    # The SDK downloader must have produced the client binaries.
    if not os.path.isdir(sdk_src):
        print(f"ERROR: SDK client directory not found: {sdk_src}")
        print("       Run the SDK downloader first to populate OPSWAT-SDK/:")
        print("         Windows:     sdk-downloader\\windows-csharp\\bin\\SDKDownloader.exe")
        print("         Linux/macOS: python3 sdk-downloader/script/src/main.py")
        sys.exit(1)

    # Validate the license files exist before copying anything.
    for required in ("license.cfg", "pass_key.txt"):
        if not os.path.isfile(os.path.join(license_src, required)):
            print(f"ERROR: {required} not found in {license_src}")
            sys.exit(1)

    # Clean the sdk directory first so stale files from a previous run (or a different
    # architecture / SDK version) don't linger.
    if os.path.isdir(sdk_dst):
        print(f"Cleaning existing SDK directory: {sdk_dst}")
        shutil.rmtree(sdk_dst)

    try:
        copy_files(sdk_src, sdk_dst, "SDK binaries")
        copy_files(license_src, sdk_dst, "license files")
        print(f"\nDone. SDK ready at: {sdk_dst}")
    except FileNotFoundError as e:
        print(f"ERROR: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()
