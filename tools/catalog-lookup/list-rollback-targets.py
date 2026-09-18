#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — list-rollback-targets
##
##  Lists every application in the catalog that has an OPSWAT-approved rollback target: an
##  earlier version whose package is flagged  is_rollback_target = true  in
##  patch_aggregation_v2.json. For each one shows the current (latest) version, the version
##  it can be rolled back to, and which signatures / architectures qualify.
##
##  The endpoint SDK has no call that returns rollback eligibility — this catalog field is
##  the only source. helloworld/python/rollback.py consumes the same flag.
##
##  Usage:
##      python3 list-rollback-targets.py [--json]
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import json
import os
import sys
from collections import defaultdict

import _catalog as cat


def collect_rollback_targets(records, sig_index):
    """Group patch records by product; return those with at least one rollback target.

    Each entry:
        {"v4_pid", "product", "vendor", "latest": [...],
         "targets": [{"version", "release_date", "patch_uuid",
                      "signatures": [...], "architectures": [...]}]}
    """
    by_pid = defaultdict(list)
    for rec in records:
        by_pid[rec.get("product", {}).get("v4_pid")].append(rec)

    results = []
    for pid, recs in by_pid.items():
        targets = {}
        for rec in recs:
            for pkg in rec.get("packages", []) or []:
                if pkg.get("is_rollback_target") is not True:
                    continue
                entry = targets.setdefault(rec.get("version"), {
                    "version":       rec.get("version"),
                    "release_date":  rec.get("release_date"),
                    "patch_uuid":    rec.get("patch_uuid"),
                    "signatures":    set(),
                    "architectures": set(),
                })
                entry["signatures"].update(rec.get("product", {}).get("v4_signatures", []))
                entry["architectures"].update(pkg.get("architectures", []))
        if not targets:
            continue

        sigs   = {s for t in targets.values() for s in t["signatures"]}
        vendor = next((sig_index[s]["vendor_name"] for s in sigs if s in sig_index), None)
        results.append({
            "v4_pid":  pid,
            "product": recs[0].get("product", {}).get("name", "Unknown"),
            "vendor":  vendor,
            "latest":  sorted({r.get("version") for r in recs if r.get("is_latest")}),
            "targets": [
                {**t, "signatures": sorted(t["signatures"]),
                      "architectures": sorted(t["architectures"])}
                for t in sorted(targets.values(), key=lambda t: t["release_date"] or "")
            ],
        })

    results.sort(key=lambda r: r["product"].lower())
    return results


def print_table(results, total_products):
    print(f"  {'Product':<38}{'Latest':<20}{'Rollback to':<20}{'Arch':<14}Signatures")
    print(f"  {'-'*38}{'-'*20}{'-'*20}{'-'*14}{'-'*18}")
    for r in results:
        latest = ", ".join(r["latest"])
        for i, t in enumerate(r["targets"]):
            name = r["product"][:37] if i == 0 else ""
            lat  = latest[:19]       if i == 0 else ""
            arch = ", ".join(a.replace("x86_64", "x64") for a in t["architectures"])
            sigs = ", ".join(str(s) for s in t["signatures"])
            flag = "  (= latest)" if t["version"] in r["latest"] else ""
            print(f"  {name:<38}{lat:<20}{t['version'][:19]:<20}{arch:<14}{sigs}{flag}")

    print(f"\n  {len(results)} of {total_products} patchable products have an approved rollback target.")
    print("  A rollback target is the specific earlier version OPSWAT has approved — not")
    print("  necessarily the previous release. Eligibility is per architecture.")


def main():
    as_json = "--json" in sys.argv

    server  = cat.require_server_dir()
    records = list(cat.read_records(os.path.join(server, "patch_aggregation_v2.json")))
    sig_index, _ = cat.load_products(server)

    total_products = len({r.get("product", {}).get("v4_pid") for r in records})
    results        = collect_rollback_targets(records, sig_index)

    if as_json:
        print(json.dumps({"products": results}, indent=2))
        return

    print("Catalog lookup: applications with an approved rollback target")
    print("=" * 70)
    print()
    print_table(results, total_products)


if __name__ == "__main__":
    main()
