import logging
import os
import sys

from platform_utils import get_os_type, get_dat_files, get_hostname, get_architecture
from platform_utils import OS_TYPE_WINDOWS, OS_TYPE_LINUX, OS_TYPE_MACOS

logger = logging.getLogger("endpoint_vuln_scanner")

# SDK method IDs — these integer values are defined by the OESIS SDK API contract.
# They are passed as the "method" field in wa_api_invoke JSON payloads to select
# which SDK function to call.
METHOD_DETECT_PRODUCTS = 0        # Enumerate all installed products on the endpoint
METHOD_GET_OS_INFO = 1            # Retrieve OS name, version, build, and architecture
METHOD_GET_VERSION = 100          # Query the SDK library version (unused at runtime, kept for reference)
METHOD_GET_MISSING_PATCHES = 1013 # List OS/system patches not yet applied, via a patch agent
METHOD_GET_LATEST_INSTALLER = 50300  # Get the download URL and metadata for the newest installer of a product
METHOD_LOAD_PATCH_DATABASE = 50302   # Feed a patch DAT file into the SDK so patch queries work offline
METHOD_GET_PRODUCT_VULNERABILITY = 50505  # Query CVE/vulnerability status for a specific product signature
METHOD_CONSUME_OFFLINE_VMOD_DB = 50520    # Feed a vulnerability DAT file into the SDK for offline vuln lookups


def _load_vulnerability_databases(sdk, dat_path):
    """Load the vulnerability DAT files into the SDK.

    The SDK ships with DAT files that contain its vulnerability intelligence.
    These must be explicitly loaded before any vulnerability queries are made —
    the SDK does not load them automatically. There are two layers:

      1. v2mod.dat — cross-platform, covers 3rd-party applications (browsers,
         media players, developer tools, etc.)
      2. A platform-specific DAT that covers OS-level vulnerabilities:
           - liv.dat   on Linux
           - mav.dat   on macOS
           - wiv-lite.dat on Windows

    If a DAT file is missing from disk, that vulnerability category is simply
    skipped with a warning rather than aborting the scan entirely.
    """
    dat_files = get_dat_files(dat_path)
    os_type = get_os_type()

    # Load v2mod.dat — 3rd-party application vulnerability data
    v2mod_path = dat_files.get("v2mod")
    if v2mod_path and os.path.isfile(v2mod_path):
        rc, result = sdk.invoke(METHOD_CONSUME_OFFLINE_VMOD_DB,
                                dat_input_source_file=v2mod_path)
        if rc < 0:
            logger.warning(f"Failed to load v2mod.dat (rc={rc})")
        else:
            logger.info("  Loaded v2mod.dat (3rd-party vulnerabilities)")

    # Load platform-specific vulnerability data.
    # Each OS has its own DAT file that the SDK uses to map installed system
    # components to known CVEs. Only the file matching the current OS is loaded.
    if os_type == OS_TYPE_LINUX:
        liv_path = dat_files.get("liv")
        if liv_path and os.path.isfile(liv_path):
            rc, result = sdk.invoke(METHOD_CONSUME_OFFLINE_VMOD_DB,
                                    dat_input_source_file=liv_path)
            if rc < 0:
                logger.warning(f"Failed to load liv.dat (rc={rc})")
            else:
                logger.info("  Loaded liv.dat (Linux system vulnerabilities)")

    elif os_type == OS_TYPE_MACOS:
        mav_path = dat_files.get("mav")
        if mav_path and os.path.isfile(mav_path):
            rc, result = sdk.invoke(METHOD_CONSUME_OFFLINE_VMOD_DB,
                                    dat_input_source_file=mav_path)
            if rc < 0:
                logger.warning(f"Failed to load mav.dat (rc={rc})")
            else:
                logger.info("  Loaded mav.dat (macOS system vulnerabilities)")

    elif os_type == OS_TYPE_WINDOWS:
        wiv_path = dat_files.get("wiv_lite")
        if wiv_path and os.path.isfile(wiv_path):
            rc, result = sdk.invoke(METHOD_CONSUME_OFFLINE_VMOD_DB,
                                    dat_input_source_file=wiv_path)
            if rc < 0:
                logger.warning(f"Failed to load wiv-lite.dat (rc={rc})")
            else:
                logger.info("  Loaded wiv-lite.dat (Windows system vulnerabilities)")


