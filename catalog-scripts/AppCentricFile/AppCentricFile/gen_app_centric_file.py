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
from patch_classes import Product
from util import load_json, to_dict, group_associations_by_product_id, load_moby, find_case_insensitive, get_section
from download_catalog import read_token, download_and_extract_analog
from system_patch import get_system_products

# -------------------- Constants --------------------

DEFAULT_DIR = "./CatalogExtract"
DEFAULT_OUT = "app_centric.json"

# -------------------- Main Build Function --------------------

def build_products_by_signature(folder: str, only_patches=True) -> Dict[str, Any]:
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
    #system_results = get_system_products(server_folder, products, moby_data) 
    
    return {
        "generated_at": datetime.utcnow().isoformat() + "Z",
        "products": product_results
    }

# -------------------- Main Entry Point --------------------

def main():
    """
    Main entry point for the script.
    Parses arguments, builds the product data, and writes the output JSON.
    """
    ap = argparse.ArgumentParser(description="Generate products_by_signature.json from an extracted directory")
    ap.add_argument("--dir", default=DEFAULT_DIR, help="Path to extracted folder (default: ./CatalogExtract)")
    ap.add_argument("--out", default=DEFAULT_OUT, help="Output JSON path (default: products_by_signature.json)")


    ap.add_argument("--token-file", default="download_token.txt", help="Path to token file (default: download_token.txt)")
    ap.add_argument("--url-template", default="https://vcr.opswat.com/gw/file/download/analog.zip?type=1&token={token}",help='Download URL template containing "{token}", e.g. "https://vcr.opswat.com/gw/file/download/analog.zip?type=1&token={token}"')
    ap.add_argument("--zip-path", default="analog.zip", help="Where to save the downloaded analog.zip (default: analog.zip)")
    ap.add_argument("--extract-dir", default="./CatalogExtract", help="Where to extract files (default: ./AnalogExtract)")
    args = ap.parse_args()


    token = read_token(args.token_file)
    download_and_extract_analog(token, args.url_template, args.zip_path, args.extract_dir)

    data = build_products_by_signature(args.dir)
    with open(args.out, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2)
    print(f"Wrote {args.out} with {len(data.get('products', []))} signature entries.")

if __name__ == "__main__":
    main()
