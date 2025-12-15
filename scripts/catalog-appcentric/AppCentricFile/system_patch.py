"""
OPSWAT Patch Management Catalog Simplifier - System Patch Logic
----------------------------------------------------------------
Functions for extracting and processing **system (OS)** patch and vulnerability
data from the OPSWAT Analog server JSONs:
  - patch_system_aggregation.json
  - vuln_system_associations.json

This mirrors the responsibilities of `third_party.py`, but for system content.

Author: Chris Seiler
"""

from operator import contains
from typing import Any, Dict, List, Optional, Iterable, Tuple
from patch_classes import Package, Property, Patch, Vulnerability, Range, Product, Vendor, Product_Line
from util import (
    normalize_architectures_list,
    create_package_name,
    generate_id,
    get_reboot_status,
    normalize_release_date,
    generate_bulletin_id,
    get_platform_from_signature,
    to_dict,
    load_json,
    find_case_insensitive,
    get_section,
)


def is_supported_windows_os(os_name: str) -> bool:
    """
    Returns True if the OS is supported, False if it is in the unsupported list.
    Case-insensitive comparison and partial match friendly.
    """

    unsupported = {
        "Windows Server 2008 R2",
        "Windows Embedded Standard 7",
        "Windows 7",
        "Windows Server 2012",
        "Windows Server 2012 R2",
        "Windows 8.1",
        "Windows 8",
        "Windows 10 LTSB",
        "Windows Server 2016",
        "Windows Server 2008",
        "Windows Vista",
        "Windows Server 2003, Datacenter Edition",
        "Windows Server 2003",
        "Windows XP Embedded",
        "Windows XP",
        "Windows XP x64 Edition",
        "Windows 2000"
    }

    # Normalize input
    name = os_name.strip().lower()

    # Check for substring match in unsupported list (case-insensitive)
    for u in unsupported:
        if u.lower() in name:
            return False

    return True


def is_supported_windows_product(product_name: str) -> bool:
    """
    Returns True if the product name is supported.
    False if it matches an unsupported product name (case-insensitive, substring match).
    """

    unsupported_products = {
        "Microsoft Office 2010",
        "Microsoft Office 2007",
        "Microsoft Visual Studio 2013",
        "Microsoft Exchange Server 2013",
        "Microsoft Exchange Server 2016",
        "Microsoft Office 2013",
        "Microsoft Office 2016",
        "Microsoft SQL Server Express 2014",
        "Microsoft Exchange Server 2007",
        "Microsoft Exchange Server 2003",
        "Microsoft Exchange Server 2000",
        "Microsoft SQL Server Express 2012",
        "Microsoft SQL Server Express 2008",
        "Microsoft Visual Studio 2012",
        "Microsoft SQL Server Express 2016",
        "Microsoft Exchange Server 2010",
        "Microsoft Visual Studio 2015",
        "Silverlight",
        "MSXML",
        "Microsoft SQL Server 2008 R2",
        "Microsoft Sharepoint Server 2016",
        "Microsoft SQL Server Express 2017",
    }

    name = product_name.strip().lower()

    for u in unsupported_products:
        if u.lower() in name:
            return False
    return True


def has_supported_windows_os(os_list: list[str]) -> bool:
    """
    Returns True if any OS in the list is supported.
    Uses is_supported_os() to evaluate each string.
    """
    for os_name in os_list:
        if not is_supported_windows_os(os_name):
            return False
    return True


# -------------------------------
# Helpers: Packages & Patch build
# -------------------------------

def group_system_associations_by_kb_id(system_vuln_associations: dict) -> dict:
    """
    Group vulnerability associations by KB article ID.
    Returns a dict mapping kb_id to a list of CVEs.
    """
    result = {}
    for assoc in system_vuln_associations.values():
        kb_list = assoc.get("kb_articles")
        cve = assoc.get("cve")
        if kb_list is not None:
            for kb_element in kb_list:
                kb_id = kb_element.get("article_name")
                if kb_id is not None:
                    if cve is not None and cve not in result.setdefault(str(kb_id), []):
                        result[str(kb_id)].append(cve)
    return result

def group_patches_by_kb_id(patch_sys_aggr: dict) -> dict:
    """
    Group patches from patch_system_aggregation by KB article ID.
    Returns a dict mapping kb_id to a list of patch records.
    """
    result = {}
    for patch_id, patch in patch_sys_aggr.items():
        kb_id = patch.get("kb_id")
        if kb_id is not None:
            kb_id_str = str(kb_id)
            result.setdefault(kb_id_str, []).append(patch)
    return result

