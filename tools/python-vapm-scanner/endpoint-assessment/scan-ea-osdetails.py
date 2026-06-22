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
import re
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


def kbs_for_cve(raw):
    # Extract the KB number(s) that remediate THIS CVE from the SDK's resolution data.
    # GetProductVulnerability returns details.resolution[] with:
    #   "text": "Install KB##### to fix this vulnerability"   and an optional
    #   "patches": [ { kb_id / patch_id, ... } ]
    # Returns bare KB numbers (e.g. "5094126"), matching the centralized mapper's convention.
    details = raw.get("details") or {}
    kbs = set()
    for res in details.get("resolution", []) or []:
        for m in re.findall(r"KB(\d+)", res.get("text", "") or "", flags=re.IGNORECASE):
            kbs.add(m)
        for patch in res.get("patches", []) or []:
            val = patch.get("kb_id") or patch.get("patch_id") or patch.get("id")
            val = str(val).upper().replace("KB", "").strip() if val is not None else ""
            if val.isdigit():
                kbs.add(val)
    return sorted(kbs)


def normalize_cve(raw, fallback_kbs):
    # Normalize an OS CVE record into the same shape used by map-ca-osdetails-result.json,
    # tolerating the various field names the SDK may use. fixed_by_kbs is the KB(s) the SDK
    # says fix THIS CVE (from its resolution); fallback is used only when none are provided.
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
        "cwe":             raw.get("cwe") or details.get("cwe"),
        "published_epoch": raw.get("published_epoch") or details.get("published_epoch"),
        "severity":        raw.get("severity") or cvss.get("base_severity"),
        "score":           cvss.get("base_score"),
        "cpes":            cpes,
        "fixed_by_kbs":    kbs_for_cve(raw) or list(fallback_kbs),
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

        # The latest OS installer (GetLatestInstaller) is the cumulative update that brings
        # the OS current. Keep it for reference, but DO NOT stamp its id onto every CVE --
        # each CVE's true remediating KB comes from GetProductVulnerability's resolution.
        latest_installer = None
        if installer:
            latest_installer = {
                "id":      str(installer.get("patch_id") or installer.get("analog_id") or "N/A"),
                "title":   installer.get("title", "Unknown"),
                "version": installer.get("minimum_version") or installer.get("version"),
            }

        cves = [normalize_cve(c, []) for c in raw_cves]
        cves = [c for c in cves if c["cve"]]
        cve_ids = sorted({c["cve"] for c in cves})

        # Group CVEs by the KB that remediates them -- the correct OS-patch -> CVE mapping.
        kb_to_cves = {}
        for c in cves:
            for kb in c["fixed_by_kbs"]:
                kb_to_cves.setdefault(kb, set()).add(c["cve"])
        unmapped = sorted({c["cve"] for c in cves if not c["fixed_by_kbs"]})

        installer_version = latest_installer["version"] if latest_installer else None
        missing_patches = []
        for kb in sorted(kb_to_cves):
            missing_patches.append({
                "kb":        kb,
                "title":     f"Security update KB{kb} for {os_info.get('name', 'Windows OS')}",
                "severity":  None,
                "version":   installer_version,
                "product":   os_info.get("name", "Windows OS"),
                "cve_count": len(kb_to_cves[kb]),
                "cves":      sorted(kb_to_cves[kb]),
            })

        print(f"\n  OS CVEs (GetProductVulnerability): {len(cve_ids)}")
        print(f"  Mapped to {len(missing_patches)} remediating KB(s):")
        for mp in missing_patches:
            print(f"    KB{mp['kb']:<10} -> {mp['cve_count']} CVE(s)")
        if unmapped:
            print(f"  {len(unmapped)} CVE(s) had no KB in the SDK resolution: {unmapped}")
        if latest_installer:
            print(f"  Latest installer (brings OS current): {latest_installer['id']}  "
                  f"{(latest_installer['title'] or '')[:50]}  (target {latest_installer['version']})")

        # Output schema matches map-ca-osdetails-result.json. The live SDK scan already
        # reflects installed state, so raw == net and covered == 0.
        output = {
            "os_info": {
                "name":    os_info.get("name"),
                "version": os_info.get("version"),
                "os_id":   os_info.get("os_id"),
                "os_type": os_info.get("os_type"),
            },
            "source": "OESIS live OS scan (GetProductVulnerability resolution -> KB)",
            "signature": signature_id,
            "databases":                       databases,
            "latest_installer":                latest_installer,
            "total_missing_patches":           len(missing_patches),
            "total_cves_raw":                  len(cve_ids),
            "total_cves_covered_by_installed": 0,
            "total_cves":                      len(cve_ids),
            "unmapped_cves":                   unmapped,
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
