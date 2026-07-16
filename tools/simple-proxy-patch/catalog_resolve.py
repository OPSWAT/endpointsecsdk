#!/usr/bin/env python3
###############################################################################################
##  simple-proxy-patch - catalog_resolve.py  (library)
##
##  Resolve a patch to a concrete download target using the OPSWAT SDK "Analog" catalog:
##    * resolve_signature(server, sig)   third-party application signature id -> latest patch
##                                        (patch_aggregation_v2.json), URL + SHA-256
##    * resolve_package(server, uuid)    package UUID -> third-party (SHA-256) or OS patch (SHA-1)
##    * resolve_target(...)              url / signature / package -> {url, hash, hash_algo, resolved}
##
##  Reuses the shared catalog helpers (_catalog.py) from the sibling catalog-lookup tool. Requires
##  the extracted catalog at OPSWAT-SDK/extract/analog/server (run the SDK downloader first).
##
##  Created by Chris Seiler - OPSWAT OEM Field CTO
###############################################################################################

import os
import re
import sys
import urllib.parse

# Reuse the shared catalog helpers from the sibling catalog-lookup tool.
_CATALOG_DIR = os.path.normpath(
    os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "catalog-lookup"))
if _CATALOG_DIR not in sys.path:
    sys.path.insert(0, _CATALOG_DIR)
try:
    import _catalog as cat  # noqa: E402
except ImportError:
    cat = None

_ARCH64 = {"x64", "amd64", "x86_64", "x86-64"}


def log(msg):
    print(msg, file=sys.stderr, flush=True)


def _require_catalog():
    if cat is None:
        log("ERROR: could not import the catalog helpers (_catalog.py) from ../catalog-lookup.")
        sys.exit(2)
    return cat.require_server_dir()


def _choose_package(packages, arch):
    """Pick a package by requested arch, else prefer a 64-bit build, else the first."""
    packages = [p for p in packages if p]
    if not packages:
        return None
    if arch:
        want = arch.strip().lower()
        for p in packages:
            archs = [str(a).lower() for a in (p.get("architectures") or [])]
            if want in archs:
                return p
        log(f"  (no package for arch '{arch}', using default)")
    for p in packages:
        archs = {str(a).lower() for a in (p.get("architectures") or [])}
        if archs & _ARCH64:
            return p
    return packages[0]


def resolve_signature(server, sig, arch=None):
    """signature id -> {url, hash, hash_algo, name, version, source, package_uuid, arch, kind}."""
    path = os.path.join(server, "patch_aggregation_v2.json")
    if not os.path.isfile(path):
        log("ERROR: patch_aggregation_v2.json not found in the catalog.")
        return None
    candidates = []
    for rec in cat.read_records(path):
        sigs = (rec.get("product") or {}).get("v4_signatures") or []
        if sig in sigs:
            candidates.append(rec)
    if not candidates:
        return None
    candidates.sort(key=lambda r: not r.get("is_latest"))  # is_latest first
    rec = candidates[0]
    pkg = _choose_package(rec.get("packages") or [], arch)
    if not pkg:
        log(f"  signature {sig} has a patch but no downloadable package.")
        return None
    links = pkg.get("download_links") or []
    if not links:
        log(f"  signature {sig} package has no download link.")
        return None
    product = rec.get("product") or {}
    return {
        "url": links[0],
        "hash": pkg.get("sha256"),
        "hash_algo": "sha256",
        "name": product.get("name"),
        "version": rec.get("version"),
        "source": rec.get("data_source"),
        "package_uuid": pkg.get("package_uuid"),
        "arch": ", ".join(pkg.get("architectures") or []) or "?",
        "kind": "3rd-party",
    }


