"""
OPSWAT Patch Management Catalog Simplifier - Utility Functions
-------------------------------------------------------------
This module provides utility functions and enums for processing and normalizing
data from the OPSWAT Patch Management Catalog.

Author: Chris Seiler

Purpose:
- Contains helpers for JSON loading, data normalization, and catalog structure parsing.
- Used by the main script and other modules to keep code DRY and maintainable.
"""
from datetime import datetime
import json
import os
import uuid
import hashlib
import base64
import re
from typing import Any, Dict, Iterable, List, Optional
from urllib.parse import urlparse, unquote
from enum import Enum

# -------------------- Enums --------------------

class RebootStatus(str, Enum):
    """Enum for reboot status values."""
    YES = "yes"
    NO = "no"
    MAYBE = "maybe"


# -------------------- Utility Functions --------------------

def load_json(path: str) -> Dict[str, Any]:
    """Load a JSON file and return its contents as a dictionary."""
    print(f"Loading JSON from ", path)
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)

def get_section(doc: Dict[str, Any], keys: List[str]):
    """
    Extract a nested section from a dictionary using a list of possible keys.
    Returns the first matching section found.
    """
    if not isinstance(doc, dict) or not doc:
        return None
    arr = next(iter(doc.values()))
    for sec in keys:
        if isinstance(arr, list):
            for item in arr:
                if isinstance(item, dict) and sec in item:
                    return item[sec]
        elif isinstance(arr, dict) and sec in arr:
            return arr[sec]
    return None

def get_bool_from_intvalue(json_element: Any, name: str) -> bool:
    """Return True if the named field in json_element is 1, else False."""
    data_int = json_element.get(name, 0)
    return data_int == 1

def get_bool_from_yesno_value(json_element: Any, name: str) -> bool:
    """Return True if the named field in json_element is 'yes', else False."""
    data_int = json_element.get(name, 0)
    return data_int == "yes"

def to_dict(section: Any) -> Dict[str, Any]:
    """Return the section if it's a dict, else an empty dict."""
    if isinstance(section, dict):
        return section
    return {}

def parse_version(v: Any) -> tuple:
    """Parse a version string into a tuple of integers for comparison."""
    if not isinstance(v, str) or not v:
        return ()
    parts = re.split(r'[^0-9]+', v)
    nums = []
    for p in parts:
        if p == '':
            continue
        try:
            nums.append(int(p))
        except Exception:
            nums.append(0)
    return tuple(nums)

def choose_latest_installer_patch(patch_ids: Iterable[str], patch_aggr: Dict[str, Any]) -> Optional[Dict[str, Any]]:
    """
    Choose the latest installer patch based on version and fresh_installable flag.
    Returns the patch dict or None if not found.
    """
    best = None
    best_key = None
    for pid in patch_ids or []:
        p = patch_aggr.get(str(pid))
        if not isinstance(p, dict):
            continue
        ver = p.get("latest_version") or ""
        fresh = bool(p.get("fresh_installable")) if p.get("fresh_installable") is not None else False
        key = (1 if fresh else 0, parse_version(ver))
        if best is None or key > best_key:
            best = p
            best_key = key
    return best

def generate_id(objct_type: str, name: str, version: str, release_date: str) -> str:
    """
    Create a deterministic UUID from name, version, and release_date.
    Used for generating unique IDs for patches and packages.
    """
    key = f"{objct_type}{name}|{version}|{release_date}"
    guid = uuid.uuid5(uuid.NAMESPACE_DNS, key)
    return str(guid)

def generate_bulletin_id(prefix_type: str, name: str, version: str, release_date: str) -> str:
    """
    Create a deterministic bulletin ID from name, version, and release_date.
    Used for generating unique bulletin IDs for patches.
    """
    key = f"{name}|{version}|{release_date}"
    digest = hashlib.sha256(key.encode("utf-8")).digest()
    b32 = base64.b32encode(digest).decode("utf-8").rstrip("=")
    return prefix_type + b32[:6].upper()

def get_platform_from_signature(signature_id: str) -> str:
    """
    Determine platform type based on the numeric range of signature_id.
    Returns 'windows', 'mac', or 'linux'.
    """
    try:
        sig_num = int(signature_id)
    except (ValueError, TypeError):
        raise ValueError(f"Invalid signature_id: {signature_id!r}. Must be numeric.")
    if sig_num < 100_000:
        return "windows"
    elif 100000 <= sig_num < 200_000:
        return "mac"
    else:
        return "linux"

def get_reboot_status(value: str) -> Optional[str]:
    """
    Normalize a string to lower-case reboot status: 'yes', 'no', or 'maybe'.
    Returns None if value is None or not recognized.
    """
    if value is None:
        return None
    s = str(value).strip().lower()
    if s in [status.value for status in RebootStatus]:
        return s
    return None