def _load_patch_database(sdk, dat_path):
    """Load the patch DAT files into the SDK.

    Patch data is separate from vulnerability data. The patch database tells
    the SDK what the latest available installer version is for each product,
    which is used by METHOD_GET_LATEST_INSTALLER and METHOD_GET_MISSING_PATCHES.

    On all platforms a primary patch DAT is loaded, optionally accompanied by
    an ap_checksum DAT that lets the SDK verify installer integrity.

    On Windows an additional wuov2.dat is loaded, which provides Windows Update
    patch metadata (KB articles, severity, etc.) that the primary patch.dat
    does not cover.
    """
    dat_files = get_dat_files(dat_path)
    patch_path = dat_files.get("patch")
    checksum_path = dat_files.get("ap_checksum")
    os_type = get_os_type()

    # Load the primary patch database for this platform.
    # The checksum file is optional — when present it allows the SDK to
    # validate patch file integrity before reporting download URLs.
    if patch_path and os.path.isfile(patch_path):
        invoke_params = {"dat_input_source_file": patch_path}
        if checksum_path and os.path.isfile(checksum_path):
            invoke_params["dat_input_checksum_file"] = checksum_path

        rc, result = sdk.invoke(METHOD_LOAD_PATCH_DATABASE, **invoke_params)
        if rc < 0:
            logger.warning(f"Failed to load patch database (rc={rc})")
        else:
            logger.info(f"  Loaded patch database: {os.path.basename(patch_path)}")

    # Windows only: load the Windows Update overlay DAT (wuov2.dat).
    # This extends patch coverage to include Microsoft OS and component patches
    # that are distributed via Windows Update rather than standalone installers.
    if os_type == OS_TYPE_WINDOWS:
        wuov2_path = dat_files.get("wuov2")
        if wuov2_path and os.path.isfile(wuov2_path):
            invoke_params = {"dat_input_source_file": wuov2_path}
            rc, result = sdk.invoke(METHOD_LOAD_PATCH_DATABASE, **invoke_params)
            if rc < 0:
                logger.warning(f"Failed to load wuov2.dat database (rc={rc})")
            else:
                logger.info(f"  Loaded database: wuov2.dat")


def _get_os_info(sdk):
    """Get OS information from the SDK.

    Returns a dict with fields like os_name, os_version, build_number, and
    architecture. This is included in scan output for context and is not used
    to drive any scan logic.
    """
    rc, result = sdk.invoke(METHOD_GET_OS_INFO)
    if rc < 0:
        logger.warning(f" Failed to get OS info (rc={rc})")
        return {}
    return result.get("result", {})


def _detect_products(sdk, category=0):
    """Detect installed products. category=0 means all categories.

    The SDK scans the endpoint for installed software and returns a list of
    product objects. Each object includes the product name, vendor, installed
    version, and a numeric signature ID that uniquely identifies the product
    in the OESIS knowledge base.

    The category parameter narrows the scan to a specific product class:
      0  — all categories
      12 — patch management / OS update agents only (used by system_scan)

    On failure (rc < 0) an empty list is returned so callers can continue
    gracefully rather than aborting the entire scan.
    """
    rc, result = sdk.invoke(METHOD_DETECT_PRODUCTS, category=category)
    if rc < 0:
        logger.warning(f" Product detection failed (rc={rc})")
        return []

    products = result.get("result", {}).get("detected_products", [])
    return products


def _get_product_vulnerability(sdk, signature_id):
    """Get vulnerability data for a specific product signature.

    Queries the SDK for CVE information about the installed version of a product.
    The SDK cross-references the installed version against the vulnerability DAT
    that was loaded earlier to determine whether any CVEs apply.

    Returns a dict containing at minimum a 'has_vulnerability' boolean and,
    when True, a 'cves' list with CVE IDs and severity details.
    Returns None if the SDK call fails (e.g. signature not in the DAT).
    """
    rc, result = sdk.invoke(METHOD_GET_PRODUCT_VULNERABILITY,
                            signature=signature_id)
    if rc < 0:
        return None
    return result.get("result", {})


