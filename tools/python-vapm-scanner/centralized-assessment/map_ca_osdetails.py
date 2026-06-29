#!/usr/bin/env python3
###############################################################################################
##  VAPM Centralized Assessment — Map OS Details to Missing Patches & CVEs
##  Reference Implementation using the OESIS "Analog" offline catalog
##
##  Mirrors the server-side CVE mapping logic from the Analog ruby sample code
##  (get_system_vuln.rb), which branches on os_type. This mapper does the same, with one
##  detection routine per platform:
##
##    detect_windows_cves (os_type 1) — KB / patch supersedence model (get_windows_vuln.rb):
##        load kb_info.json (supersedence graph + kb_base) and vuln_system_associations
##        (KB->CVE by os_id), recursively expand installed & missing KBs through
##        supersedence, then net exposure = affected CVEs - CVEs fixed by installed KBs.
##
##    detect_mac_cves (os_type 4) — OS-version-range model: a CVE applies when the endpoint
##        os_id matches an affected_os entry and the OS version falls inside its range.
##
##    detect_linux_cves (os_type 2) — package-version-range model: a CVE applies when an
##        installed package's version falls inside an affected range for the normalized
##        distro name; the fix is the range's limit_except (patched package version).
##
##  Usage:
##      python3 scan-ca-osdetails.py     # gather OS details first
##      python3 map_ca_osdetails.py
##
##  Reads:  scan-ca-endpoint-result.json or scan-ca-osdetails-result.json
##          OPSWAT-SDK/extract/analog/server/{vuln_system_associations,kb_info,cves}.json
##  Writes: map-ca-osdetails-result.json
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import json
import os
import re
import sys
from collections import deque

# Force UTF-8 console output so non-ASCII text doesn't crash on Windows (cp1252).
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ENDPOINT_RESULT = os.path.join(SCRIPT_DIR, "scan-ca-endpoint-result.json")
RESULT_FILE = os.path.join(SCRIPT_DIR, "scan-ca-osdetails-result.json")
OUTPUT_FILE = os.path.join(SCRIPT_DIR, "map-ca-osdetails-result.json")

OS_TYPE_WINDOWS = 1
OS_TYPE_LINUX   = 2
OS_TYPE_MACOS   = 4


# ===========================================================================
# Shared data loading helpers
# ===========================================================================

def load_osdetails_scan():
    if os.path.isfile(ENDPOINT_RESULT):
        with open(ENDPOINT_RESULT, "r", encoding="utf-8") as f:
            data = json.load(f)
        if data.get("osdetails") is not None:
            return data["osdetails"]
    if os.path.isfile(RESULT_FILE):
        with open(RESULT_FILE, "r", encoding="utf-8") as f:
            return json.load(f)
    return None


def find_analog_server_dir():
    current = SCRIPT_DIR
    while True:
        if os.path.isfile(os.path.join(current, "sdkroot")):
            server = os.path.join(current, "OPSWAT-SDK", "extract", "analog", "server")
            return server if os.path.isdir(server) else None
        parent = os.path.dirname(current)
        if parent == current:
            return None
        current = parent


def read_analog_records(path):
    with open(path, "r", encoding="utf-8") as f:
        data = json.load(f)
    for element in data.get("oesis", []):
        for key, value in element.items():
            if key == "header":
                continue
            if isinstance(value, dict):
                for record in value.values():
                    yield record


def read_associations(server_dir, os_type):
    # Yield vuln_system_associations records for a single OS family (mirrors the
    # DataReader.query({os_type: ...}) filter in the ruby samples).
    path = os.path.join(server_dir, "vuln_system_associations.json")
    for rec in read_analog_records(path):
        if rec.get("os_type") == os_type and rec.get("cve"):
            yield rec


def build_cve_index(server_dir):
    # Enrich CVEs with the metadata available in cves.json (cwe, published date).
    index = {}
    path = os.path.join(server_dir, "cves.json")
    if not os.path.isfile(path):
        return index
    for rec in read_analog_records(path):
        cve = rec.get("cve")
        if cve:
            index[cve] = {
                "cwe":             rec.get("cwe"),
                "published_epoch": rec.get("published_epoch"),
            }
    return index


def enrich_cve(cve, cve_index, **extra):
    meta = cve_index.get(cve, {})
    rec = {"cve": cve, "cwe": meta.get("cwe"), "published_epoch": meta.get("published_epoch")}
    rec.update(extra)
    return rec


# ===========================================================================
# WINDOWS — KB supersedence model (get_windows_vuln.rb)
# ===========================================================================

def kb_candidates(patch):
    candidates = set()
    for field in ("kb_id", "security_update_id", "id"):
        val = patch.get(field)
        if val:
            candidates.add(str(val).upper().replace("KB", "").strip())
    for m in re.findall(r"KB(\d+)", patch.get("title", ""), flags=re.IGNORECASE):
        candidates.add(m)
    return {c for c in candidates if c and c.isdigit()}


