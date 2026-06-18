#!/usr/bin/env python3
###############################################################################################
##  VAPM Centralized Assessment — Map OS Details to Missing Patches & CVEs
##  Reference Implementation using the OESIS "Analog" offline catalog
##
##  This follows the server-side CVE mapping logic from get_windows_vuln.rb:
##    1. Load kb_info.json -> kb_tree (supersedence), kb_base (build->KB), kb_cves (KB->CVEs)
##    2. Load vuln_system_associations.json -> KB->CVE mapping filtered by os_id
##    3. Seed installed KBs from scan + kb_base for current/lower builds
##    4. Recursively expand installed KBs through supersedence (installed covers all older)
##    5. Recursively expand missing KBs through supersedence
##    6. Subtract installed KBs from missing set
##    7. Collect CVEs from remaining missing KBs
##    8. Subtract CVEs already fixed by installed KBs
##    9. Result = affected CVEs
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


# ---------------------------------------------------------------------------
# Data loading helpers
# ---------------------------------------------------------------------------

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


def kb_candidates(patch):
    candidates = set()
    for field in ("kb_id", "security_update_id", "id"):
        val = patch.get(field)
        if val:
            candidates.add(str(val).upper().replace("KB", "").strip())
    for m in re.findall(r"KB(\d+)", patch.get("title", ""), flags=re.IGNORECASE):
        candidates.add(m)
    return {c for c in candidates if c and c.isdigit()}


# ---------------------------------------------------------------------------
# vuln_system_associations -> KB->CVE index (filtered by os_id)
# ---------------------------------------------------------------------------

def build_kb_to_cves(server_dir, os_id):
    kb_to_cves = {}
    path = os.path.join(server_dir, "vuln_system_associations.json")
    for rec in read_analog_records(path):
        cve = rec.get("cve")
        if not cve:
            continue
        for kb in rec.get("kb_articles", []):
            if os_id in (kb.get("os_id") or []):
                name = str(kb.get("article_name", "")).strip()
                if name:
                    kb_to_cves.setdefault(name, set()).add(cve)
    return kb_to_cves


# ---------------------------------------------------------------------------
# kb_info.json: load supersedence graph, kb_base, kb_cves for this os_id
# ---------------------------------------------------------------------------

def load_kb_info_for_os(server_dir, os_id):
    """Returns (supersede_graph, kb_base, kb_cves)."""
    path = os.path.join(server_dir, "kb_info.json")
    if not os.path.isfile(path):
        return {}, {}, {}

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
        return {}, {}, {}

    section = sections.get(platform_label) or {}

    # kb_tree -> supersedence graph {kb_str: set(superseded_kb_str)}
    tree = section.get("kb_tree") or {}
    graph = {}
    for kb, rec in tree.items():
        graph[str(kb)] = {str(v) for v in (rec.get("supersede_kbs") or [])}

    kb_base = section.get("kb_base") or {}
    kb_cves = section.get("kb_cves") or {}

    return graph, kb_base, kb_cves


# ---------------------------------------------------------------------------
# Build comparison (Ruby: compare_builds)
# ---------------------------------------------------------------------------

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


# ---------------------------------------------------------------------------
# Ruby: get_base_kbs_for_current_os
# ---------------------------------------------------------------------------

