#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — catalog-counts
##
##  Print catalog-wide totals from the OPSWAT "Analog" offline catalog:
##    * Applications      - unique products and total signatures (products.json); how many of
##                          those products have vulnerability detection (i.e. a CVE mapping in
##                          vuln_associations.json).
##    * Patchable apps    - from patch_aggregation_v2, at three deliberate granularities:
##                          patch titles (unique products, architectures merged - the number that
##                          is comparable to other patch vendors' published "software titles"),
##                          unique signatures (one per architecture/install), and raw patch
##                          entries (one per patch family/package); plus how many titles are new
##                          in the last NEW_APP_WINDOW_DAYS days (earliest release_date).
##    * Driver/firmware   - BIOS / Driver / Firmware patch counts (driver_firmware_patch_aggregation).
##    * KBs               - unique KB numbers across every OS section (kb_info.json).
##    * CVEs              - unique CVEs in cves.json, and unique CVEs referenced by mappings.
##    * CVE mappings      - OS (vuln_system_associations) + 3rd-party (vuln_associations) rows.
##    * moby-aligned      - detectable / patchable per platform in the same units as the compliance
##                          support chart (client/compliance/*/docs/moby/moby.json), compared to it
##                          product by product with every difference named.
##
##  Usage:
##      python3 catalog-counts.py
##
##  Note: cves.json (~188 MB) and vuln_system_associations.json (~670 MB) are large; the CVE
##  section takes a minute or two and a few GB of RAM to read.
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import os
import sys
from collections import Counter, defaultdict
from datetime import date, datetime

import _catalog as cat
try:
    import eol_rules        # internal, optional: vendor-comparable "active third-party titles" view
except ImportError:
    eol_rules = None

# A patch title counts as a "new application" if its earliest vendor release_date (across every
# patch row for the product, not just the latest) falls inside this many days of today.
NEW_APP_WINDOW_DAYS = 90


def parse_date(value):
    """Parse a catalog date. patch_aggregation_v2 mixes ISO ('2026-06-16', optionally with a
    time suffix) and US ('05/04/2026') strings, so accept both; return a date or None."""
    if not value:
        return None
    text = str(value).strip()
    try:
        return date.fromisoformat(text[:10])
    except ValueError:
        pass
    for fmt in ("%m/%d/%Y", "%m/%d/%y"):
        try:
            return datetime.strptime(text[:10], fmt).date()
        except ValueError:
            continue
    return None


# patch_aggregation_v2 covers Windows, macOS AND Linux in one file, but no row or package carries an
# OS field, so the platform is inferred from the package download links: the installer's file
# extension first, then URL keywords for extension-less redirect links (e.g.
# apple.com/itunes/download/win64, aka.ms/...x86...). One latest row usually targets one platform;
# a row that ships installers for several is counted under each.
_WIN_EXT = {".exe", ".msi", ".msix", ".msixbundle", ".appx", ".msp", ".cab"}
_MAC_EXT = {".dmg", ".pkg"}
_LIN_EXT = {".deb", ".rpm", ".tar", ".gz", ".tgz", ".xz", ".bz2", ".appimage"}
_WIN_KEYS = ("win32", "win64", "windows", "/win/", "x86", "x64", "amd64", "msvc")
_MAC_KEYS = ("mac", "osx", "darwin")


def platforms_of(row):
    """Return the set of platforms ('windows' | 'mac' | 'linux') a patch row's packages target,
    or {'unknown'} if none of its download links can be classified."""
    found = set()
    for pkg in row.get("packages") or []:
        for link in pkg.get("download_links") or []:
            path = link.split("?", 1)[0].lower()
            ext = os.path.splitext(path)[1]
            if ext in _WIN_EXT:
                found.add("windows")
            elif ext in _MAC_EXT:
                found.add("mac")
            elif ext in _LIN_EXT:
                found.add("linux")
            elif any(k in path for k in _WIN_KEYS):
                found.add("windows")
            elif any(k in path for k in _MAC_KEYS):
                found.add("mac")
            elif "linux" in path:
                found.add("linux")
    return found or {"unknown"}


