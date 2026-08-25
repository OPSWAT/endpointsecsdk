#!/usr/bin/env python3
###############################################################################################
##  VAPM Helper — Endpoint System-Vulnerability APIs & CVE<->KB mapping (live SDK demo)
##  Reference Implementation using the OESIS Framework
##
##  Demonstrates the OESIS *system-level* vulnerability methods on the endpoint and what they
##  do / don't provide for OS CVE<->KB mapping. These are distinct from GetProductVulnerability
##  (50505, per-product):
##
##    GetSystemVulnerabilities          (50509) -> aggregate vulnerabilities present on the
##                                                 system (CVEs + affected products + fixes).
##    GetSystemVulnsByInstalledPatches  (50508) -> intended KB-list -> CVE mapper.
##    GetSupportedDetectableVulns       (50507) -> what the loaded source can detect.
##
##  KEY INITIALIZATION NOTE (this is what causes "not initialized" errors):
##    The system methods query the OFFLINE VMOD source, which must be loaded with v2mod.dat
##    via ConsumeOfflineVmodDatabase (50520). Loading wiv-lite.dat does NOT initialize this
##    source, so the methods return:
##        -1019 WA_VMOD_ERROR_OFFLINEVMOD_NOT_INITIALIZED   (50507/50508)
##        -16   WAAPI_ERROR_DATABASE_NOT_INITIALIZED         (50509)
##    Loading v2mod.dat fixes that and 50509/50507 succeed.
##
##  WHAT WE LEARN (verified on this SDK build 4.3.x + offline data):
##    * 50509 succeeds with v2mod but returns THIRD-PARTY product CVEs (Chrome, Python, etc.)
##      with ZERO KB associations and no OS/Windows-Update-Agent (sig 1103) entry. v2mod is
##      the third-party vuln source; it does not carry OS KB<->CVE data.
##    * 50508 (the KB-list -> CVE mapper) returns -12 NOT_IMPLEMENTED in this engine build.
##    * OS CVE<->KB association therefore is NOT available from the endpoint with this data;
##      it requires a KB-bearing WIV source (wiv-lite has number_of_kbs = 0). That mapping is
##      what the CENTRALIZED workflow provides via the Analog catalog (kb_info / vuln_system_
##      associations).
##
##  Usage:
##      python3 copysdk.py            # stage the SDK + license into ./sdk first
##      python3 map_cve_to_kb.py [signature_id]   # default OS signature 1103
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import json
import os
import sys

from sdk_wrapper import OESISWrapper, SDKError
from platform_utils import validate_sdk_environment, get_lib_filename

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SDK_DIR    = os.path.join(SCRIPT_DIR, "sdk")

WUO_DAT      = os.path.join(SDK_DIR, "wuov2.dat")     # Windows OS patch database (LoadPatchDatabase)
V2MOD_DAT    = os.path.join(SDK_DIR, "v2mod.dat")     # offline vmod (vulnerability) source
WIV_LITE_DAT = os.path.join(SDK_DIR, "wiv-lite.dat")  # lite Windows vuln DB (no KB source)

DEFAULT_OS_SIGNATURE = 1103
OUTPUT_FILE = os.path.join(SCRIPT_DIR, "map-cve-to-kb-result.json")

M_GET_OS_INFO                    = 1
M_GET_MISSING_PATCHES            = 1013
M_GET_INSTALLED_PATCHES          = 1023
M_LOAD_PATCH_DATABASE            = 50302
M_GET_SUPPORTED_DETECTABLE_VULNS = 50507
M_GET_SYSTEM_VULNS_BY_PATCHES    = 50508
M_GET_SYSTEM_VULNERABILITIES     = 50509
M_CONSUME_OFFLINE_VMOD_DATABASE  = 50520


def initialize_framework():
    pass_key_path = os.path.join(SDK_DIR, "pass_key.txt")
    if not os.path.isfile(pass_key_path):
        raise Exception("License pass_key.txt not found in the sdk directory (run copysdk.py).")
    sdk = OESISWrapper(os.path.join(SDK_DIR, get_lib_filename()))
    sdk.load()
    sdk.setup(os.path.join(SDK_DIR, "license.cfg"), pass_key_path)
    return sdk


