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


def main():
    server = cat.require_server_dir()

    print("Catalog counts (OPSWAT Analog)")
    print("=" * 70)

    # --- Applications (products.json): unique products + total signatures --------------------
    total_signatures = 0
    unique_products = set()
    vendor_by_pid = {}   # product id -> vendor name (products.json stores vendor as {id, name})
    pj = os.path.join(server, "products.json")
    if os.path.isfile(pj):
        for r in cat.read_records(pj):
            product = r.get("product") or {}
            pid = product.get("id")
            if pid is not None:
                unique_products.add(pid)
                vendor = r.get("vendor") or ""
                if isinstance(vendor, dict):
                    vendor = vendor.get("name") or ""
                vendor_by_pid[pid] = str(vendor)
            total_signatures += len(r.get("signatures") or [])

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
    print(f"  Driver/FW    : {df_total} total "
          f"(BIOS {df_by_component.get('BIOS', 0)}, Driver {df_by_component.get('Driver', 0)}, "
          f"Firmware {df_by_component.get('Firmware', 0)})")
    print(f"  KBs          : {len(unique_kbs)} unique")
    print(f"  CVEs         : {len(cve_db)} unique")
    print(f"  CVE mappings : {os_mappings + tp_mappings} total")


if __name__ == "__main__":
    main()