def build_kb_to_cves(server_dir, os_id):
    # { kb_article_name -> set(cve) } for the endpoint's os_id.
    kb_to_cves = {}
    for rec in read_associations(server_dir, OS_TYPE_WINDOWS):
        cve = rec["cve"]
        for kb in rec.get("kb_articles", []):
            if os_id in (kb.get("os_id") or []):
                name = str(kb.get("article_name", "")).strip()
                if name:
                    kb_to_cves.setdefault(name, set()).add(cve)
    return kb_to_cves


def load_kb_info_for_os(server_dir, os_id):
    """Returns (supersede_graph, kb_base, kb_cves, cumulative_kbs) for this os_id from
    kb_info.json. cumulative_kbs is the set of KB ids flagged cumulative in kb_tree; it is
    used to walk supersedence *forward* (see get_superseding_cumulative_kbs)."""
    path = os.path.join(server_dir, "kb_info.json")
    if not os.path.isfile(path):
        return {}, {}, {}, set()

    with open(path, "r", encoding="utf-8") as f:
        data = json.load(f)

    id_os_map = {}
    sections = {}
    for element in data.get("oesis", []):
        for section_key, section_val in element.items():
            if section_key == "header":
                continue
            if section_key == "id_os_map" and isinstance(section_val, dict):
                for k, v in section_val.items():
                    try:
                        id_os_map[int(k)] = str(v)
                    except (TypeError, ValueError):
                        pass
            elif isinstance(section_val, dict):
                sections[str(section_key)] = section_val

    platform_label = id_os_map.get(os_id)
    if not platform_label:
        return {}, {}, {}, set()

    section = sections.get(platform_label) or {}

    tree = section.get("kb_tree") or {}
    graph = {}
    cumulative_kbs = set()
    for kb, rec in tree.items():
        graph[str(kb)] = {str(v) for v in (rec.get("supersede_kbs") or [])}
        if rec.get("cumulative"):
            cumulative_kbs.add(str(kb))

    kb_base = section.get("kb_base") or {}
    kb_cves = section.get("kb_cves") or {}
    return graph, kb_base, kb_cves, cumulative_kbs


def compare_builds(build1, build2):
    if build1 == build2:
        return 0
    try:
        a = [int(x) for x in str(build1).split(".")]
        b = [int(x) for x in str(build2).split(".")]
    except ValueError:
        return 0
    n = max(len(a), len(b))
    a += [0] * (n - len(a))
    b += [0] * (n - len(b))
    for x, y in zip(a, b):
        if x < y:
            return -1
        if x > y:
            return 1
    return 0


def graph_closure(start, graph):
    """BFS transitive closure from start (including start itself, like the ruby)."""
    visited = set()
    queue = deque([start])
    while queue:
        cur = queue.popleft()
        if cur in visited:
            continue
        visited.add(cur)
        for nxt in graph.get(cur, set()):
            if nxt not in visited:
                queue.append(nxt)
    return visited


def build_superseded_by_map(supersede_graph, cumulative_kbs):
    """Invert the supersede graph, keeping only *cumulative* superseders.

    supersede_graph[kb] = older KBs that kb supersedes (forward edges). The inverse answers
    'which newer cumulative KBs supersede this KB?' — i.e. superseded_by[old] = {newer cumuls}.
    """
    superseded_by = {}
    for kb, older_kbs in supersede_graph.items():
        if cumulative_kbs and kb not in cumulative_kbs:
            continue  # only newer *cumulative* updates are a valid recovery target
        for old in older_kbs:
            superseded_by.setdefault(old, set()).add(kb)
    return superseded_by


def get_superseding_cumulative_kbs(missing_kbs, supersede_graph, cumulative_kbs):
    """Newer cumulative KBs that supersede any reported-missing KB (one hop), excluding the
    inputs themselves. Mirrors the endpoint's appendMissingCumulKB: if an older cumulative is
    missing, every cumulative that supersedes it is missing too (cumulatives are cumulative),
    so the latest cumulative's CVEs are not dropped just because the agent reported a stale KB.
    """
    superseded_by = build_superseded_by_map(supersede_graph, cumulative_kbs)
    out = set()
    for kb in missing_kbs:
        out |= superseded_by.get(kb, set())
    return out - set(missing_kbs)


def get_base_kbs_for_build(kb_base, current_build, supersede_graph):
    """Base KBs for the current build (get_base_kbs_for_current_os)."""
    if not kb_base or not current_build:
        return set()

    build_data = kb_base.get(current_build) or {}
    direct = set()
    for patch in build_data.get("kb_articles") or []:
        kb_id = patch.get("kb_id")
        if kb_id and str(kb_id) != "0":
            direct.add(str(kb_id))
    if direct:
        return direct

    higher_builds = sorted(
        [b for b in kb_base.keys() if compare_builds(b, current_build) > 0],
        key=lambda b: [int(x) for x in b.split(".")],
    )
    common_kbs = None
    for build in higher_builds:
        bd = kb_base.get(build) or {}
        build_kb_ids = set()
        for patch in bd.get("kb_articles") or []:
            kb_id = patch.get("kb_id")
            if kb_id and str(kb_id) != "0":
                build_kb_ids.add(str(kb_id))
        if not build_kb_ids:
            continue
        recursive = set()
        for kb in build_kb_ids:
            recursive |= graph_closure(kb, supersede_graph)
        common_kbs = recursive if common_kbs is None else (common_kbs & recursive)
    if common_kbs:
        return common_kbs

    for build in higher_builds:
        bd = kb_base.get(build) or {}
        build_kb_ids = set()
        for patch in bd.get("kb_articles") or []:
            kb_id = patch.get("kb_id")
            if kb_id and str(kb_id) != "0":
                build_kb_ids.add(str(kb_id))
        if not build_kb_ids:
            continue
        closure = set()
        for kb in build_kb_ids:
            closure |= graph_closure(kb, supersede_graph)
        superseded = closure - build_kb_ids
        if superseded:
            return superseded
    return set()


