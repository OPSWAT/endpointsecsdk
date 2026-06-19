#!/usr/bin/env python3
###############################################################################################
##  VAPM Endpoint Assessment — OS Details / Vulnerabilities (live SDK scan)
##  Reference Implementation using the OESIS Framework
##
##  Live, agent-style OS assessment of THIS endpoint. Like helloworld/python/os_vulnerability.py
##  it loads the Windows OS patch database (wuov2.dat) and vulnerability database
##  (wiv-lite.dat) and queries the OS component for vulnerabilities. It also gathers OS info
##  and missing patches so the output matches the centralized mapper's result shape
##  (map-ca-osdetails-result.json), making the endpoint and centralized assessments
##  directly comparable.
##
##  Usage:
##      python3 copysdk.py                 # stage the SDK + license into ./sdk first
##      python3 scan-ea-osdetails.py [signature_id]   # default OS signature 1103
##
##  Writes: scan-ea-osdetails-result.json  (same schema as map-ca-osdetails-result.json)
##
##  SDK methods: GetOSInfo (1), LoadPatchDatabase (50302), ConsumeOfflineVmodDatabase (50520),
##               GetLatestInstaller (50300), GetProductVulnerability (50505)
##  https://software.opswat.com/OESIS_V4/html/c_method.html
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import json
import os
import sys
from datetime import datetime, timezone

from sdk_wrapper import OESISWrapper, SDKError
from platform_utils import validate_sdk_environment
from platform_utils import get_lib_filename

# Force UTF-8 console output so non-ASCII CVE text doesn't crash on Windows (cp1252).
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SDK_DIR    = os.path.join(SCRIPT_DIR, "sdk")

WUO_DAT      = os.path.join(SDK_DIR, "wuov2.dat")     # Windows OS patch database
WIV_LITE_DAT = os.path.join(SDK_DIR, "wiv-lite.dat")  # Windows OS vulnerability database

# Default OS component signature (Windows Update Agent), matching os_vulnerability.py.
DEFAULT_OS_SIGNATURE = 1103

OUTPUT_FILE = os.path.join(SCRIPT_DIR, "scan-ea-osdetails-result.json")


def initialize_framework():
    pass_key_path = os.path.join(SDK_DIR, "pass_key.txt")
    if not os.path.isfile(pass_key_path):
        print("Could not find pass_key.txt. Make sure the license is in the sdk directory.")
        raise Exception("License pass_key.txt file not found")
    sdk = OESISWrapper(os.path.join(SDK_DIR, get_lib_filename()))
    sdk.load()
    sdk.setup(os.path.join(SDK_DIR, "license.cfg"), pass_key_path)
    return sdk


def get_os_info(sdk):
    # GetOSInfo (method 1)
    rc, result = sdk.invoke(1)
    return (result.get("result", {}) if rc >= 0 else {})


def load_patch_database(sdk, database_file):
    # LoadPatchDatabase (method 50302) — Windows OS patch data. Returns the SDK's load
    # result (carries version / published_epoch / loaded_files for the patch database).
    rc, result = sdk.invoke(50302, dat_input_source_file=database_file)
    if rc < 0:
        raise Exception(f"LoadPatchDatabase failed (rc={rc}): {result}")
    print(f"  Loaded {os.path.basename(database_file)}")
    return result.get("result", {})


def consume_offline_vmod_database(sdk, database_file):
    # ConsumeOfflineVmodDatabase (method 50520) — Windows OS vulnerability data. Returns the
    # SDK's load result (version / published_epoch / details: number_of_cves / number_of_kbs).
    rc, result = sdk.invoke(50520, dat_input_source_file=database_file)
    if rc < 0:
        raise Exception(f"ConsumeOfflineVmodDatabase failed (rc={rc}): {result}")
    print(f"  Loaded {os.path.basename(database_file)}")
    return result.get("result", {})


def summarize_loaded_db(label, file_path, method, result):
    # Capture the loaded-database provenance the SDK reports (so it travels with the result
    # instead of being buried in encrypted debug logs). published_epoch is the catalog data's
    # own definition time; it is also rendered as a readable UTC timestamp.
    result = result or {}
    epoch = result.get("published_epoch")
    published_utc = None
    try:
        published_utc = datetime.fromtimestamp(int(epoch), tz=timezone.utc).strftime(
            "%Y-%m-%d %H:%M:%S UTC")
    except (TypeError, ValueError):
        pass
    file_size = os.path.getsize(file_path) if os.path.isfile(file_path) else None
    return {
        "label":           label,
        "file":            os.path.basename(file_path),
        "method":          method,
        "file_size_bytes": file_size,
        "version":         result.get("version"),
        "schema_version":  result.get("schema_version"),
        "published_epoch": epoch,
        "published_utc":   published_utc,
        "details":         result.get("details"),
        "loaded_files":    result.get("loaded_files"),
    }


def get_latest_installer(sdk, signature_id):
    # GetLatestInstaller (method 50300) — the latest available patch/installer for the product.
    rc, result = sdk.invoke(50300, signature=signature_id)
    if rc < 0:
        return None
    return result.get("result", {})


def get_os_vulnerabilities(sdk, signature_id):
    # GetProductVulnerability (method 50505) against the loaded wiv-lite.dat.
    rc, result = sdk.invoke(50505, signature=signature_id)
    if rc < 0:
        return []
    return result.get("result", {}).get("cves", []) or []


