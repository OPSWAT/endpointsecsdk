#!/usr/bin/env python3
###############################################################################################
##  VAPM Endpoint Assessment — Third-Party Vulnerabilities (live SDK scan)
##  Reference Implementation using the OESIS Framework
##
##  Live, agent-style third-party scan of THIS endpoint, modeled on
##  helloworld/python/vulnerability.py: it loads the offline CVE database (v2mod.dat),
##  detects installed products (DetectProducts), and queries each product for
##  vulnerabilities (GetProductVulnerability). It also resolves each product's version
##  (GetVersion). The output matches the centralized mapper's result shape
##  (map-ca-third-party-result.json) so the endpoint and centralized assessments are
##  directly comparable.
##
##  Usage:
##      python3 copysdk.py             # stage the SDK + license into ./sdk first
##      python3 scan-ea-third-party.py
##
##  Writes: scan-ea-third-party-result.json  (same schema as map-ca-third-party-result.json)
##
##  SDK methods: ConsumeOfflineVmodDatabase (50520), DetectProducts (0), GetVersion (100),
##               GetProductVulnerability (50505)
##  https://software.opswat.com/OESIS_V4/html/c_method.html
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
from platform_utils import get_os_type

# Force UTF-8 console output so non-ASCII product/CVE text doesn't crash on Windows (cp1252).
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SDK_DIR    = os.path.join(SCRIPT_DIR, "sdk")
V2MOD_DAT  = os.path.join(SDK_DIR, "v2mod.dat")   # cross-platform 3rd-party CVE database

OUTPUT_FILE = os.path.join(SCRIPT_DIR, "scan-ea-third-party-result.json")


def initialize_framework():
    pass_key_path = os.path.join(SDK_DIR, "pass_key.txt")
    if not os.path.isfile(pass_key_path):
        print("Could not find pass_key.txt. Make sure the license is in the sdk directory.")
        raise Exception("License pass_key.txt file not found")
    sdk = OESISWrapper(os.path.join(SDK_DIR, get_lib_filename()))
    sdk.load()
    sdk.setup(os.path.join(SDK_DIR, "license.cfg"), pass_key_path)
    return sdk


def consume_offline_vmod_database(sdk, database_file):
    # ConsumeOfflineVmodDatabase (method 50520) — load the offline 3rd-party CVE database.
    rc, result = sdk.invoke(50520, dat_input_source_file=database_file)
    if rc < 0:
        raise Exception(f"ConsumeOfflineVmodDatabase failed (rc={rc}): {result}")


def detect_products(sdk):
    # DetectProducts (method 0, category 0 = all)
    rc, result = sdk.invoke(0, category=0)
    if rc < 0:
        raise Exception(f"DetectProducts failed (rc={rc}): {result}")
    return result.get("result", {}).get("detected_products", [])


def get_version(sdk, signature_id):
    # GetVersion (method 100) — reliable installed version.
    rc, result = sdk.invoke(100, signature=signature_id)
    if rc < 0:
        return "Unknown"
    return result.get("result", {}).get("version", "Unknown")


def get_product_vulnerability(sdk, signature_id):
    # GetProductVulnerability (method 50505) against the loaded v2mod.dat.
    rc, result = sdk.invoke(50505, signature=signature_id)
    if rc < 0:
        return []
    return result.get("result", {}).get("cves", []) or []


def normalize_cve(raw):
    # Normalize a CVE record, tolerating the field names the SDK may use, and pull out
    # the CPE strings (details.cpe[].cpe_2_3) associated with the vulnerability.
    cve_id = raw.get("cve") or raw.get("id") or raw.get("static_id")
    details = raw.get("details") or {}
    cvss = details.get("cvss_3_1") or details.get("cvss_3_0") or details.get("cvss_2_0") or {}
    cpes = []
    for entry in details.get("cpe", []) or []:
        cpe = entry.get("cpe_2_3") or entry.get("cpe_2_2") or entry.get("cpe")
        if cpe:
            cpes.append(cpe)
    return {
        "cve":      str(cve_id) if cve_id is not None else None,
        "severity": raw.get("severity") or cvss.get("base_severity"),
        "score":    cvss.get("base_score"),
        "cpes":     cpes,
    }


def main():
    if not validate_sdk_environment(SDK_DIR):
        return
    if not os.path.isfile(V2MOD_DAT):
        print(f"ERROR: required database not found: {V2MOD_DAT}")
        print("       Run 'python copysdk.py' to stage the SDK data files.")
        return

    sdk = None
    try:
        sdk = initialize_framework()

        print("\nVAPM Endpoint Assessment — Third-Party Vulnerabilities (live scan)")
        print("=" * 70)

        print("Loading offline CVE database (v2mod.dat)...")
        consume_offline_vmod_database(sdk, V2MOD_DAT)

        print("Detecting products and querying vulnerabilities...")
        raw_products = detect_products(sdk)

        products = []
        cve_to_products = {}   # cve -> set(product name)

        for p in raw_products:
            sig_id = p.get("signature")
            name   = p.get("product", {}).get("name", "Unknown")
            raw_cves = get_product_vulnerability(sdk, sig_id)
            norm = [normalize_cve(c) for c in raw_cves]
            cve_ids = sorted({c["cve"] for c in norm if c["cve"]})
            cpes = sorted({cpe for c in norm for cpe in c["cpes"]})

            products.append({
                "signature_id": sig_id,
                "product_id":   p.get("product", {}).get("id"),
                "name":         name,
                "vendor":       p.get("vendor", {}).get("name", "Unknown"),
                "version":      get_version(sdk, sig_id),
                "cve_count":    len(cve_ids),
                "cves":         cve_ids,
                "cpes":         cpes,
            })
            for cid in cve_ids:
                cve_to_products.setdefault(cid, set()).add(name)

        products.sort(key=lambda x: (x["name"] or "").lower())

        cve_list = [
            {"cve": cid, "affected_products": sorted(p for p in cve_to_products[cid] if p)}
            for cid in sorted(cve_to_products)
        ]

        vulnerable = [p for p in products if p["cve_count"] > 0]
        print(f"\n  Vulnerable products: {len(vulnerable)} of {len(products)}")
        print("-" * 70)
        for p in sorted(vulnerable, key=lambda x: x["cve_count"], reverse=True):
            print(f"  {(p['name'] or 'Unknown')[:38]:<38} {(p['version'] or '')[:16]:<16} "
                  f"{p['cve_count']} CVE(s)")
        print(f"\n  Total distinct CVEs across third-party products: {len(cve_list)}")

        # Output schema matches map-ca-third-party-result.json (CVE-relevant keys).
        output = {
            "os_type":  get_os_type(),
            "source":   "OESIS live scan (v2mod.dat / GetProductVulnerability)",
            "total_products":            len(products),
            "total_vulnerable_products": len(vulnerable),
            "total_cves":                len(cve_list),
            "products": products,
            "cves":     cve_list,
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