def get_installed_kbs_from_earlier_builds(kb_base, current_build):
    """KBs from builds lower than current in the same major version line."""
    if not kb_base or not current_build:
        return set()
    current_major = str(current_build).split(".")[0]
    earlier_kbs = set()
    for build, build_data in kb_base.items():
        if str(build).split(".")[0] != current_major:
            continue
        if compare_builds(build, current_build) >= 0:
            continue
        for patch in (build_data or {}).get("kb_articles") or []:
            kb_id = patch.get("kb_id")
            if kb_id and str(kb_id) != "0":
                earlier_kbs.add(str(kb_id))
    return earlier_kbs


def get_missing_kbs_from_later_builds(kb_base, current_build):
    """KBs from builds *higher* than current in the same major version line.

    Direct port of the endpoint engine's getMissingKBFromBuildHistory
    (wa_vmod_utils_vulns_win.cpp). GetMissingPatches (method 1013) reports only the latest
    offered cumulative, so the intervening months' cumulatives the machine is also missing
    never enter the scan's missing list and their CVEs get dropped. Forward supersedence
    (get_superseding_cumulative_kbs) only walks the supersede chain up one hop, not into every
    intervening build. Any build newer than the running one fixes CVEs the machine has not
    applied yet, so its KB must be treated as missing. This is the symmetric counterpart of
    get_installed_kbs_from_earlier_builds (builds below current = installed).
    """
    if not kb_base or not current_build:
        return set()
    current_major = str(current_build).split(".")[0]
    later_kbs = set()
    for build, build_data in kb_base.items():
        if str(build).split(".")[0] != current_major:
            continue
        if compare_builds(build, current_build) <= 0:
            continue
        for patch in (build_data or {}).get("kb_articles") or []:
            kb_id = patch.get("kb_id")
            if kb_id and str(kb_id) != "0":
                later_kbs.add(str(kb_id))
    return later_kbs


def _normalize_numeric_cve(numeric_id):
    """Convert numeric CVE id (e.g. 202438203) to CVE-YYYY-NNNNN format."""
    s = str(numeric_id)
    if len(s) >= 5:
        return f"CVE-{s[:4]}-{s[4:]}"
    return None


def get_cves_for_kb(kb, kb_to_cves, kb_cves=None):
    """Get all CVEs associated with a KB.

    Merges two data sources:
      - vuln_system_associations: CVE-YYYY-NNNNN format
      - kb_cves from kb_info.json: numeric format (e.g. 202438203)
    Both are normalized to CVE-YYYY-NNNNN and deduplicated.
    """
    result = set(kb_to_cves.get(kb, set()))

    # Also pull from kb_cves (numeric IDs) and normalize
    if kb_cves:
        entry = kb_cves.get(kb) or kb_cves.get(str(kb))
        if entry:
            cve_list = entry.get("cves", []) if isinstance(entry, dict) else entry if isinstance(entry, list) else []
            for cid in cve_list:
                normalized = _normalize_numeric_cve(cid)
                if normalized:
                    result.add(normalized)

    return result


