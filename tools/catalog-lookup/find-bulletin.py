#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — find-bulletin
##
##  Find the patch(es) associated with a security bulletin id (or any substring of one, e.g.
##  "MS22-0110" or "5009497"). Searches patch_system_aggregation_v2.json's bulletin_ids and
##  shows, for each match, the bulletin, patch UUID, KB, and the package(s) it delivers.
##
##  Usage:
##      python3 find-bulletin.py MS22-0110-5009497
##      python3 find-bulletin.py MS22-0110
##
##  Created by Chris Seiler — OPSWAT OEM Solutions Architect
###############################################################################################

import os
import sys

import _catalog as cat


def main():
    if len(sys.argv) < 2:
        print("Usage: python find-bulletin.py <bulletin id or substring>   (e.g. MS22-0110-5009497)")
        sys.exit(1)
    needle = sys.argv[1].strip().lower()

    server = cat.require_server_dir()
    print(f"Catalog bulletin lookup: '{needle}'")
    print("=" * 70)

    matches = []
    for rec in cat.read_records(os.path.join(server, "patch_system_aggregation_v2.json")):
        hit = [b for b in (rec.get("bulletin_ids") or []) if needle in str(b).lower()]
        if hit:
            matches.append((hit, rec))

    if not matches:
        print(f"\nRESULT: no bulletin matching '{needle}' found.")
        sys.exit(0)

    for hit, rec in matches:
        packages = rec.get("packages") or []
        print(f"\n  Bulletin(s) : {', '.join(hit)}")
        print(f"     patch_uuid : {rec.get('patch_uuid')}")
        print(f"     KB         : {rec.get('kb_id')}")
        print(f"     packages   : {len(packages)}")
        for pkg in packages:
            arch = ", ".join(pkg.get("architectures") or []) or "?"
            print(f"        - [{arch}] {pkg.get('title')}")
            print(f"             package_uuid: {pkg.get('package_uuid')}")

    print("\n" + "=" * 70)
    distinct_kbs = sorted({str(r.get("kb_id")) for _h, r in matches})
    print(f"RESULT: {len(matches)} patch(es) match '{needle}' "
          f"(KB(s): {', '.join(distinct_kbs)}). Use find-patch.py <patch_uuid> for full detail.")


if __name__ == "__main__":
    main()
