#!/usr/bin/env python3
###############################################################################################
##  VAPM Centralized Assessment — Map Third-Party Products to CVEs
##  Reference Implementation using the OESIS "Analog" offline catalog
##
##  Takes the detected third-party products gathered by scan-ca-third-party.py
##  (scan-ca-third-party-result.json) and maps them against the Analog offline catalog to
##  produce the CVEs each product is affected by.
##
##  This follows the third-party vulnerability approach from the Analog ruby sample code
##  (OPSWAT-SDK/extract/analog/sample_code/get_vuln.rb): the vuln_associations dataset maps
##  each CVE to the affected products (v4_pids / v4_signatures, per os_type) and the
##  affected version ranges. A product is vulnerable to a CVE when its product_id matches,
##  its signature is in scope, and its version falls within an affected range.
##
##  Usage:
##      python3 scan-ca-third-party.py   # gather products first (writes the result file)
##      python3 map_ca_third_party.py
##
##  Reads:  scan-ca-third-party-result.json (this directory)
##          OPSWAT-SDK/extract/analog/server/{vuln_associations,cves}.json
##  Writes: map-ca-third-party-result.json
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import json
import os
import re
import sys

# Force UTF-8 console output so non-ASCII text doesn't crash on Windows (cp1252).
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

SCRIPT_DIR  = os.path.dirname(os.path.abspath(__file__))
RESULT_FILE = os.path.join(SCRIPT_DIR, "scan-ca-third-party-result.json")
OUTPUT_FILE = os.path.join(SCRIPT_DIR, "map-ca-third-party-result.json")


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
    # Mirror data_reader.rb: data['oesis'][].<key>{id} -> record (skipping 'header').
    with open(path, "r", encoding="utf-8") as f:
        data = json.load(f)
    for element in data.get("oesis", []):
        for key, value in element.items():
            if key == "header":
                continue
            if isinstance(value, dict):
                for record in value.values():
                    yield record


# --- Pragmatic version comparison (numeric segments) -------------------------------------
# The Analog ruby helper (utils/version.rb) also handles alphabetic segments; this is a
# simplified numeric comparison that covers the dotted-numeric versions OESIS products use.

def parse_version(s):
    if s is None:
        return None
    nums = re.findall(r"\d+", str(s))
    if not nums:
        return None
    return tuple(int(n) for n in nums)


def _cmp(a, b):
    n = max(len(a), len(b))
    a = a + (0,) * (n - len(a))
    b = b + (0,) * (n - len(b))
    return (a > b) - (a < b)


def version_in_range(version, start, limit):
    vv, sv, lv = parse_version(version), parse_version(start), parse_version(limit)
    if vv is None or sv is None or lv is None:
        return False
    return _cmp(sv, vv) <= 0 and _cmp(vv, lv) <= 0


def build_pid_index(server_dir, os_type):
    # Index the vuln_associations for this os_type by product id (v4_pids).
    index = {}
    path = os.path.join(server_dir, "vuln_associations.json")
    for rec in read_analog_records(path):
        if rec.get("os_type") != os_type:
            continue
        for pid in rec.get("v4_pids", []) or []:
            index.setdefault(pid, []).append(rec)
    return index


def build_cve_index(server_dir):
    index = {}
    path = os.path.join(server_dir, "cves.json")
    if not os.path.isfile(path):
        return index
    for rec in read_analog_records(path):
        cve = rec.get("cve")
        if cve:
            index[cve] = {"cwe": rec.get("cwe"), "published_epoch": rec.get("published_epoch")}
    return index


def build_patch_indexes(server_dir):
    # patch_associations: v4_pid -> [ {v4_signatures, patch_id, is_latest, ...} ]
    # patch_aggregation:  patch_id (_id) -> latest_version
    # (Simplified form of get_latest_installer.rb — used to find each product's latest
    #  available version so we can flag whether an update/patch is missing.)
    assoc_by_pid = {}
    for rec in read_analog_records(os.path.join(server_dir, "patch_associations.json")):
        pid = rec.get("v4_pid")
        if pid is not None:
            assoc_by_pid.setdefault(pid, []).append(rec)

    agg_latest = {}
    for rec in read_analog_records(os.path.join(server_dir, "patch_aggregation.json")):
        _id = rec.get("_id")
        if _id is not None:
            agg_latest[_id] = rec.get("latest_version")
    return assoc_by_pid, agg_latest


def latest_version_for_product(product, assoc_by_pid, agg_latest):
    # Find the latest available version for this product via the patch catalog.
    pid = product.get("product_id")
    sig = product.get("signature_id")
    if pid is None:
        return None
    candidates = []
    for asso in assoc_by_pid.get(pid, []):
        sigs = asso.get("v4_signatures")
        if sigs and sig not in sigs:
            continue
        candidates.append(asso)
    if not candidates:
        return None
    # Prefer the association flagged is_latest; otherwise take the first match.
    chosen = next((a for a in candidates if a.get("is_latest")), candidates[0])
    return agg_latest.get(chosen.get("patch_id"))