def detect_windows_cves(scan, server_dir, os_info, cve_index):
    os_id = os_info.get("os_id")

    kb_to_cves = build_kb_to_cves(server_dir, os_id)
    supersede_graph, kb_base, kb_cves, cumulative_kbs = load_kb_info_for_os(server_dir, os_id)

    current_build = str(
        os_info.get("build") or os_info.get("os_version") or os_info.get("version") or ""
    ).strip()

    print(f"  Catalog      : {len(kb_to_cves)} KB articles mapped to CVEs for os_id {os_id}")
    print(f"  Supersedence : {len(supersede_graph)} KB nodes")
    print(f"  KB base      : {len(kb_base)} builds indexed")
    print(f"  Current build: {current_build}")

    # --- Data-quality warnings (do not alter the mapping; just surface risk) -------------
    if not kb_to_cves:
        print(f"  WARNING: no Windows KB->CVE associations found for os_id {os_id}. This OS may "
              f"not be covered by the catalog (or os_id is wrong) -- OS CVE results may be "
              f"empty or incomplete.")
    if not supersede_graph or not kb_base:
        print(f"  WARNING: no kb_info supersedence/kb_base data for os_id {os_id}. Installed-KB "
              f"seeding from the build is unavailable, so 'already-fixed' CVEs may not be "
              f"subtracted (results may over-report).")
    # A valid Windows build is '<build>.<revision>' (e.g. 26100.6584). If GetOSInfo did not
    # provide it and we fell back to the version string (e.g. '10.0.26100'), the build-based
    # installed-patch seeding (compare_builds / kb_base lookups) is unreliable.
    if not re.match(r"^\d{4,}\.\d+$", current_build):
        print(f"  WARNING: OS build '{current_build}' is not a <build>.<revision> value "
              f"(expected e.g. 26100.6584). Build-based installed-patch seeding may be "
              f"unreliable for this scan.")

    # Step 1: installed KBs from scan
    installed_kbs = set()
    for product in scan.get("products", []):
        for patch in product.get("installed_patches", []):
            installed_kbs |= kb_candidates(patch)
    scan_count = len(installed_kbs)

    # Step 2 + 3: earlier-build KBs and base KBs for the current build
    earlier_kbs = get_installed_kbs_from_earlier_builds(kb_base, current_build)
    installed_kbs |= earlier_kbs
    base_kbs = get_base_kbs_for_build(kb_base, current_build, supersede_graph)
    installed_kbs |= base_kbs

    print(f"  Installed KBs: {len(installed_kbs)} total "
          f"(scan={scan_count}, earlier_builds={len(earlier_kbs)}, base={len(base_kbs)}; "
          f"sources overlap)")

    # Step 4: recursive supersedence closure of installed KBs
    recursive_installed_kbs = set()
    for kb in installed_kbs:
        recursive_installed_kbs |= graph_closure(kb, supersede_graph)
    print(f"  Installed KBs after recursion: {len(recursive_installed_kbs)}")

    # Step 5: missing KBs reported by the scan (OESIS GetMissingPatches / method 1013).
    scan_reported_missing_kbs = set()
    for product in scan.get("products", []):
        for patch in product.get("missing_patches", []):
            scan_reported_missing_kbs |= kb_candidates(patch)

    # Step 5b: forward supersedence. The agent's GetMissingPatches (1013) can report a stale
    # "latest offered" KB; promote the missing set to the newer cumulative KBs that supersede
    # it so the latest cumulative's CVEs (e.g. current-month fixes) are not dropped. This
    # mirrors the endpoint engine's appendMissingCumulKB and keeps the two workflows aligned.
    superseding_kbs = get_superseding_cumulative_kbs(
        scan_reported_missing_kbs, supersede_graph, cumulative_kbs)
    if superseding_kbs:
        print(f"  Forward supersedence: promoted missing set with "
              f"{len(superseding_kbs)} newer cumulative KB(s): {sorted(superseding_kbs)}")

    # Step 5c: build-history recovery (mirrors the endpoint engine's getMissingKBFromBuildHistory).
    # GetMissingPatches only offers the latest cumulative and forward supersedence walks the
    # chain up one hop, so the intervening months' cumulatives are still missing from the set.
    # Pull every KB whose build is newer than the running build: those fix CVEs not yet applied.
    later_build_kbs = get_missing_kbs_from_later_builds(kb_base, current_build)
    if later_build_kbs:
        print(f"  Build-history recovery: added {len(later_build_kbs)} KB(s) from builds newer "
              f"than {current_build}")
    missing_kbs_input = scan_reported_missing_kbs | superseding_kbs | later_build_kbs

    # ------------------------------------------------------------------
    # Affected-CVE calculation — ported from the endpoint engine's calcAffectedCvesList
    # (wa_vmod_impl_vulns_win.cpp:535). The missing KBs are NOT expanded downward into the
    # affected set. We take each missing KB's OWN (direct) CVEs, then SUBTRACT (a) the CVEs of
    # the older KBs those missing KBs supersede and (b) the CVEs already covered by installed
    # KBs plus their supersede chain:
    #
    #   affected = direct_cves(missing)
    #            - direct_cves(older KBs superseded by missing, not themselves missing)
    #            - direct_cves(installed + supersede chain)
    #
    # The previous approach expanded missing *downward* (closure(missing)); because the
    # installed baseline only reaches the machine's build, every cumulative newer than the
    # build leaked in as "affected", accreting the whole historical CVE tail. The engine
    # avoids this by subtracting the superseded history instead of accumulating it.
    # ------------------------------------------------------------------

    # (A) Direct CVEs of every missing KB — NO downward expansion.
    cves_from_missing = set()
    for kb in missing_kbs_input:
        cves_from_missing |= get_cves_for_kb(kb, kb_to_cves, kb_cves)

    # (B) Older KBs superseded by the missing KBs (excluding any KB that is itself missing).
    #     The scan-reported AND build-history-derived missing KBs are expanded here, mirroring
    #     the engine: getMissingKBFromBuildHistory KBs are in missingKBList at child-expansion
    #     time. Only the forward-appended cumulatives (superseding_kbs) are intentionally NOT
    #     expanded — appendMissingCumulKB runs after child expansion in the engine, so the
    #     latest cumulative's own CVEs are kept rather than subtracted.
    superseded_kbs = set()
    for kb in scan_reported_missing_kbs | later_build_kbs:
        superseded_kbs |= graph_closure(kb, supersede_graph)
    superseded_kbs -= missing_kbs_input
    cve_from_superseded = set()
    for kb in superseded_kbs:
        cve_from_superseded |= get_cves_for_kb(kb, kb_to_cves, kb_cves)

    # (C) CVEs covered by installed KBs and their full supersede chain.
    cve_from_installed = set()
    for kb in recursive_installed_kbs:
        cve_from_installed |= get_cves_for_kb(kb, kb_to_cves, kb_cves)

    net_cves = cves_from_missing - cve_from_superseded - cve_from_installed
    print(f"  Missing KBs  : {len(scan_reported_missing_kbs)} reported + "
          f"{len(superseding_kbs)} forward + {len(later_build_kbs)} build-history "
          f"= {len(missing_kbs_input)} total")
    print(f"\n  CVEs from missing KBs (direct):        {len(cves_from_missing)}")
    print(f"  - subtracted, superseded/older KBs:    {len(cve_from_superseded)}")
    print(f"  - subtracted, installed + chain:       {len(cve_from_installed)}")
    print(f"  Net affected CVEs:                     {len(net_cves)}")

    # Per-patch breakdown: attribute each net CVE to the missing KB(s) whose direct CVE list
    # contains it (mirrors the engine's cve2MissingKBMap = invert(missingKB2CveMap)).
    patch_meta = {}  # kb -> (title, severity, product)
    for product in scan.get("products", []):
        for patch in product.get("missing_patches", []):
            for kb in kb_candidates(patch):
                patch_meta.setdefault(
                    kb, (patch.get("title", "Unknown"), patch.get("severity"), product.get("name")))
    os_product_name = next((p.get("name") for p in scan.get("products", [])),
                           "Windows Update Agent")

    mapped_patches = []
    cve_to_patches = {}
    for kb in sorted(missing_kbs_input):
        kb_net = get_cves_for_kb(kb, kb_to_cves, kb_cves) & net_cves
        if not kb_net:
            continue
        title, severity, product_name = patch_meta.get(
            kb, (f"Cumulative update KB{kb} (recovered via supersedence/build history)",
                 None, os_product_name))
        mapped_patches.append({
            "kb":        kb,
            "title":     title,
            "severity":  severity,
            "product":   product_name,
            "cve_count": len(kb_net),
            "cves":      sorted(kb_net),
        })
        for cve in kb_net:
            cve_to_patches.setdefault(cve, set()).add(kb)

    cve_list = [
        enrich_cve(cve, cve_index, fixed_by_kbs=sorted(cve_to_patches.get(cve, set())))
        for cve in sorted(net_cves)
    ]

    print(f"\n  Missing patches: {len(mapped_patches)}")
    print("-" * 70)
    for mp in mapped_patches:
        print(f"  KB {mp['kb']:<10} [{mp['severity'] or 'unknown'}]  {mp['title'][:48]}")
        print(f"     -> {mp['cve_count']} net CVE(s) remediated by this patch")
    print(f"\n  Net distinct CVEs the endpoint is exposed to: {len(cve_list)}")
    _print_cve_sample(cve_list)

    return {
        "platform": "windows",
        "source": "Analog kb_info.json + vuln_system_associations.json (calcAffectedCvesList logic)",
        "total_missing_patches":   len(mapped_patches),
        "total_cves_from_missing": len(cves_from_missing),
        "total_cves_superseded":   len(cve_from_superseded),
        "total_cves_installed":    len(cve_from_installed),
        "total_cves":              len(cve_list),
        "debug": {
            "installed_kbs_from_scan":      scan_count,
            "installed_kbs_earlier_builds": len(earlier_kbs),
            "installed_kbs_base":           len(base_kbs),
            "recursive_installed_kbs":      len(recursive_installed_kbs),
            "scan_reported_missing_kbs":    sorted(scan_reported_missing_kbs),
            "superseding_cumulative_kbs":   sorted(superseding_kbs),
            "later_build_kbs":              len(later_build_kbs),
            "missing_kbs_input":            len(missing_kbs_input),
            "superseded_kbs_subtracted":    len(superseded_kbs),
        },
        "missing_patches": mapped_patches,
        "cves":            cve_list,
    }