def get_base_kbs_for_build(kb_base, current_build, supersede_graph):
    """Get base KBs for the current build. If build has explicit KBs use them;
    otherwise find common superseded KBs from higher builds."""
    if not kb_base or not current_build:
        return set()

    build_data = kb_base.get(current_build) or {}
    build_kbs = build_data.get("kb_articles") or []
    direct = set()
    for patch in build_kbs:
        kb_id = patch.get("kb_id")
        if kb_id and str(kb_id) != "0":
            direct.add(str(kb_id))

    if direct:
        return direct

    # No direct KBs (kb_id=0 means RTM/baseline build).
    # Strategy 1: intersect superseded KBs from all higher builds.
    higher_builds = sorted(
        [b for b in kb_base.keys() if compare_builds(b, current_build) > 0],
        key=lambda b: [int(x) for x in b.split(".")],
    )
    common_kbs = None
    for build in higher_builds:
        bd = kb_base.get(build) or {}
        bkbs = bd.get("kb_articles") or []
        build_kb_ids = set()
        for patch in bkbs:
            kb_id = patch.get("kb_id")
            if kb_id and str(kb_id) != "0":
                build_kb_ids.add(str(kb_id))
        if not build_kb_ids:
            continue
        recursive = set()
        for kb in build_kb_ids:
            recursive |= graph_closure(kb, supersede_graph)
        if common_kbs is None:
            common_kbs = recursive
        else:
            common_kbs &= recursive
    if common_kbs:
        return common_kbs

    # Strategy 2 (RTM fix): intersection was empty (leaf-node KBs pollute it).
    # For an RTM build, the first post-RTM cumulative update's superseded KBs
    # were already incorporated into the RTM. Use those as the base.
    for build in higher_builds:
        bd = kb_base.get(build) or {}
        bkbs = bd.get("kb_articles") or []
        build_kb_ids = set()
        for patch in bkbs:
            kb_id = patch.get("kb_id")
            if kb_id and str(kb_id) != "0":
                build_kb_ids.add(str(kb_id))
        if not build_kb_ids:
            continue
        # Only use a build whose KB has a real supersedence chain (cumulative update)
        closure = set()
        for kb in build_kb_ids:
            closure |= graph_closure(kb, supersede_graph)
        superseded = closure - build_kb_ids
        if superseded:
            return superseded
    return set()


# ---------------------------------------------------------------------------
# Ruby: get_installed_kbs_from_earlier_builds
# ---------------------------------------------------------------------------

def get_installed_kbs_from_earlier_builds(kb_base, current_build):
    """KBs from builds lower than current in the same major version line."""
    if not kb_base or not current_build:
        return set()

    current_major = str(current_build).split(".")[0]
    earlier_kbs = set()
    for build, build_data in kb_base.items():
        build_major = str(build).split(".")[0]
        if build_major != current_major:
            continue
        if compare_builds(build, current_build) >= 0:
            continue
        for patch in (build_data or {}).get("kb_articles") or []:
            kb_id = patch.get("kb_id")
            if kb_id and str(kb_id) != "0":
                earlier_kbs.add(str(kb_id))
    return earlier_kbs


# ---------------------------------------------------------------------------
# BFS closure (Ruby: get_all_superseded_kbs — includes start node)
# ---------------------------------------------------------------------------

def graph_closure(start, graph):
    """BFS transitive closure from start (including start itself, like Ruby)."""
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


# ---------------------------------------------------------------------------
# CVE enrichment
# ---------------------------------------------------------------------------

def build_cve_index(server_dir):
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


# ---------------------------------------------------------------------------
# get_cves_for_kb — check both vuln_system_associations index and kb_cves
# ---------------------------------------------------------------------------

def get_cves_for_kb(kb, kb_to_cves, kb_cves):
    """Get all CVEs associated with a KB.

    Uses vuln_system_associations (proper CVE-YYYY-NNNNN format).
    Note: kb_cves from kb_info.json contains the SAME CVEs but in numeric-only
    format (e.g. '202438203' instead of 'CVE-2024-38203'). Using both would
    double-count, so we only use vuln_system_associations here.
    """
    return set(kb_to_cves.get(kb, set()))