def invoke(sdk, method, **kwargs):
    rc, result = sdk.invoke(method, **kwargs)
    return rc, (result or {})


def err_define(result):
    e = (result or {}).get("error", {})
    return f"{e.get('define', '')} {e.get('description', '')}".strip()


def load_db(sdk, method, dat_file, label):
    if not os.path.isfile(dat_file):
        print(f"  {label}: {os.path.basename(dat_file)} not found -- skipping")
        return {}, -1
    rc, result = invoke(sdk, method, dat_input_source_file=dat_file)
    res = result.get("result", {}) if rc >= 0 else {}
    print(f"  {label}: {os.path.basename(dat_file)}  rc={rc}  v{res.get('version')}  "
          f"details={res.get('details')}")
    return res, rc


def kb_list_from_patches(patches):
    import re
    kbs = set()
    for p in patches or []:
        for field in ("security_update_id", "kb_id", "id"):
            v = p.get(field)
            if v and str(v).upper().replace("KB", "").strip().isdigit():
                kbs.add(str(v).upper().replace("KB", "").strip())
        for m in re.findall(r"KB(\d+)", p.get("title", "") or "", flags=re.IGNORECASE):
            kbs.add(m)
    return sorted(kbs)


def summarize_system_vulns(result):
    """Summarize a GetSystemVulnerabilities result: CVE count, how many carry KB
    associations, whether the OS (sig 1103) appears, and the per-product breakdown."""
    cves = result.get("result", {}).get("cves", []) or []
    with_kb, os_present, products = 0, False, {}
    cve_kb_pairs = []
    for c in cves:
        kbs = c.get("kbs") or c.get("kb") or c.get("kb_articles") or []
        if isinstance(kbs, (str, int)):
            kbs = [str(kbs)]
        kbs = [str(k.get("kb_id") if isinstance(k, dict) else k) for k in kbs]
        if kbs:
            with_kb += 1
        cve_kb_pairs.append({"cve": c.get("cve"), "kbs": kbs})
        for ap in c.get("affected_products", []) or []:
            if ap.get("signature") == DEFAULT_OS_SIGNATURE:
                os_present = True
            name = (ap.get("product") or {}).get("name")
            products[name] = products.get(name, 0) + 1
    return {
        "total_cves":          len(cves),
        "cves_with_kb":        with_kb,
        "os_signature_present": os_present,
        "by_product":          products,
        "cve_kb_pairs":        cve_kb_pairs,
    }


