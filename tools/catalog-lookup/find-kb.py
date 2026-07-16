#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — find-kb
##
##  Is a Windows KB supported by (present in) the Analog catalog? Searches kb_info.json across
##  every OS section and reports, per OS: whether the KB is in the supersedence tree (kb_tree),
##  which build(s) contain it (kb_base), whether it is a cumulative, what it supersedes, and
##  the CVEs it remediates (kb_cves).
##
##  Usage:
##      python3 find-kb.py 5094127
##      python3 find-kb.py KB5094127
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import os
import sys

import _catalog as cat


def load_kb_patch_records(server, kb):
    """All package records for a KB from patch_system_aggregation_v2.json.

    v2 groups packages under patch records (keyed by patch_uuid); a KB can span more than one
    patch record, so flatten every matching record's packages[] into a single list. Each package
    carries the same detail fields used below (title, release_date, download_link, architectures,
    severity, description, ...)."""
    out = []
    path = os.path.join(server, "patch_system_aggregation_v2.json")
    if not os.path.isfile(path):
        return out
    for rec in cat.read_records(path):
        if str(rec.get("kb_id")) == str(kb):
            out.extend(rec.get("packages") or [])
    return out


def human_size(num):
    if num is None:
        return "unknown"
    size = float(num)
    for unit in ("B", "KB", "MB", "GB"):
        if size < 1024:
            return f"{size:.0f} {unit}" if unit == "B" else f"{size:.1f} {unit}"
        size /= 1024
    return f"{size:.1f} TB"


def head_size(url, timeout=6):
    """Best-effort download size via HTTP HEAD (Content-Length). None on any failure/offline."""
    import ssl
    import urllib.request
    if not url:
        return None
    try:
        ctx = ssl._create_unverified_context()
        req = urllib.request.Request(url, method="HEAD")
        with urllib.request.urlopen(req, timeout=timeout, context=ctx) as resp:
            cl = resp.headers.get("Content-Length")
            return int(cl) if cl else None
    except Exception:
        return None


