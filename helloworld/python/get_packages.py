#!/usr/bin/env python3
###############################################################################################
##  Sample Code for GetPackages
##  Reference Implementation using OESIS Framework
##
##  The smallest possible use of GetPackages (method 50306): load patchv2.dat, hand the
##  SDK one patch_uuid, print what comes back. Nothing is downloaded or installed and no
##  Administrator rights are needed.
##
##  GetPackages returns the installable packages linked to a patch -- one per
##  architecture / language combination -- and evaluates each one against THIS endpoint:
##
##      evaluation_status   applicable      the SDK would install this package here
##                          not_applicable  wrong architecture, language, OS ...
##                          not_evaluated   the SDK could not decide
##
##  It works for ANY patch_uuid in the database, not only the latest version. That is
##  what makes it the building block for "install a specific version" and rollback --
##  see install_package.py and rollback.py. It is also the first place a package's
##  SHA-256 and download URL are exposed by the SDK.
##
##  The one thing it needs is the patch_uuid, and the SDK has no call that turns a
##  signature into one. show_packages.py shows how to get patch_uuids from the Analog
##  catalog (patch_aggregation_v2.json).
##
##  Usage:
##      python get_packages.py [patch_uuid] [--first]
##
##  Examples:
##      python get_packages.py                                          # Notepad++ 8.9.8
##      python get_packages.py a2191f8b-66b0-5d11-961b-7dc0a3c21dff     # Notepad++ 8.9.6.4
##      python get_packages.py eeeaba57-c17b-570d-8e64-f66295cfd570 --first
##
##  !! NOTE: requires patchv2.dat in the sdk directory                     !!
##  !!       (copy from OPSWAT-SDK/extract/analog/client/patchv2.dat)      !!
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

# Default: Notepad++ x64 8.9.8 (the latest at the time of writing)
DEFAULT_PATCH_UUID = "eeeaba57-c17b-570d-8e64-f66295cfd570"

# OESIS method IDs
METHOD_LOAD_PATCH_DATABASE = 50302
METHOD_GET_PACKAGES        = 50306


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


def load_patch_database(sdk):
    # GetPackages reads patchv2.dat. The v1 patch.dat has no package UUIDs and
    # will not work for this method.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50302
    patchv2 = os.path.join(SDK_DIR, "patchv2.dat")
    if not os.path.isfile(patchv2):
        raise Exception(
            "patchv2.dat not found in the sdk directory. Copy it from "
            "OPSWAT-SDK/extract/analog/client/patchv2.dat")

    rc, result = sdk.invoke(METHOD_LOAD_PATCH_DATABASE, dat_input_source_file=patchv2)
    if rc < 0:
        raise Exception(f"LoadPatchDatabase failed (rc={rc}): {result}")


def get_packages(sdk, patch_uuid, package_limit="all"):
    # The call itself. package_limit is "first" (default in the SDK) or "all".
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50306
    rc, result = sdk.invoke(METHOD_GET_PACKAGES, patch_uuid=patch_uuid,
                            package_limit=package_limit)
    if rc < 0:
        err = result.get("error", {})
        raise Exception(f"GetPackages failed (rc={rc}) {err.get('define', '')}: "
                        f"{err.get('description', '')}")
    return result.get("result", {})


def main():
    args          = [a for a in sys.argv[1:] if not a.startswith("--")]
    package_limit = "first" if "--first" in sys.argv else "all"
    patch_uuid    = args[0] if args else DEFAULT_PATCH_UUID

    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    try:
        sdk = initialize_framework()
        load_patch_database(sdk)

        print(f"GetPackages")
        print(f"  patch_uuid    : {patch_uuid}")
        print(f"  package_limit : {package_limit}")

        result   = get_packages(sdk, patch_uuid, package_limit)
        packages = result.get("packages", [])

        # A short summary first ...
        print(f"\n  Patch version : {result.get('version')}")
        print(f"  Packages      : {len(packages)}")
        for p in packages:
            arch  = ", ".join(p.get("architectures", [])) or "-"
            langs = ", ".join(p.get("languages", [])) or "-"
            print(f"\n    package_uuid      : {p.get('package_uuid')}")
            print(f"    architectures     : {arch}")
            print(f"    languages         : {langs}")
            print(f"    evaluation_status : {p.get('evaluation_status')}")
            print(f"    sha256            : {p.get('sha256') or '(none -- self-updating product)'}")
            for link in p.get("download_links", []):
                print(f"    download          : {link}")

        # ... then the raw response, so the exact shape is visible
        print(f"\n  Raw response:")
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
