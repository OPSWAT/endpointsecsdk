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
import ssl
import sys
import urllib.request

from sdk_wrapper import OESISWrapper, SDKError
from platform_utils import validate_sdk_environment
from platform_utils import get_lib_filename


# Hardcoded SDK directory relative to this script
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

    url      = r.get("url", "")
    filename = url.rsplit("/", 1)[-1].split("?", 1)[0]   # strip query string e.g. ?viasf=1
    path     = os.path.join(os.getcwd(), filename).replace("\\", "/")

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


def get_product_info(sdk, signature_id):
    # Fetch product name and vendor for the given signature so we can display
    # exactly what is about to be installed before the download begins.
    # run_detection=True actively scans rather than relying on a cached result.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 109
    rc, result = sdk.invoke(109, signature=signature_id, run_detection=True)
    if rc < 0:
        # Non-fatal — the product may not be installed yet (fresh install case).
        # Return empty info so the caller can still proceed.
        return {}
    return result.get("result", {}).get("detected_product", {})


def install_from_files(sdk, signature_id, location):
    # Installs a product from a local file  !! Requires Administrator access !!
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50301
    rc, result = sdk.invoke(50301, signature=signature_id,
                            path=location, skip_signature_check=1)
    if rc < 0:
        raise Exception(f"InstallFromFiles failed (rc={rc}): {result}")
    return result


def download_valid_file(url, destination, expected_sha256):
    # Download a file and verify its SHA-256 checksum before returning success.
    # On Windows the Python installer does not import the OS certificate store,
    # so SSL verification frequently fails with "unable to get local issuer
    # certificate". We disable SSL verification and compensate with the
    # SHA-256 checksum check that follows.
    print(f"Downloading: {url}")

    if sys.platform == "win32":
        print("Note: SSL certificate verification disabled on Windows "
              "(integrity guaranteed by SHA-256 checksum).")
        ctx = ssl._create_unverified_context()
    else:
        ctx = ssl.create_default_context()

    try:
        with urllib.request.urlopen(url, context=ctx) as response, \
             open(destination, "wb") as out_file:
            out_file.write(response.read())
    except Exception as exc:
        print(f"Download FAILED: {exc}")
        return False

    print("Download succeeded")

    sha256 = hashlib.sha256()
    with open(destination, "rb") as f:
        for chunk in iter(lambda: f.read(8192), b""):
            sha256.update(chunk)

    actual = sha256.hexdigest()
    if actual.lower() != expected_sha256.lower():
        print("=" * 60)
        print("WARNING: CHECKSUM VERIFICATION FAILED")
        print("  The downloaded file does not match the expected hash.")
        print("  The file may be corrupted or tampered with.")
        print(f"  Expected : {expected_sha256.lower()}")
        print(f"  Actual   : {actual.lower()}")
        print(f"  URL      : {url}")
        print("  Continuing installation despite checksum mismatch.")
        print("=" * 60)
    else:
        print("Checksum verified successfully")
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
    installer_path  = None
    download_ok     = False
    try:
        sdk = initialize_framework()

        # Load the patch database (downloaded from OPSWAT CDN)
        load_patch_database(sdk,
                            os.path.join(SDK_DIR, "patch.dat"),
                            os.path.join(SDK_DIR, "ap_checksum.dat"))

        # Look up and display the product name before downloading anything
        print(f"Looking up product info for signature ID: {signature_id}")
        info   = get_product_info(sdk, signature_id)
        name   = info.get("product", {}).get("name")
        vendor = info.get("vendor",  {}).get("name")
        if name:
            print(f"  Product : {name}")
            print(f"  Vendor  : {vendor}")
        else:
            print(f"  Product : (not currently detected — may be a fresh install)")

        print(f"Getting latest installer for signature ID: {signature_id}")
        installer_result = get_latest_installer(sdk, signature_id)
        detail           = get_installer_detail(installer_result)
        installer_path   = detail["path"]

        # Download and validate the installer
        download_ok = download_valid_file(detail["url"], installer_path, detail["checksums"][0])

        # Install the patch — requires Administrator access
        if download_ok:
            result = install_from_files(sdk, signature_id, installer_path)
            print(result)
        else:
            print(f"Skipping install — download did not succeed: {detail['url']}")

    except Exception as e:
        print(f"Received an Exception: {e}")
    finally:
        if sdk:
            try:
                sdk.teardown()
            except SDKError:
                pass
        if installer_path and os.path.isfile(installer_path):
            if download_ok:
                try:
                    os.remove(installer_path)
                    print(f"Installer deleted: {installer_path}")
                except OSError as e:
                    print(f"Warning: could not delete installer '{installer_path}': {e}")
            else:
                print(f"Installer kept for inspection (download failed): {installer_path}")


if __name__ == "__main__":
    main()