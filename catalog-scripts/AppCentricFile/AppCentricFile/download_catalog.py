#!/usr/bin/env python3
"""
download_and_extract_analog.py

Purpose:
  - Read a token from a text file (default: download_token.txt)
  - Download analog.zip from a URL template with "{token}"
  - If the downloaded ZIP contains a nested analog.zip, open that inner zip
  - Extract the relevant JSON files (or everything) to a target directory

Outputs:
  - Extracted directory containing (at least):
      products.json, patch_associations.json, patch_aggregation.json

Examples:
  python download_and_extract_analog.py \
    --token-file download_token.txt \
    --url-template "https://your.host/analog.zip?token={token}" \
    --zip-path analog.zip \
    --extract-dir ./AnalogExtract
"""

import argparse
import io
import os
import zipfile
import shutil
import stat
import time

from typing import Optional
from urllib.parse import urlparse

def read_token(path: str) -> str:
    if not os.path.exists(path):
        raise FileNotFoundError(f"Token file not found: {path}")
    with open(path, "r", encoding="utf-8") as f:
        token = f.read().strip()
    if not token:
        raise ValueError("Token file is empty")
    return token

def http_download(url: str, dest_path: str) -> None:
    # Try requests, fall back to urllib
    try:
        import requests  # type: ignore
        with requests.get(url, stream=True, timeout=60) as r:
            r.raise_for_status()
            with open(dest_path, "wb") as f:
                for chunk in r.iter_content(chunk_size=8192):
                    if chunk:
                        f.write(chunk)
    except Exception:
        import urllib.request
        with urllib.request.urlopen(url, timeout=60) as resp:
            data = resp.read()
            with open(dest_path, "wb") as f:
                f.write(data)

def find_first_member(z: zipfile.ZipFile, filename: str) -> Optional[str]:
    low = filename.lower()
    for name in z.namelist():
        if name.endswith("/"):
            continue
        if os.path.basename(name).lower() == low:
            return name
    return None

def open_zip_maybe_nested(zip_path: str) -> zipfile.ZipFile:
    """
    Open a zip. If it contains an inner analog.zip (or any .zip),
    return a ZipFile object for the inner archive. Otherwise return the outer.
    Caller is responsible for closing the returned ZipFile.
    """
    outer = zipfile.ZipFile(zip_path, "r")

    # Find nested zip
    inner_name = None
    for name in outer.namelist():
        if name.lower().endswith(".zip"):
            if os.path.basename(name).lower() == "analog.zip":
                inner_name = name
                break
            if inner_name is None:
                inner_name = name

    if inner_name:
        # Load entire inner zip into memory
        data = outer.read(inner_name)
        outer.close()   # important: close outer before returning
        return zipfile.ZipFile(io.BytesIO(data), "r")
    else:
        return outer


def safe_extractall(zf: zipfile.ZipFile, dest: str) -> None:
    """
    Safer extract that:
      - prevents path traversal,
      - creates parent dirs,
      - clears read-only bit on existing targets,
      - overwrites files cleanly.
    """
    dest = os.path.abspath(dest)
    os.makedirs(dest, exist_ok=True)

    for info in zf.infolist():
        # Normalize slashes and strip weird prefixes
        name = info.filename.replace("\\", "/")
        # Skip directory entries
        if name.endswith("/"):
            target_dir = os.path.abspath(os.path.join(dest, name))
            if not target_dir.startswith(dest + os.sep):
                raise RuntimeError(f"Blocked path traversal: {name}")
            os.makedirs(target_dir, exist_ok=True)
            continue

        target_path = os.path.abspath(os.path.join(dest, name))
        if not target_path.startswith(dest + os.sep):
            raise RuntimeError(f"Blocked path traversal: {name}")

        os.makedirs(os.path.dirname(target_path), exist_ok=True)

        # If a previous extract left a read-only file, clear it
        if os.path.exists(target_path):
            try:
                os.chmod(target_path, stat.S_IWRITE)
            except Exception:
                pass

        # Write file contents
        with zf.open(info, "r") as src, open(target_path, "wb") as dst:
            shutil.copyfileobj(src, dst)

        # Optionally restore external attrs (basic readonly hint)
        # If you want to preserve original readonly, uncomment:
        # if info.external_attr >> 16 & stat.S_IWRITE == 0:
        #     os.chmod(target_path, stat.S_IREAD)

def file_age_seconds(path: str) -> float:
    return time.time() - os.path.getmtime(path)


def remove_readonly(func, path, _):
    """Clear the readonly bit and retry the remove."""
    os.chmod(path, stat.S_IWRITE)
    func(path)


def download_and_extract_analog(token: str, url: str, zip_path: str, extract_dir: str) -> None:
    
    # Remove existing extraction folder
    if os.path.isdir(extract_dir):
        shutil.rmtree(extract_dir, onerror=remove_readonly)
        print(f"[+] Removed existing folder {extract_dir}")

    # Check to see if the file already exists zip_path and the catalog dir
    # Remove existing analog.zip
    reuse_zip = False
    age = 3600
    if os.path.exists(zip_path) and os.path.getsize(zip_path) > 0:
        age = file_age_seconds(zip_path)
        if age < 3600:  # < 1 hour
            print(f"[+] Keeping existing {zip_path} (age ~{age/60:.1f} min)")
            reuse_zip = True
        else:
            print(f"[+] {zip_path} is older than 1 hour (~{age/60:.1f} min). Removing...")
            os.remove(zip_path)

    if not reuse_zip:
        url = url.format(token=token)

        print(f"[+] Downloading: {url}")
        http_download(url, zip_path)
        print(f"[+] Saved to: {zip_path}")

    with open_zip_maybe_nested(zip_path) as zf:
        print(f"[+] Opened zip ({len(zf.namelist())} members). Extracting to: {extract_dir}")
        safe_extractall(zf, extract_dir)

    # Basic check for expected files
    expected = ["products.json", "patch_associations.json", "patch_aggregation.json"]
    missing = [e for e in expected if not any(p.lower().endswith("/"+e) or p.lower().endswith(e) 
                                            for p in _walk_all_files(extract_dir))]
    if missing:
        print(f"[!] WARNING: Missing expected files after extraction: {', '.join(missing)}")
    else:
        print("[+] Found required JSON files.")

def main():
    ap = argparse.ArgumentParser(description="Download analog.zip and extract required files")
    ap.add_argument("--token-file", default="download_token.txt", help="Path to token file (default: download_token.txt)")
    ap.add_argument("--url-template", default="https://vcr.opswat.com/gw/file/download/analog.zip?type=1&token={token}",help='Download URL template containing "{token}", e.g. "https://vcr.opswat.com/gw/file/download/analog.zip?type=1&token={token}"')
    ap.add_argument("--zip-path", default="analog.zip", help="Where to save the downloaded analog.zip (default: analog.zip)")
    ap.add_argument("--extract-dir", default="./CatalogExtract", help="Where to extract files (default: ./AnalogExtract)")
    args = ap.parse_args()

    token = read_token(args.token_file)
    download_and_extract_analog(token, args.url_template, args.zip_path, args.extract_dir)


def _walk_all_files(root: str):
    for d, _, files in os.walk(root):
        for f in files:
            yield os.path.join(d, f)

if __name__ == "__main__":
    main()
