#!/usr/bin/env python3
###############################################################################################
##  Sample Code for Show Packages
##  Reference Implementation using OESIS Framework
##
##  Read-only. Lists the installable packages the SDK knows about for a product, and
##  shows which of them apply to THIS endpoint. Nothing is downloaded or installed and
##  no Administrator rights are needed.
##
##  For every patch version it prints two things side by side:
##
##      SDK  (GetPackages, method 50306, from patchv2.dat)
##           package_uuid, version, architectures, languages, sha256 and the SDK's
##           evaluation_status for this endpoint: applicable / not_applicable / not_evaluated
##
##      Catalog  (patch_aggregation_v2.json, server side)
##           is_latest and is_rollback_target -- neither is exposed by the SDK
##
##  The split is deliberate: the SDK can resolve any patch_uuid it is given, but it has
##  no call that enumerates a product's versions or says which one is an approved
##  rollback target. That information only exists in the server catalog, so a product
##  needs both sources to offer "install this version" or "roll back".
##
##  Usage:
##      python show_packages.py --signature <id>            all catalog versions
##      python show_packages.py --signature <id> --latest   only the current version
##      python show_packages.py <patch_uuid>                one patch, SDK only
##
##  Examples:
##      python show_packages.py --signature 3241            # Notepad++ x64, every version
##      python show_packages.py eeeaba57-c17b-570d-8e64-f66295cfd570
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

# OESIS method IDs
METHOD_GET_VERSION         = 100
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
    # GetPackages reads patchv2.dat; the v1 patch.dat has no package UUIDs.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50302
    patchv2 = os.path.join(SDK_DIR, "patchv2.dat")
    if not os.path.isfile(patchv2):
        raise Exception(
            "patchv2.dat not found in the sdk directory. Copy it from "
            "OPSWAT-SDK/extract/analog/client/patchv2.dat")

    rc, result = sdk.invoke(METHOD_LOAD_PATCH_DATABASE, dat_input_source_file=patchv2)
    if rc < 0:
        raise Exception(f"LoadPatchDatabase failed (rc={rc}): {result}")


# ---------------------------------------------------------------------------
# Analog catalog -- the version list and the flags the SDK does not expose
# ---------------------------------------------------------------------------

def find_catalog():
    """Walk up from this script looking for the Analog server catalog."""
    search_root = os.path.dirname(os.path.abspath(__file__))
    for _ in range(4):
        search_root = os.path.dirname(search_root)
        candidate = os.path.join(search_root, "OPSWAT-SDK", "extract",
                                 "analog", "server", "patch_aggregation_v2.json")
        if os.path.isfile(candidate):
            return candidate
    return None


def catalog_records_for_signature(signature_id):
    """Return the catalog's patch records for a signature, oldest first.

    Each record carries patch_uuid, version, release_date, is_latest and, per
    package, is_rollback_target.
    """
    catalog_path = find_catalog()
    if not catalog_path:
        raise Exception("Could not find OPSWAT-SDK/extract/analog/server/"
                        "patch_aggregation_v2.json -- run the SDK downloader, or "
                        "pass a patch_uuid directly.")

    with open(catalog_path, "r", encoding="utf-8") as fh:
        data = json.load(fh)

    records = []
    for section in data.get("oesis", []):
        found = section.get("patch_aggregation_v2")
        if found is not None:
            records = list(found.values()) if isinstance(found, dict) else found
            break

    mine = [r for r in records
            if signature_id in r.get("product", {}).get("v4_signatures", [])]

    def release_key(record):
        # release_date is MM/DD/YYYY; sort by (year, month, day)
        parts = (record.get("release_date") or "").split("/")
        return (parts[2], parts[0], parts[1]) if len(parts) == 3 else ("", "", "")

    mine.sort(key=release_key)
    return mine


# ---------------------------------------------------------------------------
# SDK
# ---------------------------------------------------------------------------

def get_packages(sdk, patch_uuid):
    # Every package linked to the patch, each evaluated against this endpoint.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50306
    rc, result = sdk.invoke(METHOD_GET_PACKAGES, patch_uuid=patch_uuid,
                            package_limit="all")
    if rc < 0:
        err = result.get("error", {})
        raise Exception(f"GetPackages failed (rc={rc}) {err.get('define', '')}")
    return result.get("result", {})


