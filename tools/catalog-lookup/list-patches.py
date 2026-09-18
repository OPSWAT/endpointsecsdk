#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — list-patches
##
##  Given a product signature id, lists every 3rd-party patch (version) the catalog holds for
##  it in patch_aggregation_v2.json, with every package under each patch: package_uuid,
##  architectures, SHA256, download links and the is_rollback_target flag, plus the install
##  flags (requires_close_first / requires_uninstall_first / requires_restart).
##
##  This is the lookup the endpoint SDK does not provide. GetPackages (50306) needs a
##  patch_uuid; GetLatestInstaller (50300) returns only the latest version; nothing on the
##  endpoint turns a signature into the set of patch_uuids. --json output is shaped the way
##  such a call — GetPatches(signature) — would return it.
##
##  Usage:
##      python3 list-patches.py <signature_id> [--json] [--latest]
##
##  Examples:
##      python3 list-patches.py 3241            # Notepad++ x64, every version
##      python3 list-patches.py 3241 --latest   # only the current version
##      python3 list-patches.py 3241 --json     # GetPatches-shaped JSON
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import json
import os
import sys

import _catalog as cat


def release_key(rec):
    """Sort key for MM/DD/YYYY release dates -> (YYYY, MM, DD)."""
    parts = (rec.get("release_date") or "").split("/")
    return (parts[2], parts[0], parts[1]) if len(parts) == 3 else ("", "", "")


def get_patches(records, signature_id, latest_only=False):
    """signature -> patches -> packages. Returns a GetPatches-shaped dict or None."""
    mine = [r for r in records
            if signature_id in (r.get("product", {}).get("v4_signatures") or [])]
    if not mine:
        return None
    if latest_only:
        mine = [r for r in mine if r.get("is_latest")]
    mine.sort(key=release_key)

    patches = []
    for r in mine:
        packages = [{
            "package_uuid":       p.get("package_uuid"),
            "architectures":      p.get("architectures") or [],
            "sha256":             p.get("sha256"),
            "download_links":     p.get("download_links") or [],
            "is_rollback_target": bool(p.get("is_rollback_target")),
        } for p in (r.get("packages") or [])]

        patches.append({
            "patch_uuid":               r.get("patch_uuid"),
            "version":                  r.get("version"),
            "release_date":             r.get("release_date"),
            "data_source":              r.get("data_source"),
            "is_latest":                bool(r.get("is_latest")),
            "is_rollback_target":       any(p["is_rollback_target"] for p in packages),
            "requires_close_first":     r.get("requires_close_first"),
            "requires_uninstall_first": r.get("requires_uninstall_first"),
            "requires_restart":         r.get("requires_restart"),
            "release_notes_link":       r.get("release_notes_link"),
            "packages":                 packages,
        })

    product = mine[0].get("product", {})
    return {"signature": signature_id, "v4_pid": product.get("v4_pid"),
            "product": product.get("name", "Unknown"), "patches": patches}


def print_report(result, sig_info):
    print(f"Catalog patch lookup: signature {result['signature']}")
    print("=" * 70)
    print(f"  product   : {result['product']}  (v4_pid {result['v4_pid']})")
    if sig_info:
        print(f"  vendor    : {sig_info.get('vendor_name')}")
        print(f"  signature : {sig_info.get('signature_name')}")
    print(f"  patches   : {len(result['patches'])}")

    for patch in result["patches"]:
        tags = []
        if patch["is_latest"]:          tags.append("LATEST")
        if patch["is_rollback_target"]: tags.append("ROLLBACK TARGET")
        print(f"\n  {patch['version']}  released {patch['release_date']}  "
              f"[{patch['data_source']}]   {'  '.join(tags)}")
        print(f"    patch_uuid : {patch['patch_uuid']}")
        print(f"    install    : close_first={patch['requires_close_first']}  "
              f"uninstall_first={patch['requires_uninstall_first']}  "
              f"restart={patch['requires_restart']}")
        if patch["release_notes_link"]:
            print(f"    notes      : {patch['release_notes_link']}")
        print(f"    packages   : {len(patch['packages'])}")
        for p in patch["packages"]:
            arch = ", ".join(a.replace("x86_64", "x64") for a in p["architectures"]) or "-"
            rb   = "rollback=yes" if p["is_rollback_target"] else "rollback=no "
            print(f"      {p['package_uuid']}  {arch:<12} {rb}  sha256={p['sha256'] or '-'}")
            for link in p["download_links"]:
                print(f"          {link}")

    targets = [p["version"] for p in result["patches"] if p["is_rollback_target"]]
    print(f"\n  Approved rollback targets: {', '.join(targets) if targets else 'none'}")


def main():
    args        = [a for a in sys.argv[1:] if not a.startswith("--")]
    as_json     = "--json" in sys.argv
    latest_only = "--latest" in sys.argv

    if not args:
        print("Usage: python list-patches.py <signature_id> [--json] [--latest]")
        print("       (find-signature.py <name> gives you the signature id)")
        sys.exit(1)
    try:
        signature_id = int(args[0])
    except ValueError:
        print(f"ERROR: signature id must be an integer, got '{args[0]}'")
        sys.exit(1)

    server  = cat.require_server_dir()
    records = cat.read_records(os.path.join(server, "patch_aggregation_v2.json"))
    result  = get_patches(records, signature_id, latest_only)

    if not result:
        sig_index, _ = cat.load_products(server)
        info = sig_index.get(signature_id)
        if info:
            print(f"RESULT: signature {signature_id} is {info['product_name']} "
                  f"({info['vendor_name']}) but has no 3rd-party patches in the catalog"
                  f"{' (patchable=' + str(info['patchable']) + ')' if info.get('patchable') is not None else ''}.")
        else:
            print(f"RESULT: signature {signature_id} not found in products.json.")
        sys.exit(0)

    if as_json:
        print(json.dumps(result, indent=2))
        return

    sig_index, _ = cat.load_products(server)
    print_report(result, sig_index.get(signature_id))


if __name__ == "__main__":
    main()
