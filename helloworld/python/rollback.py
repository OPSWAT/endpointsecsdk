#!/usr/bin/env python3
###############################################################################################
##  Sample Code for Application Rollback
##  Reference Implementation using OESIS Framework
##
##  Rolls an application back to an EARLIER version that OPSWAT has approved as a
##  rollback target. Rollback is an uninstall-and-reinstall cycle, not an in-place
##  restore -- application data, settings and cache are NOT preserved.
##
##  The flow is:
##      1. Look up the rollback target in the Analog catalog (patch_aggregation_v2.json)
##         and confirm the package is flagged  is_rollback_target = true
##      2. Report the version currently installed on the endpoint
##      3. Download the older installer and verify its SHA-256
##      4. Remove the current version with AppRemover      (method 40000)
##      5. Install the older version with InstallFromFiles (method 50301) using
##         enable_rollback = true  +  requested_version = <target>
##      6. Re-detect and confirm the endpoint is on the requested version
##
##  Usage:
##      python rollback.py [signature_id] [target_version] [--yes]
##
##  Examples:
##      python rollback.py                        # Notepad++ x64  8.9.8 -> 8.9.6.4
##      python rollback.py 3241 8.9.6.4           # the same, stated explicitly
##      python rollback.py 303  8.9.6.4 --yes     # Notepad++ x86, no confirmation prompt
##
##  Only a small number of products currently carry an approved rollback target.
##  Run with --list to print every rollback target in the catalog.
##
##  !! NOTE: enable_rollback requires OESIS 4.3.6607.0 or newer            !!
##  !! NOTE: requires patchv2.dat in the sdk directory -- see              !!
##  !!       load_patch_database(); the v1 database cannot verify          !!
##  !!       requested_version and the install will fail AFTER the         !!
##  !!       current version has already been removed                      !!
##  !! NOTE: This operation requires Administrator / root access           !!
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

# Defaults: Notepad++ 64-bit (signature 3241) rolled back to the approved target
DEFAULT_SIGNATURE = 3241
DEFAULT_VERSION   = "8.9.6.4"

# OESIS method IDs
METHOD_GET_VERSION         = 100
METHOD_GET_PRODUCT_INFO    = 109
METHOD_UNINSTALL           = 40000    # AppRemover
METHOD_INSTALL_FROM_FILES  = 50301
METHOD_LOAD_PATCH_DATABASE = 50302


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
    # Loads the patch DAT file into the SDK so patch/installer queries work offline.
    #
    # Rollback MUST use patchv2.dat. requested_version makes the SDK verify the
    # installer against the version we asked for, and only the v2 database carries
    # checksums for the older builds that are flagged as rollback targets. Loading
    # the v1 pair (patch.dat + ap_checksum.dat) instead lets every earlier step
    # succeed and then fails the install with:
    #     -1052  WA_VMOD_VERSION_LOCK_NOT_SUPPORTED
    # by which point the current version has already been uninstalled.
    #
    # patchv2.dat is not copied by copy_sdk_files.py -- take it from
    # OPSWAT-SDK/extract/analog/client/patchv2.dat
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50302
    patchv2 = os.path.join(SDK_DIR, "patchv2.dat")
    if not os.path.isfile(patchv2):
        raise Exception(
            "patchv2.dat not found in the sdk directory. Rollback needs the v2 "
            "patch database to verify requested_version; copy it from "
            "OPSWAT-SDK/extract/analog/client/patchv2.dat")

    rc, result = sdk.invoke(METHOD_LOAD_PATCH_DATABASE,
                            dat_input_source_file=patchv2)
    if rc < 0:
        raise Exception(f"LoadPatchDatabase failed (rc={rc}): {result}")


# ---------------------------------------------------------------------------
# Analog catalog -- locating an approved rollback target
# ---------------------------------------------------------------------------

