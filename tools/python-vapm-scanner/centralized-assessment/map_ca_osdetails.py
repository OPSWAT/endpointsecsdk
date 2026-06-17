#!/usr/bin/env python3
###############################################################################################
##  VAPM Centralized Assessment — Map OS Details to Missing Patches & CVEs
##  Reference Implementation using the OESIS "Analog" offline catalog
##
##  Takes the OS / patch details gathered by scan-ca-osdetails.py
##  (scan-ca-osdetails-result.json) and maps them against the Analog offline catalog to
##  produce a consolidated list of MISSING PATCHES and the CVEs each patch remediates.
##
##  This follows the Windows system-vulnerability approach from the Analog ruby sample
##  code (OPSWAT-SDK/extract/analog/sample_code/get_system_vuln.rb): the
##  vuln_system_associations dataset maps each CVE to the KB articles (per os_id) that fix
##  it. A patch that is MISSING means the endpoint is still exposed to the CVEs that patch
##  would fix.
##
##  Usage:
##      python3 scan-ca-osdetails.py     # gather OS details first (writes the result file)
##      python3 map_ca_osdetails.py
##
##  Reads:  scan-ca-osdetails-result.json  (this directory)
##          OPSWAT-SDK/extract/analog/server/{vuln_system_associations,cves}.json
##  Writes: map-ca-osdetails-result.json
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

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
# Primary input: the consolidated endpoint scan produced by scan-ca-endpoint.py.
ENDPOINT_RESULT = os.path.join(SCRIPT_DIR, "scan-ca-endpoint-result.json")
# Fallback for standalone runs of scan-ca-osdetails.py.
RESULT_FILE = os.path.join(SCRIPT_DIR, "scan-ca-osdetails-result.json")
OUTPUT_FILE = os.path.join(SCRIPT_DIR, "map-ca-osdetails-result.json")

OS_TYPE_WINDOWS = 1


def load_osdetails_scan():
    # Prefer the consolidated endpoint scan file (scan-ca-endpoint-result.json) and pull
    # out the 'osdetails' section; fall back to the individual scan file if run standalone.
    if os.path.isfile(ENDPOINT_RESULT):
        with open(ENDPOINT_RESULT, "r", encoding="utf-8") as f:
            data = json.load(f)
        if data.get("osdetails") is not None:
            return data["osdetails"]
    if os.path.isfile(RESULT_FILE):
        with open(RESULT_FILE, "r", encoding="utf-8") as f:
            return json.load(f)
    return None


def find_analog_server_dir():
    # Walk up for the 'sdkroot' marker, then locate the Analog server datasets.
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


def kb_candidates(patch):
    # Collect the KB-article identifiers a patch may be keyed by, normalized to bare
    # numbers (the Analog article_name format, e.g. "5094126").
    candidates = set()
    for field in ("kb_id", "security_update_id", "id"):
        val = patch.get(field)
        if val:
            candidates.add(str(val).upper().replace("KB", "").strip())
    # Also parse a KB number out of the title, e.g. "... (KB5094126) ...".
    for m in re.findall(r"KB(\d+)", patch.get("title", ""), flags=re.IGNORECASE):
        candidates.add(m)
    return {c for c in candidates if c}


def build_kb_to_cves(server_dir, os_id):
    # Build { kb_article_name -> set(cve) } for the endpoint's os_id, from the Windows
    # vuln_system_associations records (cve -> kb_articles[{article_name, os_id[]}]).
    kb_to_cves = {}
    path = os.path.join(server_dir, "vuln_system_associations.json")
    for rec in read_analog_records(path):
        cve = rec.get("cve")
        if not cve:
            continue
        for kb in rec.get("kb_articles", []):
            if os_id in (kb.get("os_id") or []):
                name = str(kb.get("article_name", "")).strip()
                if name:
                    kb_to_cves.setdefault(name, set()).add(cve)
    return kb_to_cves


def build_cve_index(server_dir):
    # Enrich CVEs with the metadata available in cves.json (cwe, published date).
    index = {}
    path = os.path.join(server_dir, "cves.json")
    if not os.path.isfile(path):
        return index
    for rec in read_analog_records(path):
        cve = rec.get("cve")
        if cve:
            index[cve] = {
                "cwe":             rec.get("cwe"),
                "published_epoch": rec.get("published_epoch"),
            }
    return index


