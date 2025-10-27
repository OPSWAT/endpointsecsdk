#!/usr/bin/env python3
"""
OPSWAT Patch Management Catalog Simplifier
------------------------------------------
This script is used to simplify the OPSWAT Patch Management Catalog into a single, consolidated JSON file.
It reads an extracted Catalog directory (from analog.zip) and produces a JSON file that lists:
  - Each product by Signature ID
  - All CVE IDs associated via its patches
  - The patch required to get to the latest installer (prefer fresh_installable, else highest latest_version)

Author: Chris Seiler

Purpose:
- This is the main entry point and orchestrator for the catalog simplification process.
- It loads product, patch, and vulnerability data, and writes the final output file.

Usage:
  python generate_products_by_signature_from_dir.py --dir ./AnalogExtract --out products_by_signature.json
"""
import argparse
import json
import os
import uuid
import hashlib
import base64
import util
import system_patch
import download_catalog


from urllib.parse import urlparse, unquote
from datetime import datetime
from typing import Any, Dict, Iterable, List, Optional
from dataclasses import dataclass, field
from third_party import get_3rd_party_products
from patch_classes import Product, Metadata, Data
from util import load_json, to_dict, load_moby, find_case_insensitive, get_section
from download_catalog import read_token, download_and_extract_analog
from system_patch import get_windows_system_product

# -------------------- Constants --------------------

DEFAULT_DIR = "./CatalogExtract"
DEFAULT_OUT = "app_centric.json"




def read_header_local_time(
    extract_path: str = "./CatalogExtract",
) -> str:
    """
    Read the 'header' JSON file and return the header time converted to local timezone.

    - Uses 'time' (e.g., '2025-10-10 00:46:31 +0700') if present.
    - Falls back to 'timestamp' (epoch seconds, assumed UTC) if needed.
    - local_tz: tz name (e.g., 'America/Denver') or ZoneInfo; defaults to system local tz.
    """
    if not os.path.exists(extract_path):
        raise FileNotFoundError(f"File not found: {extract_path}")

    header_path = os.path.join(extract_path,"analog","header.json") 

    with open(header_path, "r", encoding="utf-8") as f:
        data = json.load(f)

    oesis_first = data.get("oesis")[0] if isinstance(data.get("oesis"), list) else data.get("oesis")
    header = oesis_first.get("header") if isinstance(oesis_first, dict) else None

    if not header:
        raise ValueError("Could not locate a valid header object in the JSON.")

    # Determine the source time
    dt_utc: Optional[datetime] = None

    # Fallback: epoch seconds (assumed UTC)
    if dt_utc is None and "timestamp" in header:
        ts = header["timestamp"]
        if not isinstance(ts, int):
            # Some files store it as string
            ts = int(str(ts))
    
    return ts

# -------------------- Main Build Function --------------------

def build_products_by_signature(folder: str, only_patches=False) -> Dict[str, Any]:
    """
    Build the products-by-signature dictionary from the extracted catalog directory.
    Returns a dictionary with a timestamp and a list of product records.
    """
    if not os.path.isdir(folder):
        raise FileNotFoundError(f"Folder not found: {folder}")

    # Load additional metadata from Moby
    print("Loading Moby data for additional product metadata...")
    compliance_path = os.path.join(folder, "analog", "client", "compliance")
    moby_data = load_moby(compliance_path)

    # Find required files
    server_folder = os.path.join(folder, "analog", "server")
    products_path = find_case_insensitive(server_folder, "products.json")
    products = to_dict(get_section(load_json(products_path), ["products"]))

    product_results = get_3rd_party_products(server_folder, products, moby_data)
    windows_system_product = get_windows_system_product(server_folder, products) 
    
    product_results.append(windows_system_product)

    return {
        "generated_at": datetime.utcnow().isoformat() + "Z",
        "products": product_results
    }

# -------------------- Main Entry Point --------------------
def write_app_centric_file(
    catalog_dir: str,
    output_path: str,
    token_file: str = "download_token.txt",
    url_template: str = None,
    zip_path: str = None,
    extract_dir: str = None,
) -> None:
    """
    Build the app-centric file and write it to disk.

    Args:
        catalog_dir: Path to the extracted catalog directory.
        output_path: Path to write the output JSON file.
        token_file: Path to the token file (default: download_token.txt).
    """
    token = read_token(token_file)
    download_and_extract_analog(token, url_template, zip_path, extract_dir)

    meta = Metadata()
    meta.release_date = read_header_local_time(catalog_dir)

    data = Data()
    products_dict = build_products_by_signature(catalog_dir)
    data.products = products_dict["products"]
    data.meta = meta

    with open(output_path, "w", encoding="utf-8") as f:
        json.dump(data.to_dict(), f, indent=2)
    print(f"Wrote {output_path} with {len(data.products)} signature entries.")


def main():
    """
    Main entry point for the script.
    Parses arguments, builds the product data, and writes the output JSON.
    """
    ap = argparse.ArgumentParser(
        description="Generate products_by_signature.json from an extracted directory"
    )
    ap.add_argument(
        "--dir",
        default=DEFAULT_DIR,
        help="Path to extracted folder (default: ./CatalogExtract)",
    )
    ap.add_argument(
        "--out",
        default=DEFAULT_OUT,
        help="Output JSON path (default: products_by_signature.json)",
    )
    ap.add_argument(
        "--token-file",
        default="download_token.txt",
        help="Path to token file (default: download_token.txt)",
    )
    ap.add_argument(
        "--url-template",
        default="https://vcr.opswat.com/gw/file/download/analog.zip?type=1&token={token}",
        help='Download URL template containing "{token}", e.g. "https://vcr.opswat.com/gw/file/download/analog.zip?type=1&token={token}"',
    )
    ap.add_argument(
        "--zip-path",
        default="analog.zip",
        help="Where to save the downloaded analog.zip (default: analog.zip)",
    )
    ap.add_argument(
        "--extract-dir",
        default="./CatalogExtract",
        help="Where to extract files (default: ./AnalogExtract)",
    )
    args = ap.parse_args()

    write_app_centric_file(args.dir, args.out, args.token_file, args.url_template, args.zip_path, args.extract_dir)


if __name__ == "__main__":
    main()
