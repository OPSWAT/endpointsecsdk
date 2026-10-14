#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — shared helpers
##
##  Common loaders for the OPSWAT "Analog" offline catalog used by the find-* lookup scripts.
##  Resolves the catalog location (OPSWAT-SDK/extract/analog/server) by walking up for the
##  'sdkroot' marker, and provides small readers over the oesis-wrapped JSON datasets.
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import json
import mmap
import os
import re
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


# --- vuln_system_associations.json: CVEs fixed by an OS package or OS version ----------------
#
# ~1 GB, in three sections: windows_* (CVE -> KB articles, which find-cve.py reads from
# kb_info.json instead), linux_* (CVE -> distro release -> package -> fixed package version) and
# mac_* (CVE -> macOS release -> fixed macOS version). Too big for read_records, so there is a
# memory-mapped single-CVE lookup and a streaming reader for the whole file.

OS_ASSOC_FILE = "vuln_system_associations.json"


def os_association_rows(rec):
    """One vuln_system_associations record -> [{os, os_type, package, fixed}].

    fixed is the version that resolves the CVE ("limit_except", or "limit" in a few records),
    or None when the vendor has not shipped one yet ("N/A"). Windows records carry KB articles
    rather than affected_os, so they produce no rows.
    """
    os_type = rec.get("os_type") or 0
    rows = []
    for affected in rec.get("affected_os") or []:
        name = affected.get("os_name") or "(unnamed OS)"
        packages = affected.get("affected_packages")
        if packages is None:
            # macOS: the ranges are OS versions, so the "package" is the OS itself.
            packages = [{"package_name": "macOS" if os_type == 4 else name,
                         "ranges": affected.get("ranges")}]
        for pkg in packages:
            for rng in pkg.get("ranges") or []:
                fixed = rng.get("limit_except") or rng.get("limit")
                rows.append({
                    "os":      name,
                    "os_type": os_type,
                    "package": pkg.get("package_name") or "?",
                    "fixed":   None if fixed in (None, "", "N/A") else fixed,
                })
    return rows


def _record_end(buf, start):
    """Index just past the JSON object opening at buf[start] ('{'), honouring strings."""
    depth, in_str, i = 0, False, start
    while i < len(buf):
        ch = buf[i]
        if in_str:
            if ch == 0x5C:      # backslash: skip the escaped byte
                i += 1
            elif ch == 0x22:    # closing quote
                in_str = False
        elif ch == 0x22:
            in_str = True
        elif ch == 0x7B:
            depth += 1
        elif ch == 0x7D:
            depth -= 1
            if depth == 0:
                return i + 1
        i += 1
    return None


def find_os_associations(server_dir, cve):
    """OS-package / OS-version rows for one CVE (see os_association_rows).

    Memory-maps the file and decodes only the records naming this CVE, so a lookup takes a
    few seconds instead of parsing 1 GB. Older catalogs without the file return [].
    """
    path = os.path.join(server_dir, OS_ASSOC_FILE)
    if not os.path.isfile(path) or os.path.getsize(path) == 0:
        return []
    pattern = re.compile(rb'"cve"\s*:\s*"' + re.escape(cve.encode()) + rb'"')
    rows = []
    with open(path, "rb") as fh, mmap.mmap(fh.fileno(), 0, access=mmap.ACCESS_READ) as buf:
        for match in pattern.finditer(buf):
            # Records look like "220267774":{"cve":"CVE-...", ...}; walk back to their '{'.
            start = buf.rfind(b"{", 0, match.start())
            if start < 0 or buf[start + 1:match.start()].strip():
                continue
            end = _record_end(buf, start)
            if end is not None:
                rows.extend(os_association_rows(json.loads(buf[start:end].decode("utf-8"))))
    return rows


def iter_os_associations(server_dir):
    """Yield (cve, rows) for every Linux/macOS record in the file.

    Streams records with raw_decode over the file text rather than json.load, which would
    build the whole 1 GB tree in memory at once. Takes ~20 s on the full catalog.
    """
    path = os.path.join(server_dir, OS_ASSOC_FILE)
    if not os.path.isfile(path):
        return
    with open(path, "r", encoding="utf-8") as fh:
        text = fh.read()
    decoder = json.JSONDecoder()
    open_re = re.compile(r'\s*:\s*\{')
    record_re = re.compile(r'\s*,?\s*"[^"]*"\s*:\s*(?=\{)')
    for name in ("linux", "mac"):
        label = f'"{name}_vuln_system_associations"'
        at = text.find(label)
        opened = open_re.match(text, at + len(label)) if at >= 0 else None
        if not opened:
            continue
        pos = opened.end()
        while True:
            key = record_re.match(text, pos)
            if not key:
                break
            rec, pos = decoder.raw_decode(text, key.end())
            rows = os_association_rows(rec)
            if rows and rec.get("cve"):
                yield rec["cve"], rows


OS_TYPE_NAMES = {1: "Windows", 2: "Linux", 4: "macOS"}


def os_sort_key(name):
    """'SLES 15.10' after 'SLES 15.9': split digits out so versions sort numerically."""
    return [int(p) if p.isdigit() else p.lower() for p in re.split(r"(\d+)", name)]


def os_association_summary(os_rows):
    """(releases affected, releases with at least one fixed version) for a result line."""
    return ({r["os"] for r in os_rows},
            {r["os"] for r in os_rows if r["fixed"]})


def print_os_associations(os_rows, indent="  "):
    """One line per OS release and fixed version, with the packages it covers."""
    grouped = {}   # (os, fixed) -> [package]
    for row in os_rows:
        packages = grouped.setdefault((row["os"], row["fixed"]), [])
        if row["package"] not in packages:
            packages.append(row["package"])
    releases, fixed_releases = os_association_summary(os_rows)
    kinds = ", ".join(OS_TYPE_NAMES.get(t, f"os_type {t}")
                      for t in sorted({r["os_type"] for r in os_rows}))
    print(f"{indent}OS packages ({kinds}, {OS_ASSOC_FILE}): {len(releases)} OS "
          f"release(s), {len(fixed_releases)} with a fixed version")
    lines = []
    for (os_name, fixed), packages in sorted(
            grouped.items(), key=lambda kv: (os_sort_key(kv[0][0]), kv[0][1] is None, kv[0][1] or "")):
        status = f"fixed in {fixed}" if fixed else "no fixed version yet"
        lines.append((os_name, status, ", ".join(packages)))
    width = max(len(name) for name, _, _ in lines)
    status_width = max(len(status) for _, status, _ in lines)
    for os_name, status, packages in lines:
        print(f"{indent}   {os_name:<{width}}  {status:<{status_width}}  {packages}")


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
