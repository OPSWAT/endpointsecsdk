#!/usr/bin/env python3
###############################################################################################
##  simple-proxy example - proxy_download.py
##
##  Demonstrate downloading a URL *through* a running simple-proxy instance. The proxy fetches
##  (or serves from cache) the content and streams it back; this script saves it to disk and
##  reports whether it was a cache HIT or MISS.
##
##  Usage:
##      python proxy_download.py <url> [options]
##
##  Options:
##      --proxy   <url>    Base URL of the proxy      (default: http://127.0.0.1:8080)
##      --token   <token>  Secure-mode token; sent as the X-Proxy-Token header
##      --out     <path>   Output file (default: basename of the URL, or "download.bin")
##      --check-only       Only ask the proxy whether the URL is cached; do not download
##
##  Examples:
##      python proxy_download.py https://download.example.com/patch.msu
##      python proxy_download.py https://download.example.com/patch.msu --token s3cret --out patch.msu
##      python proxy_download.py https://download.example.com/patch.msu --check-only
##
##  Uses only the Python standard library (no pip install required).
##
##  Created by Chris Seiler - OPSWAT OEM Field CTO
###############################################################################################

import argparse
import json
import os
import sys
import time
import urllib.parse
import urllib.request


def _request(proxy, path, target_url, token, method="GET"):
    """Build a urllib Request to <proxy>/<path>?url=<target_url> with the optional token header."""
    qs = urllib.parse.urlencode({"url": target_url})
    url = f"{proxy.rstrip('/')}/{path}?{qs}"
    req = urllib.request.Request(url, method=method)
    if token:
        req.add_header("X-Proxy-Token", token)
    return req


def check_cached(proxy, target_url, token):
    req = _request(proxy, "cached", target_url, token)
    with urllib.request.urlopen(req, timeout=30) as resp:
        return json.loads(resp.read().decode("utf-8"))


def download(proxy, target_url, token, out_path):
    req = _request(proxy, "download", target_url, token)
    start = time.time()
    total = 0
    with urllib.request.urlopen(req, timeout=600) as resp:
        cache_state = resp.headers.get("X-Cache", "?")
        content_type = resp.headers.get("Content-Type", "")
        with open(out_path, "wb") as f:
            while True:
                chunk = resp.read(64 * 1024)
                if not chunk:
                    break
                f.write(chunk)
                total += len(chunk)
    elapsed = time.time() - start
    return cache_state, content_type, total, elapsed


def default_out_name(target_url):
    name = os.path.basename(urllib.parse.urlsplit(target_url).path)
    return name if name else "download.bin"


def main():
    ap = argparse.ArgumentParser(description="Download a URL through a running simple-proxy.")
    ap.add_argument("url", help="the URL to download (fetched by the proxy)")
    ap.add_argument("--proxy", default="http://127.0.0.1:8080", help="proxy base URL")
    ap.add_argument("--token", default=os.environ.get("SIMPLE_PROXY_TOKEN"),
                    help="secure-mode token (or set SIMPLE_PROXY_TOKEN)")
    ap.add_argument("--out", help="output file path")
    ap.add_argument("--check-only", action="store_true",
                    help="only report whether the URL is cached; do not download")
    args = ap.parse_args()

    try:
        info = check_cached(args.proxy, args.url, args.token)
        print(f"cached: {info.get('cached')}"
              + (f" (size={info.get('size')} bytes)" if info.get("cached") else ""))
    except urllib.error.HTTPError as e:
        print(f"error querying proxy /cached: HTTP {e.code} {e.reason}", file=sys.stderr)
        if e.code == 401:
            print("  (proxy is in secure mode - pass --token)", file=sys.stderr)
        return 2
    except urllib.error.URLError as e:
        print(f"could not reach proxy at {args.proxy}: {e.reason}", file=sys.stderr)
        return 2

    if args.check_only:
        return 0

    out_path = args.out or default_out_name(args.url)
    try:
        cache_state, content_type, total, elapsed = download(
            args.proxy, args.url, args.token, out_path)
    except urllib.error.HTTPError as e:
        print(f"download failed: HTTP {e.code} {e.reason}", file=sys.stderr)
        try:
            print("  " + e.read().decode("utf-8", "replace"), file=sys.stderr)
        except Exception:
            pass
        return 1
    except urllib.error.URLError as e:
        print(f"download failed: {e.reason}", file=sys.stderr)
        return 1

    rate = (total / elapsed / 1_000_000) if elapsed > 0 else 0
    print(f"saved {total:,} bytes to {out_path}")
    print(f"  X-Cache: {cache_state}   type: {content_type or 'n/a'}   "
          f"{elapsed:.2f}s ({rate:.1f} MB/s)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