# Product ids are allocated in per-platform ranges (Windows < 100000 <= macOS < 200000 <= Linux),
# e.g. Google Chrome is 39 / 100004 / 200001. moby.json's per-platform product_ids follow the same
# ranges, so this is what lets the server data be split the same way moby splits it.
MOBY_PLATFORMS = (("windows", "WINDOWS"), ("mac", "MAC"), ("linux", "LINUX"))


def platform_of_pid(pid):
    pid = int(pid)
    return "windows" if pid < 100000 else "mac" if pid < 200000 else "linux"


def app_key(name):
    """Name-based identity of an application. products.json can hold several product ids for the
    same app (e.g. Flock 89 'Flock, Inc.' and 3111 'Flock FZ LLC', GoToMeeting 1394 Citrix and 3025
    LogMeIn) when the vendor or installer changed; moby.json is keyed by product name, so it holds
    one entry per name per platform and files every signature under a single id."""
    return " ".join(str(name or "").casefold().split())


def compare_to_moby(analog_pids, moby_pids, name_by_pid):
    """Compare one platform's analog product ids with moby's {pid: name}, the way moby counts.

    analog_pids are collapsed to one per app name (duplicates = extra ids folded in). Ids left
    over on both sides after matching by id are then paired by app name: the same app filed under
    a different id is not a difference. Returns (analog_app_count, duplicates, renamed, only_moby,
    only_analog) where duplicates is [(kept_pid, [other_pids], name)], renamed is
    [(analog_pid, moby_pid, name)] and the only_* lists are [(pid, name)]."""
    groups = defaultdict(list)
    for pid in sorted(analog_pids):
        groups[app_key(name_by_pid.get(pid)) or f"#{pid}"].append(pid)
    duplicates = [(pids[0], pids[1:], name_by_pid.get(pids[0], ""))
                  for pids in groups.values() if len(pids) > 1]

    moby_only = {pid: name for pid, name in moby_pids.items() if pid not in analog_pids}
    analog_only = {}
    for key, pids in groups.items():
        if not any(pid in moby_pids for pid in pids):
            analog_only[key] = pids[0]
    renamed = []
    for mpid, mname in sorted(moby_only.items()):
        apid = analog_only.pop(app_key(mname), None)
        if apid is not None:
            renamed.append((apid, mpid, mname))
    matched_moby = {mpid for _a, mpid, _n in renamed}
    only_moby = [(pid, name) for pid, name in sorted(moby_only.items()) if pid not in matched_moby]
    only_analog = sorted((pid, name_by_pid.get(pid, "")) for pid in analog_only.values())
    return len(groups), duplicates, renamed, only_moby, only_analog


def load_moby(server):
    """Per-platform product-id sets from the compliance support chart (moby.json) that ships in the
    same analog package: {'detectable'|'patchable': {platform: {pid: name}}}, plus its timestamp.
    All three platform copies of moby.json are identical; the Windows one is read. Returns
    (None, None) when the file is absent."""
    path = os.path.join(os.path.dirname(server), "client", "compliance", "windows",
                        "docs", "moby", "moby.json")
    if not os.path.isfile(path):
        return None, None
    moby = cat.load_json(path)
    sets = {"detectable": defaultdict(dict), "patchable": defaultdict(dict)}
    for name, by_plat in (moby.get("products") or {}).items():
        for plat, key in MOBY_PLATFORMS:
            p = by_plat.get(key)
            if not p:
                continue
            pid = p.get("product_id")
            if p.get("cve_detection"):
                sets["detectable"][plat][pid] = name
            if any(s.get("support_auto_patching") for s in p.get("signatures") or []):
                sets["patchable"][plat][pid] = name
    return sets, moby.get("timestamp")


