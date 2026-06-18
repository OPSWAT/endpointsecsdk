#!/usr/bin/env python3
###############################################################################################
##  VAPM Centralized Assessment — OS Details / Patches
##  Reference Implementation using OESIS Framework
##
##  Collects operating-system details and patch status for the endpoint:
##    - GetOSInfo         (method 1)    -> OS name / version / details
##    - GetMissingPatches (method 1013) -> missing patches per patch-management product
##    - GetInstalledPatches (method 1023) -> installed patches per patch-management product
##
##  On Windows the scan is limited to the Windows Update Agent (signature 1103); on
##  Linux/macOS every detected patch-management product (category 12) is assessed.
##  All results are written to scan-ca-osdetails-result.json. The result always carries
##  os_info (os_type/os_id/os_name/os_version) so the mapper can branch per platform, and
##  on Linux it additionally reports the installed 'packages' the Linux CVE mapper needs.
##
##  Usage:
##      python3 copysdk.py             # stage the SDK + license into ./sdk first
##      python3 scan-ca-osdetails.py
##
##  https://software.opswat.com/OESIS_V4/html/c_method.html -> methods 1, 1013, 1023
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
from platform_utils import get_os_type, OS_TYPE_WINDOWS

# OS type identifiers reported by GetOSInfo (match the Analog catalog / ruby samples).
OS_TYPE_LINUX = 2
OS_TYPE_MACOS = 4

# Force UTF-8 console output so non-ASCII patch text doesn't crash on Windows (cp1252).
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

# Hardcoded SDK directory relative to this script (populated by copysdk.py)
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SDK_DIR = os.path.join(SCRIPT_DIR, "sdk")

# OESIS category for patch-management products
CATEGORY_PATCH_MANAGEMENT = 12

# On Windows the OS patch source is the Windows Update Agent (signature 1103). Other
# detected patch-management agents (RMM, Intune, Dell, etc.) do not support
# GetMissingPatches, so on Windows we limit the scan to this product.
WINDOWS_UPDATE_AGENT_SIGNATURE = 1103


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


def get_os_info(sdk):
    # Retrieve operating-system information for the endpoint.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 1 (GetOSInfo)
    rc, result = sdk.invoke(1)
    if rc < 0:
        return None, rc
    return result.get("result", {}), rc


def detect_patch_management_products(sdk):
    # Detect all products in the patch-management category (12)
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> OESIS Core / Discover Products
    rc, result = sdk.invoke(0, category=CATEGORY_PATCH_MANAGEMENT)
    if rc < 0:
        raise Exception(f"DetectProducts failed (rc={rc}): {result}")
    return result.get("result", {}).get("detected_products", [])


def get_missing_patches(sdk, signature_id):
    # Retrieve the missing patches / available packages reported by the product.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 1013
    rc, result = sdk.invoke(
        1013,
        signature=signature_id,
        timeout=0,
        retry_internet_services=True,
        mode=0,
    )
    if rc < 0:
        return None, rc
    return result.get("result", {}).get("patches", []), rc


def get_installed_patches(sdk, signature_id):
    # Retrieve the patches already installed on the endpoint for this product.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 1023
    rc, result = sdk.invoke(
        1023,
        signature=signature_id,
        timeout=0,
        retry_internet_services=True,
        mode=0,
    )
    if rc < 0:
        return None, rc
    return result.get("result", {}).get("patches", []), rc


def print_patch_list(patches):
    if not patches:
        print("    None")
        return
    for patch in patches:
        kb       = patch.get("security_update_id", patch.get("kb_id", patch.get("id", "N/A")))
        title    = (patch.get("title", "Unknown") or "Unknown")[:60]
        severity = patch.get("severity", "")
        severity_str = f"  [{severity}]" if severity else ""
        print(f"    {str(kb):<20}  {title:<60}{severity_str}")


