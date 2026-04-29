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
from platform_utils import get_lib_filename


# Hardcoded SDK directory relative to this script
SDK_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "sdk")


def initialize_framework():
    # Load the SDK and initialize with the pass_key.txt in the sdk directory
    # https://software.opswat.com/OESIS_V4/html/c_sdk.html
    pass_key_path = os.path.join(SDK_DIR, "pass_key.txt")
    
    sdk = OESISWrapper(os.path.join(SDK_DIR, get_lib_filename()))
    sdk.load()
    sdk.setup(os.path.join(SDK_DIR, "license.cfg"), pass_key_path)
    return sdk


def get_report_info(sdk):
    # Fetch product name and vendor before uninstalling so we can confirm
    # to the user exactly what is being removed.
    # run_detection=True actively scans rather than using a cached result.
    rc, result = sdk.invoke(130002, framework_id="pci-dss", framework_version="4.0.1")
    if rc < 0:
        print(f"Result: {rc}")
        print(f"  ERROR: The application associated with signature ID {signature_id} could not be found.")
        print(f"  Verify the signature ID is correct and that the product is installed on this endpoint.")
        print(f"  Use detect_products.py to list all installed products and their signature IDs.")
        sys.exit(1)
    return result.get("result", {}).get("detected_product", {})


def main():

    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    try:
        sdk = initialize_framework()

        # Confirm what product is about to be removed before proceeding
        print(f"Starting Report")
        info   = get_report_info(sdk)
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
