#!/usr/bin/env python3
###############################################################################################
##  Sample Code for Product Detail
##  Reference Implementation using OESIS Framework
##
##  Dumps patch status, vulnerability data, version, and product info
##  for a given signature ID.
##
##  Usage:
##      python product_detail.py              # defaults to Firefox (3039)
##      python product_detail.py <signature>  # e.g. python product_detail.py 4046
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

# Default signature ID — Firefox
DEFAULT_SIGNATURE_ID = 3039


def initialize_framework():
    # Load the SDK and initialize with the pass_key.txt in the sdk directory
    # https://software.opswat.com/OESIS_V4/html/c_sdk.html
    pass_key_path = os.path.join(SDK_DIR, "pass_key.txt")
    sdk = OESISWrapper(os.path.join(SDK_DIR, "libwaapi.dll"))
    sdk.load()
    sdk.setup(os.path.join(SDK_DIR, "license.cfg"), pass_key_path)
    return sdk


def load_databases(sdk):
    # Load vulnerability and patch databases before querying product data
    v2mod_path    = os.path.join(SDK_DIR, "v2mod.dat")
    patch_path    = os.path.join(SDK_DIR, "patch.dat")
    checksum_path = os.path.join(SDK_DIR, "ap_checksum.dat")

    if os.path.isfile(v2mod_path):
        sdk.invoke(50520, dat_input_source_file=v2mod_path)

    if os.path.isfile(patch_path):
        params = {"dat_input_source_file": patch_path}
        if os.path.isfile(checksum_path):
            params["dat_input_checksum_file"] = checksum_path
        sdk.invoke(50302, **params)


def get_product_info(sdk, signature_id):
    # Returns basic product detection info -- name, vendor, version, category.
    # run_detection=True tells the SDK to actively scan for the product rather
    # than relying on a previously cached detection result.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 3
    rc, result = sdk.invoke(109, signature=signature_id, run_detection=True)
    if rc < 0:
        print(f"  ERROR: The application associated with signature ID {signature_id} could not be found.")
        print(f"  Verify the signature ID is correct and that the product is installed on this endpoint.")
        sys.exit(1)
    return result.get("result", {}).get("detected_product", {})


def get_version(sdk, signature_id):
    # Returns the precise installed version string for the product
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 100
    rc, result = sdk.invoke(100, signature=signature_id)
    if rc < 0:
        return None
    return result.get("result", {})


def get_vulnerability(sdk, signature_id):
    # Returns CVE vulnerability data for the installed version
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50505
    rc, result = sdk.invoke(50505, signature=signature_id)
    if rc < 0:
        return None
    return result.get("result", {})


def get_patch_status(sdk, signature_id):
    # Returns the latest available installer/patch info for the product
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50300
    rc, result = sdk.invoke(50300, signature=signature_id)
    if rc < 0:
        return None
    return result.get("result", {})


def print_section(title, data):
    print(f"\n{'=' * 60}")
    print(f"  {title}")
    print(f"{'=' * 60}")
    if data:
        print(json.dumps(data, indent=2, default=str))
    else:
        print("  No data returned.")


def main(signature_id=DEFAULT_SIGNATURE_ID):
    # Accept signature ID from the command line if provided
    if len(sys.argv) > 1:
        try:
            signature_id = int(sys.argv[1])
        except ValueError:
            print(f"Invalid signature ID '{sys.argv[1]}' -- must be an integer.")
            print(f"Usage: python product_detail.py [signature_id]  (default: {DEFAULT_SIGNATURE_ID} = Firefox)")
            return

    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    try:
        sdk = initialize_framework()
        load_databases(sdk)

        print(f"\nProduct Detail Report — Signature ID: {signature_id}")

        # --- Product Info ---
        product_info = get_product_info(sdk, signature_id)
        print_section("Product Info", product_info)

        # --- Version ---
        version = get_version(sdk, signature_id)
        print_section("Version", version)

        # --- Vulnerability ---
        vulnerability = get_vulnerability(sdk, signature_id)
        print_section("Vulnerability", vulnerability)

        # --- Patch Status ---
        patch_status = get_patch_status(sdk, signature_id)
        print_section("Patch Status", patch_status)

        # --- Combined JSON output file ---
        output = {
            "signature_id": signature_id,
            "product_info":  product_info,
            "version":       version,
            "vulnerability": vulnerability,
            "patch_status":  patch_status,
        }
        output_file = os.path.join(os.getcwd(), f"product_detail_{signature_id}.json")
        with open(output_file, "w", encoding="utf-8") as f:
            json.dump(output, f, indent=2, default=str)
        print(f"\nFull detail written to: {output_file}")

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
