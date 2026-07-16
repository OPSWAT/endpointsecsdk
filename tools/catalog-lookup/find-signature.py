#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — find-signature
##
##  Given an application name (or any part of one), return the matching product signature id(s)
##  from products.json. Matches against the product name, each signature's name, and the
##  product's marketing names (case-insensitive substring). Use the returned signature id with
##  find-patch-for-signature.py to see the available patch/installer.
##
##  Usage:
##      python3 find-signature.py chrome
##      python3 find-signature.py ".net runtime"
##      python3 find-signature.py "visual studio 2022"
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import os
import sys

import _catalog as cat


def marketing_strings(rec):
    out = []
    for m in rec.get("marketing_names", []) or []:
        if isinstance(m, str):
            out.append(m)
        elif isinstance(m, dict):
            out.append(m.get("name") or m.get("value") or "")
    return [s for s in out if s]


def main():
    if len(sys.argv) < 2:
        print("Usage: python find-signature.py <application name>   (e.g. chrome)")
        sys.exit(1)
    needle = " ".join(sys.argv[1:]).strip().lower()

    server = cat.require_server_dir()

    print(f"Catalog signature lookup by name: '{needle}'")
    print("=" * 70)

    # matches[(product_name)] -> list of (sig_id, sig_name, vendor, patchable, matched_on)
    matches = []
    for rec in cat.read_records(os.path.join(server, "products.json")):
        product = rec.get("product", {}) or {}
        vendor = rec.get("vendor", {}) or {}
        pname = product.get("name") or ""
        markets = marketing_strings(rec)
        product_hit = needle in pname.lower() or any(needle in m.lower() for m in markets)

        for sig in rec.get("signatures", []) or []:
            sid = sig.get("id")
            sname = sig.get("name") or ""
            if sid is None:
                continue
            sig_hit = needle in sname.lower()
            if product_hit or sig_hit:
                matched_on = "signature" if sig_hit else "product/marketing"
                matches.append({
                    "sig_id":       sid,
                    "sig_name":     sname,
                    "product_name": pname,
                    "vendor":       vendor.get("name"),
                    "patchable":    sig.get("support_3rd_party_patch"),
                    "matched_on":   matched_on,
                })

    if not matches:
        print(f"\nRESULT: no product/signature name matching '{needle}' found.")
        return

    matches.sort(key=lambda m: (str(m["product_name"]).lower(), m["sig_id"]))
    print(f"\n  {'SIG':>6}  {'SIGNATURE NAME':<40} {'VENDOR':<22} PATCHABLE")
    print("  " + "-" * 84)
    for m in matches:
        print(f"  {m['sig_id']:>6}  {str(m['sig_name'])[:40]:<40} "
              f"{str(m['vendor'] or '')[:22]:<22} {m['patchable']}")

    distinct_products = sorted({m["product_name"] for m in matches})
    print("\n" + "=" * 70)
    print(f"RESULT: {len(matches)} signature(s) across {len(distinct_products)} product(s) "
          f"match '{needle}'.")
    if len(distinct_products) > 1:
        print("  products: " + ", ".join(distinct_products))
    print("  Tip: python find-patch-for-signature.py <SIG> to see the patch/installer.")


if __name__ == "__main__":
    main()