def _get_latest_installer(sdk, signature_id):
    """Get latest installer / patch info for a product signature.

    Queries the SDK for the newest available version and download URL for a
    product, using the patch database loaded earlier. This is used to populate
    the patch_info field in scan output so operators know where to get the fix.

    Returns a dict with fields like url, version, and file size, or None if
    the SDK has no patch record for this product.
    """
    rc, result = sdk.invoke(METHOD_GET_LATEST_INSTALLER,
                            signature=signature_id)
    if rc < 0:
        return None
    return result.get("result", {})


def product_scan(sdk, dat_path):
    """Run a full product scan: detect products, get vulns and patches for each.

    This is the primary scan mode. It:
      1. Loads vulnerability and patch databases from disk (DAT files).
      2. Enumerates all installed products on the endpoint.
      3. For each product, queries the SDK for CVE data and latest patch info.
      4. Assembles a structured result dict with a per-product breakdown and
         a summary of totals.

    Returns a dict with scan results.
    """
    logger.info("\n--- Product Scan ---")

    # Load databases — must happen before any vulnerability or patch queries,
    # as the SDK has no data to work with until the DAT files are consumed.
    logger.info("Loading vulnerability databases...")
    _load_vulnerability_databases(sdk, dat_path)
    logger.info("Loading patch database...")
    _load_patch_database(sdk, dat_path)

    # Get OS info for context
    logger.info("Getting OS info...")
    os_info = _get_os_info(sdk)

    # Detect all products
    logger.info("Detecting installed products...")
    raw_products = _detect_products(sdk, category=0)
    logger.info(f"  Found {len(raw_products)} products")

    # For each product, get vulnerability and patch data
    product_results = []
    vuln_product_count = 0
    total_cves = 0
    patched_product_count = 0

    for product in raw_products:
        sig_id = product.get("signature")

        # Skip the Windows Update Signature(1103) since it is handled with System Scanning
        if sig_id == 1103:
            continue

        product_name = product.get("product", {}).get("name", "Unknown")
        vendor_name = product.get("vendor", {}).get("name", "Unknown")

        # Start with a skeleton entry. vulnerabilities and patch_info are
        # populated below if the SDK has data for this product.
        entry = {
            "signature_id": sig_id,
            "product_name": product_name,
            "vendor_name": vendor_name,
            "raw_detection": product,  # full SDK detection object preserved for debugging
            "vulnerabilities": None,
            "patch_info": None,
        }

        # Query CVE data for this product's installed version.
        # vuln_data is None if the SDK has no record, or a dict with
        # has_vulnerability=False if the installed version is up to date.
        vuln_data = _get_product_vulnerability(sdk, sig_id)
        if vuln_data:
            entry["vulnerabilities"] = vuln_data
            if vuln_data.get("has_vulnerability"):
                cves = vuln_data.get("cves", [])
                vuln_product_count += 1
                total_cves += len(cves)
                short_name = product_name[:40]
                logger.info(f"  {short_name:<40} Vulnerable  ({len(cves)} CVEs)")
            else:
                short_name = product_name[:40]
                logger.info(f"  {short_name:<40} Clean")

        # Query the latest available installer for this product.
        # Only recorded in the output if a download URL is present — a result
        # without a URL indicates the SDK found a record but has no patch link.
        patch_data = _get_latest_installer(sdk, sig_id)
        if patch_data and patch_data.get("url"):
            entry["patch_info"] = patch_data
            patched_product_count += 1

        product_results.append(entry)

    scan_result = {
        "scan_type": "product",
        "hostname": get_hostname(),
        "os": os_info,
        "detected_products": product_results,
        "summary": {
            "total_products_detected": len(product_results),
            "products_with_vulnerabilities": vuln_product_count,
            "total_cves": total_cves,
            "products_with_available_patches": patched_product_count,
        }
    }

    logger.info(f"\nProduct scan complete: {len(product_results)} products, "
          f"{vuln_product_count} vulnerable, {total_cves} total CVEs")

    return scan_result


