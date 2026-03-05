#!/usr/bin/env python3
###############################################################################################
##  Sample Code for Patch
##  Reference Implementation using OESIS Framework
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import hashlib
import json
import os
import sys
import urllib.request

from sdk_wrapper import OESISWrapper, SDKError
from platform_utils import validate_sdk_environment

# Hardcoded SDK directory relative to this script
SDK_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "sdk")


def initialize_framework():
    # Load the SDK and initialize with the pass_key.txt in the sdk directory
    # https://software.opswat.com/OESIS_V4/html/c_sdk.html
    pass_key_path = os.path.join(SDK_DIR, "pass_key.txt")

    if not os.path.isfile(pass_key_path):
        print("Could not find pass_key.txt. Make sure the license is in the sdk directory.")
        raise Exception("License pass_key.txt file not found")

    sdk = OESISWrapper(os.path.join(SDK_DIR, "libwaapi.dll"))
    sdk.load()
    sdk.setup(os.path.join(SDK_DIR, "license.cfg"), pass_key_path)
    return sdk


def load_patch_database(sdk, database_file, checksum_file=None):
    # Loads the patch DAT file into the SDK so patch/installer queries work offline
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50302
    params = {"dat_input_source_file": database_file}
    if checksum_file:
        params["dat_input_checksum_file"] = checksum_file

    rc, result = sdk.invoke(50302, **params)
    if rc < 0:
        raise Exception(f"LoadPatchDatabase failed (rc={rc}): {result}")


def get_latest_installer(sdk, signature_id):
    # Returns download URL and metadata for the newest installer of a product
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50300
    rc, result = sdk.invoke(50300, signature=signature_id)
    if rc < 0:
        raise Exception(f"GetLatestInstaller failed (rc={rc}): {result}")
    return result


def get_installer_detail(installer_result):
    # Parse the installer result into a flat dict matching InstallDetails.cs
    r = installer_result.get("result", {})
    if not r:
        return {"result_code": installer_result.get("error", {}).get("code")}

    url  = r.get("url", "")
    path = os.path.join(os.getcwd(), url.rsplit("/", 1)[-1]).replace("\\", "/")

    return {
        "result_code":        r.get("code"),
        "url":                url,
        "file_type":          r.get("file_type"),
        "title":              r.get("title"),
        "severity":           r.get("severity"),
        "security_update_id": r.get("security_update_id"),
        "category":           r.get("category"),
        "patch_id":           r.get("patch_id"),
        "language":           r.get("language"),
        "checksums":          r.get("expected_sha256", []),
        "path":               path,
    }


def install_from_files(sdk, signature_id, location):
    # Installs a product from a local file  !! Requires Administrator access !!
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50301
    rc, result = sdk.invoke(50301, signature=signature_id,
                            path=location, skip_signature_check=1)
    if rc < 0:
        raise Exception(f"InstallFromFiles failed (rc={rc}): {result}")
    return result


def download_valid_file(url, destination, expected_sha256):
    # Download a file and verify its SHA-256 checksum before returning success
    print(f"Downloading: {url}")
    urllib.request.urlretrieve(url, destination)

    sha256 = hashlib.sha256()
    with open(destination, "rb") as f:
        for chunk in iter(lambda: f.read(8192), b""):
            sha256.update(chunk)

    actual = sha256.hexdigest()
    if actual.lower() != expected_sha256.lower():
        print(f"Checksum mismatch — expected {expected_sha256}, got {actual}")
        return False

    print("Download verified successfully")
    return True


def main(signature_id=3039):
    # signature_id defaults to Firefox (3039). Pass a different value on the
    # command line to patch a different product:
    #     python patch.py 3039
    if len(sys.argv) > 1:
        try:
            signature_id = int(sys.argv[1])
        except ValueError:
            print(f"Invalid signature ID '{sys.argv[1]}' -- must be an integer.")
            print("Usage: python patch.py [signature_id]  (default: 4046 = Firefox)")
            return

    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    try:
        sdk = initialize_framework()

        # Load the patch database (downloaded from OPSWAT CDN)
        load_patch_database(sdk,
                            os.path.join(SDK_DIR, "patch.dat"),
                            os.path.join(SDK_DIR, "ap_checksum.dat"))

        print(f"Getting latest installer for signature ID: {signature_id}")
        installer_result = get_latest_installer(sdk, signature_id)
        detail           = get_installer_detail(installer_result)

        # Download and validate the installer
        download_ok = download_valid_file(detail["url"], detail["path"], detail["checksums"][0])

        # Install the patch — requires Administrator access
        if download_ok:
            result = install_from_files(sdk, signature_id, detail["path"])
            print(result)
        else:
            print(f"Failed to download installer: {detail['url']}")

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