def find_catalog():
    """Walk up from this script looking for the Analog server catalog.

    patch_aggregation_v2.json is the server-side catalog produced by the SDK
    downloader; it carries the is_rollback_target flag that tells us which
    older packages OPSWAT has approved as rollback destinations.
    """
    search_root = os.path.dirname(os.path.abspath(__file__))
    for _ in range(4):
        search_root = os.path.dirname(search_root)
        candidate = os.path.join(search_root, "OPSWAT-SDK", "extract",
                                 "analog", "server", "patch_aggregation_v2.json")
        if os.path.isfile(candidate):
            return candidate
    return None


def load_catalog_records(catalog_path):
    """Return the flat list of patch records from patch_aggregation_v2.json."""
    with open(catalog_path, "r", encoding="utf-8") as fh:
        data = json.load(fh)

    for section in data.get("oesis", []):
        records = section.get("patch_aggregation_v2")
        if records is not None:
            return list(records.values()) if isinstance(records, dict) else records
    return []


def find_rollback_package(records, signature_id, target_version):
    """Find the approved rollback package for a signature at a specific version.

    Returns (record, package) or (None, None). A package only qualifies when the
    catalog marks it is_rollback_target = true -- an older version that merely
    exists in the catalog is not an approved rollback destination.
    """
    for record in records:
        if signature_id not in record.get("product", {}).get("v4_signatures", []):
            continue
        if record.get("version") != target_version:
            continue
        for package in record.get("packages", []):
            if package.get("is_rollback_target") is True:
                return record, package
    return None, None


def list_rollback_targets(records):
    """Print every approved rollback target in the catalog."""
    targets = {}
    for record in records:
        for package in record.get("packages", []):
            if package.get("is_rollback_target") is not True:
                continue
            product = record.get("product", {})
            key = (product.get("name", "Unknown"), record.get("version"))
            targets.setdefault(key, set()).update(product.get("v4_signatures", []))

    print(f"\nApproved rollback targets in the catalog: {len(targets)}\n")
    print(f"  {'Product':<44}{'Target version':<22}Signatures")
    print(f"  {'-'*44}{'-'*22}{'-'*24}")
    for (name, version), signatures in sorted(targets.items()):
        sig_list = ", ".join(str(s) for s in sorted(signatures))
        print(f"  {name[:43]:<44}{str(version):<22}{sig_list}")
    print()


# ---------------------------------------------------------------------------
# Endpoint state
# ---------------------------------------------------------------------------

def get_product_info(sdk, signature_id):
    # Fetch product name and vendor so we can show exactly what is being changed.
    # run_detection=True actively scans rather than relying on a cached result.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 109
    rc, result = sdk.invoke(METHOD_GET_PRODUCT_INFO, signature=signature_id,
                            run_detection=True)
    if rc < 0:
        # Non-fatal -- the product may not be installed at all.
        return {}
    return result.get("result", {}).get("detected_product", {})


def get_installed_version(sdk, signature_id):
    # The version field returned by detection is often empty, so call GetVersion
    # explicitly for a reliable value.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 100
    rc, result = sdk.invoke(METHOD_GET_VERSION, signature=signature_id)
    if rc < 0:
        return None
    version = result.get("result", {}).get("version")
    return version or None


# ---------------------------------------------------------------------------
# Rollback steps
# ---------------------------------------------------------------------------

def download_valid_file(url, destination, expected_sha256):
    # Download a file and verify its SHA-256 checksum before returning success.
    # On Windows the Python installer does not import the OS certificate store,
    # so SSL verification frequently fails with "unable to get local issuer
    # certificate". We disable SSL verification and compensate with the
    # SHA-256 checksum check that follows.
    print(f"  Downloading: {url}")

    if sys.platform == "win32":
        print("  Note: SSL certificate verification disabled on Windows "
              "(integrity guaranteed by SHA-256 checksum).")
        ctx = ssl._create_unverified_context()
    else:
        ctx = ssl.create_default_context()

    try:
        with urllib.request.urlopen(url, context=ctx) as response, \
             open(destination, "wb") as out_file:
            out_file.write(response.read())
    except Exception as exc:
        print(f"  Download FAILED: {exc}")
        return False

    print("  Download succeeded")

    sha256 = hashlib.sha256()
    with open(destination, "rb") as f:
        for chunk in iter(lambda: f.read(8192), b""):
            sha256.update(chunk)

    actual = sha256.hexdigest()
    if actual.lower() != expected_sha256.lower():
        # A rollback installs an older build on purpose; a hash mismatch here
        # means the file is not the build the catalog approved, so stop.
        print("=" * 70)
        print("  ERROR: CHECKSUM VERIFICATION FAILED")
        print("    The downloaded file does not match the catalog hash.")
        print(f"    Expected : {expected_sha256.lower()}")
        print(f"    Actual   : {actual.lower()}")
        print("    Rollback aborted -- the endpoint has not been changed.")
        print("=" * 70)
        return False

    print("  Checksum verified successfully")
    return True


