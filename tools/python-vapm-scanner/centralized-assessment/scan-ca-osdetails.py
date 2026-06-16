#!/usr/bin/env python3
###############################################################################################
##  VAPM Centralized Assessment — OS Details Scan  (STUB)
##  Reference Implementation using OESIS Framework
##
##  Centralized-assessment scan that collects operating-system details (name, version,
##  build, architecture) for the endpoint, used to scope OS-level patch and vulnerability
##  assessment. This is an initial stub: it initializes the SDK and outlines the workflow.
##
##  Usage:
##      python3 copysdk.py             # stage the SDK + license into ./sdk first
##      python3 scan-ca-osdetails.py
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import os
import sys

from sdk_wrapper import OESISWrapper, SDKError
from platform_utils import validate_sdk_environment
from platform_utils import get_lib_filename

# Force UTF-8 console output so non-ASCII text doesn't crash on Windows (cp1252).
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

# Hardcoded SDK directory relative to this script (populated by copysdk.py)
SDK_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "sdk")


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


def scan_os_details(sdk):
    # TODO: implement the OS-details scan.
    # Planned flow: query the SDK for OS information (name, version, build, architecture)
    # to scope OS-level patch and vulnerability assessment.
    # https://software.opswat.com/OESIS_V4/html/c_method.html
    print("  [OS details scan] not yet implemented (stub)")
    return {}


def main():
    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    try:
        sdk = initialize_framework()

        print("\nVAPM Centralized Assessment — OS Details Scan (stub)")
        print("-" * 60)
        scan_os_details(sdk)
        print("\nScan complete (stub — no findings produced yet).")

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
