#!/usr/bin/env python3
###############################################################################################
##  simple-proxy-patch - download_patch.py
##
##  CLI "patch downloader": resolve a patch from the OPSWAT SDK catalog and download it, preferring
##  a proxy that already has it cached, then any reachable proxy, then a direct download. This file
##  is only the command-line front end; the real work lives in two library modules:
##      catalog_resolve.py  - resolve a signature / package / url to {url, hash, hash_algo, ...}
##      downloader.py       - discovery, proxy selection, downloading, hashing/verification
##
##  Point it at a patch three ways:
##    --signature <id>   Third-party application signature id -> latest patch (URL + SHA-256).
##    --package <uuid>   Package UUID -> third-party (SHA-256) or OS patch (SHA-1).
##    --url <url>        A direct URL (or pass it positionally), no catalog lookup.
##
##  Usage:
##      python download_patch.py --signature 3039 --proxy http://127.0.0.1:8080
##      python download_patch.py --signature 3039 --discover
##      python download_patch.py --package 50babee7-... --require-hash
##      python download_patch.py https://download.example.com/patch.msu --sha256 <hex>
##
##  Created by Chris Seiler - OPSWAT OEM Field CTO
###############################################################################################

import argparse
import os
import sys

import catalog_resolve as cr
import downloader as dl

log = dl.log


def main():
    ap = argparse.ArgumentParser(
        description="Resolve a patch via the SDK catalog and download it through simple-proxy "
                    "(cached-first, then each proxy, then direct).")
    src = ap.add_mutually_exclusive_group()
    src.add_argument("url", nargs="?", help="direct patch URL (no catalog lookup)")
    src.add_argument("--url", dest="url_opt", help="direct patch URL (no catalog lookup)")
    src.add_argument("--signature", type=int, help="third-party application signature id")
    src.add_argument("--package", help="package UUID (third-party or OS patch)")
    ap.add_argument("--arch", help="preferred architecture for --signature (e.g. x64, x86)")
    ap.add_argument("--dest", help="output file (default: from catalog metadata or URL)")
    ap.add_argument("--proxy", default=os.environ.get("SIMPLE_PROXY_URL"),
                    help="proxy base URL, e.g. http://127.0.0.1:8080 (or set SIMPLE_PROXY_URL)")
    ap.add_argument("--discover", action="store_true",
                    help="auto-discover proxies on the LAN (UDP) when --proxy is not given")
    ap.add_argument("--discovery-port", type=int, default=dl.DISCOVERY_PORT,
                    help=f"UDP discovery port (default {dl.DISCOVERY_PORT})")
    ap.add_argument("--discovery-host", action="append", metavar="HOST",
                    help="also probe this host/broadcast directly (repeatable; e.g. 192.168.0.6) "
                         "— for cross-subnet or broadcast-blocked networks")
    ap.add_argument("--token", default=os.environ.get("SIMPLE_PROXY_TOKEN"),
                    help="secure-mode token (or set SIMPLE_PROXY_TOKEN)")
    ap.add_argument("--sha256", help="override expected SHA-256 (else the catalog hash is used)")
    ap.add_argument("--require-hash", action="store_true",
                    help="refuse to download if no hash is available to verify against")
    ap.add_argument("--force", action="store_true",
                    help="re-download even if the destination file already exists")
    ap.add_argument("--health-timeout", type=float, default=5.0,
                    help="seconds to wait probing each proxy before falling back")
    args = ap.parse_args()

    # Build the list of candidate proxies: an explicit --proxy, else all discovered ones.
    proxy_list = []
    if args.proxy:
        proxy_list = [args.proxy]
    elif args.discover:
        log(f"[discover] probing UDP {args.discovery_port} for simple-proxy instances...")
        discovered = dl.discover_proxies(args.discovery_port, extra_hosts=args.discovery_host)
        if discovered:
            proxy_list = [d["base_url"] for d in discovered]
            log(f"[discover] found {len(discovered)} proxy(ies): "
                + ", ".join(f"{d['base_url']}(secure={d['secure']})" for d in discovered))
            if all(d["secure"] for d in discovered) and not args.token:
                log("[discover] WARNING: discovered proxy(ies) require a token but none was "
                    "provided (--token); they will 401 and we'll fall back to direct.")
        else:
            log("[discover] no proxy found; will download directly")

    # Resolve the target (url / signature / package) -> url + expected hash.
    url = args.url or args.url_opt
    target = cr.resolve_target(url=url, signature=args.signature, package=args.package,
                               arch=args.arch, sha256_override=args.sha256)
    if target is None:
        if not (url or args.signature is not None or args.package):
            ap.error("provide a URL, or --signature, or --package")
        return 3
    if not dl.require_hash_ok(target, args.require_hash):
        return 4
    url, expected_hash, hash_algo = target["url"], target["hash"], target["hash_algo"]

    dest = args.dest or cr.default_dest(url, target["resolved"])

    # Skip the download if the file already exists (verifying its hash when we know it).
    if not args.force and dl.file_already_present(dest, expected_hash, hash_algo):
        return 0

    try:
        result = dl.fetch(url, dest, proxies=proxy_list, token=args.token,
                          expected_hash=expected_hash, hash_algo=hash_algo,
                          health_timeout=args.health_timeout)
    except Exception as e:  # noqa: BLE001
        log(f"ERROR: {e}")
        return 1

    dl.print_result(result, dest, expected_hash)
    return 0


if __name__ == "__main__":
    sys.exit(main())
