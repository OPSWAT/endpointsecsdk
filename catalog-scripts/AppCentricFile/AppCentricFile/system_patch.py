"""
OPSWAT Patch Management Catalog Simplifier - System Patch Logic
--------------------------------------------------------------
This module contains functions for extracting and processing system patch and vulnerability data
from the OPSWAT Patch Management Catalog.

Author: Chris Seiler

Purpose:
- Handles the extraction and normalization of system patch, package, and vulnerability information.
- Used by the main script to build the simplified catalog output for system-level patches.
"""
import json
from collections import defaultdict
from typing import Dict, List

from typing import Any, Dict, List, Optional
from patch_classes import Package, Property, Patch, Vendor, Product_Line, Vulnerability, Range, Product
from util import (
    normalize_architectures_list,
    create_package_name,
    generate_id,
    get_bool_from_intvalue,
    get_bool_from_yesno_value,
    get_reboot_status,
    normalize_release_date,
    generate_bulletin_id,
    get_platform_from_signature,
    to_dict,
    group_associations_by_product_id,
    find_case_insensitive,
    load_json,
    get_section,
    is_cve_younger_than
)

def get_system_package(
    dlinks: List[Any],
    version: Optional[str],
) -> List[Package]:
    """
    Build a list of Package objects from a system patch-aggregation record.
    Each package represents a downloadable file for a system patch.
    """
    packages: List[Package] = []
    version = version or ""
    for d in dlinks:
        if not isinstance(d, dict):
            continue
        link = d.get("link") or d.get("url")
        if not link:
            continue
        hash_value = d.get("sha256") or d.get("checksum") or ""
        architecture = normalize_architectures_list(d.get("architecture"))
        language = d.get("language") or ""
        name = create_package_name(link, version, language, architecture)
        id_seed = hash_value or link
        package_id = generate_id("system_package", id_seed, version, "")
        packages.append(
            Package(
                url=link,
                id=package_id,
                language=language or None,
                sha256=hash_value or None,
                architecture=architecture[0] if architecture else None,
                name=name
            )
        )
    return packages

def get_system_patch(
    signature_id: str,
    patch_id: str,
    vuln_associations: Dict[str, Any],
    patch_aggr: Dict[str, Any],
    background_patching: bool,
    validation: bool,
    properties: List[Property]
) -> Optional[Patch]:
    """
    Build a Patch object from system patch aggregation data.
    Returns None if patch_id is not found.
    """
    if not patch_id:
        return None
    p = patch_aggr.get(str(patch_id))
    if not isinstance(p, dict):
        return None
    name = p.get("title") or p.get("system_name") or ""
    kb = p.get("kb_article") or p.get("kb") or None
    lang_default = p.get("language_default") or None
    latest = bool(p.get("fresh_installable"))
    if not latest and p.get("latest_version"):
        latest = True
    fresh_installable_int = get_bool_from_intvalue(p, "fresh_installable")
    version = p.get("latest_version") or None
    cves = []
    for key in ("cves", "v4_cves", "vulnerabilities"):
        vals = p.get(key)
        if isinstance(vals, list):
            cves = [str(x) for x in vals if x]
            break
    release_notes = p.get("release_note_link")
    eula = p.get("eula_link")
    uninstall_required = get_bool_from_yesno_value(p, "requires_uninstall_first")
    reboot_required = get_reboot_status(p.get("requires_reboot"))
    architectures = normalize_architectures_list(p.get("architectures")) or []
    lang_default = p.get("language_default")
    release_date = normalize_release_date(p.get("release_date"))
    patch_id = generate_id("system_patch", name, version, release_date)
    bulletin = generate_bulletin_id("SP", name, version, release_date)
    platform = get_platform_from_signature(signature_id)
    dlinks = p.get("download_links") or []
    packages = get_system_package(dlinks, version)
    return Patch(
        name=name,
        bulletin=bulletin,
        id=patch_id,
        platform=platform,
        kb_article=kb,
        cve=cves,
        latest=latest,
        language=lang_default,
        packages=packages,
        fresh_installable=fresh_installable_int,
        background_patching=background_patching,
        validation_supported=validation,
        properties=properties or [],
        version=version,
        release_notes=release_notes,
        eula=eula,
        uninstall_required=uninstall_required,
        reboot_required=reboot_required,
        architectures=architectures,
        language_default=lang_default,
        release_date=release_date,
        patch_type="system"
    )

def get_system_patch(kb: str, 
    cve_map: Dict[str, List[str]],
    patch_aggr_map: Dict[str, Any],
    properties: List[Property]
) -> List[Patch]:
    """
    Return a list of Patch objects for a product and signature for system patches.
    """
    patch_list: List[Patch] = []
    assoc_list = patch_associations.get(str(product_id))
    if assoc_list is not None:
        for association in assoc_list:
            sigs = association.get("v4_signatures") or []
            if str(signature_id) in [str(s) for s in sigs]:
                patch_id = association.get("patch_id") or []
                patch = get_system_patch(signature_id, patch_id, vuln_associations, patch_aggr, background_patching, validation, properties)
                if patch is not None:
                    patch_list.append(patch)
    return patch_list

