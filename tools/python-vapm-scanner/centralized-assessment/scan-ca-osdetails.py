#!/usr/bin/env python3
###############################################################################################
##  VAPM Centralized Assessment — OS Details / Missing Patches
##  Reference Implementation using OESIS Framework
##
##  Detects the patch-management products on the endpoint (category 12) and calls
##  GetMissingPatches (method 1013) for each one, reporting the missing patches /
##  available packages the product knows about. Results are written to a JSON file.
##
##  Usage:
##      python3 copysdk.py             # stage the SDK + license into ./sdk first
##      python3 scan-ca-osdetails.py
##
##  https://software.opswat.com/OESIS_V4/html/c_method.html -> method 1013
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

# Force UTF-8 console output so non-ASCII patch text doesn't crash on Windows (cp1252).
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

# Hardcoded SDK directory relative to this script (populated by copysdk.py)
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SDK_DIR = os.path.join(SCRIPT_DIR, "sdk")

# OESIS category for patch-management products
CATEGORY_PATCH_MANAGEMENT = 12


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


def detect_patch_management_products(sdk):
    # Detect all products in the patch-management category (12)
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> OESIS Core / Discover Products
    rc, result = sdk.invoke(0, category=CATEGORY_PATCH_MANAGEMENT)
    if rc < 0:
        raise Exception(f"DetectProducts failed (rc={rc}): {result}")
    return result.get("result", {}).get("detected_products", [])


def get_missing_patches(sdk, signature_id):
    # Retrieve the missing patches / available packages reported by the product.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 1013
    #   timeout=0                 use the SDK default (infinite) timeout
    #   retry_internet_services   (Windows Update Agent) fall back to internet services
    #   mode=0                    standard query
    rc, result = sdk.invoke(
        1013,
        signature=signature_id,
        timeout=0,
        retry_internet_services=True,
        mode=0,
    )
    if rc < 0:
        return None, rc
    return result.get("result", {}).get("patches", []), rc


def print_patch_list(patches):
    if not patches:
        print("    None")
        return
    for patch in patches:
        kb       = patch.get("security_update_id", patch.get("kb_id", patch.get("id", "N/A")))
        title    = (patch.get("title", "Unknown") or "Unknown")[:60]
        severity = patch.get("severity", "")
        severity_str = f"  [{severity}]" if severity else ""
        print(f"    {str(kb):<20}  {title:<60}{severity_str}")


def main():
    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    all_results = []

    try:
        sdk = initialize_framework()

        print("\nVAPM Centralized Assessment — Missing Patches (GetMissingPatches / 1013)")
        print("=" * 70)

        print("\nDetecting patch-management products (category 12)...")
        raw_products = detect_patch_management_products(sdk)

        if not raw_products:
            print("  No patch-management products detected on this endpoint.")
            return

        print(f"  Found {len(raw_products)} patch-management product(s).")

        for product in raw_products:
            sig_id = product.get("signature")
            name   = product.get("product", {}).get("name", "Unknown")
            vendor = product.get("vendor",  {}).get("name", "Unknown")

            print(f"\n{'=' * 70}")
            print(f"  {name}  ({vendor})    Signature ID: {sig_id}")
            print(f"{'=' * 70}")

            patches, rc = get_missing_patches(sdk, sig_id)
            if patches is None:
                print(f"  GetMissingPatches not supported / failed for this product (rc={rc}).")
                missing = []
            else:
                missing = patches
                print(f"  Missing patches: {len(missing)}")
                print_patch_list(missing)

            all_results.append({
                "signature_id":   sig_id,
                "name":           name,
                "vendor":         vendor,
                "missing_count":  len(missing),
                "missing_patches": missing,
            })

        total_missing = sum(r["missing_count"] for r in all_results)
        print(f"\n{'=' * 70}")
        print(f"  Total missing patches across {len(all_results)} product(s): {total_missing}")
        print(f"{'=' * 70}")

        # Write full results to a JSON output file (alongside this script)
        output = {
            "method":        1013,
            "method_name":   "GetMissingPatches",
            "total_products": len(all_results),
            "total_missing_patches": total_missing,
            "products":      all_results,
        }
        output_file = os.path.join(SCRIPT_DIR, "ca_missing_patches.json")
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