def main():
    scan = load_osdetails_scan()
    if scan is None:
        print("ERROR: endpoint scan not found "
              "(scan-ca-endpoint-result.json or scan-ca-osdetails-result.json).")
        print("       Run 'python scan-ca-endpoint.py' (or scan-ca-osdetails.py) first.")
        return

    server_dir = find_analog_server_dir()
    if not server_dir:
        print("ERROR: Analog server datasets not found under OPSWAT-SDK/extract/analog/server.")
        print("       Run the SDK downloader so the Analog data is extracted.")
        return

    os_info = scan.get("os_info", {})
    os_id   = os_info.get("os_id")
    os_type = os_info.get("os_type")

    print("VAPM Centralized Assessment — Map OS Details to Missing Patches & CVEs")
    print("=" * 70)
    print(f"  OS           : {os_info.get('name', 'Unknown')} ({os_info.get('version', '')})")
    print(f"  os_id/os_type: {os_id} / {os_type}")
    print(f"  Analog data  : {server_dir}")

    if os_type != OS_TYPE_WINDOWS:
        print("\n  This mapper implements the Windows vuln_system_associations approach.")
        print("  The gathered details are not Windows (os_type != 1) — nothing to map.")
        return

    kb_to_cves = build_kb_to_cves(server_dir, os_id)
    cve_index  = build_cve_index(server_dir)
    print(f"  Catalog      : {len(kb_to_cves)} KB articles mapped to CVEs for os_id {os_id}")

    # CVEs already remediated by patches that are INSTALLED on the endpoint. A missing
    # cumulative update lists many CVEs, but ones an earlier installed patch already
    # fixed are not real exposure — subtract them for the most accurate result.
    covered_cves = set()
    for product in scan.get("products", []):
        for patch in product.get("installed_patches", []):
            for kb in kb_candidates(patch):
                covered_cves |= kb_to_cves.get(kb, set())

    mapped_patches = []
    cve_to_patches = {}   # cve -> set(kb) that would fix it (net of installed coverage)
    raw_exposed = set()   # all CVEs implied by missing patches, before subtracting installed

    for product in scan.get("products", []):
        for patch in product.get("missing_patches", []):
            cands = kb_candidates(patch)
            cves = set()
            for kb in cands:
                cves |= kb_to_cves.get(kb, set())
            raw_exposed |= cves

            # Net exposure = CVEs this missing patch fixes that are not already covered.
            net_cves = cves - covered_cves

            kb_display = next(iter(sorted(cands)), "N/A")
            mapped_patches.append({
                "kb":        kb_display,
                "title":     patch.get("title", "Unknown"),
                "severity":  patch.get("severity"),
                "product":   product.get("name"),
                "cve_count": len(net_cves),
                "cves":      sorted(net_cves),
            })
            for cve in net_cves:
                cve_to_patches.setdefault(cve, set()).add(kb_display)

    # Consolidated, de-duplicated CVE list with enrichment.
    cve_list = []
    for cve in sorted(cve_to_patches):
        meta = cve_index.get(cve, {})
        cve_list.append({
            "cve":             cve,
            "cwe":             meta.get("cwe"),
            "published_epoch": meta.get("published_epoch"),
            "fixed_by_kbs":    sorted(cve_to_patches[cve]),
        })

    # --- Console summary ---
    print(f"\n  Missing patches: {len(mapped_patches)}")
    print("-" * 70)
    for mp in mapped_patches:
        print(f"  KB {mp['kb']:<10} [{mp['severity'] or 'unknown'}]  {mp['title'][:48]}")
        print(f"     -> {mp['cve_count']} net CVE(s) remediated by this patch")

    print(f"\n  CVEs implied by missing patches (raw):        {len(raw_exposed)}")
    print(f"  CVEs already covered by installed patches:    {len(covered_cves & raw_exposed)}")
    print(f"  Net distinct CVEs the endpoint is exposed to: {len(cve_list)}")
    if cve_list:
        print("-" * 70)
        for c in cve_list[:25]:
            print(f"  {c['cve']:<18} {c.get('cwe') or '':<10} fixed_by: {', '.join(c['fixed_by_kbs'])}")
        if len(cve_list) > 25:
            print(f"  ... and {len(cve_list) - 25} more (see {os.path.basename(OUTPUT_FILE)})")

    # --- Write JSON result ---
    output = {
        "os_info": {
            "name":    os_info.get("name"),
            "version": os_info.get("version"),
            "os_id":   os_id,
            "os_type": os_type,
        },
        "source": "Analog vuln_system_associations (offline catalog)",
        "total_missing_patches":           len(mapped_patches),
        "total_cves_raw":                  len(raw_exposed),
        "total_cves_covered_by_installed": len(covered_cves & raw_exposed),
        "total_cves":                      len(cve_list),  # net exposure
        "missing_patches":                 mapped_patches,
        "cves":                            cve_list,
    }
    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        json.dump(output, f, indent=2, default=str)
    print(f"\n  Full results written to: {OUTPUT_FILE}")


if __name__ == "__main__":
    main()