def get_cves_and_cpes_for_system(
    vulnassoc_by_product: dict,
    product_id: str,
    signature_id: str
) -> List[Vulnerability]:
    """
    Returns a list of Vulnerability objects for a given product_id and signature_id for system patches.
    """
    vulnerability_list: List[Vulnerability] = []
    if vulnassoc_by_product:
        associations = vulnassoc_by_product.get(str(product_id), [])
        for assoc in associations:
            sigs = assoc.get("v4_signatures")
            addCVE = True
            if sigs is not None and len(sigs) > 0:
                if not str(signature_id) in [str(s) for s in sigs]:
                    addCVE = False
            if addCVE:
                cve = assoc.get("cve")
                cpe = assoc.get("cpe")
                range_list = assoc.get("ranges") or []
                range_list_result: List[Range] = []
                for current_range in range_list:
                    if current_range is not None:
                        range_object = Range(
                            start=current_range.get("start", ""),
                            end=current_range.get("limit", "")
                        )
                        range_list_result.append(range_object)
                vuln = Vulnerability(cve, cpe, range_list_result)
                vulnerability_list.append(vuln)
    return vulnerability_list


def get_system_cves_by_kb(system_cves: Dict[str, List[Dict[str, Any]]]) -> Dict[str, List[str]]:

    kb_to_cves: Dict[str, set[str]] = {}

    for value in system_cves.values():
        for cve_data in value or []:
            cve = (cve_data or {}).get("cve")
            if not cve:
                continue
            for kb in (cve_data or {}).get("kb_articles", []) or []:
                article_name = kb.get("article_name") if isinstance(kb, dict) else kb
                if not article_name:
                    continue


                # fix this to also add the OS's but only add an applicable OS, Do not add for older OS's.  When adding the patch then add only the OS list for the KB's not based on CVE
                if is_cve_younger_than(cve,5):
                    kb_to_cves.setdefault(article_name, set()).add(cve)

    return {kb: sorted(cves) for kb, cves in kb_to_cves.items()}


def get_system_products(
    server_folder: str,
    products: Dict[str, Any],
    moby_data: Dict[str, Any]
) -> List[dict]:
    """
    Iterate over all products and their signatures to build a list of system patch product dictionaries.
    """
    results = []
    count = 0

    system_patch_assoc_path = find_case_insensitive(server_folder, "patch_system_aggregation.json")
    system_vulnassoc_path = find_case_insensitive(server_folder, "vuln_system_associations.json")
    system_patch_aggr_path = find_case_insensitive(server_folder, "patch_system_aggregation.json")

    # Load Associations
    system_assoc_sec = group_associations_by_product_id(get_section(load_json(system_patch_assoc_path), ["patch_system_aggregation"]))
    windows_system_vulnassoc = group_associations_by_product_id(get_section(load_json(system_vulnassoc_path), ["windows_vuln_system_associations"]))
    
    # Handle Windows First
    kb_cve_map = get_system_cves_by_kb(windows_system_vulnassoc)

    # Now iterate through each KB and add the data to the single file
    patch_aggr = to_dict(get_section(load_json(system_patch_aggr_path), ["patch_system_aggregation"]))

    # Aggregation Map

    for kb in kb_cve_map.keys:
    
        system_patch = get_system_patch(kb,kb_cve_map,patch_aggr,None)

        product_data = prod.get("product", {})
        vendor_data = prod.get("vendor", {})
        sigs = prod.get("signatures") or []

        for sig in sigs:
            sig_id = sig.get("id")
            product_id = product_data.get("id")

            # Build Product object
            product_result = Product()
            product_result.signature_id = sig_id
            product_result.name = sig.get("name")
            product_result.vendor = Vendor(vendor_data.get("id"), vendor_data.get("name"))
            product_result.product_line = Product_Line(product_data.get("id"), product_data.get("name"))
            product_result.marketing_names = prod.get("marketing_names") or None

            background_patching = get_bool_from_intvalue(product_data, "background_patching")
            validation = get_bool_from_intvalue(product_data, "validation_supported")
            properties = [Property(name=k, value=str(v)) for k, v in (product_data.get("3rd_party_patch_properties") or {}).items()]

            # Add categories and uninstallable from Moby data if available
            moby = moby_data.get(sig_id)
            if moby:
                product_result.categories = moby.get("categories", [])
                product_result.uninstallable = moby.get("support_app_remover", False)

            # Add system patches and vulnerabilities
            product_result.patches = get_system_patches(
                sig_id, product_id, patch_assoc_sec, vulnassoc,
                patch_aggr, background_patching, validation, properties
            )

            product_result.vulnerabilities = get_cves_and_cpes_for_system(
                vulnassoc, product_id, sig_id
            )

            # Add to results
            results.append(product_result.to_dict())
            count += 1

            if count % 100 == 0 and count > 0:
                print(f"Processed {count} system signatures...")

    return results