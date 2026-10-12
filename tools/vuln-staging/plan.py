#!/usr/bin/env python3
###############################################################################################
##  Vuln Staging — plan
##
##  Chooses, for every app in the staging list, an OLDER version that the catalog says is
##  vulnerable, and writes plan.json: the chosen version first, then older vulnerable
##  fallbacks in order, each with its x64 package (package_uuid, sha256, download links) and
##  the CVEs that version should be flagged for.
##
##  Version source: patch_aggregation_v2.json holds every version the catalog knows. The
##  endpoint SDK cannot list old versions, so this is the only place to get them.
##
##  CVE source: vuln_associations.json (v4_pid -> CVE -> version ranges), the data v2mod.dat is
##  built from. A version is only counted as vulnerable when start <= version < limit. The
##  data uses limit as the last vulnerable version (inclusive; e.g. '9.2.999998'), but taking
##  the strict reading means the expected CVE list is right under either interpretation.
##  CVEs where version == limit are listed separately as 'limit_equal_cves'.
##
##  Usage:
##      python plan.py [--min-cves N] [--prefer-cves N] [--fallbacks N] [--apps k1,k2]
##                     [--no-cvss] [--out PATH]
##
##  Ordering: versions with >= --prefer-cves CVEs (default 3) come first, newest first; then
##  versions with >= --min-cves (default 1). Every candidate has at least one CVE.
##
##  Examples:
##      python plan.py                              # every app, 5 fallbacks each
##      python plan.py --apps 7zip,notepadpp        # just these
##      python plan.py --min-cves 10 --out C:\vuln-staging\plan.json
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import collections
import os
import sys
from datetime import datetime, timezone

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "catalog-lookup"))

import _catalog as cat                          # noqa: E402
import _staging as st                           # noqa: E402

DEFAULT_OUT = r"C:\vuln-staging\plan.json"


def parse_args(argv):
    opts = {"min_cves": 1, "prefer_cves": 3, "fallbacks": 5, "apps": None, "cvss": True, "out": DEFAULT_OUT}
    it = iter(argv[1:])
    for a in it:
        if a == "--min-cves":
            opts["min_cves"] = int(next(it))
        elif a == "--prefer-cves":
            opts["prefer_cves"] = int(next(it))
        elif a == "--fallbacks":
            opts["fallbacks"] = int(next(it))
        elif a == "--apps":
            opts["apps"] = [k.strip() for k in next(it).split(",") if k.strip()]
        elif a == "--no-cvss":
            opts["cvss"] = False
        elif a == "--out":
            opts["out"] = next(it)
        else:
            print(f"Unknown option {a}")
            print("Usage: python plan.py [--min-cves N] [--prefer-cves N] [--fallbacks N] "
                  "[--apps k1,k2] [--no-cvss] [--out PATH]")
            sys.exit(1)
    return opts


def load_vuln_ranges(server):
    """v4_pid -> {cve: [(start_key, limit_key), ...]} for Windows associations."""
    data = cat.load_json(os.path.join(server, "vuln_associations.json"))
    out = collections.defaultdict(lambda: collections.defaultdict(list))
    for element in data.get("oesis", []):
        for rec in (element.get("vuln_associations") or {}).values():
            for pid in rec.get("v4_pids") or []:
                if pid >= st.WINDOWS_PID_LIMIT:
                    continue
                for rg in rec.get("ranges") or []:
                    out[pid][rec["cve"]].append((st.vkey(rg.get("start")), st.vkey(rg.get("limit")),
                                                 rg.get("start"), rg.get("limit")))
    return out


def cves_for(version, cve_ranges):
    """Return (strict, limit_equal) CVE lists for a version."""
    v = st.vkey(version)
    strict, equal = [], []
    for cve, ranges in cve_ranges.items():
        if any(s <= v < l for s, l, _, _ in ranges):
            strict.append(cve)
        elif any(s <= v == l for s, l, _, _ in ranges):
            equal.append(cve)
    return sorted(strict), sorted(equal)


def pick_x64_package(rec):
    """The x64 package with a sha256 and a link, preferring en-US. (package, arch_note) or (None, why)."""
    pkgs = [p for p in rec.get("packages") or [] if p.get("sha256") and p.get("download_links")]
    if not pkgs:
        return None, "no package with sha256 + download link"

    def lang_rank(p):
        langs = p.get("languages") or []
        return 0 if (not langs or "en-US" in langs) else 1

    explicit = [p for p in pkgs if "x86_64" in (p.get("architectures") or [])]
    if explicit:
        return sorted(explicit, key=lang_rank)[0], "x86_64"
    # Package arch left blank: only trust it when the record itself is x64-only.
    compat = rec.get("compatible_architectures") or []
    blank = [p for p in pkgs if not p.get("architectures")]
    if blank and compat == ["x86_64"]:
        return sorted(blank, key=lang_rank)[0], "x86_64 (from record)"
    return None, f"no x64 package (record arch {compat})"