def main(signature_id=DEFAULT_OS_SIGNATURE):
    if len(sys.argv) > 1:
        try:
            signature_id = int(sys.argv[1])
        except ValueError:
            print(f"Invalid signature ID '{sys.argv[1]}'")
            return
    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    try:
        sdk = initialize_framework()
        print("\nVAPM Helper — Endpoint system-vulnerability APIs & CVE<->KB mapping (live)")
        print("=" * 74)

        os_info = invoke(sdk, M_GET_OS_INFO)[1].get("result", {})
        os_id = os_info.get("os_id")
        print(f"  OS: {os_info.get('name','Unknown')} ({os_info.get('version','')}), os_id={os_id}")

        # The system-vuln methods query the OFFLINE VMOD source -> must load v2mod.dat (50520).
        # (Loading wiv-lite.dat here is what produced the 'not initialized' errors.)
        print("\nLoading databases (offline vmod source = v2mod.dat)...")
        load_db(sdk, M_LOAD_PATCH_DATABASE, WUO_DAT, "patch DB   (50302)")
        vuln_db, vrc = load_db(sdk, M_CONSUME_OFFLINE_VMOD_DATABASE, V2MOD_DAT, "vuln source(50520)")
        wiv, _ = load_db(sdk, M_CONSUME_OFFLINE_VMOD_DATABASE, WIV_LITE_DAT, "wiv-lite   (50520)")
        print(f"\n  Note: wiv-lite.dat number_of_kbs = {(wiv.get('details') or {}).get('number_of_kbs')} "
              f"(no KB source -> cannot map OS CVEs to KBs)")

        installed = invoke(sdk, M_GET_INSTALLED_PATCHES, signature=signature_id, timeout=0,
                           retry_internet_services=True, mode=0)[1].get("result", {}).get("patches", [])
        missing = invoke(sdk, M_GET_MISSING_PATCHES, signature=signature_id, timeout=0,
                         retry_internet_services=True, mode=0)[1].get("result", {}).get("patches", [])
        installed_kbs = kb_list_from_patches(installed)
        missing_kbs = kb_list_from_patches(missing)
        print(f"\n  Installed KBs ({len(installed_kbs)}): {installed_kbs}")
        print(f"  Missing KBs   ({len(missing_kbs)}): {missing_kbs}")

        results = {}

        # 50509 GetSystemVulnerabilities
        print("\n[50509] GetSystemVulnerabilities ...")
        rc, r = invoke(sdk, M_GET_SYSTEM_VULNERABILITIES, os_id=os_id, signature=signature_id,
                       installed_patches=installed_kbs, missing_patches=missing_kbs)
        if rc < 0:
            print(f"  rc={rc}  {err_define(r)}")
            results["get_system_vulnerabilities"] = {"rc": rc, "error": err_define(r)}
        else:
            summ = summarize_system_vulns(r)
            results["get_system_vulnerabilities"] = {"rc": rc, **{k: v for k, v in summ.items()
                                                                  if k != "cve_kb_pairs"}}
            print(f"  rc=0  CVEs={summ['total_cves']}  with_KB_association={summ['cves_with_kb']}  "
                  f"OS(sig 1103) present={summ['os_signature_present']}")
            print("  by product:")
            for n, ct in sorted(summ["by_product"].items(), key=lambda kv: -kv[1]):
                print(f"     {n}: {ct}")

        # 50508 GetSystemVulnsByInstalledPatches (the KB-list -> CVE mapper)
        print("\n[50508] GetSystemVulnsByInstalledPatches ...")
        rc, r = invoke(sdk, M_GET_SYSTEM_VULNS_BY_PATCHES, os_id=os_id,
                       installed_patches=installed_kbs, missing_patches=missing_kbs)
        results["get_system_vulns_by_installed_patches"] = {"rc": rc, "error": err_define(r) if rc < 0 else ""}
        print(f"  rc={rc}  {err_define(r) if rc < 0 else 'OK'}")

        # 50507 GetSupportedDetectableVulns
        print("\n[50507] GetSupportedDetectableVulns ...")
        rc, r = invoke(sdk, M_GET_SUPPORTED_DETECTABLE_VULNS)
        results["get_supported_detectable_vulns"] = {"rc": rc, "error": err_define(r) if rc < 0 else ""}
        print(f"  rc={rc}  {err_define(r) if rc < 0 else 'OK'}")

        output = {
            "os_info": {"name": os_info.get("name"), "version": os_info.get("version"), "os_id": os_id},
            "wiv_lite_number_of_kbs": (wiv.get("details") or {}).get("number_of_kbs"),
            "installed_kbs": installed_kbs,
            "missing_kbs": missing_kbs,
            "methods": results,
        }
        with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
            json.dump(output, f, indent=2, default=str)

        print("\n" + "=" * 74)
        print("  CONCLUSION")
        print("=" * 74)
        print("  * 50509 GetSystemVulnerabilities works (with v2mod) but returns THIRD-PARTY")
        print("    product CVEs, with 0 KB associations and no OS (sig 1103) entry -- v2mod is")
        print("    the third-party vuln source and carries no OS KB<->CVE data.")
        print("  * 50508 (KB-list -> CVE mapper) is NOT IMPLEMENTED in this engine build (-12).")
        print("  * wiv-lite.dat has number_of_kbs = 0, so there is no KB-bearing OS vuln source")
        print("    on the endpoint. OS CVE<->KB association requires a full WIV source (not")
        print("    shipped here) -- which is exactly what the CENTRALIZED catalog workflow does.")
        print(f"\n  Results written to: {OUTPUT_FILE}")

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
