#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — find-patch
##
##  Look up an OS patch by its patch UUID in patch_system_aggregation_v2.json and show the KB,
##  bulletin id(s), data source, and every package (architecture variant) it contains — each
##  with title, release info, download URL + SHA1, and applicable OS list.
##
##  Usage:
##      python3 find-patch.py 1f1b6061-3355-4ba6-ac92-5f4e625e2cc0
##
##  Created by Chris Seiler — OPSWAT OEM Solutions Architect
###############################################################################################

import os
import sys

import _catalog as cat


def main():
    if len(sys.argv) < 2:
        print("Usage: python find-patch.py <patch_uuid>")
        sys.exit(1)
    uuid = sys.argv[1].strip().lower()

    server = cat.require_server_dir()
    print(f"Catalog patch lookup: patch_uuid {uuid}")
    print("=" * 70)

    found = None
    for rec in cat.read_records(os.path.join(server, "patch_system_aggregation_v2.json")):
        if str(rec.get("patch_uuid", "")).lower() == uuid:
            found = rec
            break

    if not found:
        print(f"\nRESULT: no patch with patch_uuid '{uuid}' found.")
        sys.exit(0)

    packages = found.get("packages") or []
    print(f"  KB               : {found.get('kb_id')}")
    print(f"  Bulletin id(s)   : {', '.join(found.get('bulletin_ids') or []) or '(none)'}")
    print(f"  Data source      : {found.get('data_source')}")
    print(f"  Last modified    : {found.get('sdk_last_modified_date')}")
    print(f"  Packages         : {len(packages)}")
    print("  " + "-" * 68)
    for pkg in packages:
        cat.print_package(pkg, indent="    ")
        print()

    print("=" * 70)
    print(f"RESULT: patch {uuid} -> KB{found.get('kb_id')}, {len(packages)} package(s).")


if __name__ == "__main__":
    main()
