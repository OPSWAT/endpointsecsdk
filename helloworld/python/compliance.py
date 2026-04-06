#!/usr/bin/env python3
###############################################################################################
##  Sample Code for Compliance
##  Reference Implementation using OESIS Framework
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import json
import os
import sys

import platform_utils
from sdk_wrapper import OESISWrapper, SDKError
from platform_utils import validate_sdk_environment
from platform_utils import get_lib_filename


# Hardcoded SDK directory relative to this script
SDK_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "sdk")


def initialize_framework():
    # Load the SDK and initialize with the pass_key.txt in the same directory
    # https://software.opswat.com/OESIS_V4/html/c_sdk.html
    pass_key_path = os.path.join(SDK_DIR, "pass_key.txt")

    if not os.path.isfile(pass_key_path):
        print("Could not find pass_key.txt. Make sure the license is in the sdk directory.")
        raise Exception("License pass_key.txt file not found")

        sdk = OESISWrapper(os.path.join(SDK_DIR, get_lib_filename()))
    sdk.load()
    sdk.setup(os.path.join(SDK_DIR, "license.cfg"), pass_key_path)
    return sdk


def detect_products(sdk, category):
    # Returns all detected products for the given category
    # https://software.opswat.com/OESIS_V4/html/c_method.html
    # Category 7 = Firewall
    rc, result = sdk.invoke(0, category=category)
    return rc, result


def get_product_list(detect_result):
    # Parse detected_products from the SDK result into a simple list
    products = detect_result.get("result", {}).get("detected_products", [])
    return [
        {
            "signature_id": p.get("signature"),
            "name":         p.get("product", {}).get("name", "Unknown"),
            "vendor":       p.get("vendor",  {}).get("name", "Unknown"),
        }
        for p in products
    ]


def is_firewall_running(sdk, signature_id):
    # Returns whether the firewall product is currently enabled
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> Manageability/GetFirewallState
    rc, result = sdk.invoke(1007, signature=signature_id)
    if rc >= 0:
        return result.get("result", {}).get("enabled", False)
    return False


def main():
    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    try:
        sdk = initialize_framework()

        # Detect firewall products (category 7)
        print("Discovering Firewall Products")
        rc, detect_result = detect_products(sdk, category=7)
        if rc < 0:
            raise Exception(f"DetectProducts failed (rc={rc})")

        product_list = get_product_list(detect_result)
        for product in product_list:
            running = is_firewall_running(sdk, product["signature_id"])
            print(f"Found: {product['name']}  Running: {running}")

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
