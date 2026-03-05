#!/usr/bin/env python3
###############################################################################################
##  Sample Code for Patch Status
##  Reference Implementation using OESIS Framework
##
##  Detects all patch management products (category 12) on the endpoint,
##  then queries each one for its missing and installed patches.
##
##  Usage:
##      python patch_status.py
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import json
import os
import sys

from sdk_wrapper import OESISWrapper, SDKError
from platform_utils import validate_sdk_environment

# Hardcoded SDK directory relative to this script
SDK_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "sdk")

# OESIS category for patch management products
CATEGORY_PATCH_MANAGEMENT = 12


def initialize_framework():
    # Load the SDK and initialize with the pass_key.txt in the sdk directory
    # https://software.opswat.com/OESIS_V4/html/c_sdk.html
    pass_key_path = os.path.join(SDK_DIR, "pass_key.txt")
    sdk = OESISWrapper(os.path.join(SDK_DIR, "libwaapi.dll"))
    sdk.load()
    sdk.setup(os.path.join(SDK_DIR, "license.cfg"), pass_key_path)
    return sdk


def detect_patch_management_products(sdk):
    # Detect all products in the patch management category (12)
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> OESIS Core/Discover Products
    rc, result = sdk.invoke(0, category=CATEGORY_PATCH_MANAGEMENT)
    if rc < 0:
        raise Exception(f"DetectProducts failed (rc={rc}): {result}")
    return result.get("result", {}).get("detected_products", [])


def get_missing_patches(sdk, signature_id):
    # Returns the list of patches that are available but not yet installed
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 1013
    # timeout=0              use SDK default timeout
    # retry_internet_services fall back to online lookup if DAT is incomplete
    # mode=0                 standard patch query
    rc, result = sdk.invoke(
        1013,
        signature=signature_id,
        timeout=0,
        retry_internet_services=True,
        mode=0,
    )
    if rc < 0:
        return None
    return result.get("result", {}).get("patches", [])


def get_installed_patches(sdk, signature_id):
    # Returns the list of patches that are already installed on the endpoint
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 1014
    rc, result = sdk.invoke(
        1023,
        signature=signature_id,
        timeout=0,
        retry_internet_services=True,
        mode=0,
    )
    if rc < 0:
        return None
    return result.get("result", {}).get("patches", [])


def print_patch_list(patches, label):
    # Print a summary table of patches with KB ID, title, and severity
    if not patches:
        print(f"    None")
        return
    for patch in patches:
        kb      = patch.get("security_update_id", patch.get("patch_id", "N/A"))
        title   = patch.get("title", "Unknown")[:60]
        severity = patch.get("severity", "")
        severity_str = f"  [{severity}]" if severity else ""
        print(f"    {kb:<20}  {title:<60}{severity_str}")


def main():
    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    all_results = []

    try:
        sdk = initialize_framework()

        print("\nLoading patch database...")

        # Detect all patch management products
        print("\nDetecting patch management products...")
        raw_products = detect_patch_management_products(sdk)

        if not raw_products:
            print("  No patch management products detected on this endpoint.")
            return

        print(f"  Found {len(raw_products)} patch management product(s):\n")

        for product in raw_products:
            sig_id = product.get("signature")
            name   = product.get("product", {}).get("name", "Unknown")
            vendor = product.get("vendor",  {}).get("name", "Unknown")

            print(f"{'=' * 60}")
            print(f"  {name}  ({vendor})")
            print(f"  Signature ID: {sig_id}")
            print(f"{'=' * 60}")

            # --- Missing Patches ---
            print(f"\n  Missing Patches:")
            missing = get_missing_patches(sdk, sig_id)
            if missing is None:
                print("    Not supported for this product.")
            else:
                print(f"    Count: {len(missing)}")
                print()
                print_patch_list(missing, "Missing")

            # --- Installed Patches ---
            print(f"\n  Installed Patches:")
            installed = get_installed_patches(sdk, sig_id)
            if installed is None:
                print("    Not supported for this product.")
            else:
                print(f"    Count: {len(installed)}")
                print()
                print_patch_list(installed, "Installed")

            print()

            all_results.append({
                "signature_id":      sig_id,
                "name":              name,
                "vendor":            vendor,
                "missing_patches":   missing   or [],
                "installed_patches": installed or [],
                "summary": {
                    "missing_count":   len(missing)   if missing   is not None else 0,
                    "installed_count": len(installed) if installed is not None else 0,
                },
            })

        # Overall summary
        total_missing   = sum(r["summary"]["missing_count"]   for r in all_results)
        total_installed = sum(r["summary"]["installed_count"] for r in all_results)
        print(f"{'=' * 60}")
        print(f"  Summary across all patch management products:")
        print(f"    Total missing patches:   {total_missing}")
        print(f"    Total installed patches: {total_installed}")
        print(f"{'=' * 60}")

        # Write full results to JSON
        output = {
            "total_agents":          len(all_results),
            "total_missing_patches": total_missing,
            "total_installed_patches": total_installed,
            "agents": all_results,
        }
        output_file = os.path.join(os.getcwd(), "patch_status.json")
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