def plan_app(app, records, cve_ranges, opts):
    mine = [r for r in records if (r.get("product") or {}).get("v4_pid") == app["pid"]]
    entry = {"key": app["key"], "name": app["name"], "pid": app["pid"], "pin": app["pin"],
             "signatures": sorted({s for r in mine for s in (r["product"].get("v4_signatures") or [])}),
             "catalog_versions": len({r.get("version") for r in mine}),
             "candidates": [], "rejected": []}
    if not mine:
        entry["skipped_reason"] = "product not in patch_aggregation_v2.json"
        return entry

    latest = sorted({r["version"] for r in mine if r.get("is_latest")}, key=st.vkey)
    entry["latest_versions"] = latest
    entry["newest_version"] = latest[-1] if latest else None
    if not latest:
        entry["skipped_reason"] = "catalog has no is_latest record"
        return entry
    newest = st.vkey(latest[-1])

    seen = set()
    for rec in sorted(mine, key=lambda r: st.vkey(r.get("version")), reverse=True):
        ver = rec.get("version")
        if ver in seen or rec.get("is_latest") or st.vkey(ver) >= newest:
            continue
        why = []
        if rec.get("data_source") != "opswat":
            why.append(f"data_source={rec.get('data_source')}")
        if rec.get("fresh_installable") != "yes":
            why.append(f"fresh_installable={rec.get('fresh_installable')}")
        pkg, arch_note = pick_x64_package(rec)
        if not pkg:
            why.append(arch_note)
        strict, equal = cves_for(ver, cve_ranges)
        if len(strict) < opts["min_cves"]:
            why.append(f"{len(strict)} CVEs (< {opts['min_cves']})")
        if why:
            entry["rejected"].append({"version": ver, "why": why})
            continue
        seen.add(ver)
        # The patch the box should be told is missing: the next is_latest above this version
        # (the same release track for multi-track products such as Wireshark).
        above = [v for v in latest if st.vkey(v) > st.vkey(ver)]
        entry["candidates"].append({
            "version":                  ver,
            "patch_uuid":               rec.get("patch_uuid"),
            "release_date":             rec.get("release_date"),
            "requires_uninstall_first": rec.get("requires_uninstall_first"),
            "requires_close_first":     rec.get("requires_close_first"),
            "expected_patch_version":   above[0] if above else latest[-1],
            "package": {
                "package_uuid":  pkg.get("package_uuid"),
                "sha256":        pkg.get("sha256", "").lower(),
                "download_links": pkg.get("download_links"),
                "architecture":  arch_note,
                "languages":     pkg.get("languages") or [],
            },
            "expected_cves":    strict,
            "limit_equal_cves": equal,
        })

    # Strong candidates (>= prefer_cves) first, each group newest first; then cap the list.
    entry["candidates"].sort(key=lambda c: st.vkey(c["version"]), reverse=True)
    entry["candidates"].sort(key=lambda c: len(c["expected_cves"]) < opts["prefer_cves"])
    entry["candidates"] = entry["candidates"][:opts["fallbacks"] + 1]

    if not entry["candidates"]:
        entry["skipped_reason"] = "no older x64 opswat version with enough CVEs"
    return entry


def attach_cvss(server, plan):
    """One pass over cves.json; adds severity/score/EPSS/KEV to every expected CVE."""
    wanted = {c for a in plan["apps"] for cand in a["candidates"] for c in cand["expected_cves"]}
    if not wanted:
        return
    print("  Loading cves.json (large — one moment)...", flush=True)
    info = {}
    for rec in cat.read_records(os.path.join(server, "cves.json")):
        cve = rec.get("cve")
        if cve in wanted:
            v3 = rec.get("cvss_3_0") or {}
            info[cve] = {"severity": rec.get("severity"),
                         "cvss3": v3.get("base_score"),
                         "epss": (rec.get("epss") or {}).get("score"),
                         "kev": bool(rec.get("cisa"))}
    for a in plan["apps"]:
        for cand in a["candidates"]:
            cand["cve_details"] = {c: info.get(c) for c in cand["expected_cves"]}
            sev = collections.Counter((info.get(c) or {}).get("severity") or "UNKNOWN"
                                      for c in cand["expected_cves"])
            cand["severity_counts"] = dict(sev)


def main():
    opts = parse_args(sys.argv)
    server = cat.require_server_dir()
    apps = st.APPS
    if opts["apps"]:
        unknown = set(opts["apps"]) - {a["key"] for a in apps}
        if unknown:
            print(f"ERROR: unknown app key(s) {sorted(unknown)}. Known: {[a['key'] for a in apps]}")
            sys.exit(1)
        apps = [a for a in apps if a["key"] in opts["apps"]]

    print("Vuln staging plan")
    print("  Reading patch_aggregation_v2.json ...", flush=True)
    records = list(cat.read_records(os.path.join(server, "patch_aggregation_v2.json")))
    print("  Reading vuln_associations.json ...", flush=True)
    ranges = load_vuln_ranges(server)

    agg_path = os.path.join(server, "patch_aggregation_v2.json")
    plan = {
        "generated":        datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
        "catalog_modified": datetime.fromtimestamp(os.path.getmtime(agg_path),
                                                   tz=timezone.utc).strftime("%Y-%m-%d"),
        "min_cves":         opts["min_cves"],
        "prefer_cves":      opts["prefer_cves"],
        "range_rule":       "start <= version < limit",
        "apps":             [plan_app(a, records, ranges[a["pid"]], opts) for a in apps],
    }
    if opts["cvss"]:
        attach_cvss(server, plan)

    st.save_json(opts["out"], plan)

    print(f"\n  {'App':<26} {'Pick':<16} {'CVEs':>5}  {'Expected patch':<18} Fallbacks")
    print("  " + "-" * 86)
    for a in plan["apps"]:
        if not a["candidates"]:
            print(f"  {a['name']:<26} SKIPPED: {a.get('skipped_reason')}")
            continue
        c = a["candidates"][0]
        fb = ", ".join(x["version"] for x in a["candidates"][1:])
        print(f"  {a['name']:<26} {c['version']:<16} {len(c['expected_cves']):>5}  "
              f"{c['expected_patch_version']:<18} {fb}")
    ok = sum(1 for a in plan["apps"] if a["candidates"])
    print(f"\n  {ok}/{len(plan['apps'])} apps have a vulnerable version.  Plan: {opts['out']}")


if __name__ == "__main__":
    main()
