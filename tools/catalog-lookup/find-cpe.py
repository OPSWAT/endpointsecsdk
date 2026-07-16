#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — find-cpe
##
##  Search the catalog for a CPE (or any substring of one) and show what it maps to:
##  the matching CPE string(s), the product(s)/signature(s) they belong to, and the CVEs
##  associated with each (from vuln_associations.json, the 3rd-party CVE<->product<->CPE table).
##
##  Usage:
##      python3 find-cpe.py cpe:/a:microsoft:.net
##      python3 find-cpe.py "google:chrome"
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import os
import sys

import _catalog as cat


def main():
    if len(sys.argv) < 2:
        print("Usage: python find-cpe.py <cpe or substring>   (e.g. cpe:/a:microsoft:.net)")
        sys.exit(1)
    needle = sys.argv[1].strip().lower()

    server = cat.require_server_dir()
    _sig_index, pid_index = cat.load_products(server)

    print(f"Catalog CPE lookup: '{needle}'")
    print("=" * 70)

    # cpe -> {pids:set, sigs:set, cves:set}
    by_cpe = {}
    for rec in cat.read_records(os.path.join(server, "vuln_associations.json")):
        cpe = rec.get("cpe")
        if not cpe or needle not in cpe.lower():
            continue
        entry = by_cpe.setdefault(cpe, {"pids": set(), "sigs": set(), "cves": set()})
        entry["pids"].update(rec.get("v4_pids") or [])
        entry["sigs"].update(rec.get("v4_signatures") or [])
        if rec.get("cve"):
            entry["cves"].add(rec["cve"])

    if not by_cpe:
        print(f"\nRESULT: no CPE matching '{needle}' found in the catalog.")
        return

    total_cves = set()
    for cpe in sorted(by_cpe):
        e = by_cpe[cpe]
        total_cves |= e["cves"]
        names = ", ".join(pid_index.get(p, f"pid {p}") for p in sorted(e["pids"])) or "(unknown)"
        print(f"\n  {cpe}")
        print(f"     product(s)   : {names}")
        print(f"     signature(s) : {', '.join(str(s) for s in sorted(e['sigs'])) or '(none)'}")
        print(f"     CVEs         : {len(e['cves'])}")
        sample = sorted(e["cves"])[:15]
        if sample:
            more = f"  ... (+{len(e['cves']) - 15} more)" if len(e["cves"]) > 15 else ""
            print(f"        {', '.join(sample)}{more}")

    print("\n" + "=" * 70)
    print(f"RESULT: {len(by_cpe)} matching CPE(s), {len(total_cves)} distinct CVE(s).")


if __name__ == "__main__":
    main()
