#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — shared helpers
##
##  Common loaders for the OPSWAT "Analog" offline catalog used by the find-* lookup scripts.
##  Resolves the catalog location (OPSWAT-SDK/extract/analog/server) by walking up for the
##  'sdkroot' marker, and provides small readers over the oesis-wrapped JSON datasets.
##
##  Created by Chris Seiler — OPSWAT OEM Solutions Architect
###############################################################################################

import json
import os
import sys
from datetime import datetime, timezone

# Force UTF-8 console output so non-ASCII catalog text doesn't crash on Windows (cp1252).
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))


def find_server_dir():
    """Walk up from this script for the 'sdkroot' marker; return the Analog server dir."""
    current = SCRIPT_DIR
    while True:
        if os.path.isfile(os.path.join(current, "sdkroot")):
            server = os.path.join(current, "OPSWAT-SDK", "extract", "analog", "server")
            return server if os.path.isdir(server) else None
        parent = os.path.dirname(current)
        if parent == current:
            return None
        current = parent


def require_server_dir():
    server = find_server_dir()
    if not server:
        print("ERROR: Analog catalog not found (OPSWAT-SDK/extract/analog/server).")
        print("       Run the SDK downloader first so the catalog is extracted.")
        sys.exit(2)
    return server


def read_records(path):
    """Yield every record from an oesis-wrapped dataset: {"oesis": [ {header}, {section:{id:rec}} ]}."""
    with open(path, "r", encoding="utf-8") as f:
        data = json.load(f)
    for element in data.get("oesis", []):
        for key, value in element.items():
            if key == "header":
                continue
            if isinstance(value, dict):
                for record in value.values():
                    yield record


def load_json(path):
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def fmt_epoch(epoch):
    """Epoch seconds -> 'YYYY-MM-DD' (UTC), or None."""
    try:
        return datetime.fromtimestamp(int(epoch), tz=timezone.utc).strftime("%Y-%m-%d")
    except (TypeError, ValueError, OSError):
        return None


def normalize_numeric_cve(numeric_id):
    """Numeric CVE id (e.g. 202438203) -> 'CVE-2024-38203', else None."""
    s = str(numeric_id)
    if s.isdigit() and len(s) >= 5:
        return f"CVE-{s[:4]}-{s[4:]}"
    return None


def normalize_kb(value):
    """Return the bare KB digits (e.g. '5094127') from KB5094127 / 'KB5094127' / 5094127."""
    s = str(value).upper().strip()
    if s.startswith("KB"):
        s = s[2:]
    return s if s.isdigit() else None


# --- os_info.json: os_id -> os_name ---------------------------------------------------------

def load_os_names(server_dir):
    """os_id -> os_name, parsed from os_info.json's nested platform/family/list structure."""
    path = os.path.join(server_dir, "os_info.json")
    names = {}
    if not os.path.isfile(path):
        return names
    data = load_json(path)
    for element in data.get("oesis", []):
        os_info = element.get("os_info")
        if not isinstance(os_info, dict):
            continue
        for _platform, groups in os_info.items():
            if not isinstance(groups, list):
                continue
            for group in groups:
                for entry in (group.get("list") or []):
                    oid = entry.get("os_id")
                    if oid is not None:
                        names[oid] = entry.get("os_name") or ""
    return names


def os_label(os_id, os_names):
    nm = os_names.get(os_id)
    return f"{os_id} ({nm})" if nm else str(os_id)


# --- products.json: signature/product lookup ------------------------------------------------

def load_products(server_dir):
    """Returns (sig_index, pid_index):
       sig_index[sig_id] = {signature_name, product_id, product_name, vendor_name, patchable}
       pid_index[product_id] = product_name
    """
    sig_index, pid_index = {}, {}
    path = os.path.join(server_dir, "products.json")
    if not os.path.isfile(path):
        return sig_index, pid_index
    for rec in read_records(path):
        product = rec.get("product", {}) or {}
        vendor = rec.get("vendor", {}) or {}
        pid = product.get("id")
        pname = product.get("name")
        if pid is not None and pname:
            pid_index[pid] = pname
        for sig in rec.get("signatures", []) or []:
            sid = sig.get("id")
            if sid is None:
                continue
            sig_index[sid] = {
                "signature_name": sig.get("name"),
                "product_id":     pid,
                "product_name":   pname,
                "vendor_name":    vendor.get("name"),
                "patchable":      sig.get("support_3rd_party_patch"),
            }
    return sig_index, pid_index


# --- OS patch/package pretty-printer (patch_system_aggregation_v2 packages) ----------------

def print_package(pkg, indent="    "):
    """Print one OS patch package (title, arch, release meta, download url+sha1, os_info)."""
    dl = pkg.get("download_link") or {}
    arch = ", ".join(pkg.get("architectures") or []) or "?"
    print(f"{indent}package_uuid : {pkg.get('package_uuid') or pkg.get('_id')}")
    print(f"{indent}  title      : {pkg.get('title')}")
    print(f"{indent}  arch       : {arch}")
    meta = []
    if pkg.get("release_date"):
        meta.append(f"released {pkg['release_date']}")
    if pkg.get("severity"):
        meta.append(f"severity {pkg['severity']}")
    if pkg.get("category"):
        meta.append(str(pkg["category"]))
    if pkg.get("requires_reboot") is not None:
        meta.append(f"reboot={pkg['requires_reboot']}")
    if meta:
        print(f"{indent}  meta       : {', '.join(meta)}")
    desc = (pkg.get("description") or "").strip().replace("\n", " ")
    if desc:
        print(f"{indent}  description : {desc[:300]}{'...' if len(desc) > 300 else ''}")
    if pkg.get("release_note_link"):
        print(f"{indent}  kb article : {pkg['release_note_link']}")
    if dl.get("link"):
        print(f"{indent}  url        : {dl['link']}")
        print(f"{indent}  sha1       : {dl.get('sha1') or '(none)'}")
    os_info = pkg.get("os_info") or []
    if os_info:
        joined = ", ".join(os_info)
        print(f"{indent}  os_info    : {joined[:200]}{'...' if len(joined) > 200 else ''}")
