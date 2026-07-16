#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — list-supported-apps
##
##  List every supported third-party application the catalog can patch, from
##  patch_aggregation_v2.json, showing each application's latest version, product signature(s),
##  and patch source (OPSWAT-verified = "opswat", or "winget"). Prints a total count and a
##  per-source breakdown at the end.
##
##  Only the latest patch per family (is_latest) is listed, so the version shown is the newest
##  the catalog offers.
##
##  Usage:
##      python3 list-supported-apps.py                 # all sources
##      python3 list-supported-apps.py --source winget # only winget
##      python3 list-supported-apps.py --source opswat # only OPSWAT-verified
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import os
import sys
from collections import Counter

import _catalog as cat


def main():
    args = sys.argv[1:]
    source_filter = None
    if "--source" in args:
        i = args.index("--source")
        if i + 1 < len(args):
            source_filter = args[i + 1].strip().lower()
        else:
            print("Usage: python list-supported-apps.py [--source winget|opswat]")
            sys.exit(1)

    server = cat.require_server_dir()
    path = os.path.join(server, "patch_aggregation_v2.json")
    if not os.path.isfile(path):
        print(f"ERROR: patch_aggregation_v2.json not found under {server}.")
        sys.exit(2)

    title = "Catalog supported applications (patch_aggregation_v2.json)"
    if source_filter:
        title += f"  [source = {source_filter}]"
    print(title)
    print("=" * 78)

    rows = []   # (source, sig_str, version, name)
    seen = set()
    for rec in cat.read_records(path):
        if not rec.get("is_latest"):
            continue
        source = str(rec.get("data_source") or "unknown").lower()
        if source_filter and source != source_filter:
            continue
        product = rec.get("product") or {}
        name = product.get("name") or "(unknown)"
        version = rec.get("version") or "?"
        sigs = product.get("v4_signatures") or []
        sig_str = ", ".join(str(s) for s in sigs) or "(none)"

        key = (source, sig_str, version, name)
        if key in seen:
            continue
        seen.add(key)
        rows.append((source, sig_str, version, name))

    rows.sort(key=lambda r: (r[0], r[3].lower(), r[1]))

    print(f"  {'SOURCE':<9} {'SIGNATURE(S)':<16} {'LATEST VERSION':<22} APPLICATION")
    print("  " + "-" * 74)
    for source, sig_str, version, name in rows:
        print(f"  {source:<9} {sig_str[:16]:<16} {str(version)[:22]:<22} {name}")

    by_source = Counter(r[0] for r in rows)
    unique_all = {r[3] for r in rows}
    print("\n" + "=" * 78)
    print(f"Total: {len(rows)} supported application patch(es) across "
          f"{len(unique_all)} unique application(s)")
    for src in sorted(by_source):
        label = "OPSWAT-verified (opswat)" if src == "opswat" else src
        unique_src = len({r[3] for r in rows if r[0] == src})
        print(f"  {label:<26}: {by_source[src]} patch(es), {unique_src} unique app(s)")


if __name__ == "__main__":
    main()
