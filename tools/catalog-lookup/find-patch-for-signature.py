#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — find-patch-for-signature
##
##  Given a product signature id, show the product it identifies and the patch/installer the
##  catalog offers for it: product & vendor (products.json), the patch association(s)
##  (patch_associations.json), and the latest available version + download links
##  (patch_aggregation.json).
##
##  Usage:
##      python3 find-patch-for-signature.py 3880      # e.g. Microsoft .NET Runtime 8.0 x64
##
##  Created by Chris Seiler — OPSWAT OEM Solutions Architect
###############################################################################################

import os
import sys

import _catalog as cat


def main():
    if len(sys.argv) < 2:
        print("Usage: python find-patch-for-signature.py <signature_id>   (e.g. 3880)")
        sys.exit(1)
    try:
        sig = int(sys.argv[1])
    except ValueError:
        print(f"Invalid signature id '{sys.argv[1]}' -- expected an integer.")
        sys.exit(1)

    server = cat.require_server_dir()
    sig_index, _pid_index = cat.load_products(server)

    print(f"Catalog patch lookup for signature: {sig}")
    print("=" * 70)

    info = sig_index.get(sig)
    if info:
        print(f"  Product   : {info.get('product_name')} (product_id {info.get('product_id')})")
        print(f"  Signature : {info.get('signature_name')}")
        print(f"  Vendor    : {info.get('vendor_name')}")
        print(f"  3rd-party patchable : {info.get('patchable')}")
    else:
        print("  (signature not found in products.json — still checking patch associations)")

    # patch_aggregation: _id (== patch_id) -> details
    agg = {}
    agg_path = os.path.join(server, "patch_aggregation.json")
    if os.path.isfile(agg_path):
        for rec in cat.read_records(agg_path):
            _id = rec.get("_id")
            if _id is not None:
                agg[_id] = rec

    # patch_associations for this signature
    matches = []
    pa_path = os.path.join(server, "patch_associations.json")
    if os.path.isfile(pa_path):
        for rec in cat.read_records(pa_path):
            if sig in (rec.get("v4_signatures") or []):
                matches.append(rec)

    print(f"\n  Patch association(s): {len(matches)}")
    print("  " + "-" * 68)
    if not matches:
        print("    None — this signature has no associated patch in the catalog "
              "(may be detect-only / no OPSWAT-supplied installer).")
    for rec in sorted(matches, key=lambda r: not r.get("is_latest")):
        pid = rec.get("patch_id")
        info = agg.get(pid, {})
        print(f"    patch_id {pid}  {'[LATEST]' if rec.get('is_latest') else ''}  {rec.get('title')}")
        if info:
            print(f"       latest_version : {info.get('latest_version')}")
            if info.get("release_note_link"):
                print(f"       release notes  : {info.get('release_note_link')}")
            dls = info.get("download_links") or []
            for dl in dls[:5]:
                print(f"       download ({dl.get('language', '?')}):")
                print(f"          url    : {dl.get('link')}")
                sha = dl.get("sha256") or dl.get("sha1")
                if sha:
                    label = "sha256" if dl.get("sha256") else "sha1"
                    print(f"          {label} : {sha}")
            extra = len(dls) - 5
            if extra > 0:
                print(f"       ... (+{extra} more download link(s))")
        if rec.get("os_deny"):
            print(f"       os_deny        : {rec.get('os_deny')}")

    print("\n" + "=" * 70)
    if matches:
        latest = next((agg.get(m.get("patch_id"), {}).get("latest_version")
                       for m in matches if m.get("is_latest")), None)
        print(f"RESULT: signature {sig} -> {info.get('product_name') if info else 'product'}"
              f"{(' — latest patch version ' + latest) if latest else ''}.")
    else:
        print(f"RESULT: no patch/installer is associated with signature {sig} in the catalog.")


if __name__ == "__main__":
    main()
