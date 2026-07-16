#!/usr/bin/env python3
###############################################################################################
##  simple-proxy-patch sample #2 - PROXY LIST then download
##
##  Demonstrates downloading a patch when you already have a list of proxies (e.g. a site's known
##  caches), given in priority order:
##    1. Ask each proxy in the list whether it already has the content cached.
##    2. Download from a proxy that already has it (fast cache HIT); otherwise try each proxy from
##       the top of the list until one succeeds.
##    3. If no proxy succeeds, download directly from the origin.
##  The result is verified against the catalog's hash (SHA-256 / SHA-1).
##
##  Usage:
##      python sample_proxylist.py --signature 3039 \
##          --proxy http://192.168.0.6:8080 --proxy http://192.168.0.7:8080
##      python sample_proxylist.py https://example.com/patch.msu --sha256 <hex> \
##          --proxy http://cache-a:8080 --proxy http://cache-b:8080 --token s3cret
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
    ap = argparse.ArgumentParser(
        description="Download a patch through a given list of proxies (cache-first, then in "
                    "order, then direct).")
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
    ap.add_argument("--proxy", action="append", metavar="URL", required=True,
                    help="proxy base URL in priority order (repeatable), "
                         "e.g. --proxy http://192.168.0.6:8080")
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

    proxies = args.proxy
    dl.log(f"[plan] proxy list ({len(proxies)}): " + ", ".join(proxies))

    # 1) Find which proxy already has the content (ordered cache-first; unreachable dropped,
    #    otherwise preserving the given list order).
    ordered, any_cached = dl.rank_proxies(proxies, url, args.token, args.timeout)
    if any_cached:
        dl.log(f"[plan] a proxy already has it cached -> {ordered[0]}")
    elif ordered:
        dl.log(f"[plan] not cached anywhere; will try {len(ordered)} proxy(ies) in order, then direct")
    else:
        dl.log("[plan] no usable proxy in the list; downloading direct")

    # 2+3) Download: cached proxy first, else each proxy in order, else direct.
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