# ===========================================================================
# macOS — OS-version-range model (get_list_cves_macos)
# ===========================================================================

def _ver_is_valid(v):
    return bool(v) and any(ch.isdigit() for ch in str(v))


def _ver_compare(v1, v2):
    """Dotted numeric version compare: 1 if v1>v2, 0 if equal, -1 if v1<v2,
    None if not comparable (mirrors Version#<=> for numeric macOS versions)."""
    if not (_ver_is_valid(v1) and _ver_is_valid(v2)):
        return None
    a = [int(x) for x in re.findall(r"\d+", str(v1))]
    b = [int(x) for x in re.findall(r"\d+", str(v2))]
    n = max(len(a), len(b))
    a += [0] * (n - len(a))
    b += [0] * (n - len(b))
    for x, y in zip(a, b):
        if x > y:
            return 1
        if x < y:
            return -1
    return 0


def is_mac_version_in_range(version, rng):
    # Port of is_mac_version_in_range?: inclusive start, exclusive limit_except.
    start = rng.get("start")
    limit_except = rng.get("limit_except")
    if start is None or limit_except is None:
        return False
    if not (_ver_is_valid(version) and _ver_is_valid(start) and _ver_is_valid(limit_except)):
        return False
    if _ver_compare(version, start) == -1:           # version < start
        return False
    if _ver_compare(version, limit_except) in (0, 1):  # version >= limit_except
        return False
    return True


