#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — find-package
##
##  Look up a single OS patch package by its package UUID. Scans the packages in
##  patch_system_aggregation_v2.json, shows the package detail (title, arch, release info,
##  download URL + SHA1, applicable OS list) and the parent patch it belongs to (patch UUID,
##  KB, bulletin id(s)).
##
##  Usage:
##      python3 find-package.py a633e05d-ad72-4553-8752-9c8cb89dbe10
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import os
import sys

import _catalog as cat


def main():
    if len(sys.argv) < 2:
        print("Usage: python find-package.py <package_uuid>")
        sys.exit(1)
    uuid = sys.argv[1].strip().lower()

    server = cat.require_server_dir()
    print(f"Catalog package lookup: package_uuid {uuid}")
    print("=" * 70)

    parent, package = None, None
    for rec in cat.read_records(os.path.join(server, "patch_system_aggregation_v2.json")):
        for pkg in rec.get("packages") or []:
            if str(pkg.get("package_uuid", "")).lower() == uuid:
                parent, package = rec, pkg
                break
        if package:
            break

    if not package:
        print(f"\nRESULT: no package with package_uuid '{uuid}' found.")
        sys.exit(0)

    print("  Parent patch:")
    print(f"    patch_uuid   : {parent.get('patch_uuid')}")
    print(f"    KB           : {parent.get('kb_id')}")
    print(f"    bulletin(s)  : {', '.join(parent.get('bulletin_ids') or []) or '(none)'}")
    print("\n  Package:")
    cat.print_package(package, indent="    ")

    print("\n" + "=" * 70)
    print(f"RESULT: package {uuid} -> KB{parent.get('kb_id')} "
          f"(patch {parent.get('patch_uuid')}).")


if __name__ == "__main__":
    main()
