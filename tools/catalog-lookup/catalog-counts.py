#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — catalog-counts
##
##  Print catalog-wide totals from the OPSWAT "Analog" offline catalog:
##    * Applications      - unique products and total signatures (products.json); how many of
##                          those products have vulnerability detection (i.e. a CVE mapping in
##                          vuln_associations.json); and the patchable apps (patch_aggregation_v2).
##    * Driver/firmware   - BIOS / Driver / Firmware patch counts (driver_firmware_patch_aggregation).
##    * KBs               - unique KB numbers across every OS section (kb_info.json).
##    * CVEs              - unique CVEs in cves.json, and unique CVEs referenced by mappings.
##    * CVE mappings      - OS (vuln_system_associations) + 3rd-party (vuln_associations) rows.
##
##  Usage:
##      python3 catalog-counts.py
##
##  Note: cves.json (~188 MB) and vuln_system_associations.json (~670 MB) are large; the CVE
##  section takes a minute or two and a few GB of RAM to read.
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import os
import sys
from collections import Counter

import _catalog as cat


def main():
    server = cat.require_server_dir()

    print("Catalog counts (OPSWAT Analog)")
    print("=" * 70)

    # --- Applications (products.json): unique products + total signatures --------------------
    total_signatures = 0
    unique_products = set()
    pj = os.path.join(server, "products.json")
    if os.path.isfile(pj):
        for r in cat.read_records(pj):
            product = r.get("product") or {}
            pid = product.get("id")
            if pid is not None:
                unique_products.add(pid)
            total_signatures += len(r.get("signatures") or [])

    # --- Patchable applications (patch_aggregation_v2.json, latest) --------------------------
    pav = os.path.join(server, "patch_aggregation_v2.json")
    patch_total = 0
    patch_unique = set()
    by_source = Counter()
    if os.path.isfile(pav):
        for r in cat.read_records(pav):
            if not r.get("is_latest"):
                continue
            patch_total += 1
            name = (r.get("product") or {}).get("name")
            if name:
                patch_unique.add(name)
            by_source[str(r.get("data_source") or "unknown").lower()] += 1

    # --- Driver / firmware / BIOS patches (driver_firmware_patch_aggregation.json) -----------
    # This dataset is vendor-keyed ({header, dell, lenovo, ...}); each vendor maps patch-id ->
    # record with an opswat_component (BIOS / Driver / Firmware / Application / Other).
    df_total = 0
    df_by_component = Counter()
    df_by_vendor = Counter()
    df_path = os.path.join(server, "driver_firmware_patch_aggregation.json")
    if os.path.isfile(df_path):
        dfj = cat.load_json(df_path)
        for vendor, section in dfj.items():
            if vendor == "header" or not isinstance(section, dict):
                continue
            for _pid, rec in section.items():
                if not isinstance(rec, dict):
                    continue
                df_total += 1
                df_by_vendor[vendor] += 1
                df_by_component[str(rec.get("opswat_component") or "unknown")] += 1

    # --- KBs (kb_info.json): unique KB numbers across all OS sections ------------------------
    unique_kbs = set()
    ki = cat.load_json(os.path.join(server, "kb_info.json"))
    for element in ki.get("oesis", []):
        for key, value in element.items():
            if key in ("header", "id_os_map") or not isinstance(value, dict):
                continue
            for kb in (value.get("kb_tree") or {}):
                unique_kbs.add(str(kb))
            for kb in (value.get("kb_cves") or {}):
                unique_kbs.add(str(kb))
            for _build, bd in (value.get("kb_base") or {}).items():
                for patch in (bd or {}).get("kb_articles", []):
                    k = patch.get("kb_id")
                    if k and str(k) != "0":
                        unique_kbs.add(str(k))

    # --- CVE mappings + unique CVEs (large files) --------------------------------------------
    print("Reading CVE data (large files, please wait)...")

    print("  scanning vuln_system_associations.json (OS mappings)...", flush=True)
    os_mappings = 0
    os_cves = set()
    for r in cat.read_records(os.path.join(server, "vuln_system_associations.json")):
        cve = r.get("cve")
        if cve:
            os_mappings += 1
            os_cves.add(cve)

    print("  scanning vuln_associations.json (3rd-party mappings)...", flush=True)
    tp_mappings = 0
    tp_cves = set()
    vuln_pids = set()   # products (v4_pid) that have a CVE mapping = have vulnerability detection
    for r in cat.read_records(os.path.join(server, "vuln_associations.json")):
        cve = r.get("cve")
        if cve:
            tp_mappings += 1
            tp_cves.add(cve)
            for pid in (r.get("v4_pids") or []):
                vuln_pids.add(pid)

    print("  scanning cves.json (CVE database)...", flush=True)
    cve_db = set()
    for r in cat.read_records(os.path.join(server, "cves.json")):
        cve = r.get("cve")
        if cve:
            cve_db.add(cve)

    mapped_unique = os_cves | tp_cves
    apps_with_vuln = len(vuln_pids & unique_products)

    # --- Report ------------------------------------------------------------------------------
    print("\nApplications (products.json):")
    print(f"  unique applications (products)     : {len(unique_products)}")
    print(f"    with vulnerability detection     : {apps_with_vuln}   (have a CVE mapping)")
    print(f"    detection only (no CVE mapping)  : {len(unique_products) - apps_with_vuln}")
    print(f"  total application signatures       : {total_signatures}")

    print("Patchable applications (patch_aggregation_v2.json, latest):")
    print(f"  unique applications                : {len(patch_unique)}")
    print(f"  total patch entries                : {patch_total}")
    for src in sorted(by_source):
        label = "opswat (OPSWAT-verified)" if src == "opswat" else src
        print(f"    {label:<24}: {by_source[src]}")

    print("Driver / firmware patches (driver_firmware_patch_aggregation.json):")
    print(f"  BIOS                               : {df_by_component.get('BIOS', 0)}")
    print(f"  Driver                             : {df_by_component.get('Driver', 0)}")
    print(f"  Firmware                           : {df_by_component.get('Firmware', 0)}")
    for comp in sorted(df_by_component):
        if comp not in ("BIOS", "Driver", "Firmware"):
            print(f"  {comp:<34} : {df_by_component[comp]}")
    print(f"  total                              : {df_total}")
    if df_by_vendor:
        print(f"  by vendor                          : "
              + ", ".join(f"{v}={df_by_vendor[v]}" for v in sorted(df_by_vendor)))

    print("KBs (kb_info.json):")
    print(f"  unique KBs                         : {len(unique_kbs)}")

    print("CVE mappings:")
    print(f"  OS mappings (vuln_system_associations)   : {os_mappings}")
    print(f"  3rd-party mappings (vuln_associations)   : {tp_mappings}")
    print(f"  total CVE mappings                       : {os_mappings + tp_mappings}")

    print("CVEs:")
    print(f"  unique CVEs in cves.json                 : {len(cve_db)}")
    print(f"  unique CVEs referenced by mappings       : {len(mapped_unique)}")
    print(f"    - OS-mapped unique CVEs                 : {len(os_cves)}")
    print(f"    - 3rd-party-mapped unique CVEs          : {len(tp_cves)}")

    print("\n" + "=" * 70)
    print("Summary:")
    print(f"  Applications : {len(unique_products)} unique "
          f"({apps_with_vuln} with vulnerability detection) / {total_signatures} signatures")
    print(f"  Patchable    : {len(patch_unique)} unique / {patch_total} patch entries")
    print(f"  Driver/FW    : {df_total} total "
          f"(BIOS {df_by_component.get('BIOS', 0)}, Driver {df_by_component.get('Driver', 0)}, "
          f"Firmware {df_by_component.get('Firmware', 0)})")
    print(f"  KBs          : {len(unique_kbs)} unique")
    print(f"  CVEs         : {len(cve_db)} unique")
    print(f"  CVE mappings : {os_mappings + tp_mappings} total")


if __name__ == "__main__":
    main()
