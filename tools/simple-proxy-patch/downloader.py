#!/usr/bin/env python3
###############################################################################################
##  simple-proxy-patch - downloader.py  (library)
##
##  Everything about actually getting the bytes:
##    * discover_proxies(...)        find simple-proxy instances on the LAN (UDP)
##    * proxy_cache_status/rank_proxies   ask which proxy already has the content
##    * download_ordered(...)        try each proxy in order (cached-first), then direct
##    * fetch(...)                   rank_proxies + download_ordered convenience wrapper
##    * hashing + verify helpers     hash_of / file_already_present / require_hash_ok / print_result
##
##  Strategy: find the proxy that already has the content cached (fast HIT); if none, try each
##  proxy from the top of the list until one succeeds; if none succeed, download directly. Every
##  download is verified against the expected hash when one is known.
##
##  Standard library only.
##
##  Created by Chris Seiler - OPSWAT OEM Field CTO
###############################################################################################

import concurrent.futures
import hashlib
import json
import os
import socket
import sys
import time
import urllib.error
import urllib.parse
import urllib.request

CHUNK = 64 * 1024

# UDP self-discovery (must match the proxy's discovery.rs)
DISCOVERY_MAGIC = b"SIMPLE-PROXY-DISCOVER v1"
DISCOVERY_MULTICAST = "239.255.42.98"
DISCOVERY_PORT = 8099


def log(msg):
    print(msg, file=sys.stderr, flush=True)


# --------------------------------------------------------------------------------------------
# Self-discovery: find a simple-proxy via UDP loopback + broadcast + multicast + subnet sweep
# --------------------------------------------------------------------------------------------

def _local_ipv4s():
    """Best-effort set of this machine's IPv4 addresses (for per-subnet directed broadcast)."""
    ips = set()
    try:
        for info in socket.getaddrinfo(socket.gethostname(), None, socket.AF_INET):
            ips.add(info[4][0])
    except OSError:
        pass
    try:
        # The address used to reach the internet — reveals the primary LAN interface.
        probe = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        probe.connect(("8.8.8.8", 80))
        ips.add(probe.getsockname()[0])
        probe.close()
    except OSError:
        pass
    return {ip for ip in ips if ip and not ip.startswith("127.")}


def _probe_targets(extra_hosts):
    """Destinations to send the probe to: loopback, global + per-subnet directed broadcast,
    multicast, and any explicit --discovery-host values."""
    targets = ["127.0.0.1", "255.255.255.255", DISCOVERY_MULTICAST]
    # Per-interface directed broadcast (assume /24, correct for typical 192.168.x/10.x.x LANs).
    for ip in _local_ipv4s():
        parts = ip.split(".")
        if len(parts) == 4:
            targets.append(".".join(parts[:3] + ["255"]))
    for h in (extra_hosts or []):
        targets.append(h)
    # de-dup, preserve order
    seen, ordered = set(), []
    for t in targets:
        if t not in seen:
            seen.add(t)
            ordered.append(t)
    return ordered


def _record_from(info, ip):
    """Build a discovery record from a parsed reply + the responder's IP, or None if invalid."""
    if info.get("service") != "simple-proxy" or not info.get("proxy_port"):
        return None
    scheme = info.get("scheme", "http")
    return {
        "base_url": f"{scheme}://{ip}:{info['proxy_port']}",
        "secure": bool(info.get("secure")),
        "version": info.get("version"),
        "host": ip,
    }


def _probe_unicast(host, discovery_port, timeout):
    """
    Probe a single host with a *connected* UDP socket. This is the reliable path on Windows: the
    OS/firewall treats the reply as return traffic for an established flow, whereas the reply to a
    broadcast/multicast probe is often dropped as unsolicited. Returns a record or None.
    """
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.settimeout(timeout)
    try:
        s.connect((host, discovery_port))
        s.send(DISCOVERY_MAGIC)
        data = s.recv(4096)
        info = json.loads(data.decode("utf-8"))
        return _record_from(info, s.getpeername()[0])
    except Exception:  # noqa: BLE001 - timeout / refused / bad payload => no result
        return None
    finally:
        s.close()


def _sweep_hosts():
    """Every unicast host on this machine's directly-connected /24 subnet(s), for a connected-probe
    sweep. Skips link-local (169.254) interfaces. Assumes /24, correct for typical LANs."""
    hosts = []
    for ip in _local_ipv4s():
        if ip.startswith("169.254."):
            continue
        parts = ip.split(".")
        if len(parts) != 4:
            continue
        base = ".".join(parts[:3])
        hosts.extend(f"{base}.{h}" for h in range(1, 255))
    return list(dict.fromkeys(hosts))  # de-dup, preserve order


def _sweep_subnets(discovery_port, per_timeout=1.0, workers=128):
    """
    Connected-unicast probe of every host on the local /24 subnet(s), concurrently. This is the
    reliable auto-discovery path on Windows, where the reply to a broadcast probe is dropped by the
    client firewall but a connected-socket reply is allowed. Returns a list of records.
    """
    hosts = _sweep_hosts()
    found = []
    if not hosts:
        return found
    with concurrent.futures.ThreadPoolExecutor(max_workers=min(workers, len(hosts))) as ex:
        for rec in ex.map(lambda h: _probe_unicast(h, discovery_port, per_timeout), hosts):
            if rec:
                found.append(rec)
    return found


