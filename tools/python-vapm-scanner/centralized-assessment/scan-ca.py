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
##       map-ca-result.json).
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
import subprocess
import sys
from datetime import datetime

# Force UTF-8 console output so non-ASCII text doesn't crash on Windows (cp1252).
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

SCRIPT_DIR    = os.path.dirname(os.path.abspath(__file__))
MAP_RESULT    = os.path.join(SCRIPT_DIR, "map-ca-result.json")
FINAL_RESULT  = os.path.join(SCRIPT_DIR, "ca-result.json")

PIPELINE = [
    ("Endpoint scan (gather)", "scan-ca-endpoint.py"),
    ("Map + combine",          "map-ca.py"),
]


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
    # Derive a consolidated final report from map-ca-result.json.
    os_assessment = combined.get("os_assessment") or {}
    tp_assessment = combined.get("third_party_assessment") or {}
    summary       = combined.get("summary") or {}

    # Missing OS patches (kb + the CVE count each remediates)
    missing_os_patches = [
        {
            "kb":        p.get("kb"),
            "title":     p.get("title"),
            "severity":  p.get("severity"),
            "cve_count": p.get("cve_count", 0),
        }
        for p in os_assessment.get("missing_patches", [])
    ]

    # Vulnerable / out-of-date third-party products
    vulnerable_products = [
        {
            "name":           p.get("name"),
            "version":        p.get("version"),
            "latest_version": p.get("latest_version"),
            "patch_missing":  p.get("patch_missing"),
            "cve_count":      p.get("cve_count", 0),
        }
        for p in tp_assessment.get("products", [])
        if p.get("cve_count", 0) > 0 or p.get("patch_missing")
    ]

    # Unified, de-duplicated CVE list, tagged by which assessment surfaced it.
    cve_map = {}
    for c in os_assessment.get("cves", []):
        cve = c.get("cve")
        if not cve:
            continue
        entry = cve_map.setdefault(cve, {"cve": cve, "cwe": c.get("cwe"),
                                         "published_epoch": c.get("published_epoch"),
                                         "sources": set()})
        entry["sources"].add("os")
        entry["fixed_by_kbs"] = c.get("fixed_by_kbs", [])
    for c in tp_assessment.get("cves", []):
        cve = c.get("cve")
        if not cve:
            continue
        entry = cve_map.setdefault(cve, {"cve": cve, "cwe": c.get("cwe"),
                                         "published_epoch": c.get("published_epoch"),
                                         "sources": set()})
        entry["sources"].add("third_party")
        entry["affected_products"] = c.get("affected_products", [])

    cves = []
    for cve in sorted(cve_map):
        e = cve_map[cve]
        e["sources"] = sorted(e["sources"])
        cves.append(e)

    return {
        "assessment":   "centralized",
        "generated_at": datetime.now().isoformat(timespec="seconds"),
        "os": {
            "name":    (os_assessment.get("os_info") or {}).get("name"),
            "version": (os_assessment.get("os_info") or {}).get("version"),
            "os_id":   (os_assessment.get("os_info") or {}).get("os_id"),
        },
        "summary":             summary,
        "missing_os_patches":  missing_os_patches,
        "vulnerable_products": vulnerable_products,
        "cves":                cves,
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
    with open(FINAL_RESULT, "w", encoding="utf-8") as f:
        json.dump(final, f, indent=2, default=str)

    # --- Console summary ---
    s = final["summary"]
    print(f"\n{'=' * 70}")
    print("  Centralized Assessment — Final Result")
    print(f"{'=' * 70}")
    print(f"  Endpoint            : {final['os'].get('name')} ({final['os'].get('version')})")
    print(f"  Missing OS patches  : {len(final['missing_os_patches'])}")
    print(f"  Vulnerable/outdated products: {len(final['vulnerable_products'])}")
    print(f"  Total distinct CVEs : {s.get('total_distinct_cves', len(final['cves']))}")
    print(f"\n  Final report written to: {FINAL_RESULT}")

    if any(rc != 0 for _, _, rc in statuses):
        sys.exit(1)


if __name__ == "__main__":
    main()
