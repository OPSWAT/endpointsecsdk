#!/usr/bin/env python3
###############################################################################################
##  VAPM Centralized Assessment — Third-Party Products
##  Reference Implementation using OESIS Framework
##
##  Detects the installed products on the endpoint (DetectProducts, method 0) and then
##  resolves each product's precise version (GetVersion, method 100). The version field in
##  the DetectProducts response is often empty, so GetVersion is called per product for a
##  reliable value. Results are written to a JSON output file.
##
##  Usage:
##      python3 copysdk.py                # stage the SDK + license into ./sdk first
##      python3 scan-ca-third-party.py
##
##  https://software.opswat.com/OESIS_V4/html/c_method.html -> method 0, method 100
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

# Force UTF-8 console output so non-ASCII product names don't crash on Windows (cp1252).
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

# Hardcoded SDK directory relative to this script (populated by copysdk.py)
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SDK_DIR = os.path.join(SCRIPT_DIR, "sdk")


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


def detect_products(sdk, category=0):
    # Detect installed products. category=0 returns all products across every category.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> OESIS Core / Discover Products
    rc, result = sdk.invoke(0, category=category)
    if rc < 0:
        raise Exception(f"DetectProducts failed (rc={rc}): {result}")
    return result.get("result", {}).get("detected_products", [])


def get_version(sdk, signature_id):
    # Returns the precise installed version string for a product. The version field in the
    # DetectProducts response is often empty, so call GetVersion (method 100) explicitly.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 100
    rc, result = sdk.invoke(100, signature=signature_id)
    if rc < 0:
        return "Unknown"
    return result.get("result", {}).get("version", "Unknown")


def main():
    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    try:
        sdk = initialize_framework()

        print("\nVAPM Centralized Assessment — Third-Party Products")
        print("DetectProducts (method 0) + GetVersion (method 100)")
        print("-" * 70)

        raw_products = detect_products(sdk, category=0)

        products = []
        for p in raw_products:
            sig_id = p.get("signature")
            products.append({
                "signature_id": sig_id,
                "name":         p.get("product", {}).get("name", "Unknown"),
                "vendor":       p.get("vendor",  {}).get("name", "Unknown"),
                "version":      get_version(sdk, sig_id),
                "category":     p.get("category"),
            })

        products.sort(key=lambda x: (x["name"] or "").lower())

        if not products:
            print("  No products detected.")
        else:
            for p in products:
                name    = (p["name"]    or "Unknown")[:40]
                vendor  = (p["vendor"]  or "Unknown")[:30]
                version = (p["version"] or "Unknown")[:20]
                print(f"  {name:<40}  {vendor:<30}  {version:<20}  sig={p['signature_id']}")

        print(f"\n  Total: {len(products)} product(s) detected")

        # Write full results to a JSON output file (alongside this script)
        output = {
            "methods":  [0, 100],
            "method_names": ["DetectProducts", "GetVersion"],
            "total":    len(products),
            "products": products,
        }
        output_file = os.path.join(SCRIPT_DIR, "ca_third_party.json")
        with open(output_file, "w", encoding="utf-8") as f:
            json.dump(output, f, indent=2, default=str)
        print(f"  Full results written to: {output_file}")

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