def discover_proxies(discovery_port=DISCOVERY_PORT, timeout=2.0, extra_hosts=None, sweep=True):
    """
    Probe for a simple-proxy over UDP and collect replies. Returns a list of dicts:
      {"base_url": "http://<ip>:<port>", "secure": bool, "version": str, "host": str}

    Mechanisms, in order:
      1. Connected unicast probes to any explicit `extra_hosts` (--discovery-host) — always
         reliable, including through a Windows client firewall.
      2. Broadcast (global + each interface's directed broadcast) + multicast — instant on Linux
         and permissive networks; a Windows client firewall may drop the replies.
      3. If nothing was found yet and `sweep` is set, a concurrent connected-unicast sweep of the
         local /24 subnet(s). This makes plain auto-discovery work on Windows without needing to
         name the host or change the client firewall.

    Loopback replies are preferred. The reply advertises whether a token is required ("secure");
    the token itself is never sent.
    """
    found = {}

    # 1) Reliable connected unicast probes for explicitly-named hosts.
    for host in (extra_hosts or []):
        rec = _probe_unicast(host, discovery_port, timeout)
        if rec:
            found[rec["base_url"]] = rec

    # 2) Best-effort broadcast + multicast for auto-discovery.
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        s.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
        s.settimeout(timeout)
        for dest in _probe_targets(None):  # loopback + broadcast + directed + multicast
            try:
                s.sendto(DISCOVERY_MAGIC, (dest, discovery_port))
            except OSError:
                pass  # a transport may be unavailable (e.g. no LAN); the others still work
        deadline = time.time() + timeout
        while time.time() < deadline:
            try:
                data, addr = s.recvfrom(4096)
            except (socket.timeout, OSError):
                break
            try:
                info = json.loads(data.decode("utf-8"))
            except (ValueError, UnicodeDecodeError):
                continue
            rec = _record_from(info, addr[0])
            if rec:
                found[rec["base_url"]] = rec
    finally:
        s.close()

    # 3) Fallback: connected-unicast sweep of the local subnet(s) (reliable on Windows).
    if not found and sweep:
        for rec in _sweep_subnets(discovery_port):
            found[rec["base_url"]] = rec

    # Prefer a loopback proxy (same host, always reachable) over LAN addresses.
    def _rank(entry):
        h = entry.get("host", "")
        return (0 if h == "localhost" or h.startswith("127.") else 1, h)

    return sorted(found.values(), key=_rank)


# --------------------------------------------------------------------------------------------
# Hashing
# --------------------------------------------------------------------------------------------

def hash_of(path, algo="sha256"):
    h = hashlib.new(algo)
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(CHUNK), b""):
            h.update(chunk)
    return h.hexdigest()


# --------------------------------------------------------------------------------------------
# Proxy selection + download (cached-first, try-each, then direct)
# --------------------------------------------------------------------------------------------

def proxy_cache_status(proxy, url, token, timeout):
    """
    Ask a proxy whether `url` is already cached. Returns one of:
      "cached"    - reachable and the URL is already in its cache
      "available" - reachable but not cached (a MISS download would be needed)
      "down"      - unreachable, errored, or auth-rejected (don't use it)
    The /cached query doubles as a liveness check.
    """
    qs = urllib.parse.urlencode({"url": url})
    req = urllib.request.Request(f"{proxy.rstrip('/')}/cached?{qs}")
    if token:
        req.add_header("X-Proxy-Token", token)
    try:
        with urllib.request.urlopen(req, timeout=timeout) as resp:
            info = json.loads(resp.read().decode("utf-8"))
        return "cached" if info.get("cached") else "available"
    except Exception:  # noqa: BLE001 - any failure means "don't use this proxy"
        return "down"


def rank_proxies(proxies, url, token, timeout):
    """
    Query every proxy's cache status and return them ordered best-first: proxies that already
    have the file cached come first (fast HIT), then merely-available ones (in their original
    list order). Unreachable proxies are dropped. Returns (ordered_list, any_cached_bool).
    """
    scored = []
    for p in proxies:
        status = proxy_cache_status(p, url, token, timeout)
        if status == "cached":
            log(f"[proxy] {p} has it CACHED")
            scored.append((0, p))
        elif status == "available":
            log(f"[proxy] {p} available (not cached)")
            scored.append((1, p))
        else:
            log(f"[proxy] {p} not usable; skipping")
    scored.sort(key=lambda t: t[0])  # stable: preserves list order within each rank
    return [p for _, p in scored], any(rank == 0 for rank, _ in scored)


def _stream_to_file(resp, dest):
    total = 0
    with open(dest, "wb") as f:
        while True:
            chunk = resp.read(CHUNK)
            if not chunk:
                break
            f.write(chunk)
            total += len(chunk)
    return total


