"""
OPSWAT Patch Management Catalog Simplifier - 3rd Party Patch Logic
------------------------------------------------------------------
This module contains functions for extracting and processing 3rd party patch and vulnerability data
from the OPSWAT Patch Management Catalog.

Author: Chris Seiler

Purpose:
- Handles the extraction and normalization of 3rd party patch, package, and vulnerability information.
- Used by the main script to build the simplified catalog output.
"""
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
    find_case_insensitive,
    load_json,
    get_section
)

def get_3rd_party_package(
    dlinks: List[Any],
    version: Optional[str],
) -> List[Package]:
    """
    Build a list of Package objects from a patch-aggregation record.
    Each package represents a downloadable file for a patch.
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
        package_id = generate_id("package", id_seed, version, "")
        packages.append(
            Package(
                url=link,
                package_id=package_id,
                language=language or None,
                sha256=hash_value or None,
                architecture=architecture[0] if architecture else None,
                name=name
            )
        )
    return packages

def get_3rd_party_patch(
    signature_id: str,
    patch_id: str,
    vuln_associations: Dict[str, Any],
    patch_aggr: Dict[str, Any],
    background_patching: bool,
    validation: bool,
    delivery_mode: str,
    properties: List[Property]
) -> Optional[Patch]:
    """
    Build a Patch object from patch aggregation data.
    Returns None if patch_id is not found.
    """
    if not patch_id:
        return None
    p = patch_aggr.get(str(patch_id))
    if not isinstance(p, dict):
        return None
    name = p.get("title") or p.get("product_name") or ""
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
    patch_id = generate_id("patch", name, version, release_date)
    bulletin = generate_bulletin_id("TP", name, version, release_date)
    platform = get_platform_from_signature(signature_id)
    dlinks = p.get("download_links") or []
    packages = get_3rd_party_package(dlinks, version)
    return Patch(
        name=name,
        bulletin=bulletin,
        patch_id=patch_id,
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
        delivery_mode=delivery_mode,
        patch_type="3rd_party"
    )

def get_3rd_party_patches(
    signature_id: str,
    product_id: str,
    patch_associations: Dict[str, Any],
    vuln_associations: Dict[str, Any],
    patch_aggr: Dict[str, Any],
    background_patching: bool,
    validation: bool,
    delivery_mode: str,
    properties: List[Property]
) -> List[Patch]:
    """
    Return a list of Patch objects for a product and signature.
    Only 3rd party patches are currently supported.
    """
    patch_list: List[Patch] = []
    assoc_list = patch_associations.get(str(product_id))
    if assoc_list is not None:
        for association in assoc_list:
            sigs = association.get("v4_signatures") or []
            if str(signature_id) in [str(s) for s in sigs]:
                patch_id = association.get("patch_id") or []
                patch = get_3rd_party_patch(signature_id, patch_id, vuln_associations, patch_aggr, background_patching, validation, delivery_mode, properties)
                if patch is not None:
                    patch_list.append(patch)
    return patch_list

def get_cves_and_cpes_for_3rd_party(
    vulnassoc_by_product: dict,
    product_id: str,
    signature_id: str
) -> List[Vulnerability]:
    """
    Returns a list of Vulnerability objects for a given product_id and signature_id.
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

def group_associations_by_product_id(vuln_associations: list) -> dict:
    """
    Group vulnerability associations by product ID.
    Returns a dict mapping product_id to a list of associations.
    """
    result = {}
    for assoc in vuln_associations.values():
        pid_list = assoc.get("v4_pids")
        if pid_list is not None:
            for product_id in pid_list:
                result.setdefault(str(product_id), []).append(assoc)
        else:
            product_id = assoc.get("v4_pid")
            if product_id is not None:
                result.setdefault(str(product_id), []).append(assoc)
    return result

def group_status_by_signature_id(status_list: list) -> dict:
    """
    Group vulnerability associations by product ID.
    Returns a dict mapping product_id to a list of associations.
    """
    result: dict[str, Any] = {}
    for status in status_list:
        sig_id = status.get("signature_id")
        if sig_id is not None:
            result[str(sig_id)] = status
    return result



def get_3rd_party_products(
    server_folder: str,
    products: Dict[str, Any],
    moby_data: Dict[str, Any]
) -> List[dict]:
    """
    Iterate over all products and their signatures to build a list of 3rd party product dictionaries.
    """
    results = []
    max_products = -1 # This is used to limit the file size by default -1 means no limit
    count = 0

    passoc_path = find_case_insensitive(server_folder, "patch_associations.json")
    paggr_path = find_case_insensitive(server_folder, "patch_aggregation.json")
    vulnassoc_path = find_case_insensitive(server_folder, "vuln_associations.json")
    patch_status_path = find_case_insensitive(server_folder, "patch_status.json")

    # Load Associations
    patch_assoc_sec = group_associations_by_product_id(get_section(load_json(passoc_path), ["patch_associations", "patch_associations_1.2", "patch_associations_1.1"]))
    vulnassoc = group_associations_by_product_id(get_section(load_json(vulnassoc_path), ["vuln_associations", "vuln_associations_1.1"]))
    patch_aggr = to_dict(get_section(load_json(paggr_path), ["patch_aggregations"]))

    patch_status_doc = load_json(patch_status_path)
    patch_status_dict = group_status_by_signature_id(patch_status_doc.get("patch_status"))

    for prod in products.values():
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

            
            delivery_mode = None

            if str(sig_id) in patch_status_dict:
                product_status = patch_status_dict.get(str(sig_id),any)
                if product_status:
                    method_status = product_status.get("method_status")
                    if method_status:
                        delivery_mode = "url" # Set the default if there is a method_status
                        download = method_status.get("get_latest_installer").get("download_0").get("is_supported")
    
                        if download is False:
                            delivery_mode = "orchestrated"


            # Add patches and vulnerabilities
            product_result.patches = get_3rd_party_patches(
                sig_id, product_id, patch_assoc_sec, vulnassoc,
                patch_aggr, background_patching, validation, delivery_mode, properties
            )

            product_result.vulnerabilities = get_cves_and_cpes_for_3rd_party(
                vulnassoc, product_id, sig_id
            )

            # Add to results
            results.append(product_result.to_dict())
            count += 1

            if count % 100 == 0 and count > 0:
                print(f"Processed {count} signatures...")

        if max_products > 0 and count >= max_products:
            print(f"Reached maximum product limit of {max_products}. Stopping further processing.")
            break


    return results