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
    # Derive a consolidated, product-centric final report from map-ca-result.json.
    # Schema matches the endpoint scan-ea result: a list of products, each with
    # signature_id, product_id, name, version, latest_version, cves[], cpes[].
    os_assessment = combined.get("os_assessment") or {}
    tp_assessment = combined.get("third_party_assessment") or {}

    products = []
    by_sig = {}

    def add(entry):
        # Merge entries that share a signature_id (the OS / Windows Update Agent is both
        # the OS product and a DetectProducts result, both signature 1103).
        sig = entry["signature_id"]
        existing = by_sig.get(sig)
        if existing is None:
            by_sig[sig] = entry
            products.append(entry)
            return
        existing["cves"] = sorted(set(existing.get("cves", [])) | set(entry.get("cves", [])))
        existing["cpes"] = sorted(set(existing.get("cpes", [])) | set(entry.get("cpes", [])))
        for k in ("product_id", "name", "version", "latest_version"):
            if not existing.get(k) and entry.get(k):
                existing[k] = entry[k]

    # The OS as a single product entry (signature 1103 = Windows Update Agent).
    os_info = os_assessment.get("os_info") or {}
    os_cves = sorted({c.get("cve") for c in os_assessment.get("cves", []) if c.get("cve")})
    os_latest = None
    mps = os_assessment.get("missing_patches", [])
    if mps:
        # Target build is often in the patch title, e.g. "... (KB5094126) (26100.8655)".
        m = re.search(r"\((\d+\.\d[\d.]*)\)\s*$", mps[0].get("title", "") or "")
        os_latest = m.group(1) if m else None
    if os_info or os_cves:
        add({
            "signature_id":   1103,
            "product_id":     None,
            "name":           os_info.get("name"),
            "version":        os_info.get("version"),
            "latest_version": os_latest,
            "cves":           os_cves,
            "cpes":           [],
        })

    # Third-party products (trimmed to the shared schema).
    for p in tp_assessment.get("products", []):
        add({
            "signature_id":   p.get("signature_id"),
            "product_id":     p.get("product_id"),
            "name":           p.get("name"),
            "version":        p.get("version"),
            "latest_version": p.get("latest_version"),
            "cves":           p.get("cves", []),
            "cpes":           p.get("cpes", []),
        })

    distinct_cves = sorted({cve for pr in products for cve in pr["cves"]})
    summary = {
        "total_products":            len(products),
        "total_vulnerable_products": sum(1 for pr in products if pr["cves"]),
        "total_distinct_cves":       len(distinct_cves),
    }

    return {
        "assessment":   "centralized",
        "generated_at": datetime.now().isoformat(timespec="seconds"),
        "os": {
            "name":    os_info.get("name"),
            "version": os_info.get("version"),
            "os_id":   os_info.get("os_id"),
        },
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
    print(f"  Products            : {s['total_products']}")
    print(f"  Vulnerable products : {s['total_vulnerable_products']}")
    print(f"  Total distinct CVEs : {s['total_distinct_cves']}")
    if removed:
        print(f"  Cleaned up          : {len(removed)} intermediate file(s)")
    print(f"\n  Final report: {FINAL_RESULT}")

    if any(rc != 0 for _, _, rc in statuses):
        sys.exit(1)


if __name__ == "__main__":
    main()