def normalize_architectures_list(values: Any) -> List[str]:
    """
    Normalize a list of architecture strings to unique codes: ['x86', 'x64', 'arm64'].
    Accepts a string or list of strings.
    """
    if not values:
        return []
    if isinstance(values, str):
        values = [values]
    def normalize_architecture(value: str) -> Optional[str]:
        if value is None:
            return None
        s = str(value).strip().lower()
        mapping = {
            "x86": "x86",
            "x86_all": "x86",
            "32": "x86",
            "32-bit": "x86",
            "i386": "x86",
            "ia32": "x86",
            "x64": "x64",
            "64": "x64",
            "64-bit": "x64",
            "amd64": "x64",
            "win64": "x64",
            "arm64": "arm64",
            "aarch64": "arm64",
        }
        return mapping.get(s, None)
    normalized = [norm for v in values if (norm := normalize_architecture(v))]
    seen = set()
    result = []
    for arch in normalized:
        if arch not in seen:
            seen.add(arch)
            result.append(arch)
    return result

def create_package_name(
    link: str,
    version: str = "",
    language: str = "",
    architecture: Any = ""
) -> str:
    """
    Build a package name from URL, version, language, and architecture.
    Appends version, language, and architecture to the filename if present.
    """
    filename = os.path.basename(urlparse(link).path)
    filename = unquote(filename)
    parts = []
    if version and version not in filename:
        parts.append(version)
    if language:
        parts.append(language)
    if architecture:
        if isinstance(architecture, list):
            parts.extend(architecture)
        else:
            parts.append(architecture)
    suffix = "-".join(parts)
    return f"{filename}-{suffix}" if suffix else filename


def find_case_insensitive(root: str, filename: str) -> Optional[str]:
    """
    Find a file in a directory tree, case-insensitive.
    Returns the full path if found, else None.
    """
    target = filename.lower()
    for d, _, files in os.walk(root):
        for f in files:
            if f.lower() == target:
                return os.path.join(d, f)
    return None

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

def is_recent_cve(cve_id, years=10):
    """
    Return True if the CVE is less than `years` old.
    Used for filtering recent vulnerabilities.
    """
    try:
        parts = cve_id.split("-")
        if len(parts) < 3 or not parts[1].isdigit():
            return False
        cve_year = int(parts[1])
        current_year = datetime.now().year
        return (current_year - cve_year) < years
    except Exception:
        return False

def normalize_release_date(date_str: Optional[str]) -> Optional[str]:
    """
    Normalize a release_date string to ISO format YYYY-MM-DD.
    Accepts "YYYY-MM-DD" or "MM/DD/YYYY".
    Returns None if invalid or empty.
    """
    if not date_str:
        return None
    date_str = date_str.strip()
    try:
        return datetime.strptime(date_str, "%Y-%m-%d").strftime("%Y-%m-%d")
    except ValueError:
        pass
    try:
        return datetime.strptime(date_str, "%m/%d/%Y").strftime("%Y-%m-%d")
    except ValueError:
        pass
    print(f"Warning: Unrecognized date format: {date_str}")
    return None

def build_signature_dict(root_folder: str, files: List[str]) -> Dict[int, Dict[str, Any]]:
    """
    Merge multiple JSON files and return a dict of signature_id to signature JSON object.
    Used for loading Moby data.
    """
    sig_dict: Dict[int, Dict[str, Any]] = {}
    for file_path in files:
        source_path = root_folder + file_path
        data = load_json(source_path)
        products = data.get("products", {})
        for _product_name, platforms in products.items():
            for platform_data in platforms.values():
                for sig in platform_data.get("signatures", []):
                    sig_id = sig.get("signature_id")
                    if sig_id is None:
                        continue
                    sig_dict[sig_id] = sig
    return sig_dict

def load_moby(root_folder: str) -> dict:
    """
    Load and merge all platform files into a single signature dictionary.
    Used for additional product metadata.
    """
    files = ["/windows/docs/moby/moby.json", "/mac/docs/moby/moby.json", "/linux/docs/moby/moby.json"]
    return build_signature_dict(root_folder, files)


def is_cve_younger_than(cve_id: str, max_age_years: int) -> bool:
    """
    Returns True if the CVE is newer (younger) than the given max_age_years.
    Expects CVE IDs in the standard format: CVE-YYYY-NNNN...
    
    Example:
        is_cve_younger_than("CVE-2022-12345", 5) -> True if current year <= 2027
    """
    if not cve_id:
        return False

    # Extract the year from the CVE ID
    match = re.match(r"^CVE-(\d{4})-\d+$", cve_id)
    if not match:
        return False

    cve_year = int(match.group(1))
    current_year = datetime.now().year

    return (current_year - cve_year) <= max_age_years