def main():
    server = cat.require_server_dir()

    print("Catalog counts (OPSWAT Analog)")
    print("=" * 70)

    # --- Applications (products.json): unique products + total signatures --------------------
    total_signatures = 0
    unique_products = set()
    vendor_by_pid = {}   # product id -> vendor name (products.json stores vendor as {id, name})
    name_by_pid = {}
    # Products flagged patchable in products.json: third-party patching on the product or any of
    # its signatures, OR system patching (Microsoft products updated through Windows Update - SQL
    # Server, Office MSI, Exchange, SharePoint, ...). This union is what moby.json calls
    # auto-patchable; patch_aggregation_v2 alone misses the system-patched ones.
    flag_patchable = set()
    pj = os.path.join(server, "products.json")
    if os.path.isfile(pj):
        for r in cat.read_records(pj):
            product = r.get("product") or {}
            pid = product.get("id")
            sigs = r.get("signatures") or []
            if pid is not None:
                unique_products.add(pid)
                name_by_pid[pid] = product.get("name") or ""
                vendor = r.get("vendor") or ""
                if isinstance(vendor, dict):
                    vendor = vendor.get("name") or ""
                vendor_by_pid[pid] = str(vendor)
                if (r.get("support_3rd_party_patch") or r.get("support_system_patch")
                        or any(s.get("support_3rd_party_patch") for s in sigs)):
                    flag_patchable.add(pid)
            total_signatures += len(sigs)

    # --- Patchable applications (patch_aggregation_v2.json, latest) --------------------------
    # Prefer the v2 dataset (has is_latest / product.v4_pid / v4_signatures / data_source). If it
    # isn't present, fall back to the v1 patch_aggregation.json (flat records keyed by _id with
    # product_name and no data_source) and, per the v1 format, count every application as
    # OPSWAT-verified.
    #
    # An is_latest row is one latest PATCH PACKAGE per (product, patch family) - i.e. typically one
    # row per architecture (x86 / x86_64) or edition. So the raw row count over-counts products, and
    # we report three deliberately different granularities:
    #
    #   * patch titles     - unique products (product.v4_pid), architectures MERGED. This is the
    #                        apples-to-apples number patch vendors publish as their headline catalog
    #                        size: Ivanti's "software titles" lists one row per vendor/product/edition
    #                        with Architecture as a multi-valued column, and Patch My PC calls the same
    #                        thing its "Unique Product Count". Editions stay separate (Firefox vs
    #                        Firefox ESR), architectures collapse.
    #   * unique signatures - distinct v4_signature ids (one per detectable install, so x86 and x64 are
    #                        separate). Comparable to a per-variant count such as Patch My PC's
    #                        "Product Count".
    #   * patch entries    - raw is_latest rows (per patch family / package).
    #
    # product.name is NOT used as the unit: distinct products can share a display name, and it
    # collapses architecture variants, so it lands below the title count.
    pav = os.path.join(server, "patch_aggregation_v2.json")
    pav1 = os.path.join(server, "patch_aggregation.json")
    patch_total = 0
    patch_titles = set()      # unique products (v4_pid) - architectures merged
    title_rows = {}           # v4_pid -> (vendor, name), for the EOL / comparable-title pass
    patch_sigs = set()        # unique v4_signature ids - one per architecture/install
    patch_names = set()       # distinct product display names (reference only)
    by_source = Counter()
    # Earliest vendor release_date seen for each title, across EVERY patch row (older versions
    # included, not just is_latest). A title whose oldest known patch is recent only just entered
    # the catalog, which is the best available proxy for "new application". (sdk_last_modified_date
    # is not usable for this: it is bumped on every catalog rebuild, so it is recent for everything.)
    first_release = {}
    # Per-platform breakdown (windows / mac / linux / unknown), inferred by platforms_of(). The
    # all-platform totals above are unions; a multi-platform title appears under each platform.
    plat_titles = defaultdict(set)
    plat_sigs = defaultdict(set)
    plat_entries = Counter()
    if os.path.isfile(pav):
        patch_src = "patch_aggregation_v2.json"
        for r in cat.read_records(pav):
            product = r.get("product") or {}
            pid = product.get("v4_pid")
            released = parse_date(r.get("release_date"))
            if pid is not None and released and (pid not in first_release
                                                 or released < first_release[pid]):
                first_release[pid] = released
            if not r.get("is_latest"):
                continue
            patch_total += 1
            sigs = product.get("v4_signatures") or []
            if pid is not None:
                patch_titles.add(pid)
                title_rows[pid] = (vendor_by_pid.get(pid, ""), product.get("name") or "")
            for sig in sigs:
                patch_sigs.add(sig)
            name = product.get("name")
            if name:
                patch_names.add(name)
            by_source[str(r.get("data_source") or "unknown").lower()] += 1
            for plat in platforms_of(r):
                plat_entries[plat] += 1
                if pid is not None:
                    plat_titles[plat].add(pid)
                plat_sigs[plat].update(sigs)
    elif os.path.isfile(pav1):
        patch_src = "patch_aggregation.json"
        # v1 has no is_latest (each record is the latest patch for a product), no data_source and
        # no v4_pid / v4_signatures; the best available title unit is product_name, and signatures
        # cannot be counted. Assume all applications are OPSWAT-verified.
        patch_sigs = None
        for r in cat.read_records(pav1):
            patch_total += 1
            name = r.get("product_name")
            released = parse_date(r.get("release_date"))
            if name:
                patch_titles.add(name)
                patch_names.add(name)
                if released and (name not in first_release or released < first_release[name]):
                    first_release[name] = released
            by_source["opswat"] += 1
    else:
        patch_src = "patch_aggregation(_v2).json (not found)"
        patch_sigs = None

    today = date.today()
    new_titles = sum(1 for released in first_release.values()
                     if (today - released).days <= NEW_APP_WINDOW_DAYS)

    # --- Driver / firmware / BIOS patches (driver_firmware_patch_aggregation.json) -----------
    # This dataset is vendor-keyed ({header, dell, lenovo, ...}); each vendor maps patch-id ->
    # record with an opswat_component (BIOS / Driver / Firmware / Application / Other).
    df_total = 0
    df_by_component = Counter()
    df_by_vendor = Counter()
    df_path = os.path.join(server, "driver_firmware_patch_aggregation.json")
    if os.path.isfile(df_path):
        dfj = cat.load_json(df_path)
        for vendor, section in dfj.items():
            if vendor == "header" or not isinstance(section, dict):
                continue
            for _pid, rec in section.items():
                if not isinstance(rec, dict):
                    continue
                df_total += 1
                df_by_vendor[vendor] += 1
                df_by_component[str(rec.get("opswat_component") or "unknown")] += 1

    # --- KBs (kb_info.json): unique KB numbers across all OS sections ------------------------
    unique_kbs = set()
    ki = cat.load_json(os.path.join(server, "kb_info.json"))
    for element in ki.get("oesis", []):
        for key, value in element.items():
            if key in ("header", "id_os_map") or not isinstance(value, dict):
                continue
            for kb in (value.get("kb_tree") or {}):
                unique_kbs.add(str(kb))
            for kb in (value.get("kb_cves") or {}):
                unique_kbs.add(str(kb))
            for _build, bd in (value.get("kb_base") or {}).items():
                for patch in (bd or {}).get("kb_articles", []):
                    k = patch.get("kb_id")
                    if k and str(k) != "0":
                        unique_kbs.add(str(k))

    # --- CVE mappings + unique CVEs (large files) --------------------------------------------
    print("Reading CVE data (large files, please wait)...")

    print("  scanning vuln_system_associations.json (OS mappings)...", flush=True)
    os_mappings = 0
    os_cves = set()
    for r in cat.read_records(os.path.join(server, "vuln_system_associations.json")):
        cve = r.get("cve")
        if cve:
            os_mappings += 1
            os_cves.add(cve)

    print("  scanning vuln_associations.json (3rd-party mappings)...", flush=True)
    tp_mappings = 0
    tp_cves = set()
    vuln_pids = set()   # products (v4_pid) that have a CVE mapping = have vulnerability detection
    for r in cat.read_records(os.path.join(server, "vuln_associations.json")):
        cve = r.get("cve")
        if cve:
            tp_mappings += 1
            tp_cves.add(cve)
            for pid in (r.get("v4_pids") or []):
                vuln_pids.add(pid)

    print("  scanning cves.json (CVE database)...", flush=True)
    cve_db = set()
    for r in cat.read_records(os.path.join(server, "cves.json")):
        cve = r.get("cve")
        if cve:
            cve_db.add(cve)

    mapped_unique = os_cves | tp_cves
    apps_with_vuln = len(vuln_pids & unique_products)

    # --- moby-aligned view: the same units moby.json (and anything built on it) reports ----------
    #   detectable = products with a CVE mapping (vuln_associations)  ~ moby cve_detection
    #   patchable  = products.json third-party OR system patch flag    ~ moby support_auto_patching
    aligned = {"detectable": defaultdict(set), "patchable": defaultdict(set)}
    for pid in vuln_pids & unique_products:
        aligned["detectable"][platform_of_pid(pid)].add(pid)
    for pid in flag_patchable:
        aligned["patchable"][platform_of_pid(pid)].add(pid)
    moby_sets, moby_ts = load_moby(server)

    # --- Report ------------------------------------------------------------------------------
    print("\nApplications (products.json):")
    print(f"  unique applications (products)     : {len(unique_products)}")
    print(f"    with vulnerability detection     : {apps_with_vuln}   (have a CVE mapping)")
    print(f"    detection only (no CVE mapping)  : {len(unique_products) - apps_with_vuln}")
    print(f"  total application signatures       : {total_signatures}")

    sigs_str = "n/a (not in this dataset)" if patch_sigs is None else str(len(patch_sigs))
    print(f"Patchable applications ({patch_src}, latest):")
    print(f"  patch titles (unique products)     : {len(patch_titles)}   (architectures merged - "
          f"vendor-comparable 'software titles')")
    print(f"  unique signatures                  : {sigs_str}   (one per architecture/install)")
    print(f"  total patch entries                : {patch_total}   (one per patch family/package)")
    new_label = f"  new applications (last {NEW_APP_WINDOW_DAYS}d)"
    print(f"{new_label:<37}: {new_titles}   "
          f"(first patch release_date within {NEW_APP_WINDOW_DAYS}d of {today})")
    print(f"    distinct product names           : {len(patch_names)}   (reference only)")
    for src in sorted(by_source):
        label = "opswat (OPSWAT-verified)" if src == "opswat" else src
        print(f"    {label:<24}: {by_source[src]}")
    if plat_entries:
        # The all-platform figures above are unions; a title that ships for several platforms is
        # counted under each platform here, so the platform rows can sum to more than the total.
        print(f"  by platform (inferred from package type; multi-platform titles count in each):")
        print(f"    {'platform':<10} {'titles':>7} {'signatures':>11} {'entries':>8}")
        for plat in ("windows", "mac", "linux", "unknown"):
            if plat in plat_entries:
                print(f"    {plat:<10} {len(plat_titles[plat]):>7} "
                      f"{len(plat_sigs[plat]):>11} {plat_entries[plat]:>8}")

    # Vendor-comparable view (see eol_rules.py): versions collapsed into titles, Microsoft rows
    # split out, vendor-announced EOL removed. This is the only basis on which competitor lists
    # can be compared fairly - their headline counts carry Microsoft OS/server rows and EOL
    # products that OPSWAT's third-party catalog does not.
    comparable = eol_rules.summarize(list(title_rows.values())) if (title_rows and eol_rules) else None
    if comparable:
        print(f"  comparable view (eol_rules.py, rules as of {eol_rules.RULES_AS_OF}):")
        print(f"    titles, versions collapsed      : {comparable['titles']}")
        print(f"    Microsoft titles                : {comparable['microsoft']}")
        print(f"    EOL titles                      : {comparable['eol']}   "
              f"(Microsoft {comparable['eol_microsoft']}, third-party {comparable['eol_third_party']})")
        print(f"    ACTIVE third-party titles       : {comparable['active_third_party']}   "
              f"<- compare this to competitors")
        if comparable["eol_third_party_titles"] or comparable["eol_microsoft_titles"]:
            print(f"    EOL titles still in the catalog (cleanup candidates):")
            for display, reason in comparable["eol_third_party_titles"] + comparable["eol_microsoft_titles"]:
                print(f"      - {display.split(' | ', 1)[-1]}   [{reason}]")

    # Same units as moby.json's counts block, per platform. When moby.json ships in the package the
    # two are compared product by product and every difference is named, so a mismatch can be
    # chased to a catalog record rather than argued about as a number.
    print("moby-aligned view (compare to moby.json / dashboards built on it):")
    print("  detectable = has a CVE mapping (vuln_associations); "
          "patchable = products.json 3rd-party or system patch flag")
    print("  counted per application name, as moby does: product ids sharing a name are one app")
    if moby_sets:
        print(f"  moby.json timestamp: {moby_ts}")
        print(f"    {'':<18} {'platform':<8} {'analog':>7} {'moby':>6} {'diff':>6}")
    else:
        print("  (moby.json not found next to the server data - analog counts only)")
        print(f"    {'':<18} {'platform':<8} {'analog':>7}")
    aligned_totals = {}
    for kind in ("detectable", "patchable"):
        total_a = total_m = 0
        for plat, _key in MOBY_PLATFORMS:
            m = moby_sets[kind][plat] if moby_sets else {}
            apps, dups, renamed, only_moby, only_analog = compare_to_moby(
                aligned[kind][plat], m, name_by_pid)
            total_a += apps
            if not moby_sets:
                print(f"    {kind:<18} {plat:<8} {apps:>7}")
            else:
                total_m += len(m)
                print(f"    {kind:<18} {plat:<8} {apps:>7} {len(m):>6} {apps - len(m):>+6}")
            for kept, others, name in dups:
                print(f"      duplicate app : {kept} {name}  (also {', '.join(map(str, others))})")
            for apid, mpid, name in renamed:
                print(f"      same app      : {apid} {name}  (moby files it under {mpid})")
            for pid, name in only_moby:
                print(f"      only in moby  : {pid} {name}")
            for pid, name in only_analog:
                print(f"      only in analog: {pid} {name}")
        aligned_totals[kind] = (total_a, total_m if moby_sets else None)
        if moby_sets:
            print(f"    {kind:<18} {'total':<8} {total_a:>7} {total_m:>6} {total_a - total_m:>+6}")
        else:
            print(f"    {kind:<18} {'total':<8} {total_a:>7}")

    print("Driver / firmware patches (driver_firmware_patch_aggregation.json):")
    print(f"  BIOS                               : {df_by_component.get('BIOS', 0)}")
    print(f"  Driver                             : {df_by_component.get('Driver', 0)}")
    print(f"  Firmware                           : {df_by_component.get('Firmware', 0)}")
    for comp in sorted(df_by_component):
        if comp not in ("BIOS", "Driver", "Firmware"):
            print(f"  {comp:<34} : {df_by_component[comp]}")
    print(f"  total                              : {df_total}")
    if df_by_vendor:
        print(f"  by vendor                          : "
              + ", ".join(f"{v}={df_by_vendor[v]}" for v in sorted(df_by_vendor)))

    print("KBs (kb_info.json):")
    print(f"  unique KBs                         : {len(unique_kbs)}")

    print("CVE mappings:")
    print(f"  OS mappings (vuln_system_associations)   : {os_mappings}")
    print(f"  3rd-party mappings (vuln_associations)   : {tp_mappings}")
    print(f"  total CVE mappings                       : {os_mappings + tp_mappings}")

    print("CVEs:")
    print(f"  unique CVEs in cves.json                 : {len(cve_db)}")
    print(f"  unique CVEs referenced by mappings       : {len(mapped_unique)}")
    print(f"    - OS-mapped unique CVEs                 : {len(os_cves)}")
    print(f"    - 3rd-party-mapped unique CVEs          : {len(tp_cves)}")

    print("\n" + "=" * 70)
    print("Summary:")
    print(f"  Applications : {len(unique_products)} unique "
          f"({apps_with_vuln} with vulnerability detection) / {total_signatures} signatures")
    print(f"  Patchable    : {len(patch_titles)} titles / {sigs_str} signatures / "
          f"{patch_total} patch entries / {new_titles} new in {NEW_APP_WINDOW_DAYS}d")
    if comparable:
        print(f"  Comparable   : {comparable['active_third_party']} active third-party titles "
              f"({comparable['titles']} collapsed titles - {comparable['microsoft']} Microsoft - "
              f"{comparable['eol_third_party']} EOL); {comparable['eol']} EOL titles to clean up")
    det, pat = aligned_totals["detectable"], aligned_totals["patchable"]
    moby_note = (f"   (moby.json: {det[1]} / {pat[1]})" if moby_sets else "")
    print(f"  moby-aligned : {det[0]} detectable / {pat[0]} patchable{moby_note}")
    print(f"  Driver/FW    : {df_total} total "
          f"(BIOS {df_by_component.get('BIOS', 0)}, Driver {df_by_component.get('Driver', 0)}, "
          f"Firmware {df_by_component.get('Firmware', 0)})")
    print(f"  KBs          : {len(unique_kbs)} unique")
    print(f"  CVEs         : {len(cve_db)} unique")
    print(f"  CVE mappings : {os_mappings + tp_mappings} total")


if __name__ == "__main__":
    main()
