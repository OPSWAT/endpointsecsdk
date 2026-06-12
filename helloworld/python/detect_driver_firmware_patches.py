#!/usr/bin/env python3
###############################################################################################
##  Sample Code for Detect Driver/Firmware Patches  (Windows only)
##  Reference Implementation using OESIS Framework
##
##  Detects applicable driver/firmware patches by matching the device inventory against the
##  loaded driver/firmware database via DetectDriverFirmwarePatches (method 50902). Prints a
##  summary to the console and writes the full patch list to a JSON output file.
##
##  Usage:
##      python detect_driver_firmware_patches.py                 # all components/categories
##      python detect_driver_firmware_patches.py <inventory.json>  # use a prebuilt inventory
##
##  Flow (driver/firmware patch management is Windows only and licensed):
##      setup -> LoadDriverFirmwareDatabase (50900) -> DetectDriverFirmwarePatches (50902)
##
##  If 50900/50902 return rc=-5 (WAAPI_ERROR_NOT_INITIALIZED), the active license does not
##  entitle the driver/firmware feature -- verify the license, not the script.
##
##  https://software.opswat.com/OESIS_V4/html/c_method.html -> methods 50900, 50902
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import json
import os
import sys

from sdk_wrapper import OESISWrapper, SDKError
from platform_utils import validate_sdk_environment
from platform_utils import get_lib_filename
from platform_utils import get_os_type, OS_TYPE_WINDOWS

# Hardcoded SDK directory relative to this script
SDK_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "sdk")

# Driver/firmware metadata database filename (the only file currently accepted by method 50900)
DRIVER_FIRMWARE_DB = "patch_driver_firmware.dat"

# Returned by method 50902 when the detected device model has no entries in the
# loaded driver/firmware database -- a coverage gap, not an error in the call.
MODEL_NOT_SUPPORTED = -1067   # WA_VMOD_ERROR_MODEL_NOT_SUPPORTED

# Human-readable labels for the patches[].reboot_required field
REBOOT_LABELS = {
    -1: "Unknown",
     0: "No reboot",
     1: "Requires reboot",
     2: "Forces reboot",
     3: "Forces shutdown",
     4: "Delayed forced reboot",
}


def find_driver_firmware_db():
    # Locate patch_driver_firmware.dat. It is not staged into sdk/ by
    # copy_sdk_files.py, so fall back to the SDK Downloader's extract location:
    #   <repo_root>/OPSWAT-SDK/extract/analog/client/patch_driver_firmware.dat
    # The repo root is found by walking up for the 'sdkroot' marker file.
    local = os.path.join(SDK_DIR, DRIVER_FIRMWARE_DB)
    if os.path.isfile(local):
        return local

    current = os.path.dirname(os.path.abspath(__file__))
    while True:
        if os.path.isfile(os.path.join(current, "sdkroot")):
            candidate = os.path.join(
                current, "OPSWAT-SDK", "extract", "analog", "client", DRIVER_FIRMWARE_DB
            )
            if os.path.isfile(candidate):
                return candidate
            break
        parent = os.path.dirname(current)
        if parent == current:
            break
        current = parent

    return None


def initialize_framework():
    # Load the SDK and initialize with the pass_key.txt in the sdk directory
    # https://software.opswat.com/OESIS_V4/html/c_sdk.html
    pass_key_path = os.path.join(SDK_DIR, "pass_key.txt")

    if not os.path.isfile(pass_key_path):
        print("Could not find pass_key.txt. Make sure the license is in the sdk directory.")
        raise Exception("License pass_key.txt file not found")

    sdk = OESISWrapper(os.path.join(SDK_DIR, get_lib_filename()))
    sdk.load()
    sdk.setup(os.path.join(SDK_DIR, "license.cfg"), pass_key_path)
    return sdk


def load_driver_firmware_database(sdk, db_path):
    # Load the driver/firmware metadata database. This initializes the
    # driver/firmware vmod component; without it DetectDriverFirmwarePatches
    # fails with rc=-5 (WAAPI_ERROR_NOT_INITIALIZED).
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50900
    rc, result = sdk.invoke(50900, input_path=db_path)
    if rc < 0:
        raise Exception(f"LoadDriverFirmwareDatabase failed (rc={rc}): {result}")
    return result.get("result", {})


def detect_driver_firmware_patches(sdk, inventory_file=None):
    # Detect applicable driver/firmware patches by matching system inventory
    # against the loaded driver/firmware database. With no inventory_file the
    # engine collects inventory internally. Omitting components/categories
    # returns all applicable patches.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50902
    params = {}
    if inventory_file:
        params["inventory_file"] = inventory_file

    rc, result = sdk.invoke(50902, **params)
    # MODEL_NOT_SUPPORTED means the catalog has no data for this device model;
    # surface it as a clean "no coverage" outcome rather than a hard failure.
    if rc < 0 and rc != MODEL_NOT_SUPPORTED:
        raise Exception(f"DetectDriverFirmwarePatches failed (rc={rc}): {result}")
    return rc, result.get("result", {})


