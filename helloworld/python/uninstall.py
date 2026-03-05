#!/usr/bin/env python3
###############################################################################################
##  Sample Code for Uninstall Product
##  Reference Implementation using OESIS Framework
##
##  Uninstalls a product from the endpoint using its OESIS signature ID.
##  A signature ID is required -- use detect_products.py to find the
##  signature ID for any installed product.
##
##  Usage:
##      python uninstall_product.py <signature_id>
##
##  Example:
##      python uninstall_product.py 3039    # Uninstall Firefox
##
##  !! NOTE: This operation requires Administrator / root access !!
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


def initialize_framework():
    # Load the SDK and initialize with the pass_key.txt in the sdk directory
    # https://software.opswat.com/OESIS_V4/html/c_sdk.html
    pass_key_path = os.path.join(SDK_DIR, "pass_key.txt")
    sdk = OESISWrapper(os.path.join(SDK_DIR, "libwaapi.dll"))
    sdk.load()
    sdk.setup(os.path.join(SDK_DIR, "license.cfg"), pass_key_path)
    return sdk


def get_product_info(sdk, signature_id):
    # Fetch product name and vendor before uninstalling so we can confirm
    # to the user exactly what is being removed.
    # run_detection=True actively scans rather than using a cached result.
    rc, result = sdk.invoke(109, signature=signature_id, run_detection=True)
    if rc < 0:
        print(f"  ERROR: The application associated with signature ID {signature_id} could not be found.")
        print(f"  Verify the signature ID is correct and that the product is installed on this endpoint.")
        print(f"  Use detect_products.py to list all installed products and their signature IDs.")
        sys.exit(1)
    return result.get("result", {}).get("detected_product", {})


def uninstall_product(sdk, signature_id):
    # Uninstall the product identified by signature_id.
    # !! Requires Administrator / root access !!
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50303
    rc, result = sdk.invoke(40000, signature=signature_id, type="auto")
    if rc < 0:
        raise Exception(f"Uninstall failed (rc={rc}): {result}")
    return result.get("result", {})


def main():
    # A signature ID is mandatory — print usage and exit if not provided
    if len(sys.argv) < 2:
        print("ERROR: A signature ID is required.")
        print()
        print("Usage:   python uninstall_product.py <signature_id>")
        print("Example: python uninstall_product.py 3039")
        print()
        print("Run detect_products.py to list all installed products and their signature IDs.")
        return

    try:
        signature_id = int(sys.argv[1])
    except ValueError:
        print(f"ERROR: Invalid signature ID '{sys.argv[1]}' -- must be an integer.")
        print()
        print("Usage:   python uninstall_product.py <signature_id>")
        print("Example: python uninstall_product.py 3039")
        return

    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    try:
        sdk = initialize_framework()

        # Confirm what product is about to be removed before proceeding
        print(f"\nLooking up product for signature ID: {signature_id}...")
        info   = get_product_info(sdk, signature_id)
        name   = info.get("product", {}).get("name", "Unknown")
        vendor = info.get("vendor",  {}).get("name", "Unknown")

        print(f"\n  Product : {name}")
        print(f"  Vendor  : {vendor}")
        print(f"  Sig ID  : {signature_id}")
        print(f"\n  !! This will uninstall {name} from this endpoint !!")

        # Require explicit confirmation before uninstalling
        answer = input("\n  Proceed with uninstall? [y/N]: ").strip().lower()
        if answer != "y":
            print("  Uninstall cancelled.")
            return

        print(f"\n  Uninstalling {name}...")
        result = uninstall_product(sdk, signature_id)

        print(f"  Uninstall completed successfully.")
        print(f"\n  Result:")
        print(json.dumps(result, indent=4, default=str))

    except Exception as e:
        print(f"\nReceived an Exception: {e}")
    finally:
        if sdk:
            try:
                sdk.teardown()
            except SDKError:
                pass


if __name__ == "__main__":
    main()