def _build_system_package(dlink: Optional[dict], version: Optional[str]) -> List[Package]:
    """Create Package objects from a patch_system_aggregation download_link entry.

    Schema (server): patch_system_aggregation.json
      download_link: {
          "architecture": "64-bit" | "32-bit" | "arm64" (optional),
          "os_architecture": string (optional),
          "language": string (optional),
          "link": string,
          "sha1": string (optional)
      }
    """
    if not isinstance(dlink, dict):
        return []
    link = dlink.get("link")
    if not link:
        return []
    version = version or ""
    language = dlink.get("language") or ""
    arch = normalize_architectures_list(dlink.get("architecture"))  # normalize to [x86_64] etc.
    # Package.sha256 does not have a dedicated sha1 slot; preserve sha1 in the name for traceability.
    name = create_package_name(link, version, language, arch)  # e.g., vendor_filename ver lang arch
    pkg_id = generate_id("package", dlink.get("sha1") or link, version, "system")
    return [
        Package(
            url=link,
            package_id=pkg_id,
            language=language or None,
            sha256=None,  # sha1 available in properties on Patch
            architecture=arch[0] if arch else None,
            name=name,
        )
    ]


def get_system_patch(
    kb_id: str,
    rec: Any,
    cve_list: List[str],
) -> Optional[Patch]:
    """Build a Patch object from patch_system_aggregation using an analog_id.

    * signature_id: OESIS signature (e.g., Windows Update Agent)
    * analog_id: the _id from patch_system_aggregation.json (GUID-like string)
    * patch_sys_aggr: dict keyed by _id for fast lookups
    """


    # Fields per schema
    # title or fallback to product_name
    name = rec.get("title") or rec.get("product_name") or "Windows Security Update"
    kb = rec.get("kb_id") or None
    cves = cve_list
    release_notes = rec.get("release_note_link")
    release_date = normalize_release_date(rec.get("release_date"))
    reboot_required = get_reboot_status(rec.get("requires_reboot"))
    architectures = normalize_architectures_list(rec.get("architectures") or [])
    version = None  # system patches typically don't expose a semantic version in this record
    lang_default = (rec.get("download_link") or {}).get("language")

    # Packages
    packages = _build_system_package(rec.get("download_link"), version)

    # Properties carry any extra bits like sha1, category, severity, vendor, optional flag
    prop_list = []
    for k in ("sha1", "category", "severity", "vendor", "optional"):
        v = (rec.get("download_link", {}).get(k) if k == "sha1" else rec.get(k))
        if v is not None and v != "":
            prop_list.append(Property(name=k, value=str(v)))

    # Identity
    patch_id = generate_id("patch", name, version or "", release_date or "")
    bulletin = generate_bulletin_id("MS", name, version or "", release_date or "")  # "SP" = System Patch
    platform = "Windows"

    return Patch(
        name=name,
        bulletin=bulletin,
        patch_id=patch_id,
        platform=platform,
        patch_type="system",
        kb_article=str(kb) if kb else None,
        cve=cves,
        latest=False,  # server does not determine "latest" for system patches
        language=lang_default,
        packages=packages,
        fresh_installable=None,
        background_patching=None,
        validation_supported=None,
        properties=prop_list,
        version=version,
        release_notes=release_notes,
        eula=None,
        uninstall_required=None,
        reboot_required=reboot_required,
        architectures=architectures or [],
        language_default=lang_default,
        release_date=release_date,
        
    )






def get_windows_system_patches(
    patch_sys_aggr: Dict[str, List[dict]],
    kb_cve_sys_assoc: Dict[str, List[str]],
) -> List[Patch]:
    """Resolve a list of system patches from a collection of KB IDs mapped to patch records."""
    patches: List[Patch] = []
    allOSs: List[str] = []
    name_list: List[str] = []
    totalCount = 0
    count = 0

    for kb_id, patch_list in (patch_sys_aggr or {}).items():
        for patch in patch_list:

            totalCount += 1

            product_name = patch.get("product_name")
            if is_supported_windows_product(product_name):

                os_info_list = patch.get("os_info")
                if has_supported_windows_os(os_info_list):

                    cve_list = []
                    if kb_id in kb_cve_sys_assoc:
                        cve_list = kb_cve_sys_assoc[kb_id]

                    p = get_system_patch(
                        kb_id,
                        patch,
                        cve_list
                    )
                    if p is not None:
                        patches.append(p)
                    count += 1

                if count % 50 == 0 and count > 0:
                    print(f"Processed {count} windows system entries...")
        
    return patches


# ------------------------------------------
# Vulnerabilities from vuln_system_associations
# ------------------------------------------

def _windows_vulns_for_os_id(win_assoc: List[dict], os_id: Optional[int]) -> List[Vulnerability]:
    vulns: List[Vulnerability] = []
    if os_id is None:
        return vulns
    for rec in win_assoc or []:
        kb_items = rec.get("kb_articles") or []
        for kb in kb_items:
            os_ids = kb.get("os_id") or []
            if any(str(os_id) == str(x) for x in os_ids):
                vulns.append(Vulnerability(cve=rec.get("cve")))
                break
    return vulns