def detect_mac_cves(scan, server_dir, os_info, cve_index):
    os_id = os_info.get("os_id")
    os_version = str(os_info.get("os_version") or os_info.get("version") or "").strip()

    print(f"  Matching against os_id {os_id}, version {os_version}")

    cve_to_ranges = {}   # cve -> set of "start..limit_except" matched
    for rec in read_associations(server_dir, OS_TYPE_MACOS):
        cve = rec["cve"]
        for aos in rec.get("affected_os", []):
            if aos.get("os_id") != os_id:
                continue
            matched = False
            for rng in aos.get("ranges", []):
                if is_mac_version_in_range(os_version, rng):
                    span = f"{rng.get('start')}..{rng.get('limit_except')}"
                    cve_to_ranges.setdefault(cve, set()).add(span)
                    matched = True
            if matched:
                break

    cve_list = [
        enrich_cve(cve, cve_index, affected_ranges=sorted(cve_to_ranges[cve]))
        for cve in sorted(cve_to_ranges)
    ]

    print(f"\n  Net distinct CVEs the endpoint is exposed to: {len(cve_list)}")
    _print_cve_sample(cve_list)

    return {
        "platform": "macos",
        "source": "Analog vuln_system_associations.json (get_list_cves_macos logic)",
        "total_cves": len(cve_list),
        "cves":       cve_list,
    }


# ===========================================================================
# Linux — package-version-range model (get_list_cves_linux)
# ===========================================================================

# linux_os_info.rb OS_NAME_MAP: os name -> [normalized name, major_only]
OS_NAME_MAP = [
    ("Red Hat",                            ("RedHat", True)),
    ("AlmaLinux",                          ("Alma",   True)),
    ("Linux Mint",                         ("Mint",   False)),
    ("Ubuntu",                             ("Ubuntu", False)),
    ("CentOS",                             ("CentOS", False)),
    ("Debian",                             ("Debian", False)),
    ("Amazon Linux",                       ("Amazon", False)),
    ("Rocky Linux",                        ("Rocky",  True)),
    ("Oracle Linux",                       ("Oracle", True)),
    ("SLED",                               ("SLED",   False)),
    ("SUSE Linux Enterprise Desktop",      ("SLED",   False)),
    ("SLES",                               ("SLES",   False)),
    ("SUSE Linux Enterprise Server",       ("SLES",   False)),
    ("SLETC",                              ("SLETC",  False)),
    ("SUSE Linux Enterprise Thin Client",  ("SLETC",  False)),
]


def _extract_version(s, major_only):
    # Port of linux_os_info.rb#extract_version.
    version = ""
    dot_encountered = False
    count_digits = 0
    for ch in str(s):
        if ch.isdigit():
            version += ch
            if count_digits == 0:
                count_digits = 1
        elif ch == "." and count_digits == 1 and not dot_encountered:
            if major_only:
                break
            version += ch
            dot_encountered = True
        elif dot_encountered and not ch.isdigit():
            break
        elif count_digits > 0:
            break
    return version


def normalize_os_info(input_name, os_version=""):
    # Port of linux_os_info.rb#normalize_os_info -> returns the full "Name Version"
    # label that matches affected_os[].os_name, or None if it can't be matched.
    input_name = str(input_name or "")
    os_version = str(os_version or "")
    for key, (short, major_only) in OS_NAME_MAP:
        if not input_name.startswith(key):
            continue
        if os_version:
            ver = _extract_version(os_version, major_only)
        else:
            ver = _extract_version(input_name[len(key):], major_only)
        if not ver:
            return None
        return f"{short} {ver}"
    return None


# ---- dpkg version comparison (compare_linux_version.rb) --------------------

def _lx_epoch(version):
    parts = version.split(":")
    if len(parts) > 1:
        try:
            return int(parts[0])
        except ValueError:
            return 0
    return 0


def _lx_upstream(version):
    up_start = 0
    colon = version.find(":")
    if colon != -1:
        up_start = colon + 1
    last_hyphen = version.rfind("-")
    if last_hyphen != -1:
        return version[up_start:last_hyphen]
    return version[up_start:]


def _lx_debian(version):
    lhi = version.rfind("-")
    return version[lhi + 1:] if lhi != -1 else ""