def format_patch_list(raw_patches):
    # Extract the fields relevant for display from each raw patch record.
    # The first download URL's size is surfaced for the console table; the
    # full download_urls array (with hashes) is preserved for the JSON output.
    patches = []
    for p in raw_patches or []:
        downloads = p.get("download_urls", []) or []
        first     = downloads[0] if downloads else {}
        reboot    = p.get("reboot_required", -1)
        patches.append({
            "patch_id":        p.get("patch_id"),
            "title":           p.get("title",     "Unknown"),
            "component":       p.get("component", "Unknown"),
            "category":        p.get("category",  "Unknown"),
            "severity":        p.get("severity",  "Unknown"),
            "current_version": p.get("current_version", ""),
            "target_version":  p.get("target_version",  ""),
            "reboot_required": reboot,
            "reboot_label":    REBOOT_LABELS.get(reboot, "Unknown"),
            "size":            first.get("size", 0),
            "download_urls":   downloads,
        })
    return patches


def main(inventory_file=None):
    # Accept an optional prebuilt inventory JSON (from collect_device_inventory.py)
    if len(sys.argv) > 1:
        inventory_file = sys.argv[1]
        if not os.path.isfile(inventory_file):
            print(f"Inventory file not found: {inventory_file}")
            return

    # DetectDriverFirmwarePatches is a Windows-only method
    if get_os_type() != OS_TYPE_WINDOWS:
        print("DetectDriverFirmwarePatches (method 50902) is supported on Windows only.")
        return

    if not validate_sdk_environment(SDK_DIR):
        return

    # Locate the driver/firmware database required to initialize the vmod component
    db_path = find_driver_firmware_db()
    if not db_path:
        print(f"ERROR: {DRIVER_FIRMWARE_DB} not found in {SDK_DIR} or under "
              f"OPSWAT-SDK/extract/analog/client.")
        print("       Run the SDK Downloader to populate the driver/firmware database.")
        return

    sdk = None
    try:
        sdk = initialize_framework()

        # Driver/firmware vmod must be initialized before detecting patches
        print(f"Loading driver/firmware database: {os.path.basename(db_path)}")
        load_result = load_driver_firmware_database(sdk, db_path)
        for f in load_result.get("loaded_files", []) or []:
            print(f"  Loaded: {f.get('file_name', '?')}  "
                  f"(version={f.get('version', '?')}, "
                  f"published={f.get('published_epoch', '?')})")

        print(f"\nDetecting applicable driver/firmware patches...")
        rc, detect_result = detect_driver_firmware_patches(sdk, inventory_file)
        detected_vendor   = detect_result.get("detected_vendor", "")
        patches           = sorted(format_patch_list(detect_result.get("patches", [])),
                                   key=lambda p: (p["title"] or "").lower())

        print("-" * 78)
        if detected_vendor:
            print(f"  Detected vendor: {detected_vendor}")

        if rc == MODEL_NOT_SUPPORTED:
            print("  This device model is not covered by the loaded driver/firmware catalog")
            print("  (WA_VMOD_ERROR_MODEL_NOT_SUPPORTED). No patches can be matched -- the")
            print("  catalog currently supports a limited set of vendor models.")
        elif not patches:
            print("  No applicable patches found -- all detected drivers/firmware are current.")
        else:
            # Print a formatted table to the console
            for p in patches:
                title  = (p["title"]           or "Unknown")[:38]
                comp   = (p["component"]        or "Unknown")[:14]
                sev    = (p["severity"]         or "Unknown")[:9]
                cur    = (p["current_version"]  or "?")[:12]
                tgt    = (p["target_version"]   or "?")[:12]
                reboot = (p["reboot_label"]     or "Unknown")[:18]
                print(f"  {title:<38}  {comp:<14}  {sev:<9}  {cur:>12} -> {tgt:<12}  {reboot}")

        print(f"\n  Total: {len(patches)} patch(es) applicable")

        # Write full results to a JSON output file
        output = {
            "status":          "model_not_supported" if rc == MODEL_NOT_SUPPORTED else "ok",
            "detected_vendor": detected_vendor,
            "loaded_files":    load_result.get("loaded_files", []),
            "total":           len(patches),
            "patches":         patches,
        }
        output_file = os.path.join(os.getcwd(), "driver_firmware_patches.json")
        with open(output_file, "w", encoding="utf-8") as f:
            json.dump(output, f, indent=2, default=str)
        print(f"\n  Full results written to: {output_file}")

    except Exception as e:
        print(f"Received an Exception: {e}")
    finally:
        if sdk:
            try:
                sdk.teardown()
            except SDKError:
                pass


if __name__ == "__main__":
    main()