def remove_current_version(sdk, signature_id):
    # Remove the installed version with AppRemover before laying down the older
    # build. type="auto" uses the managed removal path, which cleans up leftovers
    # the native uninstaller can leave behind -- important when downgrading,
    # because a newer version's files must not survive into the older install.
    # !! Requires Administrator / root access !!
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 40000
    rc, result = sdk.invoke(METHOD_UNINSTALL, signature=signature_id, type="auto")
    if rc < 0:
        raise Exception(f"AppRemover uninstall failed (rc={rc}): {result}")
    return result.get("result", {})


def install_rollback_version(sdk, signature_id, location, requested_version):
    # Install the older build.
    #
    #   enable_rollback    -- tells the SDK this is a downgrade, so it always runs
    #                         the uninstall step first even when the patch itself
    #                         would not normally require one.
    #   requested_version  -- the version we expect the installer to deliver. The
    #                         SDK halts if the installer does not match it, which
    #                         is what stops a rollback from silently installing
    #                         the wrong build.
    #
    # enable_rollback without requested_version returns WAAPI_ERROR_INVALID_INPUT_ARGS.
    # !! Requires Administrator / root access !!
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50301
    rc, result = sdk.invoke(METHOD_INSTALL_FROM_FILES,
                            signature=signature_id,
                            path=location,
                            skip_signature_check=1,
                            enable_rollback=True,
                            requested_version=requested_version)
    if rc < 0:
        raise Exception(f"InstallFromFiles (rollback) failed (rc={rc}): {result}")
    return result


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

def parse_args(argv):
    """Return (signature_id, target_version, assume_yes, list_only)."""
    args       = [a for a in argv[1:] if not a.startswith("--")]
    flags      = [a for a in argv[1:] if a.startswith("--")]
    assume_yes = "--yes" in flags or "-y" in flags
    list_only  = "--list" in flags

    signature_id   = DEFAULT_SIGNATURE
    target_version = DEFAULT_VERSION

    if len(args) >= 1:
        try:
            signature_id = int(args[0])
        except ValueError:
            print(f"ERROR: Invalid signature ID '{args[0]}' -- must be an integer.")
            print("Usage: python rollback.py [signature_id] [target_version] [--yes]")
            return None, None, None, None
    if len(args) >= 2:
        target_version = args[1]

    return signature_id, target_version, assume_yes, list_only