def _lx_compare_epoch(e1, e2):
    r = e1 - e2
    return 1 if r > 0 else (2 if r < 0 else 0)


def _lx_compare_upstream(u1, u2):
    s1 = s2 = 0
    while s1 < len(u1) or s2 < len(u2):
        m1 = re.match(r"\D+", u1[s1:]);  ndp1 = m1.group(0) if m1 else ""
        m2 = re.match(r"\D+", u2[s2:]);  ndp2 = m2.group(0) if m2 else ""
        for i, c1 in enumerate(ndp1):
            c2 = ndp2[i] if i < len(ndp2) else ""
            if c1 == c2:
                continue
            if c1 == "~":
                return 2
            if c2 == "~":
                return 1
            if c1 == "":
                return 2
            if c2 == "":
                return 1
            c1_alpha, c2_alpha = c1.isalpha(), c2.isalpha()
            if c1_alpha and not c2_alpha:
                return 2
            if c2_alpha and not c1_alpha:
                return 1
            if c1 > c2:
                return 1
            if c1 < c2:
                return 2
        if len(ndp1) < len(ndp2):
            return 1 if ndp2[len(ndp1)] == "~" else 2
        s1 += len(ndp1)
        s2 += len(ndp2)
        md1 = re.match(r"\d+", u1[s1:]);  dc1 = md1.group(0) if md1 else None
        if dc1:
            s1 += len(dc1)
        md2 = re.match(r"\d+", u2[s2:]);  dc2 = md2.group(0) if md2 else None
        if dc2:
            s2 += len(dc2)
        n1 = int(dc1) if dc1 else 0
        n2 = int(dc2) if dc2 else 0
        if n1 > n2:
            return 1
        if n1 < n2:
            return 2
    return 0


def compare_linux_version(v1, v2):
    # Returns 0 equal, 1 if v1>v2, 2 if v1<v2 (dpkg semantics, per the ruby util).
    if v1 == v2:
        return 0
    cp = _lx_compare_epoch(_lx_epoch(v1), _lx_epoch(v2))
    if cp != 0:
        return cp
    cp = _lx_compare_upstream(_lx_upstream(v1), _lx_upstream(v2))
    if cp != 0:
        return cp
    return _lx_compare_upstream(_lx_debian(v1), _lx_debian(v2))


def is_linux_package_version_in_range(version, rng):
    # Port of is_linux_package_version_in_range?.
    has_start = "start" in rng or "start_except" in rng
    has_limit = "limit" in rng or "limit_except" in rng
    if not has_start or not has_limit:
        return False

    start = rng.get("start")
    start_except = rng.get("start_except")
    limit = rng.get("limit")
    limit_except = rng.get("limit_except")

    if start is not None:
        if compare_linux_version(version, start) in (2, -1):
            return False
    else:
        if compare_linux_version(version, start_except) in (0, 2, -1):
            return False

    if limit_except is not None:
        if compare_linux_version(version, limit_except) in (0, 1, -1) and limit_except != "N/A":
            return False
    else:
        if compare_linux_version(version, limit) in (1, -1) and limit != "N/A":
            return False
    return True


def get_linux_packages(scan):
    # Installed packages reported by the agent for the Linux vuln check. Prefer an
    # explicit 'packages' list; otherwise fall back to the patch-management product's
    # installed-patch entries (each represents an installed package on Linux).
    packages = []
    seen = set()

    def add(name, version):
        if name and version and (name, version) not in seen:
            seen.add((name, version))
            packages.append({"package_name": name, "package_version": str(version)})

    for pkg in scan.get("packages", []) or []:
        add(pkg.get("package_name") or pkg.get("name"),
            pkg.get("package_version") or pkg.get("version"))

    for product in scan.get("products", []):
        for patch in product.get("installed_patches", []):
            add(patch.get("package_name") or patch.get("name") or patch.get("title"),
                patch.get("package_version") or patch.get("version"))
    return packages