def main():
    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    os_info = {}
    all_results = []

    try:
        sdk = initialize_framework()

        print("\nVAPM Centralized Assessment — OS Details / Patches")
        print("=" * 70)

        # --- OS info (GetOSInfo / method 1) ---
        print("\nOperating system (GetOSInfo / 1):")
        os_info, os_rc = get_os_info(sdk)
        if os_info is None:
            print(f"  GetOSInfo failed (rc={os_rc}).")
            os_info = {}
        else:
            print(f"  Name        : {os_info.get('os_name', os_info.get('name', 'Unknown'))}")
            print(f"  Version     : {os_info.get('os_version', os_info.get('version', 'Unknown'))}")
            print(f"  Architecture: {os_info.get('architecture', 'Unknown')}")

        # --- Patch-management products ---
        print("\nDetecting patch-management products (category 12)...")
        raw_products = detect_patch_management_products(sdk)

        # On Windows, limit to the Windows Update Agent (signature 1103). On Linux/macOS,
        # assess every detected patch-management product.
        if get_os_type() == OS_TYPE_WINDOWS:
            raw_products = [
                p for p in raw_products
                if p.get("signature") == WINDOWS_UPDATE_AGENT_SIGNATURE
            ]
            print(f"  Windows: limiting to the Windows Update Agent (signature "
                  f"{WINDOWS_UPDATE_AGENT_SIGNATURE}).")

        if not raw_products:
            print("  No patch-management products detected on this endpoint.")
        else:
            print(f"  Found {len(raw_products)} patch-management product(s).")

        for product in raw_products:
            sig_id = product.get("signature")
            name   = product.get("product", {}).get("name", "Unknown")
            vendor = product.get("vendor",  {}).get("name", "Unknown")

            print(f"\n{'=' * 70}")
            print(f"  {name}  ({vendor})    Signature ID: {sig_id}")
            print(f"{'=' * 70}")

            # Missing patches (1013)
            missing, m_rc = get_missing_patches(sdk, sig_id)
            if missing is None:
                print(f"  Missing patches: not supported / failed (rc={m_rc}).")
                missing = []
            else:
                print(f"  Missing patches: {len(missing)}")
                print_patch_list(missing)

            # Installed patches (1023)
            installed, i_rc = get_installed_patches(sdk, sig_id)
            if installed is None:
                print(f"  Installed patches: not supported / failed (rc={i_rc}).")
                installed = []
            else:
                print(f"  Installed patches: {len(installed)}")
                print_patch_list(installed)

            all_results.append({
                "signature_id":     sig_id,
                "name":             name,
                "vendor":           vendor,
                "missing_count":    len(missing),
                "installed_count":  len(installed),
                "missing_patches":  missing,
                "installed_patches": installed,
            })

        total_missing   = sum(r["missing_count"]   for r in all_results)
        total_installed = sum(r["installed_count"] for r in all_results)
        print(f"\n{'=' * 70}")
        print(f"  Products assessed : {len(all_results)}")
        print(f"  Total missing     : {total_missing}")
        print(f"  Total installed   : {total_installed}")
        print(f"{'=' * 70}")

        # On Linux the system-vulnerability check is package/version based (not KB based),
        # so surface the installed packages the mapper's detect_linux_cves needs. Each
        # installed "patch" reported by the Linux package-management product represents an
        # installed package; normalize to {package_name, package_version}.
        packages = []
        if os_info.get("os_type") == OS_TYPE_LINUX:
            seen = set()
            for r in all_results:
                for patch in r.get("installed_patches", []):
                    name = patch.get("package_name") or patch.get("name") or patch.get("title")
                    version = patch.get("package_version") or patch.get("version")
                    if name and version and (name, version) not in seen:
                        seen.add((name, version))
                        packages.append({"package_name": name, "package_version": str(version)})
            print(f"\n  Linux packages reported for vulnerability mapping: {len(packages)}")

        # Write the full result set to JSON
        output = {
            "os_info":  os_info,
            "methods":  {"os_info": 1, "missing_patches": 1013, "installed_patches": 1023},
            "total_products":          len(all_results),
            "total_missing_patches":   total_missing,
            "total_installed_patches": total_installed,
            "products": all_results,
            "packages": packages,
        }
        output_file = os.path.join(SCRIPT_DIR, "scan-ca-osdetails-result.json")
        with open(output_file, "w", encoding="utf-8") as f:
            json.dump(output, f, indent=2, default=str)
        print(f"\n  Full results written to: {output_file}")

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