def cves_for_product(product, pid_index):
    # Apply the get_vuln.rb matching: product_id in v4_pids, signature in scope (if listed),
    # and version within an affected range. Returns the matched CVEs and their CPEs.
    pid = product.get("product_id")
    sig = product.get("signature_id")
    version = product.get("version")
    cves, cpes = set(), set()
    if pid is None:
        return cves, cpes

    for asso in pid_index.get(pid, []):
        # We don't capture the product 'channel'; per get_vuln.rb a channel-scoped
        # association cannot match a product without a channel, so skip those.
        if "channel_pattern" in asso:
            continue
        sigs = asso.get("v4_signatures")
        if sigs and sig not in sigs:
            continue
        for rng in asso.get("ranges", []) or []:
            if version_in_range(version, rng.get("start"), rng.get("limit")):
                if asso.get("cve"):
                    cves.add(asso["cve"])
                if asso.get("cpe"):
                    cpes.add(asso["cpe"])
                break
    return cves, cpes


def main():
    if not os.path.isfile(RESULT_FILE):
        print(f"ERROR: {os.path.basename(RESULT_FILE)} not found.")
        print("       Run 'python scan-ca-third-party.py' first to gather the products.")
        return

    server_dir = find_analog_server_dir()
    if not server_dir:
        print("ERROR: Analog server datasets not found under OPSWAT-SDK/extract/analog/server.")
        print("       Run the SDK downloader so the Analog data is extracted.")
        return

    with open(RESULT_FILE, "r", encoding="utf-8") as f:
        scan = json.load(f)

    os_type  = scan.get("os_type")
    products = scan.get("products", [])

    print("VAPM Centralized Assessment — Map Third-Party Products to CVEs")
    print("=" * 70)
    print(f"  Products     : {len(products)}")
    print(f"  os_type      : {os_type}")
    print(f"  Analog data  : {server_dir}")

    pid_index = build_pid_index(server_dir, os_type)
    cve_index = build_cve_index(server_dir)
    assoc_by_pid, agg_latest = build_patch_indexes(server_dir)
    print(f"  Catalog      : {len(pid_index)} product ids with vuln associations for os_type {os_type}")

    mapped_products = []
    cve_to_products = {}   # cve -> set(product name) affected

    for product in products:
        cves, cpes = cves_for_product(product, pid_index)

        # Determine the latest available version and whether an update/patch is missing.
        current_version = product.get("version")
        latest_version = latest_version_for_product(product, assoc_by_pid, agg_latest)
        patch_missing = None  # unknown if we can't resolve the latest version
        if latest_version:
            cv, lv = parse_version(current_version), parse_version(latest_version)
            if cv is not None and lv is not None:
                patch_missing = _cmp(cv, lv) < 0   # current is older than latest

        mapped_products.append({
            "signature_id":   product.get("signature_id"),
            "product_id":     product.get("product_id"),
            "name":           product.get("name"),
            "vendor":         product.get("vendor"),
            "version":        current_version,
            "latest_version": latest_version,
            "patch_missing":  patch_missing,
            "cve_count":      len(cves),
            "cves":           sorted(cves),
            "cpes":           sorted(cpes),
        })
        for cve in cves:
            cve_to_products.setdefault(cve, set()).add(product.get("name"))

    # Consolidated, de-duplicated CVE list with enrichment.
    cve_list = []
    for cve in sorted(cve_to_products):
        meta = cve_index.get(cve, {})
        cve_list.append({
            "cve":             cve,
            "cwe":             meta.get("cwe"),
            "published_epoch": meta.get("published_epoch"),
            "affected_products": sorted(p for p in cve_to_products[cve] if p),
        })

    # --- Console summary ---
    vulnerable = [m for m in mapped_products if m["cve_count"] > 0]
    patch_missing_count = sum(1 for m in mapped_products if m["patch_missing"])
    print(f"\n  Vulnerable products: {len(vulnerable)} of {len(mapped_products)}")
    print(f"  {'Product':<34} {'Current':<14}    {'Latest':<14} {'Patch':<10} CVEs")
    print("-" * 78)
    for m in sorted(vulnerable, key=lambda x: x["cve_count"], reverse=True):
        latest = m["latest_version"] or "?"
        if m["patch_missing"] is True:
            flag = "MISSING"
        elif m["patch_missing"] is False:
            flag = "current"
        else:
            flag = "unknown"
        print(f"  {(m['name'] or 'Unknown')[:34]:<34} {(m['version'] or '')[:14]:<14} -> "
              f"{str(latest)[:14]:<14} {flag:<10} {m['cve_count']}")

    print(f"\n  Products with a patch missing (any product): {patch_missing_count} of {len(mapped_products)}")
    print(f"  Total distinct CVEs across third-party products: {len(cve_list)}")

    # --- Write JSON result ---
    output = {
        "os_type":   os_type,
        "source":    "Analog vuln_associations (offline catalog)",
        "total_products":            len(mapped_products),
        "total_vulnerable_products": len(vulnerable),
        "total_patch_missing":       patch_missing_count,
        "total_cves":                len(cve_list),
        "products":  mapped_products,
        "cves":      cve_list,
    }
    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        json.dump(output, f, indent=2, default=str)
    print(f"\n  Full results written to: {OUTPUT_FILE}")


if __name__ == "__main__":
    main()
