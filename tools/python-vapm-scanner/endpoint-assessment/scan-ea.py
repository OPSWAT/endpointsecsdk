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

    OS_SIG = 1103

    # The OS goes in its own section (its CVEs/CPEs come from the live OS scan; the latest
    # version is the OS patch's target version from GetLatestInstaller).
    os_section = None
    if osd:
        os_info = osd.get("os_info", {})
        os_cves = sorted({c.get("cve") for c in osd.get("cves", []) if c.get("cve")})
        os_cpes = sorted({cpe for c in osd.get("cves", []) for cpe in (c.get("cpes") or [])})
        mp = osd.get("missing_patches", [])
        # Patches associated with the missing OS CVEs: id + name + the CVEs each remediates.
        os_patches = [{
            "id":        p.get("kb"),
            "name":      p.get("title"),
            "severity":  p.get("severity"),
            "cve_count": p.get("cve_count", len(p.get("cves", []))),
            "cves":      p.get("cves", []),
        } for p in mp]
        # Direct CVE -> remediating KB(s) lookup (inverse of os_patches) so a consumer can
        # answer "which KB fixes this OS CVE?" without scanning the patch list.
        cve_kbs = {}
        for p in os_patches:
            for c in p["cves"]:
                cve_kbs.setdefault(c, set()).add(p["id"])
        cve_kbs = {c: sorted(v) for c, v in sorted(cve_kbs.items())}
        os_section = {
            "signature_id":   osd.get("signature") or OS_SIG,
            "product_id":     None,
            "name":           os_info.get("name"),
            "version":        os_info.get("version"),
            "os_id":          os_info.get("os_id"),
            "latest_version": mp[0].get("version") if mp else None,
            "cves":           os_cves,
            "cpes":           os_cpes,
            "patches":        os_patches,
            "cve_kbs":        cve_kbs,
        }
    os_sig = os_section["signature_id"] if os_section else OS_SIG

    # Third-party products, enriched with the latest available version (offline catalog).
    # The OS / Windows Update Agent signature is folded into the OS section, not listed here.
    products = []
    if tp:
        for p in tp.get("products", []):
            if p.get("signature_id") == os_sig:
                if os_section is not None:
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
                "latest_version": latest_version_for(p.get("product_id"), p.get("signature_id"),
                                                      assoc_by_pid, agg_latest),
                "cves":           p.get("cves", []),
                "cpes":           p.get("cpes", []),
            })

    os_cve_set = set(os_section["cves"]) if os_section else set()
    tp_cve_set = {cve for pr in products for cve in pr["cves"]}
    summary = {
        "os_cve_count":              len(os_cve_set),
        "total_products":            len(products),
        "total_vulnerable_products": sum(1 for pr in products if pr["cves"]),
        "total_distinct_cves":       len(os_cve_set | tp_cve_set),
    }

    final = {
        "assessment":   "endpoint",
        "generated_at": datetime.now().isoformat(timespec="seconds"),
        "os":       os_section or {},
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
    print(f"  OS CVEs             : {summary['os_cve_count']}")
    print(f"  Third-party products: {summary['total_products']}  ({summary['total_vulnerable_products']} vulnerable)")
    print(f"  Total distinct CVEs : {summary['total_distinct_cves']}")
    if removed:
        print(f"  Cleaned up          : {len(removed)} intermediate file(s)")
    print(f"\n  Final report: {FINAL_RESULT}")

    if any(rc != 0 for _, _, rc in statuses):
        sys.exit(1)


if __name__ == "__main__":
    main()