def main():
    args = [a for a in sys.argv[1:]]
    fetch_size = "--no-size" not in args
    args = [a for a in args if a != "--no-size"]
    if not args:
        print("Usage: python find-kb.py <KB> [--no-size]   (e.g. 5094127 or KB5094127)")
        print("       --no-size  skip the live download-size lookup (offline)")
        sys.exit(1)

    kb = cat.normalize_kb(args[0])
    if not kb:
        print(f"Invalid KB '{args[0]}' -- expected a number like 5094127 or KB5094127.")
        sys.exit(1)

    server = cat.require_server_dir()
    os_names = cat.load_os_names(server)

    print(f"Catalog KB lookup: KB{kb}")
    print("=" * 70)

    data = cat.load_json(os.path.join(server, "kb_info.json"))

    # id_os_map (os_id -> section label) and the per-label sections (kb_tree/kb_base/kb_cves).
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

    # label -> [os_ids] so we can print the OSes a section covers.
    label_os_ids = {}
    for oid, label in id_os_map.items():
        label_os_ids.setdefault(label, []).append(oid)

    hits = 0
    for label, section in sections.items():
        kb_tree = section.get("kb_tree") or {}
        kb_base = section.get("kb_base") or {}
        kb_cves = section.get("kb_cves") or {}

        tree_rec = kb_tree.get(kb) or kb_tree.get(str(kb))
        builds = [b for b, bd in kb_base.items()
                  if any(str(p.get("kb_id")) == kb for p in (bd or {}).get("kb_articles", []))]
        cve_entry = kb_cves.get(kb) or kb_cves.get(str(kb))

        if not (tree_rec or builds or cve_entry):
            continue  # KB not present in this OS section

        hits += 1
        os_ids = sorted(label_os_ids.get(label, []))
        os_desc = ", ".join(cat.os_label(o, os_names) for o in os_ids) or "(no os_id mapping)"
        print(f"\n[{label}]  OS ids: {os_desc}")

        if builds:
            print(f"  In build(s)     : {', '.join(sorted(builds))}")
        if tree_rec is not None:
            print(f"  In supersedence : yes  (cumulative={bool(tree_rec.get('cumulative'))})")
            supersedes = [str(x) for x in (tree_rec.get('supersede_kbs') or [])]
            if supersedes:
                print(f"  Supersedes      : {', '.join('KB'+s for s in sorted(supersedes))}")
        # CVEs remediated (kb_cves stores numeric ids).
        cve_list = []
        if cve_entry:
            raw = cve_entry.get("cves", []) if isinstance(cve_entry, dict) else cve_entry
            cve_list = sorted({c for c in (cat.normalize_numeric_cve(x) for x in raw) if c})
        print(f"  Remediates CVEs : {len(cve_list)}")
        if cve_list:
            shown = ", ".join(cve_list[:20])
            more = f"  ... (+{len(cve_list) - 20} more)" if len(cve_list) > 20 else ""
            print(f"     {shown}{more}")

    # Release details (patch_system_aggregation_v2.json) + download size (live HEAD).
    patch_recs = load_kb_patch_records(server, kb)
    if patch_recs:
        r0 = patch_recs[0]
        print("\nRelease details:")
        print(f"  Title       : {r0.get('title')}")
        print(f"  Released    : {r0.get('release_date')}")
        print(f"  Severity    : {r0.get('severity')}   Category: {r0.get('category')}   "
              f"Reboot: {r0.get('requires_reboot')}")
        desc = (r0.get("description") or "").strip().replace("\n", " ")
        if desc:
            print(f"  Description : {desc[:500]}{'...' if len(desc) > 500 else ''}")
        if r0.get("release_note_link"):
            print(f"  KB article  : {r0.get('release_note_link')}")
        # De-duplicate downloads by link (the catalog often repeats the same file per record).
        downloads = {}  # link -> (arch, sha1)
        for rec in patch_recs:
            dl = rec.get("download_link") or {}
            link = dl.get("link")
            if link and link not in downloads:
                downloads[link] = (", ".join(rec.get("architectures") or []) or "?", dl.get("sha1"))
        print(f"  Downloads   : {len(downloads)} distinct file(s)"
              f"{'' if fetch_size else '  (size lookup skipped: --no-size)'}")
        for link, (arch, sha1) in downloads.items():
            size = human_size(head_size(link)) if fetch_size else "skipped"
            print(f"    [{arch}]  size: {size}")
            print(f"       url  : {link}")
            print(f"       sha1 : {sha1 or '(none)'}")
    else:
        print("\nRelease details: not yet in patch_system_aggregation_v2.json for this KB "
              "(no release date / download URL / hash).")
        print("  The KB is still supported (see kb_info above) — its patch release metadata just "
              "hasn't been populated in this catalog yet, usually because the KB is newer than "
              "the catalog's patch-release data. A fresher catalog should include it.")

    # Explicit per-dataset presence so it's clear which files cover the KB.
    kbinfo_status = (f"FOUND in {hits} OS section(s)" if hits else "NOT FOUND")
    psa_status = (f"FOUND ({len(patch_recs)} package record(s))" if patch_recs else "NOT FOUND")
    print("\nCatalog presence:")
    print(f"  kb_info.json                  : {kbinfo_status}"
          f"    (supersedence / build / CVE data)")
    print(f"  patch_system_aggregation_v2.json : {psa_status}"
          f"    (release date / download URL / hash)")

    print("\n" + "=" * 70)
    if hits and patch_recs:
        print(f"RESULT: KB{kb} is SUPPORTED — present in BOTH kb_info.json ({hits} OS section(s)) "
              f"and patch_system_aggregation_v2.json (release details available).")
    elif hits and not patch_recs:
        print(f"RESULT: KB{kb} is SUPPORTED (kb_info.json, {hits} OS section(s)). Its release "
              f"metadata (date / download URL / hash) is just not populated in "
              f"patch_system_aggregation_v2.json yet — the catalog isn't fully updated for this KB "
              f"(typically a very recent release). A fresher catalog should include it.")
    elif not hits and patch_recs:
        print(f"RESULT: KB{kb} has a patch_system_aggregation_v2.json record (release details) but "
              f"is NOT in kb_info.json (no supersedence / CVE mapping).")
    else:
        print(f"RESULT: KB{kb} was NOT found in the catalog — absent from both kb_info.json and "
              f"patch_system_aggregation_v2.json.")


if __name__ == "__main__":
    main()
