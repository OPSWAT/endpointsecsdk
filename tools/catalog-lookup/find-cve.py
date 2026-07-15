#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — find-cve
##
##  Look up a CVE in the Analog catalog and show:
##    * details from cves.json (CWE, severity, CVSS vector, published date, description)
##    * corresponding OS patches  — KB(s) that remediate it, by OS (kb_info.json / kb_cves)
##    * corresponding 3rd-party patches — affected product(s)/signature(s), the patched
##      version to upgrade to (patch_associations + patch_aggregation), and version ranges
##    * associated CPEs (vuln_associations.json)
##
##  Usage:
##      python3 find-cve.py CVE-2024-38063
##
##  Note: cves.json is large; the first lookup takes a few seconds to load it.
##
##  Created by Chris Seiler — OPSWAT OEM Solutions Architect
###############################################################################################

import os
import sys

import _catalog as cat


def find_cve_details(server, cve):
    print("  Loading cves.json (large — one moment)...", flush=True)
    for rec in cat.read_records(os.path.join(server, "cves.json")):
        if rec.get("cve") == cve:
            return rec
    return None


def find_os_patches(server, cve):
    """KBs (by OS section) whose kb_cves list includes this CVE, with the build they live in."""
    data = cat.load_json(os.path.join(server, "kb_info.json"))
    id_os_map, sections = {}, {}
    for element in data.get("oesis", []):
        for key, value in element.items():
            if key == "header":
                continue
            if key == "id_os_map" and isinstance(value, dict):
                for k, v in value.items():
                    try:
                        id_os_map[int(k)] = str(v)
                    except (TypeError, ValueError):
                        pass
            elif isinstance(value, dict):
                sections[str(key)] = value
    label_os_ids = {}
    for oid, label in id_os_map.items():
        label_os_ids.setdefault(label, []).append(oid)

    out = []  # (label, os_ids, kb, build)
    for label, section in sections.items():
        kb_cves = section.get("kb_cves") or {}
        kb_base = section.get("kb_base") or {}
        # KB -> earliest build it appears in
        kb_build = {}
        for build, bd in kb_base.items():
            for p in (bd or {}).get("kb_articles", []):
                k = p.get("kb_id")
                if k and str(k) != "0":
                    kb_build.setdefault(str(k), build)
        for kb, entry in kb_cves.items():
            raw = entry.get("cves", []) if isinstance(entry, dict) else entry
            if any(cat.normalize_numeric_cve(x) == cve for x in raw):
                out.append((label, sorted(label_os_ids.get(label, [])),
                            str(kb), kb_build.get(str(kb))))
    return out


def find_thirdparty(server, cve):
    """3rd-party (vuln_associations) records for this CVE: signatures/pids/ranges/cpe."""
    recs = []
    for rec in cat.read_records(os.path.join(server, "vuln_associations.json")):
        if rec.get("cve") == cve:
            recs.append(rec)
    return recs


def load_patch_for_signatures(server, signatures):
    """signature -> {patch_id, title, is_latest, latest_version} via patch_associations + patch_aggregation."""
    sig_to_patch = {}
    agg = {}
    agg_path = os.path.join(server, "patch_aggregation.json")
    if os.path.isfile(agg_path):
        for rec in cat.read_records(agg_path):
            _id = rec.get("_id")
            if _id is not None:
                agg[_id] = rec
    pa_path = os.path.join(server, "patch_associations.json")
    if os.path.isfile(pa_path):
        for rec in cat.read_records(pa_path):
            sigs = set(rec.get("v4_signatures") or [])
            hit = sigs & set(signatures)
            if not hit:
                continue
            pid = rec.get("patch_id")
            info = agg.get(pid, {})
            for s in hit:
                # prefer the is_latest association
                if s not in sig_to_patch or rec.get("is_latest"):
                    sig_to_patch[s] = {
                        "patch_id":       pid,
                        "title":          rec.get("title") or info.get("product_name"),
                        "is_latest":      rec.get("is_latest"),
                        "latest_version": info.get("latest_version"),
                        "release_note":   info.get("release_note_link"),
                    }
    return sig_to_patch


def load_kb_release_details(server, kb_ids):
    """kb_id(str) -> release details from patch_system_aggregation_v2.json (first match per KB):
    the OS patch's release title/date/severity/category, KB article (release note) link,
    download link, and reboot flag for the KB(s) that remediate the CVE."""
    wanted = {str(k) for k in kb_ids}
    out = {}
    path = os.path.join(server, "patch_system_aggregation_v2.json")
    if not wanted or not os.path.isfile(path):
        return out
    for rec in cat.read_records(path):
        kid = rec.get("kb_id")
        if kid is None:
            continue
        ks = str(kid)
        if ks not in wanted or ks in out:
            continue
        pkgs = rec.get("packages") or []   # v2 nests the detail under packages[]
        if not pkgs:
            continue
        p = pkgs[0]
        out[ks] = {
            "product_name":      p.get("product_name"),
            "title":             p.get("title"),
            "release_date":      p.get("release_date"),
            "severity":          p.get("severity"),
            "category":          p.get("category"),
            "requires_reboot":   p.get("requires_reboot"),
            "release_note_link": p.get("release_note_link"),
            "download_link":     (p.get("download_link") or {}).get("link"),
        }
        if len(out) == len(wanted):
            break
    return out