def resolve_package(server, uuid):
    """package UUID -> download info. Searches 3rd-party then OS patch datasets."""
    uuid = uuid.strip().lower()

    # 1) Third-party packages (patch_aggregation_v2.json): download_links[] + sha256
    tp = os.path.join(server, "patch_aggregation_v2.json")
    if os.path.isfile(tp):
        for rec in cat.read_records(tp):
            for pkg in rec.get("packages") or []:
                if str(pkg.get("package_uuid", "")).lower() == uuid:
                    links = pkg.get("download_links") or []
                    if not links:
                        continue
                    product = rec.get("product") or {}
                    return {
                        "url": links[0],
                        "hash": pkg.get("sha256"),
                        "hash_algo": "sha256",
                        "name": product.get("name"),
                        "version": rec.get("version"),
                        "source": rec.get("data_source"),
                        "package_uuid": pkg.get("package_uuid"),
                        "arch": ", ".join(pkg.get("architectures") or []) or "?",
                        "kind": "3rd-party",
                    }

    # 2) OS patch packages (patch_system_aggregation_v2.json): download_link.link + sha1
    osp = os.path.join(server, "patch_system_aggregation_v2.json")
    if os.path.isfile(osp):
        for rec in cat.read_records(osp):
            for pkg in rec.get("packages") or []:
                if str(pkg.get("package_uuid", "")).lower() == uuid:
                    dl = pkg.get("download_link") or {}
                    if not dl.get("link"):
                        continue
                    return {
                        "url": dl["link"],
                        "hash": dl.get("sha1"),
                        "hash_algo": "sha1",
                        "name": pkg.get("title") or f"KB{rec.get('kb_id')}",
                        "version": None,
                        "source": rec.get("data_source"),
                        "package_uuid": pkg.get("package_uuid"),
                        "arch": ", ".join(pkg.get("architectures") or []) or "?",
                        "kind": "OS",
                    }
    return None


def resolve_target(url=None, signature=None, package=None, arch=None, sha256_override=None):
    """
    Turn a URL / signature id / package UUID into a concrete download target. Exactly one input
    should be given. Returns {"url", "hash", "hash_algo", "resolved"} (resolved is the catalog
    metadata dict, or None for a bare URL), or None if it couldn't be resolved. Logs progress.
    """
    resolved = None
    if signature is not None:
        server = _require_catalog()
        log(f"[catalog] resolving signature {signature}...")
        resolved = resolve_signature(server, signature, arch)
        if not resolved:
            log(f"ERROR: no downloadable patch found for signature {signature}.")
            return None
    elif package:
        server = _require_catalog()
        log(f"[catalog] resolving package {package}...")
        resolved = resolve_package(server, package)
        if not resolved:
            log(f"ERROR: no package with UUID '{package}' found in the catalog.")
            return None

    if resolved:
        url = resolved["url"]
        log(f"[catalog] {resolved['kind']} patch: {resolved.get('name')} "
            f"{resolved.get('version') or ''}".rstrip()
            + f"  [{resolved.get('source')}, arch {resolved.get('arch')}]")
        log(f"[catalog] url : {url}")
        log(f"[catalog] {resolved['hash_algo']} : {resolved.get('hash') or '(none)'}")

    if not url:
        return None

    if sha256_override:
        expected_hash, hash_algo = sha256_override, "sha256"
    elif resolved:
        expected_hash, hash_algo = resolved.get("hash"), resolved.get("hash_algo", "sha256")
    else:
        expected_hash, hash_algo = None, "sha256"

    if resolved and not expected_hash:
        log(f"[catalog] WARNING: the catalog has no {hash_algo} hash for this patch; the "
            f"download will NOT be integrity-verified (pass --require-hash to refuse).")

    return {"url": url, "hash": expected_hash, "hash_algo": hash_algo, "resolved": resolved}


def _safe_name(s):
    return re.sub(r"[^A-Za-z0-9._-]+", "_", s).strip("_") or "patch"


def default_dest(url, resolved=None):
    """Prefer a name from resolved catalog metadata; else the URL basename; else patch.bin."""
    name = os.path.basename(urllib.parse.urlsplit(url).path)
    _, ext = os.path.splitext(name)
    if resolved and resolved.get("name"):
        base = _safe_name(resolved["name"])
        if resolved.get("version"):
            base += "-" + _safe_name(str(resolved["version"]))
        return base + (ext if ext else ".bin")
    return name if name else "patch.bin"
