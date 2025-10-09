#!/usr/bin/env python3
import argparse
import json
import sys
import os
import time
from pathlib import Path
from typing import Any, Dict, List, Optional
from datetime import datetime
import logging

sys.path.insert(0, '../AppCentricFile')
from gen_app_centric_file import write_app_centric_file

def load_products(obj: Any) -> List[Dict[str, Any]]:
    """
    Extracts the 'products' list from the given object, searching recursively if needed.
    """
    if isinstance(obj, dict):
        if isinstance(obj.get("products"), list):
            return obj["products"]

        def _walk(x: Any) -> Optional[List[Dict[str, Any]]]:
            if isinstance(x, dict):
                if isinstance(x.get("products"), list):
                    return x["products"]
                for v in x.values():
                    found = _walk(v)
                    if found is not None:
                        return found
            elif isinstance(x, list):
                for v in x:
                    found = _walk(v)
                    if found is not None:
                        return found
            return None

        found = _walk(obj)
        return found or []
    return []

def normalize_cve(cve: str) -> str:
    """Normalizes CVE strings for comparison."""
    return cve.strip().upper()

def product_matches_cve(prod: Dict[str, Any], target_cve: str) -> Dict[str, Any]:
    """
    Returns a summary of all matches for this product (if any).
    Looks in vulnerabilities and patches for the target CVE.
    """
    matches = {"vulnerability_hits": [], "patch_hits": []}

    for vul in prod.get("vulnerabilities", []) or []:
        cve_val = vul.get("cve")
        if isinstance(cve_val, str) and normalize_cve(cve_val) == target_cve:
            matches["vulnerability_hits"].append({
                "cve": cve_val,
                "cpe": vul.get("cpe"),
                "ranges": vul.get("ranges", [])
            })

    for patch in prod.get("patches", []) or []:
        cves = patch.get("cve")
        if isinstance(cves, list) and any(
            isinstance(c, str) and normalize_cve(c) == target_cve for c in cves
        ):
            matches["patch_hits"].append({
                "patch_name": patch.get("name"),
                "version": patch.get("version"),
                "bulletin": patch.get("bulletin"),
                "release_date": patch.get("release_date"),
                "reboot_required": patch.get("reboot_required"),
                "architectures": patch.get("architectures"),
            })
    return matches

def find_products_by_cve(path: Path, cve: str) -> List[Dict[str, Any]]:
    """
    Finds products in the given file that reference the specified CVE.
    """
    with path.open("r", encoding="utf-8") as f:
        data = json.load(f)
    products = load_products(data)
    target = normalize_cve(cve)
    results = []
    for p in products:
        hits = product_matches_cve(p, target)
        if hits["vulnerability_hits"] or hits["patch_hits"]:
            results.append({
                "name": p.get("name"),
                "signature_id": p.get("signature_id"),
                "vendor": (p.get("vendor") or {}).get("name"),
                "product_line": (p.get("product_line") or {}).get("name"),
                "vulnerability_hits": hits["vulnerability_hits"],
                "patch_hits": hits["patch_hits"],
            })
    return results

def get_local_release_date(path: Path) -> str:
    """
    Extracts and formats the release_date from the meta section as local time.
    """
    with path.open("r", encoding="utf-8") as f:
        data = json.load(f)
    meta = data.get("meta", {})
    release_date = meta.get("release_date")
    if not release_date:
        return "Unknown"
    try:
        ts = int(release_date)
        dt = datetime.fromtimestamp(ts)
        return dt.strftime("%Y-%m-%d %H:%M:%S %Z")
    except Exception:
        return f"Invalid timestamp: {release_date}"

def ensure_app_centric_file(args: argparse.Namespace) -> None:
    """
    Ensures the app_centric file exists and is not older than 2 hours.
    Regenerates it if missing or outdated.
    """
    appcentric_path = args.appcentric
    needs_update = True
    if os.path.exists(appcentric_path):
        mtime = os.path.getmtime(appcentric_path)
        age_seconds = time.time() - mtime
        if age_seconds < 2 * 3600:
            needs_update = False
    if needs_update:
        write_app_centric_file(
            catalog_dir=args.catalogdir,
            output_path=args.appcentric,
            token_file=args.tokenfile,
            url_template="https://vcr.opswat.com/gw/file/download/analog.zip?type=1&token={token}",
            zip_path="analog.zip",
            extract_dir=args.catalogdir
        )