def detect_linux_cves(scan, server_dir, os_info, cve_index):
    os_name = normalize_os_info(os_info.get("os_name") or os_info.get("name"),
                                os_info.get("os_version") or os_info.get("version"))
    packages = get_linux_packages(scan)

    print(f"  Normalized OS: {os_name}")
    print(f"  Installed packages reported: {len(packages)}")

    if not os_name:
        print("  WARNING: could not normalize the distro name — no Linux mapping possible.")
        return {"platform": "linux", "source": "Analog vuln_system_associations.json "
                "(get_list_cves_linux logic)", "total_cves": 0, "cves": [],
                "affected_packages": [], "normalized_os": None}
    if not packages:
        print("  WARNING: no installed packages reported by the scan — nothing to map.")

    pkg_versions = {p["package_name"]: p["package_version"] for p in packages}

    # cve -> { package_name -> fix_version }
    findings = {}
    for rec in read_associations(server_dir, OS_TYPE_LINUX):
        cve = rec["cve"]
        for aos in rec.get("affected_os", []):
            if aos.get("os_name") != os_name:
                continue
            for ap in aos.get("affected_packages", []):
                pname = ap.get("package_name")
                if pname not in pkg_versions:
                    continue
                installed_ver = pkg_versions[pname]
                for rng in ap.get("ranges", []):
                    if is_linux_package_version_in_range(installed_ver, rng):
                        fix = rng.get("limit_except") or rng.get("limit") or "N/A"
                        findings.setdefault(cve, {})[pname] = {
                            "installed_version": installed_ver,
                            "fix_version":       fix,
                        }
                        break

    # Per-package breakdown
    affected_packages = {}
    for cve, pkgs in findings.items():
        for pname, info in pkgs.items():
            affected_packages.setdefault(pname, {
                "package_name":      pname,
                "installed_version": info["installed_version"],
                "fix_versions":      set(),
                "cves":              set(),
            })
            affected_packages[pname]["cves"].add(cve)
            if info["fix_version"] and info["fix_version"] != "N/A":
                affected_packages[pname]["fix_versions"].add(info["fix_version"])

    affected_list = []
    for pname in sorted(affected_packages):
        ap = affected_packages[pname]
        affected_list.append({
            "package_name":      ap["package_name"],
            "installed_version": ap["installed_version"],
            "fix_versions":      sorted(ap["fix_versions"]),
            "cve_count":         len(ap["cves"]),
            "cves":              sorted(ap["cves"]),
        })

    cve_list = []
    for cve in sorted(findings):
        pkgs = findings[cve]
        fixes = sorted({i["fix_version"] for i in pkgs.values() if i["fix_version"] != "N/A"})
        cve_list.append(enrich_cve(
            cve, cve_index,
            packages=sorted(pkgs.keys()),
            fix_versions=fixes,
        ))

    print(f"\n  Vulnerable packages: {len(affected_list)}")
    print("-" * 70)
    for ap in affected_list:
        print(f"  {ap['package_name']:<28} {ap['installed_version']:<28} "
              f"-> {ap['cve_count']} CVE(s); fix: {', '.join(ap['fix_versions']) or 'N/A'}")
    print(f"\n  Net distinct CVEs the endpoint is exposed to: {len(cve_list)}")
    _print_cve_sample(cve_list)

    return {
        "platform": "linux",
        "source": "Analog vuln_system_associations.json (get_list_cves_linux logic)",
        "normalized_os":      os_name,
        "packages_scanned":   len(packages),
        "total_vulnerable_packages": len(affected_list),
        "total_cves":         len(cve_list),
        "affected_packages":  affected_list,
        "cves":               cve_list,
    }


# ===========================================================================
# Shared output / dispatch
# ===========================================================================

def _print_cve_sample(cve_list):
    if not cve_list:
        return
    print("-" * 70)
    for c in cve_list[:25]:
        print(f"  {c['cve']:<18} {c.get('cwe') or '':<10}")
    if len(cve_list) > 25:
        print(f"  ... and {len(cve_list) - 25} more (see {os.path.basename(OUTPUT_FILE)})")


def main():
    scan = load_osdetails_scan()
    if scan is None:
        print("ERROR: endpoint scan not found "
              "(scan-ca-endpoint-result.json or scan-ca-osdetails-result.json).")
        print("       Run 'python scan-ca-endpoint.py' (or scan-ca-osdetails.py) first.")
        return

    server_dir = find_analog_server_dir()
    if not server_dir:
        print("ERROR: Analog server datasets not found under OPSWAT-SDK/extract/analog/server.")
        print("       Run the SDK downloader so the Analog data is extracted.")
        return

    os_info = scan.get("os_info", {})
    os_id   = os_info.get("os_id")
    os_type = os_info.get("os_type")

    print("VAPM Centralized Assessment — Map OS Details to Missing Patches & CVEs")
    print("=" * 70)
    print(f"  OS           : {os_info.get('os_name', os_info.get('name', 'Unknown'))} "
          f"({os_info.get('os_version', os_info.get('version', ''))})")
    print(f"  os_id/os_type: {os_id} / {os_type}")
    print(f"  Analog data  : {server_dir}")

    cve_index = build_cve_index(server_dir)

    if os_type == OS_TYPE_WINDOWS:
        result = detect_windows_cves(scan, server_dir, os_info, cve_index)
    elif os_type == OS_TYPE_MACOS:
        result = detect_mac_cves(scan, server_dir, os_info, cve_index)
    elif os_type == OS_TYPE_LINUX:
        result = detect_linux_cves(scan, server_dir, os_info, cve_index)
    else:
        print(f"\n  Unsupported os_type ({os_type}). Expected 1 (Windows), 2 (Linux), or 4 (macOS).")
        return

    output = {
        "os_info": {
            "name":    os_info.get("os_name", os_info.get("name")),
            "version": os_info.get("os_version", os_info.get("version")),
            "os_id":   os_id,
            "os_type": os_type,
        },
    }
    output.update(result)
    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        json.dump(output, f, indent=2, default=str)
    print(f"\n  Full results written to: {OUTPUT_FILE}")


if __name__ == "__main__":
    main()
