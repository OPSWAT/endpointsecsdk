#!/usr/bin/env python3
###############################################################################################
##  Sample Code for Discover Products
##  Reference Implementation using OESIS Framework
##
##  Discovers installed products on the endpoint using the OESIS Discover Products method.
##
##  This sample intentionally uses Discover Products (method 100001), not DetectProducts
##  (method 0). Discover Products is intended to enumerate products in a way that is
##  similar to how the operating system discovers installed software: by using the
##  endpoint's native inventory sources and returning a normalized OESIS product list.
##
##  Use this when you want a broad installed-product inventory from the endpoint rather
##  than category-specific OESIS product detection.
##
##  Documentation:
##      https://software.opswat.com/OESIS_V4/html/c_method.html
##
##  Usage:
##      python discover_products.py
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import json
import os

from sdk_wrapper import OESISWrapper, SDKError
from platform_utils import validate_sdk_environment
from platform_utils import get_lib_filename


# Hardcoded SDK directory relative to this script
SDK_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "sdk")

# OESIS Core / Discover Products
METHOD_DISCOVER_PRODUCTS = 100001


def initialize_framework():
    # Load the SDK and initialize with the pass_key.txt in the sdk directory
    # https://software.opswat.com/OESIS_V4/html/c_sdk.html
    pass_key_path = os.path.join(SDK_DIR, "pass_key.txt")

    sdk = OESISWrapper(os.path.join(SDK_DIR, get_lib_filename()))
    sdk.load()
    sdk.setup(os.path.join(SDK_DIR, "license.cfg"), pass_key_path)
    return sdk


def discover_products(sdk):
    """
    Discover installed products using OESIS Core / Discover Products.

    This method is designed for OS-style installed-product discovery. It lets
    OESIS enumerate products similarly to how the operating system would build
    a software inventory, then normalizes the result into OESIS product data.

    OESIS method:
        100001 = Discover Products

    Reference:
        https://software.opswat.com/OESIS_V4/html/c_method.html
    """
    rc, result = sdk.invoke(METHOD_DISCOVER_PRODUCTS)
    if rc < 0:
        raise Exception(f"DiscoverProducts failed (rc={rc}): {result}")
    return result


def get_discovered_product_list(discover_result):
    """Parse discovered products from the SDK response into a simple list."""
    result = discover_result.get("result", {})

    # Prefer the expected discovered_products field, but keep a fallback for
    # SDK builds that return detected_products for compatibility.
    products = result.get("discovered_products") or result.get("detected_products") or []

    product_list = []
    for p in products:
        product_list.append({
            "signature_id": p.get("signature") or p.get("signature_id"),
            "name":         p.get("product", {}).get("name", p.get("name", "Unknown")),
            "vendor":       p.get("vendor", {}).get("name", p.get("vendor", "Unknown")),
            "version":      p.get("version", "Unknown"),
            "category":     p.get("category"),
        })

    return product_list


def main():
    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    try:
        sdk = initialize_framework()

        print("\nDiscovering installed products using OS-style product inventory...")
        print("OESIS Method: Discover Products (100001)")
        print("-" * 70)

        discover_result = discover_products(sdk)
        products = sorted(
            get_discovered_product_list(discover_result),
            key=lambda p: (p["name"] or "").lower(),
        )

        if not products:
            print("  No products discovered.")
        else:
            for p in products:
                name = str(p["name"])[:40]
                vendor = str(p["vendor"])[:30]
                version = str(p["version"])[:20]
                sig = p["signature_id"]
                print(f"  {name:<40}  {vendor:<30}  {version:<20}  sig={sig}")

        print(f"\n  Total: {len(products)} product(s) discovered")

        output = {
            "method": METHOD_DISCOVER_PRODUCTS,
            "method_name": "Discover Products",
            "description": "OS-style installed-product discovery using OESIS.",
            "total": len(products),
            "products": products,
            "raw_result": discover_result,
        }

        output_file = os.path.join(os.getcwd(), "discovered_products.json")
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
