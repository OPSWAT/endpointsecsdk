#!/usr/bin/env python3
###############################################################################################
##  simple-proxy-patch sample #1 - AUTO-DISCOVER then download
##
##  Demonstrates downloading a patch when you DON'T know the proxies up front:
##    1. Auto-discover simple-proxy instances on the LAN (UDP).
##    2. Ask each discovered proxy whether it already has the content cached.
##    3. Download from a proxy that already has it (fast cache HIT); otherwise try each proxy from
##       the top of the list until one succeeds.
##    4. If no proxy succeeds, download directly from the origin.
##  The result is verified against the catalog's hash (SHA-256 / SHA-1).
##
##  Usage:
##      python sample_autodiscover.py --signature 3039
##      python sample_autodiscover.py --package <uuid> --token s3cret
##      python sample_autodiscover.py https://example.com/patch.msu --sha256 <hex>
##      python sample_autodiscover.py --signature 3039 --discovery-host 192.168.0.6
##
##  Reuses the reusable helpers in download_patch.py.
##
##  Created by Chris Seiler - OPSWAT OEM Field CTO
###############################################################################################

import argparse
import sys

import catalog_resolve as cr
import downloader as dl


def main():
    ap = argparse.ArgumentParser(description="Auto-discover a simple-proxy and download a patch.")
    src = ap.add_mutually_exclusive_group()
    src.add_argument("url", nargs="?", help="direct patch URL (no catalog lookup)")
    src.add_argument("--signature", type=int, help="third-party application signature id")
    src.add_argument("--package", help="package UUID (third-party or OS patch)")
    ap.add_argument("--arch", help="preferred architecture for --signature (e.g. x64, x86)")
    ap.add_argument("--sha256", help="override expected SHA-256")
    ap.add_argument("--require-hash", action="store_true",
                    help="refuse to download if no hash is available to verify against")
    ap.add_argument("--dest", help="output file")
    ap.add_argument("--force", action="store_true", help="re-download even if dest exists")
    ap.add_argument("--token", help="secure-mode token (X-Proxy-Token)")
    ap.add_argument("--discovery-port", type=int, default=dl.DISCOVERY_PORT,
                    help=f"UDP discovery port (default {dl.DISCOVERY_PORT})")
    ap.add_argument("--discovery-host", action="append", metavar="HOST",
                    help="also probe this host/broadcast directly (repeatable)")
    ap.add_argument("--timeout", type=float, default=5.0, help="per-proxy probe timeout (s)")
    args = ap.parse_args()

    # Resolve the target (url / signature / package) -> url + expected hash.
    target = cr.resolve_target(url=args.url, signature=args.signature, package=args.package,
                               arch=args.arch, sha256_override=args.sha256)
    if target is None:
        if not (args.url or args.signature is not None or args.package):
            ap.error("provide a URL, or --signature, or --package")
        return 3
    if not dl.require_hash_ok(target, args.require_hash):
        return 4
    url, expected_hash, hash_algo = target["url"], target["hash"], target["hash_algo"]

    dest = args.dest or cr.default_dest(url, target["resolved"])
    if not args.force and dl.file_already_present(dest, expected_hash, hash_algo):
        return 0

    # 1) Auto-discover proxies on the LAN.
    dl.log(f"[discover] probing UDP {args.discovery_port} for simple-proxy instances...")
    discovered = dl.discover_proxies(args.discovery_port, extra_hosts=args.discovery_host)
    proxies = [d["base_url"] for d in discovered]
    if proxies:
        dl.log(f"[discover] found {len(proxies)}: "
               + ", ".join(f"{d['base_url']}(secure={d['secure']})" for d in discovered))
        if all(d["secure"] for d in discovered) and not args.token:
            dl.log("[discover] WARNING: proxy(ies) require a token; pass --token or it falls back "
                   "to direct.")
    else:
        dl.log("[discover] none found; will download directly")

    # 2) Find which proxy already has the content (ordered cache-first; unreachable dropped).
    ordered, any_cached = dl.rank_proxies(proxies, url, args.token, args.timeout) if proxies \
        else ([], False)
    if any_cached:
        dl.log(f"[plan] a proxy already has it cached -> {ordered[0]}")
    elif ordered:
        dl.log(f"[plan] not cached anywhere; will try {len(ordered)} proxy(ies) in order, then direct")
    else:
        dl.log("[plan] no usable proxy; downloading direct")

    # 3+4) Download: cached proxy first, else each proxy in order, else direct.
    try:
        result = dl.download_ordered(url, dest, ordered, token=args.token,
                                     expected_hash=expected_hash, hash_algo=hash_algo)
    except Exception as e:  # noqa: BLE001
        dl.log(f"ERROR: {e}")
        return 1

    dl.print_result(result, dest, expected_hash)
    return 0


if __name__ == "__main__":
    sys.exit(main())