def normalize_cve(raw, fixed_by_kbs):
    # Normalize an OS CVE record into the same shape used by map-ca-osdetails-result.json,
    # tolerating the various field names the SDK may use.
    cve_id = raw.get("cve") or raw.get("id") or raw.get("static_id")
    details = raw.get("details") or {}
    cvss = details.get("cvss_3_0") or details.get("cvss_3_1") or details.get("cvss_2_0") or {}
    cpes = []
    for entry in details.get("cpe", []) or []:
        cpe = entry.get("cpe_2_3") or entry.get("cpe_2_2") or entry.get("cpe")
        if cpe:
            cpes.append(cpe)
    return {
        "cve":             str(cve_id) if cve_id is not None else None,
        "cwe":             raw.get("cwe"),
        "published_epoch": raw.get("published_epoch"),
        "severity":        raw.get("severity") or cvss.get("base_severity"),
        "score":           cvss.get("base_score"),
        "cpes":            cpes,
        "fixed_by_kbs":    fixed_by_kbs,
    }


def main(signature_id=DEFAULT_OS_SIGNATURE):
    if len(sys.argv) > 1:
        try:
            signature_id = int(sys.argv[1])
        except ValueError:
            print(f"Invalid signature ID '{sys.argv[1]}' -- must be an integer.")
            return

    if not validate_sdk_environment(SDK_DIR):
        return
    for dat in (WUO_DAT, WIV_LITE_DAT):
        if not os.path.isfile(dat):
            print(f"ERROR: required database not found: {dat}")
            print("       Run 'python copysdk.py' to stage the SDK data files.")
            return

    sdk = None
    try:
        sdk = initialize_framework()

        print("\nVAPM Endpoint Assessment — OS Details / Vulnerabilities (live scan)")
        print("=" * 70)

        os_info = get_os_info(sdk)
        print(f"  OS: {os_info.get('name', 'Unknown')} ({os_info.get('version', '')}), "
              f"os_id={os_info.get('os_id')}")

        print("\nLoading OS databases...")
        patch_db_result = load_patch_database(sdk, WUO_DAT)
        vmod_db_result  = consume_offline_vmod_database(sdk, WIV_LITE_DAT)

        # Capture the loaded-database details (version / published date / record counts) so
        # the result is self-documenting and the DB provenance need not be recovered from the
        # encrypted SDK debug logs.
        databases = [
            summarize_loaded_db("OS patch database (LoadPatchDatabase)",
                                WUO_DAT, 50302, patch_db_result),
            summarize_loaded_db("OS vulnerability database (ConsumeOfflineVmodDatabase)",
                                WIV_LITE_DAT, 50520, vmod_db_result),
        ]
        print("\nLoaded database details:")
        for db in databases:
            print(f"  {db['file']:<14} v{db['version']}  "
                  f"published={db['published_utc'] or db['published_epoch'] or 'unknown'}  "
                  f"details={db['details']}")

        print(f"\nQuerying OS component signature {signature_id}...")
        installer = get_latest_installer(sdk, signature_id)
        raw_cves  = get_os_vulnerabilities(sdk, signature_id)

        # KB / identifier for the latest available OS installer (GetLatestInstaller).
        installer_kb = None
        if installer:
            installer_kb = str(installer.get("patch_id") or installer.get("analog_id") or "N/A")
        kbs = [installer_kb] if installer_kb else []

        # CVEs found for the OS component are remediated by the latest OS patch.
        cves = [normalize_cve(c, kbs) for c in raw_cves]
        cves = [c for c in cves if c["cve"]]
        cve_ids = sorted({c["cve"] for c in cves})

        # Patch entry derived from GetLatestInstaller (kept under the same schema key as
        # map-ca-osdetails-result.json for comparability).
        missing_patches = []
        if installer:
            missing_patches.append({
                "kb":        installer_kb,
                "title":     installer.get("title", "Unknown"),
                "severity":  installer.get("severity"),
                "version":   installer.get("minimum_version") or installer.get("version"),
                "product":   os_info.get("name", "Windows OS"),
                "cve_count": len(cve_ids),
                "cves":      cve_ids,
            })

        print(f"\n  Latest installer (GetLatestInstaller): {len(missing_patches)}")
        for mp in missing_patches:
            print(f"    {str(mp['kb']):<12} {(mp['title'] or 'Unknown')[:50]}  (target {mp.get('version')})")
        print(f"  OS CVEs (GetProductVulnerability / wiv-lite.dat): {len(cve_ids)}")

        # Output schema matches map-ca-osdetails-result.json. The live SDK scan already
        # reflects installed state, so raw == net and covered == 0.
        output = {
            "os_info": {
                "name":    os_info.get("name"),
                "version": os_info.get("version"),
                "os_id":   os_info.get("os_id"),
                "os_type": os_info.get("os_type"),
            },
            "source": "OESIS live OS scan (wiv-lite.dat / wuov2.dat)",
            "signature": signature_id,
            "databases":                       databases,
            "total_missing_patches":           len(missing_patches),
            "total_cves_raw":                  len(cve_ids),
            "total_cves_covered_by_installed": 0,
            "total_cves":                      len(cve_ids),
            "missing_patches":                 missing_patches,
            "cves":                            cves,
        }
        with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
            json.dump(output, f, indent=2, default=str)
        print(f"\n  Full results written to: {OUTPUT_FILE}")

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