def _linux_vulns_for_os_name(lin_assoc: List[dict], os_name: Optional[str]) -> List[Vulnerability]:
    vulns: List[Vulnerability] = []
    if not os_name:
        return vulns
    for rec in lin_assoc or []:
        for affected in rec.get("affected_os") or []:
            if str(affected.get("os_name")).strip().lower() == str(os_name).strip().lower():
                ranges = []
                for pkg in affected.get("affected_packages") or []:
                    for r in pkg.get("ranges") or []:
                        ranges.append(Range(start=r.get("start", ""), end=r.get("limit_except", "")))
                vulns.append(Vulnerability(cve=rec.get("cve"), ranges=ranges or None))
                break
    return vulns


def _mac_vulns_for_os_id(mac_assoc: List[dict], os_id: Optional[int]) -> List[Vulnerability]:
    vulns: List[Vulnerability] = []
    if os_id is None:
        return vulns
    for rec in mac_assoc or []:
        for affected in rec.get("affected_os") or []:
            if str(affected.get("os_id")) == str(os_id):
                ranges = []
                for r in affected.get("ranges") or []:
                    ranges.append(Range(start=r.get("start", ""), end=r.get("limit_except", "")))
                vulns.append(Vulnerability(cve=rec.get("cve"), ranges=ranges or None))
                break
    return vulns


def get_system_vulnerabilities(
    vuln_system_assoc: Dict[str, Any],
    os_type: str,
    os_id: Optional[int] = None,
    os_name: Optional[str] = None,
) -> List[Vulnerability]:
    """Return Vulnerability objects for a given OS.

    * os_type: one of "windows", "linux", "macos"
    * os_id: numeric OS id when appropriate (Windows/macOS schemas)
    * os_name: string OS name when appropriate (Linux schema)
    """
    os_type = (os_type or "").strip().lower()
    if os_type == "windows":
        win_list = get_section(vuln_system_assoc, ["windows_vuln_system_associations"]) or []
        return _windows_vulns_for_os_id(win_list, os_id)
    elif os_type == "linux":
        lin_list = get_section(vuln_system_assoc, ["linux_vuln_system_associations"]) or []
        return _linux_vulns_for_os_name(lin_list, os_name)
    elif os_type == "macos":
        mac_list = get_section(vuln_system_assoc, ["macos_vuln_system_associations"]) or []
        return _mac_vulns_for_os_id(mac_list, os_id)
    return []


# ------------------------------------------
# High-level builder similar to get_3rd_party_products
# ------------------------------------------

def get_windows_system_product(
    server_folder: str,
    inputs: List[Dict[str, Any]],
) -> List[dict]:
    """Build Product dicts for system patching, given client-reported data.

    Parameters
    ----------
    server_folder: str
        Path to the Analog /server folder containing:
          - patch_system_aggregation.json
          - vuln_system_associations.json
    inputs: List[dict]
        Each item is expected to look like:
        {
            "signature_id": 101,          # OESIS signature for the OS agent (e.g., WUA on Windows)
            "name": "Windows Update Agent",   # Optional display name
            "vendor": {"id": 90, "name": "Microsoft"},  # Optional
            "product_line": {"id": 1090, "name": "Windows"},  # Optional
            "patches": ["<analog_id>", ...],  # List of missing analog_ids returned by client (WUO/WUOV2)
            "os": { "type": "windows|linux|macos", "os_id": 81, "name": "Debian 12" }  # Optional; used for vuln mapping
        }

    Returns
    -------
    List[dict] serialized Product objects that include patches and vulnerabilities.
    """





    # Locate & load server files (case-insensitive)
    sys_aggr_path = find_case_insensitive(server_folder, "patch_system_aggregation.json")
    vsys_path = find_case_insensitive(server_folder, "vuln_system_associations.json")

    sys_aggr_raw = load_json(sys_aggr_path)
    vuln_sys_raw = load_json(vsys_path)

    # Find the element in the list that is 'windows_vuln_system_associations'
    windows_associations = None
    for elem in vuln_sys_raw.get("oesis", []):
        if isinstance(elem, dict) and "windows_vuln_system_associations" in elem:
            windows_associations = elem["windows_vuln_system_associations"]
            break

    system_aggregations = None
    for elem in sys_aggr_raw.get("oesis", []):
        if isinstance(elem, dict) and "patch_system_aggregation" in elem:
            system_aggregations = elem["patch_system_aggregation"]
            break

    kb_to_patch_map = group_patches_by_kb_id(system_aggregations)
    kb_to_cve_map = group_system_associations_by_kb_id(windows_associations)

    count = 0

    # Build Product shell
    product = Product()
    product.patches = get_windows_system_patches(kb_to_patch_map, kb_to_cve_map)


    product.signature_id = 1031  # Default to Windows Update Agent
    product.name = "Windows Update Agent"
    product.vendor = Vendor(0,"Microsoft")
    product.product_line = Product_Line("0", "Windows") #TODO: Need to fix this

    # Patches
    
    # Vulnerabilities (optional; requires OS hint)
    #os_info = item.get("os") or {}
    #os_type = os_info.get("type")
    #os_id = os_info.get("os_id") if os_info else None
    #os_name = os_info.get("name") if os_info else None
    #product.vulnerabilities = get_system_vulnerabilities(vuln_sys_raw, os_type, os_id=os_id, os_name=os_name)

    return product