# ---------------------------------------------------------------------------
# Main — mirrors get_windows_vuln.rb#get_affected_cves
# ---------------------------------------------------------------------------

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
    print(f"  OS           : {os_info.get('name', 'Unknown')} ({os_info.get('version', '')})")
    print(f"  os_id/os_type: {os_id} / {os_type}")
    print(f"  Analog data  : {server_dir}")

    if os_type != OS_TYPE_WINDOWS:
        print("\n  This mapper implements the Windows vuln_system_associations approach.")
        print("  The gathered details are not Windows (os_type != 1) — nothing to map.")
        return

    # Load all catalog data
    kb_to_cves = build_kb_to_cves(server_dir, os_id)
    cve_index = build_cve_index(server_dir)
    supersede_graph, kb_base, kb_cves = load_kb_info_for_os(server_dir, os_id)

    current_build = str(
        os_info.get("build")
        or os_info.get("os_version")
        or os_info.get("version")
        or ""
    ).strip()

    print(f"  Catalog      : {len(kb_to_cves)} KB articles mapped to CVEs for os_id {os_id}")
    print(f"  Supersedence : {len(supersede_graph)} KB nodes")
    print(f"  KB base      : {len(kb_base)} builds indexed")
    print(f"  Current build: {current_build}")

    # -----------------------------------------------------------------------
    # Step 1: Collect installed KBs from scan input
    # -----------------------------------------------------------------------
    installed_kbs = set()
    for product in scan.get("products", []):
        for patch in product.get("installed_patches", []):
            installed_kbs |= kb_candidates(patch)
    scan_count = len(installed_kbs)

    # Step 2: Add KBs from earlier builds (Ruby: get_installed_kbs_from_earlier_builds)
    earlier_kbs = get_installed_kbs_from_earlier_builds(kb_base, current_build)
    installed_kbs |= earlier_kbs

    # Step 3: Add base KBs for current OS build (Ruby: get_base_kbs_for_current_os)
    base_kbs = get_base_kbs_for_build(kb_base, current_build, supersede_graph)
    installed_kbs |= base_kbs

    # Note: the three sources overlap, so the deduplicated total is <= scan+earlier+base.
    print(f"  Installed KBs: {len(installed_kbs)} total "
          f"(scan={scan_count}, earlier_builds={len(earlier_kbs)}, base={len(base_kbs)}; "
          f"sources overlap)")

    # Step 4: Recursively expand installed KBs (Ruby: get_all_superseded_kbs for each)
    recursive_installed_kbs = set()
    for kb in installed_kbs:
        recursive_installed_kbs |= graph_closure(kb, supersede_graph)

    print(f"  Installed KBs after recursion: {len(recursive_installed_kbs)}")

    # -----------------------------------------------------------------------
    # Step 5: Collect missing KBs from scan input
    # -----------------------------------------------------------------------
    missing_kbs_input = set()
    for product in scan.get("products", []):
        for patch in product.get("missing_patches", []):
            missing_kbs_input |= kb_candidates(patch)

    # Step 6: Recursively expand missing KBs (Ruby: get_all_superseded_kbs)
    recursive_missing_kbs = set()
    for kb in missing_kbs_input:
        recursive_missing_kbs |= graph_closure(kb, supersede_graph)

    # Step 7: Subtract installed (Ruby: missing_kbs = superseded_missing_kbs - installed_kbs)
    effective_missing_kbs = recursive_missing_kbs - recursive_installed_kbs

    print(f"  Missing KBs  : {len(missing_kbs_input)} input -> "
          f"{len(recursive_missing_kbs)} recursive -> "
          f"{len(effective_missing_kbs)} after subtracting installed")

    # -----------------------------------------------------------------------
    # Step 8: Get CVEs for missing KBs (Ruby: affected_cves)
    # -----------------------------------------------------------------------
    affected_cves = set()
    for kb in effective_missing_kbs:
        affected_cves |= get_cves_for_kb(kb, kb_to_cves, kb_cves)

    # Step 9: Get CVEs for installed KBs (Ruby: fixed_cves)
    fixed_cves = set()
    for kb in recursive_installed_kbs:
        fixed_cves |= get_cves_for_kb(kb, kb_to_cves, kb_cves)

    # Step 10: Result = affected - fixed (Ruby: affected_cves - fixed_cves)
    net_cves = affected_cves - fixed_cves

    print(f"\n  Affected CVEs (raw from missing KBs): {len(affected_cves)}")
    print(f"  Fixed CVEs (from installed KBs):      {len(fixed_cves)}")
    print(f"  Net affected CVEs:                    {len(net_cves)}")

    # -----------------------------------------------------------------------
    # Build per-patch breakdown for output
    # -----------------------------------------------------------------------
    mapped_patches = []
    cve_to_patches = {}

    for product in scan.get("products", []):
        for patch in product.get("missing_patches", []):
            cands = kb_candidates(patch)
            patch_recursive = set()
            for kb in cands:
                patch_recursive |= graph_closure(kb, supersede_graph)
            patch_recursive -= recursive_installed_kbs

            patch_cves = set()
            for kb in patch_recursive:
                patch_cves |= get_cves_for_kb(kb, kb_to_cves, kb_cves)
            patch_net = patch_cves & net_cves

            kb_display = next(iter(sorted(cands)), "N/A")
            mapped_patches.append({
                "kb":        kb_display,
                "title":     patch.get("title", "Unknown"),
                "severity":  patch.get("severity"),
                "product":   product.get("name"),
                "cve_count": len(patch_net),
                "cves":      sorted(patch_net),
            })
            for cve in patch_net:
                cve_to_patches.setdefault(cve, set()).add(kb_display)

    # Consolidated CVE list
    cve_list = []
    for cve in sorted(net_cves):
        meta = cve_index.get(cve, {})
        cve_list.append({
            "cve":             cve,
            "cwe":             meta.get("cwe"),
            "published_epoch": meta.get("published_epoch"),
            "fixed_by_kbs":    sorted(cve_to_patches.get(cve, set())),
        })

    # --- Console summary ---
    print(f"\n  Missing patches: {len(mapped_patches)}")
    print("-" * 70)
    for mp in mapped_patches:
        print(f"  KB {mp['kb']:<10} [{mp['severity'] or 'unknown'}]  {mp['title'][:48]}")
        print(f"     -> {mp['cve_count']} net CVE(s) remediated by this patch")

    print(f"\n  Net distinct CVEs the endpoint is exposed to: {len(cve_list)}")
    if cve_list:
        print("-" * 70)
        for c in cve_list[:25]:
            print(f"  {c['cve']:<18} {c.get('cwe') or '':<10} fixed_by: {', '.join(c['fixed_by_kbs'])}")
        if len(cve_list) > 25:
            print(f"  ... and {len(cve_list) - 25} more (see {os.path.basename(OUTPUT_FILE)})")

    # --- Write JSON result ---
    output = {
        "os_info": {
            "name":    os_info.get("name"),
            "version": os_info.get("version"),
            "os_id":   os_id,
            "os_type": os_type,
            "build":   current_build,
        },
        "source": "Analog kb_info.json + vuln_system_associations.json (get_windows_vuln.rb logic)",
        "total_missing_patches":    len(mapped_patches),
        "total_cves_affected_raw":  len(affected_cves),
        "total_cves_fixed":         len(fixed_cves),
        "total_cves":               len(cve_list),
        "debug": {
            "installed_kbs_from_scan":      scan_count,
            "installed_kbs_earlier_builds": len(earlier_kbs),
            "installed_kbs_base":           len(base_kbs),
            "recursive_installed_kbs":      len(recursive_installed_kbs),
            "missing_kbs_input":            len(missing_kbs_input),
            "recursive_missing_kbs":        len(recursive_missing_kbs),
            "effective_missing_kbs":        len(effective_missing_kbs),
        },
        "missing_patches": mapped_patches,
        "cves":            cve_list,
    }
    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        json.dump(output, f, indent=2, default=str)
    print(f"\n  Full results written to: {OUTPUT_FILE}")


if __name__ == "__main__":
    main()
