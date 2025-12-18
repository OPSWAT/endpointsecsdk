#!/usr/bin/env python3
"""
OPSWAT CVE Filter (by Date)
---------------------------
Downloads & extracts analog.zip (using your token), then loads cves.json and
writes a filtered JSON preserving the original structure but keeping only CVEs
with epoch >= cutoff (published_epoch by default).

Author: Chris Seiler

Usage:
  python gen-changes.py --catalogdir ./CatalogExtract --tokenfile download_token.txt \
                        --cutoff 2025-11-01 --epochfield published_epoch --out cve_delta.json
"""
import argparse
from asyncio.windows_events import NULL
from collections import defaultdict
import json
import os
import sys
import time as time_mod
from datetime import timezone, datetime
from typing import Any, Dict

# Allow importing helper module from sibling path if present

sys.path.insert(0, '../AppCentricFile')
from gen_app_centric_file import write_app_centric_file

def parse_cutoff(cutoff_str: str) -> int:
    """
    Convert cutoff date/time string to epoch seconds (UTC).

    Supported formats:
      - YYYY-MM-DD            (assumes 00:00:00 UTC)
      - YYYY-MM-DDTHH:MM
      - YYYY-MM-DDTHH:MM:SS
    """
    formats = (
        "%Y-%m-%d",
        "%Y-%m-%dT%H:%M",
        "%Y-%m-%dT%H:%M:%S",
    )

    for fmt in formats:
        try:
            dt = datetime.strptime(cutoff_str, fmt)
            dt = dt.replace(tzinfo=timezone.utc)
            return int(dt.timestamp())
        except ValueError:
            continue

    raise ValueError(
        f"Invalid cutoff_date '{cutoff_str}'. "
        "Expected YYYY-MM-DD or YYYY-MM-DDTHH:MM[:SS]"
    )


def build_cve_product_map(app_centric_data: Dict[str, Any]):
    cve_map = defaultdict(list)

    for product in app_centric_data["products"]:
        
        sigId = product["signature_id"]
        productName = product["name"]

        product_key = str(sigId) + "-" + productName

        if product.get("vulnerabilities"): 
            for vuln in product["vulnerabilities"]:
                cve_id = vuln["cve"]

                if cve_id in cve_map:
                    if product_key in cve_map[cve_id]:
                        continue

                cve_map[cve_id].append(product_key)

    return cve_map


def filter_cves(app_centric_data: Dict[str, Any],data: Dict[str, Any], cutoff_epoch: int, cutoff: str, epoch_field: str = "published_epoch") -> Dict[str, Any]:
    """Return object with same outer format, keeping CVEs where <epoch_field> >= cutoff."""
    if not isinstance(data, dict) or "oesis" not in data or not isinstance(data["oesis"], list):
        raise ValueError("Input JSON does not match expected 'oesis' list structure.")
    oesis_list = data["oesis"]
    if len(oesis_list) < 2 or "header" not in oesis_list[0] or "cves" not in oesis_list[1]:
        raise ValueError("Input JSON missing required header or cves blocks.")



    """Build the map of the CVEs"""
    cve_to_product_map = build_cve_product_map(app_centric_data)




    header_obj = oesis_list[0]
    header_obj["header"]["cutoff_timestamp"] = cutoff_epoch
    header_obj["header"]["cutoff_time"] = cutoff


    cves_map = oesis_list[1].get("cves", {})
    if not isinstance(cves_map, dict):
        raise ValueError("'cves' must be a JSON object/dictionary.")

    filtered_cves: Dict[str, Any] = {}
    for cve_id, cve_obj in cves_map.items():
        if not isinstance(cve_obj, dict):
            continue
        try:
            epoch_value_int = int(cve_obj.get(epoch_field))
        except Exception:
            continue
        if epoch_value_int >= cutoff_epoch:
            if cve_to_product_map.get(cve_id):
                filtered_cves[cve_id] = cve_to_product_map[cve_id]

    return {"oesis": [header_obj, {"cves": filtered_cves}]}


def ensure_app_centric_file(args: argparse.Namespace) -> None:
    """
    Ensures the app_centric file exists and is not older than 2 hours.
    Regenerates it if missing or outdated.
    """
    appcentric_path = args.appcentric
    needs_update = True
    if os.path.exists(appcentric_path):
        mtime = os.path.getmtime(appcentric_path)
        age_seconds = time_mod.time() - mtime
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


def main() -> None:

    print("RAW argv:", sys.argv)
    p = argparse.ArgumentParser(description="Filter cves.json by cutoff date and epoch field.")
    p.add_argument("--catalogdir", default="./CatalogExtract", help="Extracted catalog directory.")
    p.add_argument("--tokenfile", default="download_token.txt", help="Token file to download analog.zip.")
    p.add_argument("--cutoff", default="2025-11-01T00:00:00", help="Cutoff date YYYY-MM-DD (inclusive) in UTC")
    p.add_argument("--epochfield", default="last_modified_epoch", choices=["published_epoch", "last_modified_epoch"],help="Which epoch field to compare against.")
    p.add_argument("--appcentric", default="app_centric.json", help="Output JSON path (default: app_centric.json)")
    p.add_argument("--out", default="cve_delta.json", help="Output JSON path.")
    args = p.parse_args()

    # Optional download step if helpers are available
    if "download_and_extract_analog" in globals() and "read_token" in globals():
        token = read_token(args.tokenfile)
        url_template = "https://vcr.opswat.com/gw/file/download/analog.zip?type=1&token={token}"
        download_and_extract_analog(token, url_template, "analog.zip", args.catalogdir)

    cutoff_epoch = parse_cutoff(args.cutoff)



    ensure_app_centric_file(args)
    app_centric_path = "app_centric.json"
    if not os.path.isfile(app_centric_path):
        raise FileNotFoundError(f"Could not find app_centric.json at: {app_centric_path}")

    with open(app_centric_path, "r", encoding="utf-8") as f:
        app_centric_data = json.load(f)


    cves_path = os.path.join(args.catalogdir, "analog", "server", "cves.json")
    if not os.path.isfile(cves_path):
        raise FileNotFoundError(f"Could not find cves.json at: {cves_path}")

    with open(cves_path, "r", encoding="utf-8") as f:
        data = json.load(f)


    result = filter_cves(app_centric_data, data, cutoff_epoch, args.cutoff, epoch_field=args.epochfield)
        

    with open(args.out, "w", encoding="utf-8") as f:
        json.dump(result, f, ensure_ascii=False, indent=2)

    kept = len(result["oesis"][1]["cves"])
    print(f"[OK] {kept} CVEs kept where {args.epochfield} >= {args.cutoff}. Wrote: {args.out}")

if __name__ == "__main__":
    main()

