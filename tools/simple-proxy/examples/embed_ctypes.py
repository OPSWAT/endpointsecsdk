#!/usr/bin/env python3
###############################################################################################
##  simple-proxy example - embed_ctypes.py
##
##  Demonstrate EMBEDDING the proxy in an existing process by loading the dynamic library
##  (simple_proxy.dll / libsimple_proxy.so / libsimple_proxy.dylib) via ctypes and driving the
##  C ABI: simple_proxy_start(...) -> handle, then simple_proxy_stop(handle). This mirrors how a
##  non-Python host (C/C++/C#/Java...) would consume the library.
##
##  Usage:
##      python embed_ctypes.py <url-to-download> [--port 8080] [--token TOK] [--lib PATH]
##
##  Created by Chris Seiler - OPSWAT OEM Field CTO
###############################################################################################

import argparse
import ctypes
import json
import os
import platform
import sys
import time
import urllib.parse
import urllib.request


def default_lib_path():
    here = os.path.dirname(os.path.abspath(__file__))
    bindir = os.path.join(here, "..", "bin")
    system = platform.system()
    name = {
        "Windows": "simple_proxy.dll",
        "Linux": "libsimple_proxy.so",
        "Darwin": "libsimple_proxy.dylib",
    }.get(system, "libsimple_proxy.so")
    return os.path.normpath(os.path.join(bindir, name))


def load_lib(path):
    lib = ctypes.CDLL(path)
    lib.simple_proxy_version.restype = ctypes.c_char_p
    lib.simple_proxy_start.restype = ctypes.c_void_p
    lib.simple_proxy_start.argtypes = [
        ctypes.c_char_p,   # bind
        ctypes.c_uint16,   # port
        ctypes.c_char_p,   # cache_dir
        ctypes.c_char_p,   # token
        ctypes.c_uint64,   # max_cache_bytes
        ctypes.c_uint64,   # ttl_seconds
        ctypes.c_uint64,   # max_download_bytes
    ]
    lib.simple_proxy_stop.restype = None
    lib.simple_proxy_stop.argtypes = [ctypes.c_void_p]
    return lib


def main():
    ap = argparse.ArgumentParser(description="Embed simple-proxy via its C ABI (ctypes).")
    ap.add_argument("url", help="URL to download through the embedded proxy")
    ap.add_argument("--port", type=int, default=8088)
    ap.add_argument("--token", default="embed-token")
    ap.add_argument("--cache-dir", default="simple-proxy-embed-cache")
    ap.add_argument("--lib", default=default_lib_path(), help="path to the dynamic library")
    args = ap.parse_args()

    print(f"loading {args.lib}")
    lib = load_lib(args.lib)
    print("version:", lib.simple_proxy_version().decode())

    def s(x):
        return x.encode() if x is not None else None

    handle = lib.simple_proxy_start(
        s("127.0.0.1"), ctypes.c_uint16(args.port), s(args.cache_dir), s(args.token),
        ctypes.c_uint64(1_000_000_000), ctypes.c_uint64(3600), ctypes.c_uint64(0))
    if not handle:
        print("simple_proxy_start returned NULL (failed to start)", file=sys.stderr)
        return 1
    print(f"proxy started (handle={handle}) on 127.0.0.1:{args.port}")

    try:
        time.sleep(1.0)  # let the listener come up
        base = f"http://127.0.0.1:{args.port}"
        hdr = {"X-Proxy-Token": args.token}

        req = urllib.request.Request(f"{base}/health", headers=hdr)
        with urllib.request.urlopen(req, timeout=10) as r:
            print("health:", json.loads(r.read().decode()))

        qs = urllib.parse.urlencode({"url": args.url})
        req = urllib.request.Request(f"{base}/download?{qs}", headers=hdr)
        with urllib.request.urlopen(req, timeout=120) as r:
            data = r.read()
            print(f"downloaded {len(data):,} bytes  X-Cache={r.headers.get('X-Cache')}")
    finally:
        print("stopping proxy...")
        lib.simple_proxy_stop(handle)
        print("stopped.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
