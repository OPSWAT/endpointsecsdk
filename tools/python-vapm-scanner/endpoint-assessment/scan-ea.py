#!/usr/bin/env python3
###############################################################################################
##  VAPM Endpoint Assessment — Full Pipeline
##  Reference Implementation using the OESIS Framework
##
##  One-command endpoint assessment. It runs the live scans:
##      scan-ea-osdetails.py    -> OS details, latest installer, OS CVEs
##      scan-ea-third-party.py  -> detected products + CVEs + CPEs
##  then combines them into a single product-centric report at results/ea-result.json.
##
##  The schema matches the centralized scan-ca result (results/ca-result.json): a list of
##  products, each with signature_id, product_id, name, version, latest_version, and the
##  vulnerable CVEs and CPEs. All other detail is trimmed.
##
##  Usage:
##      python3 copysdk.py     # stage the SDK + license into ./sdk first
##      python3 scan-ea.py
##
##  Writes: results/ea-result.json
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import json
import os
import subprocess
import sys
from datetime import datetime

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

SCRIPT_DIR   = os.path.dirname(os.path.abspath(__file__))
RESULTS_DIR  = os.path.join(SCRIPT_DIR, "results")
FINAL_RESULT = os.path.join(RESULTS_DIR, "ea-result.json")

OSDETAILS_RESULT   = os.path.join(SCRIPT_DIR, "scan-ea-osdetails-result.json")
THIRDPARTY_RESULT  = os.path.join(SCRIPT_DIR, "scan-ea-third-party-result.json")

PIPELINE = [
    ("OS details",  "scan-ea-osdetails.py"),
    ("Third-party", "scan-ea-third-party.py"),
]

INTERMEDIATE_JSON = [
    "scan-ea-osdetails-result.json",
    "scan-ea-third-party-result.json",
    "ea-result.json",   # any older copy left in the script dir
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


# --- Offline patch-catalog lookup for latest_version (same source as the centralized map) --

def find_analog_server_dir():
    current = SCRIPT_DIR
    while True:
        if os.path.isfile(os.path.join(current, "sdkroot")):
            server = os.path.join(current, "OPSWAT-SDK", "extract", "analog", "server")
            return server if os.path.isdir(server) else None
        parent = os.path.dirname(current)
        if parent == current:
            return None
        current = parent


def read_analog_records(path):
    with open(path, "r", encoding="utf-8") as f:
        data = json.load(f)
    for element in data.get("oesis", []):
        for key, value in element.items():
            if key == "header":
                continue
            if isinstance(value, dict):
                for record in value.values():
                    yield record


def build_patch_indexes(server_dir):
    assoc_by_pid, agg_latest = {}, {}
    if not server_dir:
        return assoc_by_pid, agg_latest
    try:
        for rec in read_analog_records(os.path.join(server_dir, "patch_associations.json")):
            pid = rec.get("v4_pid")
            if pid is not None:
                assoc_by_pid.setdefault(pid, []).append(rec)
        for rec in read_analog_records(os.path.join(server_dir, "patch_aggregation.json")):
            _id = rec.get("_id")
            if _id is not None:
                agg_latest[_id] = rec.get("latest_version")
    except OSError:
        pass
    return assoc_by_pid, agg_latest


def latest_version_for(product_id, signature_id, assoc_by_pid, agg_latest):
    if product_id is None:
        return None
    candidates = []
    for asso in assoc_by_pid.get(product_id, []):
        sigs = asso.get("v4_signatures")
        if sigs and signature_id not in sigs:
            continue
        candidates.append(asso)
    if not candidates:
        return None
    chosen = next((a for a in candidates if a.get("is_latest")), candidates[0])
    return agg_latest.get(chosen.get("patch_id"))


def load_result(path):
    if not os.path.isfile(path):
        return None
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


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


def main():
    print("VAPM Endpoint Assessment — Full Pipeline (scan-ea)")

    statuses = [(label, name, run_step(label, name)) for label, name in PIPELINE]

    osd = load_result(OSDETAILS_RESULT)
    tp  = load_result(THIRDPARTY_RESULT)

    assoc_by_pid, agg_latest = build_patch_indexes(find_analog_server_dir())

    products = []
    by_sig = {}

    def add(entry):
        # Merge entries that share a signature_id (the OS / Windows Update Agent is
        # both the OS product and a DetectProducts result, both signature 1103).
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

    # The OS as a single product entry (its CVEs/CPEs come from the live OS scan; the
    # latest version is the OS patch's target version from GetLatestInstaller).
    if osd:
        os_info = osd.get("os_info", {})
        os_cves = sorted({c.get("cve") for c in osd.get("cves", []) if c.get("cve")})
        os_cpes = sorted({cpe for c in osd.get("cves", []) for cpe in (c.get("cpes") or [])})
        mp = osd.get("missing_patches", [])
        add({
            "signature_id":   osd.get("signature"),
            "product_id":     None,
            "name":           os_info.get("name"),
            "version":        os_info.get("version"),
            "latest_version": mp[0].get("version") if mp else None,
            "cves":           os_cves,
            "cpes":           os_cpes,
        })

    # Third-party products, enriched with the latest available version (offline catalog).
    if tp:
        for p in tp.get("products", []):
            add({
                "signature_id":   p.get("signature_id"),
                "product_id":     p.get("product_id"),
                "name":           p.get("name"),
                "version":        p.get("version"),
                "latest_version": latest_version_for(p.get("product_id"), p.get("signature_id"),
                                                      assoc_by_pid, agg_latest),
                "cves":           p.get("cves", []),
                "cpes":           p.get("cpes", []),
            })

    distinct_cves = sorted({cve for pr in products for cve in pr["cves"]})
    summary = {
        "total_products":            len(products),
        "total_vulnerable_products": sum(1 for pr in products if pr["cves"]),
        "total_distinct_cves":       len(distinct_cves),
    }

    os_info = (osd or {}).get("os_info", {})
    final = {
        "assessment":   "endpoint",
        "generated_at": datetime.now().isoformat(timespec="seconds"),
        "os": {
            "name":    os_info.get("name"),
            "version": os_info.get("version"),
            "os_id":   os_info.get("os_id"),
        },
        "summary":  summary,
        "products": products,
    }

    os.makedirs(RESULTS_DIR, exist_ok=True)
    with open(FINAL_RESULT, "w", encoding="utf-8") as f:
        json.dump(final, f, indent=2, default=str)
    removed = cleanup_intermediates()

    print(f"\n{'=' * 70}")
    print("  Endpoint Assessment — Final Result")
    print(f"{'=' * 70}")
    print(f"  Endpoint            : {final['os'].get('name')} ({final['os'].get('version')})")
    print(f"  Products            : {summary['total_products']}")
    print(f"  Vulnerable products : {summary['total_vulnerable_products']}")
    print(f"  Total distinct CVEs : {summary['total_distinct_cves']}")
    if removed:
        print(f"  Cleaned up          : {len(removed)} intermediate file(s)")
    print(f"\n  Final report: {FINAL_RESULT}")

    if any(rc != 0 for _, _, rc in statuses):
        sys.exit(1)


if __name__ == "__main__":
    main()