def main():
    signature_id, target_version, assume_yes, list_only = parse_args(sys.argv)
    if signature_id is None and not list_only:
        return

    # --- Locate the catalog -------------------------------------------------
    catalog_path = find_catalog()
    if not catalog_path:
        print("ERROR: Could not find OPSWAT-SDK/extract/analog/server/patch_aggregation_v2.json")
        print("       Run the SDK downloader to populate the Analog catalog.")
        return

    records = load_catalog_records(catalog_path)
    if not records:
        print(f"ERROR: No patch records found in {catalog_path}")
        return

    if list_only:
        list_rollback_targets(records)
        return

    # --- Confirm the target is an approved rollback destination -------------
    print(f"Rollback")
    print(f"  Catalog   : {catalog_path}")
    print(f"  Signature : {signature_id}")
    print(f"  Target    : {target_version}")

    record, package = find_rollback_package(records, signature_id, target_version)
    if not record:
        print(f"\nERROR: Version {target_version} is not an approved rollback target "
              f"for signature {signature_id}.")
        print("       Run 'python rollback.py --list' to see the approved targets.")
        return

    product_name = record.get("product", {}).get("name", "Unknown")
    url          = package.get("download_links", [None])[0]
    expected_sha = package.get("sha256", "")
    architecture = ", ".join(package.get("architectures", []))

    print(f"\n  Product         : {product_name}")
    print(f"  Rollback to     : {record.get('version')}  ({record.get('release_date')})")
    print(f"  Architecture    : {architecture}")
    print(f"  Approved target : yes  (is_rollback_target = true)")

    if not url:
        print("\nERROR: The catalog entry has no download link.")
        return

    if not validate_sdk_environment(SDK_DIR):
        return

    sdk            = None
    installer_path = None
    download_ok    = False
    try:
        sdk = initialize_framework()
        load_patch_database(sdk)

        # --- Report what is on the endpoint right now -----------------------
        info             = get_product_info(sdk, signature_id)
        detected_name    = info.get("product", {}).get("name") or product_name
        current_version  = get_installed_version(sdk, signature_id)

        print(f"\n  Installed now   : {current_version or '(not detected)'}")

        if current_version is None:
            print(f"\nERROR: {detected_name} is not installed for signature {signature_id}.")
            print("       Rollback replaces an installed version; nothing to roll back.")
            return

        if current_version == target_version:
            print(f"\n  {detected_name} is already on {target_version}. Nothing to do.")
            return

        # --- Confirm ---------------------------------------------------------
        print(f"\n  !! This will REMOVE {detected_name} {current_version} with AppRemover")
        print(f"  !! and install {target_version} in its place.")
        print(f"  !! Application data, settings and cache are NOT preserved.")

        if not assume_yes:
            answer = input("\n  Proceed with rollback? [y/N]: ").strip().lower()
            if answer != "y":
                print("  Rollback cancelled.")
                return

        # --- Step 1: download the approved older installer -------------------
        filename       = url.rsplit("/", 1)[-1].split("?", 1)[0]
        installer_path = os.path.join(os.getcwd(), filename).replace("\\", "/")

        print(f"\n[1/3] Downloading {target_version} installer")
        download_ok = download_valid_file(url, installer_path, expected_sha)
        if not download_ok:
            print("\nRollback aborted -- the endpoint has not been changed.")
            return

        # --- Step 2: remove the current version with AppRemover --------------
        print(f"\n[2/3] Removing {detected_name} {current_version} with AppRemover")
        removal_result = remove_current_version(sdk, signature_id)
        print("  AppRemover completed")
        if removal_result:
            print(json.dumps(removal_result, indent=4, default=str))

        # --- Step 3: install the older version -------------------------------
        print(f"\n[3/3] Installing {target_version} (enable_rollback=true)")
        install_result = install_rollback_version(sdk, signature_id,
                                                  installer_path, target_version)
        print("  Install completed")
        print(json.dumps(install_result, indent=4, default=str))

        # --- Verify ----------------------------------------------------------
        final_version = get_installed_version(sdk, signature_id)
        print(f"\n{'='*70}")
        if final_version == target_version:
            print(f"  ROLLBACK SUCCEEDED")
            print(f"    {detected_name}: {current_version}  ->  {final_version}")
        else:
            print(f"  ROLLBACK DID NOT REACH THE REQUESTED VERSION")
            print(f"    Requested : {target_version}")
            print(f"    Detected  : {final_version or '(not detected)'}")
            print(f"    The SDK does not auto-restore the previous version if a")
            print(f"    rollback step fails -- check the endpoint before retrying.")
        print(f"{'='*70}")

    except Exception as e:
        print(f"\nReceived an Exception: {e}")
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
                    print(f"\nInstaller deleted: {installer_path}")
                except OSError as e:
                    print(f"\nWarning: could not delete installer '{installer_path}': {e}")
            else:
                print(f"\nInstaller kept for inspection (download failed): {installer_path}")


if __name__ == "__main__":
    main()
