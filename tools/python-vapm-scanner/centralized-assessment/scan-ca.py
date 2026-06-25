#!/usr/bin/env python3
###############################################################################################
##  VAPM Centralized Assessment — Full Pipeline
##  Reference Implementation using the OESIS Framework + "Analog" offline catalog
##
##  One-command centralized assessment. It:
##    1. Runs the endpoint scan   (scan-ca-endpoint.py) -> gathers OS details, patches,
##       and detected products into the scan-ca-*-result.json files.
##    2. Runs the combined mapper (map-ca.py)           -> maps those results to missing
##       patches and CVEs via the Analog catalog -> map-ca-result.json.
##    3. Produces a final, consolidated report: ca-result.json (derived from
##       map-ca-result.json). The OS is reported in its own "os" section and third-party in
##       "products"; the OS section carries:
##         patches  -> [{id: KB, name, cves[]}]  the KB(s) remediating the missing OS CVEs
##         cve_kbs  -> {CVE: [KB]}               direct CVE -> remediating-KB lookup (inverse)
##
##  Usage:
##      python3 copysdk.py      # stage the SDK + license into ./sdk first
##      python3 scan-ca.py
##
##  Writes: ca-result.json  (plus the intermediate scan/map result files)
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import json
import os
import re
import subprocess
import sys
from datetime import datetime

# Force UTF-8 console output so non-ASCII text doesn't crash on Windows (cp1252).
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

SCRIPT_DIR   = os.path.dirname(os.path.abspath(__file__))
RESULTS_DIR  = os.path.join(SCRIPT_DIR, "results")
MAP_RESULT   = os.path.join(SCRIPT_DIR, "map-ca-result.json")
FINAL_RESULT = os.path.join(RESULTS_DIR, "ca-result.json")

PIPELINE = [
    ("Endpoint scan (gather)", "scan-ca-endpoint.py"),
    ("Map + combine",          "map-ca.py"),
]

# Intermediate JSON produced by the gather/map steps. After the final report is written
# to results/ca-result.json, these are removed so there is a single, clear output.
INTERMEDIATE_JSON = [
    "scan-ca-endpoint-result.json",
    "scan-ca-osdetails-result.json",
    "scan-ca-third-party-result.json",
    "map-ca-osdetails-result.json",
    "map-ca-third-party-result.json",
    "map-ca-result.json",
    "ca-result.json",            # any older copy left in the script dir
    "ca_missing_patches.json",   # legacy filenames
    "ca_third_party.json",
]


def cleanup_intermediates():
    removed = []
    for name in INTERMEDIATE_JSON:
        path = os.path.join(SCRIPT_DIR, name)
        if os.path.isfile(path):
            try:
                os.remove(path)
                removed.append(name)
            except OSError:
                pass
    return removed


def run_step(label, script_name):
    script_path = os.path.join(SCRIPT_DIR, script_name)
    print(f"\n{'#' * 70}")
    print(f"#  {label}  ({script_name})")
    print(f"{'#' * 70}")
    if not os.path.isfile(script_path):
        print(f"  ERROR: step script not found: {script_path}")
        return 1
    return subprocess.run([sys.executable, script_path], cwd=SCRIPT_DIR).returncode


