#!/usr/bin/env python3
###############################################################################################
##  VAPM Centralized Assessment — Combined Mapping
##  Reference Implementation using the OESIS "Analog" offline catalog
##
##  Runs both Analog mappers and combines their output into a single result file:
##      map_ca_osdetails.py    -> OS / system missing patches + CVEs
##      map_ca_third_party.py  -> third-party product CVEs + latest-version / patch-missing
##
##  Each mapper reads the corresponding scan result (produced by scan-ca-osdetails.py and
##  scan-ca-third-party.py) and writes its own JSON; this script merges them and computes a
##  unified, de-duplicated CVE view across the OS and third-party assessments.
##
##  Usage:
##      # gather first:
##      python3 scan-ca-osdetails.py
##      python3 scan-ca-third-party.py
##      # then map + combine:
##      python3 map-ca.py
##
##  Writes: map-ca-result.json
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import json
import os
import subprocess
import sys

# Force UTF-8 console output so non-ASCII text doesn't crash on Windows (cp1252).
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))

# (mapper script, the result file it produces)
MAPPERS = [
    ("OS / System",   "map_ca_osdetails.py",   "map-ca-osdetails-result.json"),
    ("Third-Party",   "map_ca_third_party.py", "map-ca-third-party-result.json"),
]

COMBINED_FILE = os.path.join(SCRIPT_DIR, "map-ca-result.json")


def run_mapper(label, script_name):
    script_path = os.path.join(SCRIPT_DIR, script_name)
    print(f"\n{'#' * 70}")
    print(f"#  {label}  ({script_name})")
    print(f"{'#' * 70}")
    if not os.path.isfile(script_path):
        print(f"  ERROR: mapper not found: {script_path}")
        return 1
    return subprocess.run([sys.executable, script_path], cwd=SCRIPT_DIR).returncode


def load_result(file_name):
    path = os.path.join(SCRIPT_DIR, file_name)
    if not os.path.isfile(path):
        return None
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def main():
    print("VAPM Centralized Assessment — Combined Mapping (map-ca)")
    print("Running both mappers, then combining their results.")

    statuses = []
    for label, script_name, _ in MAPPERS:
        rc = run_mapper(label, script_name)
        statuses.append((label, script_name, rc))

    os_result = load_result("map-ca-osdetails-result.json")
    tp_result = load_result("map-ca-third-party-result.json")

    # Unified, de-duplicated CVE set across both assessments.
    all_cves = set()
    if os_result:
        all_cves |= {c.get("cve") for c in os_result.get("cves", []) if c.get("cve")}
    if tp_result:
        all_cves |= {c.get("cve") for c in tp_result.get("cves", []) if c.get("cve")}

    summary = {
        "os_missing_patches":              (os_result or {}).get("total_missing_patches", 0),
        "os_net_cves":                     (os_result or {}).get("total_cves", 0),
        "third_party_vulnerable_products": (tp_result or {}).get("total_vulnerable_products", 0),
        "third_party_patch_missing":       (tp_result or {}).get("total_patch_missing", 0),
        "third_party_cves":                (tp_result or {}).get("total_cves", 0),
        "total_distinct_cves":             len(all_cves),
    }

    combined = {
        "source": "Analog offline catalog (combined OS + third-party assessment)",
        "inputs": {
            "os_assessment_present":          os_result is not None,
            "third_party_assessment_present": tp_result is not None,
        },
        "summary":                summary,
        "os_assessment":          os_result,
        "third_party_assessment": tp_result,
    }
    with open(COMBINED_FILE, "w", encoding="utf-8") as f:
        json.dump(combined, f, indent=2, default=str)

    # --- Console summary ---
    print(f"\n{'=' * 70}")
    print("  Combined Centralized Assessment Summary")
    print(f"{'=' * 70}")
    if os_result is None:
        print("  OS / System assessment   : MISSING (run scan-ca-osdetails.py + map_ca_osdetails.py)")
    else:
        print(f"  OS / System assessment   : {summary['os_missing_patches']} missing patch(es), "
              f"{summary['os_net_cves']} net CVE(s)")
    if tp_result is None:
        print("  Third-Party assessment   : MISSING (run scan-ca-third-party.py + map_ca_third_party.py)")
    else:
        print(f"  Third-Party assessment   : {summary['third_party_vulnerable_products']} vulnerable product(s), "
              f"{summary['third_party_patch_missing']} with a patch missing, "
              f"{summary['third_party_cves']} CVE(s)")
    print(f"  Total distinct CVEs       : {summary['total_distinct_cves']}")
    print(f"\n  Combined results written to: {COMBINED_FILE}")

    if any(rc != 0 for _, _, rc in statuses):
        sys.exit(1)


if __name__ == "__main__":
    main()