def get_installed_version(sdk, signature_id):
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 100
    rc, result = sdk.invoke(METHOD_GET_VERSION, signature=signature_id)
    if rc < 0:
        return None
    return result.get("result", {}).get("version") or None


# ---------------------------------------------------------------------------
# Output
# ---------------------------------------------------------------------------

def print_patch(sdk, patch_uuid, catalog_record=None, installed_version=None):
    """Print one patch: SDK packages plus the catalog flags, if we have them."""
    info     = get_packages(sdk, patch_uuid)
    version  = info.get("version")
    packages = info.get("packages", [])

    tags = []
    if catalog_record:
        if catalog_record.get("is_latest"):
            tags.append("LATEST")
        if any(p.get("is_rollback_target") for p in catalog_record.get("packages", [])):
            tags.append("ROLLBACK TARGET")
    if installed_version and version == installed_version:
        tags.append("INSTALLED")

    released = f"  released {catalog_record['release_date']}" if catalog_record else ""
    print(f"\n  {version}{released}   {'  '.join(tags)}")
    print(f"    patch_uuid : {patch_uuid}")

    if not packages:
        print("    (no packages)")
        return

    # Catalog rollback flags are per package_uuid; build a lookup
    rollback_by_pkg = {}
    if catalog_record:
        for p in catalog_record.get("packages", []):
            rollback_by_pkg[p.get("package_uuid")] = bool(p.get("is_rollback_target"))

    for p in packages:
        arch   = ", ".join(p.get("architectures", [])) or "-"
        langs  = ", ".join(p.get("languages", [])) or "-"
        status = p.get("evaluation_status", "?")
        sha    = (p.get("sha256") or "")[:16]
        flag   = ""
        if p.get("package_uuid") in rollback_by_pkg:
            flag = "  rollback=yes" if rollback_by_pkg[p["package_uuid"]] else "  rollback=no"
        print(f"    {p.get('package_uuid')}  {arch:<8} {langs:<6} {status:<15}"
              f" sha256={sha or '-':<17}{flag}")
        for link in p.get("download_links", []):
            print(f"        {link}")


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

USAGE = """Usage:
  python show_packages.py --signature <id>            all catalog versions
  python show_packages.py --signature <id> --latest   only the current version
  python show_packages.py <patch_uuid>                one patch, SDK only"""


def parse_args(argv):
    opts = {"patch_uuid": None, "signature": None, "latest_only": False}
    args = argv[1:]
    i = 0
    while i < len(args):
        a = args[i]
        if a == "--latest":
            opts["latest_only"] = True
        elif a == "--signature" and i + 1 < len(args):
            i += 1
            try:
                opts["signature"] = int(args[i])
            except ValueError:
                print(f"ERROR: Invalid signature ID '{args[i]}' -- must be an integer.")
                return None
        elif not a.startswith("--"):
            opts["patch_uuid"] = a
        else:
            print(f"ERROR: Unknown option '{a}'")
            return None
        i += 1

    if not opts["patch_uuid"] and opts["signature"] is None:
        print(USAGE)
        return None
    return opts


def main():
    opts = parse_args(sys.argv)
    if not opts:
        return

    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    try:
        sdk = initialize_framework()
        load_patch_database(sdk)

        # --- One patch_uuid: pure SDK, no catalog ---------------------------
        if opts["patch_uuid"]:
            print(f"Show Packages  (SDK only -- no catalog flags)")
            print_patch(sdk, opts["patch_uuid"])
            print()
            return

        # --- A signature: catalog gives the version list ---------------------
        signature_id = opts["signature"]
        records = catalog_records_for_signature(signature_id)
        if not records:
            print(f"No catalog records for signature {signature_id}.")
            return
        if opts["latest_only"]:
            records = [r for r in records if r.get("is_latest")]

        product_name = records[0].get("product", {}).get("name", "Unknown")
        installed    = get_installed_version(sdk, signature_id)

        print(f"Show Packages")
        print(f"  Product   : {product_name}")
        print(f"  Signature : {signature_id}")
        print(f"  Installed : {installed or '(not detected)'}")
        print(f"  Versions  : {len(records)} in catalog")

        for record in records:
            print_patch(sdk, record["patch_uuid"], record, installed)

        rollback_versions = sorted({r["version"] for r in records
                                    if any(p.get("is_rollback_target")
                                           for p in r.get("packages", []))})
        print(f"\n  Approved rollback targets: "
              f"{', '.join(rollback_versions) if rollback_versions else 'none'}")
        print()

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
