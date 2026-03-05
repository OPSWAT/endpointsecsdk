#!/usr/bin/env python3
###############################################################################################
##  Sample Code for Detect Products
##  Reference Implementation using OESIS Framework
##
##  Detects all installed applications on the endpoint using DetectProducts
##  and writes the results to a JSON output file.
##
##  Usage:
##      python detect_products.py              # detects all categories
##      python detect_products.py <category>   # e.g. python detect_products.py 5 (Antimalware)
##
##  Category constants:
##      0  = All
##      1  = Public File Sharing
##      2  = Backup
##      3  = Disk Encryption
##      4  = Antiphishing
##      5  = Antimalware
##      6  = Browser
##      7  = Firewall
##      8  = Instant Messenger
##      9  = Cloud Storage
##      10 = Unclassified
##      11 = Data Loss Prevention
##      12 = Patch Management
##      13 = VPN Client
##      14 = Virtual Machine
##      15 = Health Agent
##      16 = Remote Control
##      17 = Peer to Peer
##      18 = Web Conference
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

# Human-readable category names for console output
CATEGORY_NAMES = {
    0:  "All",
    1:  "Public File Sharing",
    2:  "Backup",
    3:  "Disk Encryption",
    4:  "Antiphishing",
    5:  "Antimalware",
    6:  "Browser",
    7:  "Firewall",
    8:  "Instant Messenger",
    9:  "Cloud Storage",
    10: "Unclassified",
    11: "Data Loss Prevention",
    12: "Patch Management",
    13: "VPN Client",
    14: "Virtual Machine",
    15: "Health Agent",
    16: "Remote Control",
    17: "Peer to Peer",
    18: "Web Conference",
}


def initialize_framework():
    # Load the SDK and initialize with the pass_key.txt in the sdk directory
    # https://software.opswat.com/OESIS_V4/html/c_sdk.html
    pass_key_path = os.path.join(SDK_DIR, "pass_key.txt")
    sdk = OESISWrapper(os.path.join(SDK_DIR, "libwaapi.dll"))
    sdk.load()
    sdk.setup(os.path.join(SDK_DIR, "license.cfg"), pass_key_path)
    return sdk


def detect_products(sdk, category=0):
    # Detect all installed products for the given category.
    # category=0 returns all products across every category.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> OESIS Core/Discover Products
    rc, result = sdk.invoke(0, category=category)
    if rc < 0:
        raise Exception(f"DetectProducts failed (rc={rc}): {result}")
    return result.get("result", {}).get("detected_products", [])


def get_version(sdk, signature_id):
    # Returns the precise installed version string for a product.
    # The version field in DETECT_PRODUCTS is often empty, so we call
    # METHOD_GET_VERSION (method 100) explicitly to get the reliable value.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 100
    rc, result = sdk.invoke(100, signature=signature_id)
    if rc < 0:
        return "Unknown"
    return result.get("result", {}).get("version", "Unknown")


def format_product_list(sdk, raw_products):
    # Extract the fields relevant for display from each raw detection result.
    # get_version is called per product because the version field in the
    # DETECT_PRODUCTS response is unreliable and often empty.
    products = []
    for p in raw_products:
        sig_id = p.get("signature")
        products.append({
            "signature_id": sig_id,
            "name":         p.get("product", {}).get("name",  "Unknown"),
            "vendor":       p.get("vendor",  {}).get("name",  "Unknown"),
            "version":      get_version(sdk, sig_id),
            "category":     p.get("category"),
        })
    return products


def main(category=0):
    # Accept an optional category from the command line
    if len(sys.argv) > 1:
        try:
            category = int(sys.argv[1])
        except ValueError:
            print(f"Invalid category '{sys.argv[1]}' -- must be an integer.")
            print("Usage: python detect_products.py [category]  (default: 0 = All)")
            return

    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    try:
        sdk = initialize_framework()

        category_label = CATEGORY_NAMES.get(category, str(category))
        print(f"\nDetecting products -- Category: {category_label} ({category})")
        print("-" * 60)

        raw_products = detect_products(sdk, category)
        products     = sorted(format_product_list(sdk, raw_products),
                              key=lambda p: p["name"].lower())

        if not products:
            print("  No products detected.")
        else:
            # Print a formatted table to the console
            for p in products:
                name    = p["name"][:40]
                vendor  = p["vendor"][:30]
                version = p["version"][:20]
                print(f"  {name:<40}  {vendor:<30}  {version:<20}  sig={p['signature_id']}")

        print(f"\n  Total: {len(products)} product(s) detected")

        # Write full results to a JSON output file
        output = {
            "category":       category,
            "category_label": category_label,
            "total":          len(products),
            "products":       products,
        }
        output_file = os.path.join(os.getcwd(), f"detected_products_{category}.json")
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