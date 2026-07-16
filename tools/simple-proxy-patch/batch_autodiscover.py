#!/usr/bin/env python3
###############################################################################################
##  simple-proxy-patch - batch_autodiscover.py
##
##  Download ~N different patches by driving sample_autodiscover.py once per signature. Each run
##  auto-discovers a proxy on the LAN and downloads through it (cached-first, then proxy, then
##  direct), verifying the catalog hash. Signatures are gathered from the SDK catalog so they are
##  guaranteed to resolve.
##
##  Usage:
##      python batch_autodiscover.py                          # 20 patches, auto-discover
##      python batch_autodiscover.py --count 20 --out patches --token s3cret
##      python batch_autodiscover.py --discovery-host 192.168.0.6 --token s3cret   # faster/targeted
##
##  --discovery-host is optional; passing your proxy's address makes each run probe it directly
##  (instant) instead of doing a LAN sweep every time.
##
##  Created by Chris Seiler - OPSWAT OEM Field CTO
###############################################################################################

import argparse
import os
import subprocess
import sys
import time
import urllib.parse

import catalog_resolve as cr

HERE = os.path.dirname(os.path.abspath(__file__))
SAMPLE = os.path.join(HERE, "sample_autodiscover.py")

# Signatures skipped by default because their catalog hash is known to be stale: the vendor
# re-spins the installer under the same filename/version, so the live file no longer matches the
# recorded SHA-256 and the (correct) integrity check rejects it. Refresh the catalog to re-include.
EXCLUDE_SIGS = {
    3115,  # IrfanView 4.75 (iview475_x64_setup.exe re-released under the same name)
}


def gather_signatures(count, exclude=None):
    """Pick `count` distinct signatures whose latest patch (the package sample_autodiscover.py
    would choose) has an https download link + sha256. Returns (sig, name, url) so we can name the
    saved file after the real executable. `exclude` sigs are skipped."""
    exclude = set(exclude or ())
    server = cr.cat.require_server_dir()
    path = os.path.join(server, "patch_aggregation_v2.json")
    picked, seen = [], set()
    for rec in cr.cat.read_records(path):
        if len(picked) >= count:
            break
        if not rec.get("is_latest"):
            continue
        prod = rec.get("product") or {}
        sigs = prod.get("v4_signatures") or []
        if not sigs or sigs[0] in seen or sigs[0] in exclude:
            continue
        # Use the same package the sample will pick (arch-preferred), so the URL/filename match.
        pkg = cr._choose_package(rec.get("packages") or [], None)
        if not pkg:
            continue
        links = pkg.get("download_links") or []
        if links and pkg.get("sha256") and links[0].lower().startswith("https://"):
            picked.append((sigs[0], prod.get("name") or "?", links[0]))
            seen.add(sigs[0])
    return picked


def dest_filename(url, sig, used):
    """Real executable filename from the download URL (e.g. winrar-x32-701.exe). Falls back to
    <sig>.bin for query-based/extensionless URLs, and de-duplicates collisions across apps."""
    base = urllib.parse.unquote(os.path.basename(urllib.parse.urlsplit(url).path))
    if not base or "." not in base:
        base = f"{sig}.bin"
    if base in used:
        base = f"{sig}-{base}"
    used.add(base)
    return base


def main():
    ap = argparse.ArgumentParser(description="Download N patches via sample_autodiscover.py.")
    ap.add_argument("--count", type=int, default=20, help="number of patches (default 20)")
    ap.add_argument("--out", default="autodiscover-patches", help="output directory")
    ap.add_argument("--token", help="secure-mode token (passed through)")
    ap.add_argument("--discovery-host", action="append", metavar="HOST",
                    help="proxy host/broadcast to probe directly (passed through, repeatable)")
    ap.add_argument("--discovery-port", type=int, help="UDP discovery port (passed through)")
    ap.add_argument("--exclude", type=int, action="append", metavar="SIG",
                    help="signature id to skip (repeatable; in addition to the built-in list)")
    ap.add_argument("--list", action="store_true",
                    help="just print the selected signatures and exit (no downloads)")
    args = ap.parse_args()

    exclude = EXCLUDE_SIGS | set(args.exclude or ())
    sigs = gather_signatures(args.count, exclude=exclude)
    print(f"[batch] selected {len(sigs)} signatures "
          f"(excluding {sorted(exclude)}) to download via sample_autodiscover.py\n")
    used = set()
    if args.list:
        for sig, name, url in sigs:
            print(f"  sig {sig:<6} {name:<20} -> {dest_filename(url, sig, used)}")
        return 0

    os.makedirs(args.out, exist_ok=True)

    ok = 0
    start = time.time()
    for i, (sig, name, url) in enumerate(sigs, 1):
        fname = dest_filename(url, sig, used)
        dest = os.path.join(args.out, fname)
        cmd = [sys.executable, SAMPLE, "--signature", str(sig), "--dest", dest]
        if args.token:
            cmd += ["--token", args.token]
        if args.discovery_port:
            cmd += ["--discovery-port", str(args.discovery_port)]
        for h in (args.discovery_host or []):
            cmd += ["--discovery-host", h]
        print(f"[batch] ({i}/{len(sigs)}) sig {sig}  {name}  -> {fname}")
        rc = subprocess.run(cmd).returncode
        status = "OK" if rc == 0 else f"FAILED (rc={rc})"
        if rc == 0:
            ok += 1
        print(f"[batch]   -> {status}\n")

    elapsed = time.time() - start
    print(f"== {ok}/{len(sigs)} patches downloaded in {elapsed:.0f}s -> {args.out}/ ==")
    return 0 if ok == len(sigs) else 1


if __name__ == "__main__":
    sys.exit(main())