def write_results_to_file(cve: str, matches: List[Dict[str, Any]], local_release: str) -> str:
    """
    Writes the CVE search results to a timestamped file and returns the filename.
    """
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    safe_cve = cve.replace(":", "_").replace("/", "_").replace("\\", "_").replace(" ", "_")
    filename = f"results_{safe_cve}_{timestamp}.txt"
    with open(filename, "w", encoding="utf-8") as f:
        if not matches:
            f.write(f"No products reference {cve}.\n")
        else:
            f.write(f"Products referencing {cve}:\n")
            for m in matches:
                f.write("-" * 60 + "\n")
                f.write(f"Product: {m['name']}  (sig_id: {m.get('signature_id')})\n")
                if m.get("vendor"):
                    f.write(f"Vendor:  {m['vendor']}\n")
                if m.get("product_line"):
                    f.write(f"Line:    {m['product_line']}\n")
                if m["vulnerability_hits"]:
                    f.write("Vulnerabilities:\n")
                    for v in m["vulnerability_hits"]:
                        f.write(f"  CVE: {v['cve']}, CPE: {v.get('cpe')}, Ranges: {v.get('ranges')}\n")
                if m["patch_hits"]:
                    f.write("Patch Hits:\n")
                    for p in m["patch_hits"]:
                        f.write(
                            f"  Patch: {p['patch_name']}, Version: {p['version']}, Bulletin: {p['bulletin']}, "
                            f"Release Date: {p['release_date']}, Reboot Required: {p['reboot_required']}, "
                            f"Architectures: {p['architectures']}\n"
                        )
            f.write("\n")
            f.write(f"Catalog release date (local time): {local_release}\n")
    return filename

def report_cve_matches(appcentric_path: str, cve: str) -> None:
    """
    Looks up CVEs in the generated file, prints matching products, and writes results to a file.
    """
    path = Path(appcentric_path)
    matches = find_products_by_cve(path, cve)
    local_release = get_local_release_date(path)
    if not matches:
        print(f"No products reference {cve}.")
    else:
        print(f"Products referencing {cve}:")
        for m in matches:
            print("-" * 60)
            print(f"Product: {m['name']}  (sig_id: {m.get('signature_id')})")
            if m.get("vendor"):
                print(f"Vendor:  {m['vendor']}")
            if m.get("product_line"):
                print(f"Line:    {m['product_line']}")
            if m["vulnerability_hits"]:
                print("Vulnerabilities:")
                for v in m["vulnerability_hits"]:
                    print(f"  CVE: {v['cve']}, CPE: {v.get('cpe')}, Ranges: {v.get('ranges')}")
            if m["patch_hits"]:
                print("Patch Hits:")
                for p in m["patch_hits"]:
                    print(f"  Patch: {p['patch_name']}, Version: {p['version']}, Bulletin: {p['bulletin']}, Release Date: {p['release_date']}, Reboot Required: {p['reboot_required']}, Architectures: {p['architectures']}")
        print()
        print(f"Catalog release date (local time): {local_release}")

    result_file = write_results_to_file(cve, matches, local_release)
    print(f"Results written to: {result_file}")

def main():
    parser = argparse.ArgumentParser(
        description="Generate products_by_signature.json from an extracted directory"
    )
    parser.add_argument(
        "--cve",
        default="UNKNOWN",
        help="CVE to search for (default: UNKNOWN)",
    )
    parser.add_argument(
        "--appcentric",
        default="app_centric.json",
        help="Output JSON path (default: app_centric.json)",
    )
    parser.add_argument(
        "--catalogdir",
        default="./CatalogExtract",
        help="Path to extracted catalog directory (default: ./CatalogExtract)",
    )
    parser.add_argument(
        "--tokenfile",
        default="download_token.txt",
        help="Path to token file (default: download_token.txt)",
    )
    args = parser.parse_args()

    logging.basicConfig(level=logging.INFO)

    ensure_app_centric_file(args)
    report_cve_matches(args.appcentric, args.cve)

if __name__ == "__main__":
    main()