def system_scan(sdk, dat_path):
    """Run a system-level scan: OS patches and system vulnerabilities.

    Complements product_scan by focusing on the OS itself rather than
    installed applications. It does two things:

      1. Missing patch enumeration — detects patch management agents
         (Windows Update, apt, yum, etc.) and asks each one which OS patches
         are not yet applied. Results include KB/CVE identifiers and severity.

      2. System vulnerability detection — queries the SDK for CVEs affecting
         OS-level components by running _get_product_vulnerability against
         the patch management product signatures. This works because the
         platform-specific DATs (liv/mav/wiv-lite) map OS component versions
         to CVEs via the same signature mechanism as application products.

    Returns a dict with scan results.
    """
    logger.info("\n--- System Scan ---")

    # Get OS info
    logger.info("Getting OS info...")
    os_info = _get_os_info(sdk)

    # Detect patch management products specifically (category 12).
    # These are agents like Windows Update, apt/dpkg, or macOS Software Update
    # that the SDK can query for missing OS-level patches.
    logger.info("Detecting system patch management agents...")
    patch_products = _detect_products(sdk, category=12)
    logger.info(f"  Found {len(patch_products)} patch management products")

    # Query each patch management agent for missing patches.
    # timeout=0 means use the SDK default timeout.
    # retry_internet_services=True allows the SDK to fall back to online lookups
    # if the locally loaded DAT doesn't have complete patch data.
    # mode=0 is the standard patch query mode (as opposed to preview/staged modes).
    missing_patches = []
    for product in patch_products:
        sig_id = product.get("signature")
        product_name = product.get("product", {}).get("name", "Unknown")

        rc, result = sdk.invoke(METHOD_GET_MISSING_PATCHES,
                                signature=sig_id,
                                timeout=0,
                                retry_internet_services=True,
                                mode=0)
        if rc >= 0:
            patches = result.get("result", {}).get("patches", [])
            if patches:
                logger.info(f"  {product_name}: {len(patches)} missing patches")
                missing_patches.append({
                    "source_product": product_name,
                    "signature_id": sig_id,
                    "patches": patches,
                })
        else:
            # A negative rc here is expected for agents that don't support this
            # method (e.g. a patch agent detected but not actively queried by
            # the SDK on this OS). It is not treated as an error.
            pass

    # Query system-level CVEs for each detected patch management product.
    # The platform DATs (liv/mav/wiv-lite) use the patch agent signatures as
    # anchors to report OS component vulnerabilities, so iterating over
    # patch_products here (rather than a fixed OS signature) covers all the
    # system components the SDK tracks on this platform.
    system_vulns = []
    os_type = get_os_type()  # retained for potential platform-specific logic below

    for product in patch_products:
        sig_id = product.get("signature")
        vuln_data = _get_product_vulnerability(sdk, sig_id)
        if vuln_data and vuln_data.get("has_vulnerability"):
            system_vulns.append({
                "source_product": product.get("product", {}).get("name", "Unknown"),
                "signature_id": sig_id,
                "vulnerability_data": vuln_data,
            })

    scan_result = {
        "scan_type": "system",
        "hostname": get_hostname(),
        "os": os_info,
        "missing_system_patches": missing_patches,
        "system_vulnerabilities": system_vulns,
        "summary": {
            "total_patch_management_products": len(patch_products),
            "total_missing_patch_sources": len(missing_patches),
            "total_system_vuln_sources": len(system_vulns),
        }
    }

    # Compute total individual patch count across all agents for the log summary.
    # Each entry in missing_patches may contain multiple patch records.
    total_patch_count = sum(len(p["patches"]) for p in missing_patches)

    logger.info(
        f"\nSystem scan complete: {len(patch_products)} patch agents, "
        f"{total_patch_count} missing patches"
        + (", system vulnerabilities found" if len(system_vulns) > 0 else ", no vulnerabilities")
    )

    return scan_result