def main():
    if len(sys.argv) < 2:
        print("Usage: python find-cve.py <CVE>    (e.g. CVE-2024-38063)")
        sys.exit(1)
    cve = sys.argv[1].strip().upper()
    if not cve.startswith("CVE-"):
        cve = "CVE-" + cve

    server = cat.require_server_dir()
    os_names = cat.load_os_names(server)
    _sig_index, pid_index = cat.load_products(server)

    print(f"Catalog CVE lookup: {cve}")
    print("=" * 70)

    # 1) CVE details
    rec = find_cve_details(server, cve)
    if rec:
        cvss3 = rec.get("cvss_3_0") or {}
        cvss2 = rec.get("cvss_2_0") or {}
        print(f"  CWE           : {rec.get('cwe')}")
        print(f"  Severity      : {rec.get('severity')}")
        print(f"  CVSS 3.x      : {cvss3.get('base_score') or cvss3.get('impact_score') or '?'}"
              f"  {cvss3.get('vector_string') or ''}")
        if cvss2:
            print(f"  CVSS 2.0      : {cvss2.get('score')}")
        print(f"  Published     : {cat.fmt_epoch(rec.get('published_epoch')) or 'unknown'}")
        desc = (rec.get("description") or "").strip().replace("\n", " ")
        if desc:
            print(f"  Description   : {desc[:400]}{'...' if len(desc) > 400 else ''}")
    else:
        print("  (not found in cves.json — no NVD-style metadata; still checking patches/CPEs)")

    # 2) OS patches (KBs) + release details
    os_patches = find_os_patches(server, cve)
    kb_groups = {}  # kb -> {builds, os_ids, labels}
    for label, os_ids, kb, build in os_patches:
        g = kb_groups.setdefault(kb, {"builds": set(), "os_ids": set(), "labels": set()})
        if build:
            g["builds"].add(build)
        g["os_ids"].update(os_ids)
        g["labels"].add(label)
    kb_release = load_kb_release_details(server, kb_groups.keys())

    print(f"\n  OS patches (KB) : {len(kb_groups)} distinct KB(s)")
    print("  " + "-" * 68)
    for kb in sorted(kb_groups):
        g = kb_groups[kb]
        rel = kb_release.get(kb, {})
        builds = ", ".join(sorted(g["builds"])) or "?"
        osd = ", ".join(cat.os_label(o, os_names) for o in sorted(g["os_ids"]))
        print(f"    KB{kb}   build(s): {builds}")
        if rel:
            title = rel.get("title") or rel.get("product_name")
            if title:
                print(f"       release : {title}")
            meta = []
            if rel.get("release_date"):    meta.append(f"released {rel['release_date']}")
            if rel.get("severity"):        meta.append(f"severity {rel['severity']}")
            if rel.get("category"):        meta.append(str(rel["category"]))
            if rel.get("requires_reboot") is not None:
                meta.append(f"reboot={rel['requires_reboot']}")
            if meta:
                print(f"                 {', '.join(meta)}")
            if rel.get("release_note_link"):
                print(f"       notes   : {rel['release_note_link']}")
            if rel.get("download_link"):
                print(f"       download: {rel['download_link']}")
        print(f"       os_ids  : {osd}")

    # 3) Third-party products + corresponding patch + CPEs
    tp = find_thirdparty(server, cve)
    all_sigs, cpes = set(), set()
    for r in tp:
        all_sigs.update(r.get("v4_signatures") or [])
        if r.get("cpe"):
            cpes.add(r["cpe"])
    sig_patch = load_patch_for_signatures(server, all_sigs) if all_sigs else {}

    print(f"\n  3rd-party products affected : {len(tp)} association(s)")
    print("  " + "-" * 68)
    seen = set()
    for r in tp:
        pids = r.get("v4_pids") or []
        names = ", ".join(pid_index.get(p, f"pid {p}") for p in pids) or "(unknown product)"
        ranges = "; ".join(f"{rg.get('start','')}..{rg.get('limit', rg.get('limit_except',''))}"
                           for rg in (r.get("ranges") or []))
        key = (names, ranges)
        if key in seen:
            continue
        seen.add(key)
        print(f"    {names}")
        if ranges:
            print(f"       vulnerable versions: {ranges}")
        for s in (r.get("v4_signatures") or []):
            p = sig_patch.get(s)
            if p and p.get("latest_version"):
                print(f"       -> patch: upgrade to {p['latest_version']} "
                      f"(sig {s}, patch_id {p.get('patch_id')})")
                if p.get("release_note"):
                    print(f"          release notes: {p['release_note']}")

    # 4) CPEs
    print(f"\n  CPEs : {len(cpes)}")
    for c in sorted(cpes):
        print(f"    {c}")

    print("\n" + "=" * 70)
    total = len(os_patches) + len(tp)
    if rec or total:
        print(f"RESULT: {cve} found — {len(os_patches)} OS KB(s), {len(tp)} 3rd-party "
              f"association(s), {len(cpes)} CPE(s).")
    else:
        print(f"RESULT: {cve} not found anywhere in the catalog.")


if __name__ == "__main__":
    main()
