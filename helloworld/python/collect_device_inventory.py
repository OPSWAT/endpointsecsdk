#!/usr/bin/env python3
###############################################################################################
##  Sample Code for Collect Device Inventory  (Windows only)
##  Reference Implementation using OESIS Framework
##
##  Collects endpoint inventory data (system, OS, BIOS, and hardware devices) used for
##  driver/firmware patch matching via CollectDeviceInventory (method 50901). Prints a
##  summary to the console and writes the full inventory to a JSON output file.
##
##  Usage:
##      python collect_device_inventory.py                 # writes device_inventory.json
##      python collect_device_inventory.py <output_file>   # custom output path
##
##  CollectDeviceInventory returns inventory_collection grouped into:
##      system   -- vendor, system_family, system_product_name, system_sku, machine_type
##      os       -- os_name, os_version, architecture, build
##      bios     -- bios_vendor, bios_version, release_date
##      devices  -- list of detected hardware devices (driver/firmware metadata)
##
##  https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50901
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
    # driver/firmware vmod component; without it CollectDeviceInventory and
    # DetectDriverFirmwarePatches fail with rc=-5 (WAAPI_ERROR_NOT_INITIALIZED).
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50900
    rc, result = sdk.invoke(50900, input_path=db_path)
    if rc < 0:
        raise Exception(f"LoadDriverFirmwareDatabase failed (rc={rc}): {result}")
    return result.get("result", {})


def collect_device_inventory(sdk, output_file):
    # Collect endpoint inventory and export it to output_file.
    # output_file accepts absolute or relative paths; if omitted entirely the
    # inventory is only returned in the response and not persisted to disk.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50901
    rc, result = sdk.invoke(50901, output_file=output_file)
    if rc < 0:
        raise Exception(f"CollectDeviceInventory failed (rc={rc}): {result}")
    return result.get("result", {})


def print_inventory_report(result):
    # Print a human-readable summary of the collected inventory.
    inv     = result.get("inventory_collection", {})
    system  = inv.get("system", {})
    os_info = inv.get("os", {})
    bios    = inv.get("bios", {})
    devices = inv.get("devices", [])

    print(f"\n  Device Inventory")
    print("-" * 60)

    print("  System")
    print(f"    Vendor        : {system.get('vendor', 'Unknown')}")
    print(f"    Family        : {system.get('system_family', 'Unknown')}")
    print(f"    Product Name  : {system.get('system_product_name', 'Unknown')}")
    print(f"    SKU           : {system.get('system_sku', 'Unknown')}")
    print(f"    Machine Type  : {system.get('machine_type', 'Unknown')}")

    print("\n  Operating System")
    print(f"    Name          : {os_info.get('os_name', 'Unknown')}")
    print(f"    Version       : {os_info.get('os_version', 'Unknown')}")
    print(f"    Architecture  : {os_info.get('architecture', 'Unknown')}")
    print(f"    Build         : {os_info.get('build', 'Unknown')}")

    print("\n  BIOS")
    print(f"    Vendor        : {bios.get('bios_vendor', 'Unknown')}")
    print(f"    Version       : {bios.get('bios_version', 'Unknown')}")
    print(f"    Release Date  : {bios.get('release_date', 'Unknown')}")

    # Summarise devices: total count plus a count per setup class
    print(f"\n  Devices: {len(devices)} detected")
    if devices:
        class_counts = {}
        for d in devices:
            cls = d.get("class", "Unknown") or "Unknown"
            class_counts[cls] = class_counts.get(cls, 0) + 1

        print("-" * 60)
        for cls in sorted(class_counts):
            print(f"    {cls:<24}  {class_counts[cls]}")


def main(output_file=None):
    # Accept an optional output path; default to device_inventory.json in cwd
    if len(sys.argv) > 1:
        output_file = sys.argv[1]
    if not output_file:
        output_file = os.path.join(os.getcwd(), "device_inventory.json")

    # CollectDeviceInventory is a Windows-only method
    if get_os_type() != OS_TYPE_WINDOWS:
        print("CollectDeviceInventory (method 50901) is supported on Windows only.")
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

        # Driver/firmware vmod must be initialized before collecting inventory
        print(f"Loading driver/firmware database: {os.path.basename(db_path)}")
        load_driver_firmware_database(sdk, db_path)

        print(f"\nCollecting device inventory...")
        result = collect_device_inventory(sdk, output_file)

        print_inventory_report(result)

        # The SDK writes the inventory to output_file; echo where it landed.
        inventory_file = result.get("inventory_file", output_file)
        print(f"\n  Inventory exported by SDK to: {inventory_file}")

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
