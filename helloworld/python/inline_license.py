#!/usr/bin/env python3
###############################################################################################
##  Sample Code for InlineLicense
##  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
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


def initialize_framework():
    # Initialize the SDK using both pass_key.txt and inline license bytes from license.cfg
    # This approach embeds the license directly in the config rather than relying on a
    # license file on disk at runtime.
    # https://software.opswat.com/OESIS_V4/html/c_sdk.html
    pass_key_path = os.path.join(SDK_DIR, "pass_key.txt")
    license_path  = os.path.join(SDK_DIR, "license.cfg")

    if not os.path.isfile(pass_key_path):
        print("Could not find pass_key.txt. Make sure the license is in the sdk directory.")
        raise Exception("License pass_key.txt file not found")

        sdk = OESISWrapper(os.path.join(SDK_DIR, get_lib_filename()))
    sdk.load()
    sdk.setup(license_path, pass_key_path)
    return sdk


def main():
    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    try:
        sdk = initialize_framework()
        print("Success")
    except Exception as e:
        print(f"Failed Initialization: {e}")
    finally:
        if sdk:
            try:
                sdk.teardown()
            except SDKError:
                pass


if __name__ == "__main__":
    main()