def download_via_proxy(proxy, url, token, dest, timeout):
    qs = urllib.parse.urlencode({"url": url})
    full = f"{proxy.rstrip('/')}/download?{qs}"
    req = urllib.request.Request(full)
    if token:
        req.add_header("X-Proxy-Token", token)
    start = time.time()
    with urllib.request.urlopen(req, timeout=timeout) as resp:
        cache_state = resp.headers.get("X-Cache", "?")
        total = _stream_to_file(resp, dest)
    return total, cache_state, time.time() - start


def download_direct(url, dest, timeout):
    req = urllib.request.Request(url, headers={"User-Agent": "simple-proxy-patch/1.0"})
    start = time.time()
    with urllib.request.urlopen(req, timeout=timeout) as resp:
        total = _stream_to_file(resp, dest)
    return total, time.time() - start


def download_ordered(url, dest, ordered_proxies, token=None, expected_hash=None,
                     hash_algo="sha256", download_timeout=600):
    """
    Try to download `url` from each proxy in `ordered_proxies` (already in priority order), in
    order, until one succeeds and passes the hash check; if none do, download direct. Returns
    {method, proxy, size, cache, hash, hash_algo, seconds}. Raises on a direct-download hash
    mismatch. This is the core "try each, then direct" step — `fetch` wraps it with ranking.
    """
    expected = expected_hash.lower() if expected_hash else None

    for p in (ordered_proxies or []):
        try:
            size, cache_state, secs = download_via_proxy(p, url, token, dest, download_timeout)
            actual = hash_of(dest, hash_algo)
            if expected and actual.lower() != expected:
                log(f"[proxy {p}] {hash_algo} mismatch (got {actual}); trying next source")
                continue
            return {"method": "proxy", "proxy": p, "size": size, "cache": cache_state,
                    "hash": actual, "hash_algo": hash_algo, "seconds": secs}
        except urllib.error.HTTPError as e:
            body = ""
            try:
                body = e.read().decode("utf-8", "replace")
            except Exception:
                pass
            log(f"[proxy {p}] download failed: HTTP {e.code} {e.reason} {body}; trying next source")
        except Exception as e:  # noqa: BLE001
            log(f"[proxy {p}] download failed: {e}; trying next source")

    # Direct download (fallback, or when no proxy is usable/configured)
    log("[direct] downloading from origin")
    size, secs = download_direct(url, dest, download_timeout)
    actual = hash_of(dest, hash_algo)
    if expected and actual.lower() != expected:
        raise RuntimeError(
            f"{hash_algo} mismatch after direct download: expected {expected}, got {actual}")
    return {"method": "direct", "proxy": None, "size": size, "cache": None,
            "hash": actual, "hash_algo": hash_algo, "seconds": secs}


def fetch(url, dest, proxies=None, proxy=None, token=None, expected_hash=None,
          hash_algo="sha256", health_timeout=5, download_timeout=600):
    """
    Download `url` to `dest`: find which proxy already has it CACHED (tried first), else try each
    proxy top-to-bottom until one succeeds, else download direct. `proxies` is a list of base URLs;
    `proxy` (a single string) is also accepted. Returns the `download_ordered` result dict.
    """
    proxy_list = list(proxies) if proxies else []
    if proxy and proxy not in proxy_list:
        proxy_list.append(proxy)

    # Order proxies best-first (already-cached before merely-available); drop unreachable ones.
    ordered = rank_proxies(proxy_list, url, token, health_timeout)[0] if proxy_list else []
    return download_ordered(url, dest, ordered, token, expected_hash, hash_algo, download_timeout)


# --------------------------------------------------------------------------------------------
# Verify / output helpers
# --------------------------------------------------------------------------------------------

def file_already_present(dest, expected_hash, hash_algo):
    """If `dest` exists and (its hash matches when known / no hash to check), report it and return
    True (skip download). Otherwise return False."""
    if not os.path.exists(dest):
        return False
    size = os.path.getsize(dest)
    if expected_hash:
        if hash_of(dest, hash_algo).lower() == expected_hash.lower():
            print(f"OK  {size:,} bytes -> {dest}")
            print(f"    already present ({hash_algo} verified); skipping download "
                  f"(use --force to re-download)")
            return True
        log(f"[exists] {dest} present but {hash_algo} mismatch; re-downloading")
        return False
    print(f"OK  {size:,} bytes -> {dest}")
    print("    already present; skipping download (no hash to verify; use --force to re-download)")
    return True


def require_hash_ok(target, require_hash):
    """When --require-hash is set, refuse to proceed if the target has no hash to verify against."""
    if require_hash and not target.get("hash"):
        log("ERROR: --require-hash was set but no hash is available for this target; refusing to "
            "download an unverifiable file.")
        return False
    return True


def print_result(result, dest, expected_hash):
    rate = (result["size"] / result["seconds"] / 1_000_000) if result.get("seconds") else 0
    via = result["method"]
    if via == "proxy":
        via += f" (cache {result['cache']} @ {result['proxy']})"
    verified = " (verified)" if expected_hash else ""
    print(f"OK  {result['size']:,} bytes -> {dest}")
    print(f"    via: {via}   {result['hash_algo']}: {result['hash']}{verified}   "
          f"{result['seconds']:.2f}s ({rate:.1f} MB/s)")