def build_final_report(combined):
    # Derive a consolidated final report from map-ca-result.json. The OS is reported in its
    # own "os" section; "products" holds the third-party products. Both the OS section and
    # each product use the shared shape: signature_id, product_id, name, version,
    # latest_version, cves[], cpes[]. Schema matches the endpoint scan-ea result. The OS
    # section additionally carries a "patches" list — the name and ID of each patch that
    # remediates the missing CVEs, with the CVEs each patch covers.
    os_assessment = combined.get("os_assessment") or {}
    tp_assessment = combined.get("third_party_assessment") or {}

    OS_SIG = 1103

    # The OS section (signature 1103 = Windows Update Agent).
    os_info = os_assessment.get("os_info") or {}
    os_cves = sorted({c.get("cve") for c in os_assessment.get("cves", []) if c.get("cve")})

    # Patches (KBs) associated with the missing OS CVEs: id + name + the CVEs each remediates.
    os_patches = []
    for mp in os_assessment.get("missing_patches", []):
        os_patches.append({
            "id":        mp.get("kb"),
            "name":      mp.get("title"),
            "severity":  mp.get("severity"),
            "cve_count": mp.get("cve_count", len(mp.get("cves", []))),
            "cves":      mp.get("cves", []),
        })

    # Direct CVE -> remediating KB(s) lookup (inverse of os_patches) so a consumer can answer
    # "which KB fixes this OS CVE?" without scanning the patch list.
    cve_kbs = {}
    for p in os_patches:
        for c in p["cves"]:
            cve_kbs.setdefault(c, set()).add(p["id"])
    cve_kbs = {c: sorted(v) for c, v in sorted(cve_kbs.items())}

    os_latest = None
    mps = os_assessment.get("missing_patches", [])
    if mps:
        # Target build is often in the patch title, e.g. "... (KB5094126) (26100.8655)".
        m = re.search(r"\((\d+\.\d[\d.]*)\)\s*$", mps[0].get("title", "") or "")
        os_latest = m.group(1) if m else None
    os_section = {
        "signature_id":   OS_SIG,
        "product_id":     None,
        "name":           os_info.get("name"),
        "version":        os_info.get("version"),
        "os_id":          os_info.get("os_id"),
        "latest_version": os_latest,
        "cves":           os_cves,
        "cpes":           [],
        "patches":        os_patches,
        "cve_kbs":        cve_kbs,
    }

    # Third-party products. The OS / Windows Update Agent signature is folded into the OS
    # section, not listed as a product.
    products = []
    for p in tp_assessment.get("products", []):
        if p.get("signature_id") == OS_SIG:
            os_section["cves"] = sorted(set(os_section["cves"]) | set(p.get("cves", [])))
            os_section["cpes"] = sorted(set(os_section["cpes"]) | set(p.get("cpes", [])))
            if not os_section.get("product_id") and p.get("product_id"):
                os_section["product_id"] = p.get("product_id")
            continue
        products.append({
            "signature_id":   p.get("signature_id"),
            "product_id":     p.get("product_id"),
            "name":           p.get("name"),
            "version":        p.get("version"),
            "latest_version": p.get("latest_version"),
            "cves":           p.get("cves", []),
            "cpes":           p.get("cpes", []),
        })

    os_cve_set = set(os_section["cves"])
    tp_cve_set = {cve for pr in products for cve in pr["cves"]}
    summary = {
        "os_cve_count":              len(os_cve_set),
        "total_products":            len(products),
        "total_vulnerable_products": sum(1 for pr in products if pr["cves"]),
        "total_distinct_cves":       len(os_cve_set | tp_cve_set),
    }

    return {
        "assessment":   "centralized",
        "generated_at": datetime.now().isoformat(timespec="seconds"),
        "os":       os_section,
        "summary":  summary,
        "products": products,
    }


def main():
    print("VAPM Centralized Assessment — Full Pipeline (scan-ca)")

    statuses = []
    for label, script_name in PIPELINE:
        rc = run_step(label, script_name)
        statuses.append((label, script_name, rc))

    if not os.path.isfile(MAP_RESULT):
        print(f"\nERROR: {os.path.basename(MAP_RESULT)} was not produced — cannot build the "
              f"final report. Check the steps above (did copysdk.py stage the SDK?).")
        sys.exit(1)

    with open(MAP_RESULT, "r", encoding="utf-8") as f:
        combined = json.load(f)

    final = build_final_report(combined)
    os.makedirs(RESULTS_DIR, exist_ok=True)
    with open(FINAL_RESULT, "w", encoding="utf-8") as f:
        json.dump(final, f, indent=2, default=str)

    # The final report is the single deliverable — clean up the intermediate JSON files.
    removed = cleanup_intermediates()

    # --- Console summary ---
    s = final["summary"]
    print(f"\n{'=' * 70}")
    print("  Centralized Assessment — Final Result")
    print(f"{'=' * 70}")
    print(f"  Endpoint            : {final['os'].get('name')} ({final['os'].get('version')})")
    print(f"  OS CVEs             : {s['os_cve_count']}")
    print(f"  Third-party products: {s['total_products']}  ({s['total_vulnerable_products']} vulnerable)")
    print(f"  Total distinct CVEs : {s['total_distinct_cves']}")
    if removed:
        print(f"  Cleaned up          : {len(removed)} intermediate file(s)")
    print(f"\n  Final report: {FINAL_RESULT}")

    if any(rc != 0 for _, _, rc in statuses):
        sys.exit(1)


if __name__ == "__main__":
    main()
