# simple-proxy-patch

A sample **patch downloader** that resolves a patch from the OPSWAT **SDK catalog** and fetches it,
preferring a shared [simple-proxy](../simple-proxy) cache and **falling back to a traditional
direct download** when the proxy isn't available.

This is the intended real-world pattern: your tooling knows a patch by its **signature id** or
**package UUID**, looks up the real download URL (and hash) in the catalog, then pulls the bytes
once through the proxy so many endpoints share a single WAN download — while every endpoint can
still self-serve if the proxy is down.

## Ways to specify the patch

| Input | Resolved via | Hash |
|---|---|---|
| `--signature <id>` | `patch_aggregation_v2.json` — the latest patch for that third-party application signature | SHA-256 |
| `--package <uuid>` | `patch_aggregation_v2.json` (3rd-party) or `patch_system_aggregation_v2.json` (OS patch) | SHA-256 / SHA-1 |
| `--url <url>` (or positional) | used directly, no catalog lookup | — |

For `--signature`, the newest (`is_latest`) patch is chosen and a package is picked by `--arch`
(e.g. `x64`, `x86`) — defaulting to a 64-bit build, else the first available.

## Download flow (all inputs)

1. **Already downloaded?** — if the destination file already exists, it's kept and the download is
   skipped (its hash is verified first when the catalog hash is known). Use `--force` to
   re-download.
2. **Pick the best proxy** — with one `--proxy` or several discovered via `--discover`, each
   proxy's `/cached` endpoint is queried and the one that **already has the file cached** is used
   first (a fast HIT), then any other reachable proxy (a MISS download).
3. **Fall back to direct** — if no proxy is usable, errors, or delivers content that fails the
   expected hash, download straight from the resolved origin URL.
4. **Verify** — for `--signature` / `--package` the expected hash is pulled from the catalog
   (SHA-256 for third-party apps, SHA-1 for OS patches) and the download is checked against it
   (or against `--sha256` if you pass one). A proxy result that fails the check moves on to the
   next source; a failing direct download is a hard error. The handful of catalog entries with no
   published hash log a **warning** that the download can't be verified — pass `--require-hash` to
   refuse those instead.

Catalog resolution needs the extracted SDK catalog at `OPSWAT-SDK/extract/analog/server` (run the
[SDK downloader](../sdk-downloader) first). It reuses the shared helpers from
[../catalog-lookup](../catalog-lookup). Standard library only otherwise.

## Usage

```bash
# By signature id, through the proxy (falls back to direct if the proxy is down)
python download_patch.py --signature 3128 --proxy http://127.0.0.1:8080 --token s3cret
#   -> resolves "WinRAR 7.1.0", downloads winrar-x32-701.exe, verifies SHA-256

# Pick a specific architecture for a signature
python download_patch.py --signature 41 --arch x64 --proxy http://127.0.0.1:8080 --token s3cret

# By package UUID (third-party or OS patch — auto-detected)
python download_patch.py --package e2aff842-9273-4435-bc59-cf8e7ab9d05a
python download_patch.py --package a633e05d-ad72-4553-8752-9c8cb89dbe10   # an OS patch (SHA-1)

# By direct URL (no catalog), with an explicit hash
python download_patch.py https://download.example.com/patch.msu --sha256 <hex>

# Auto-discover a proxy on the LAN (no --proxy needed) — see "Self-discovery" below
python download_patch.py --signature 3128 --discover --token s3cret

# Using environment variables for the proxy
export SIMPLE_PROXY_URL=http://127.0.0.1:8080
export SIMPLE_PROXY_TOKEN=s3cret
python download_patch.py --signature 3128
```

## Self-discovery

Instead of passing `--proxy`, use `--discover` to find a proxy on the local network via UDP. The
client tries, in order: any explicit `--discovery-host`; broadcast + multicast; and — if those
find nothing — a **connected-unicast sweep of the local /24 subnet(s)**. The sweep is what makes
plain `--discover` reliable on Windows, where the client firewall silently drops the reply to a
broadcast probe but allows a connected reply; it adds a few seconds. The proxy must be started with
**`--discovery` and exposed with `--bind 0.0.0.0`**, and its discovery UDP port + HTTP port must be
reachable through any firewall on the proxy host. The reply advertises whether a token is required
(`secure`) but never transmits the token, so discovery works with or without a security token; for
a secure proxy you still supply `--token` yourself (otherwise the proxy 401s and this tool falls
back to a direct download).

```bash
# proxy side (on the shared host, e.g. 192.168.0.6)
simple-proxy-demo --bind 0.0.0.0 --discovery --open-firewall --token s3cret

# client side
python download_patch.py --signature 3128 --discover --token s3cret
```

**Checklist if discovery finds nothing:**
1. Proxy started with `--bind 0.0.0.0 --discovery` (loopback bind can't be reached from other hosts).
2. Firewall on the proxy host allows inbound **UDP** (discovery port, default 8099) **and TCP**
   (proxy port). `--open-firewall` opens both (needs admin/root).
3. Client and proxy are on the same subnet (broadcast/multicast don't cross routers).
4. Want it faster / cross-subnet? Name the host: `--discovery-host 192.168.0.6` probes it directly
   (connected unicast) and skips the subnet sweep.
5. Or just skip discovery when you know the address: `--proxy http://192.168.0.6:8080`.

| Option | Meaning |
|---|---|
| `--discover` | Auto-discover proxies on the LAN when `--proxy` is not given. |
| `--discovery-port <n>` | UDP discovery port (default 8099); must match the proxy's `--discovery-port`. |
| `--discovery-host <ip>` | Also probe this host/broadcast directly (repeatable) — for cross-subnet or broadcast-blocked networks. |

| Option | Meaning |
|---|---|
| `--signature <id>` | Third-party application signature id → latest patch (catalog). |
| `--package <uuid>` | Package UUID → third-party or OS patch download (catalog). |
| `--url <url>` / positional | Direct URL, no catalog lookup. |
| `--arch <a>` | Preferred architecture for `--signature` (e.g. `x64`, `x86`). |
| `--dest <path>` | Output file (default: from catalog name+version, else URL basename). |
| `--force` | Re-download even if the destination file already exists. |
| `--proxy <url>` | Proxy base URL. Env: `SIMPLE_PROXY_URL`. |
| `--discover` | Auto-discover proxies on the LAN when `--proxy` is not given (tries all found). |
| `--discovery-port <n>` | UDP discovery port (default 8099); must match the proxy's `--discovery-port`. |
| `--discovery-host <ip>` | Also probe this host/broadcast directly (repeatable); for cross-subnet or broadcast-blocked networks. |
| `--token <tok>` | Secure-mode token (`X-Proxy-Token`). Env: `SIMPLE_PROXY_TOKEN`. |
| `--sha256 <hex>` | Override the expected hash (else the catalog hash is used). |
| `--require-hash` | Refuse to download when no hash is available to verify against. |
| `--health-timeout <s>` | Seconds to wait probing each proxy before falling back (default 5). |

> **Note on OS patches:** many Windows Update URLs are `http://`. The proxy is HTTPS-only by
> default, so those are rejected by the proxy and pulled via the direct fallback (SHA-1 verified).
> Start the proxy with `--allow-http` if you want OS patches cached through it too.

## Two ready-made samples

Two focused sample scripts show the same download strategy with different ways of obtaining the
proxy list. Both implement: **find which proxy already has the content → if none, try each proxy
from the top of the list until one succeeds → if none succeed, download direct** (then verify the
hash). They reuse the helpers in `download_patch.py`.

### `sample_autodiscover.py` — auto-discover, then download
Discovers proxies on the LAN (no addresses needed), then runs the strategy above.
```bash
python sample_autodiscover.py --signature 3039
python sample_autodiscover.py --signature 3039 --discovery-host 192.168.0.6   # targeted probe
python sample_autodiscover.py https://example.com/patch.msu --sha256 <hex>
```

### `sample_proxylist.py` — given a proxy list, then download
Takes an explicit, priority-ordered list of proxies (`--proxy`, repeatable).
```bash
python sample_proxylist.py --signature 3039 \
    --proxy http://192.168.0.6:8080 --proxy http://192.168.0.7:8080
python sample_proxylist.py https://example.com/patch.msu --sha256 <hex> \
    --proxy http://cache-a:8080 --proxy http://cache-b:8080 --token s3cret
```

Both print each proxy's cache status while choosing, e.g.:
```
[proxy] http://192.168.0.6:8080 has it CACHED
[plan] a proxy already has it cached -> http://192.168.0.6:8080
...
    via: proxy (cache HIT @ http://192.168.0.6:8080)   sha256: ... (verified)
```
and fall back through the list (`not usable; skipping` → next) and finally to a direct download.

## Files

| File | Role |
|---|---|
| `download_patch.py` | CLI front end (argument parsing only); uses the two libraries below |
| `catalog_resolve.py` | **library** — resolve a signature / package / url to a download target |
| `downloader.py` | **library** — discovery, proxy selection, downloading, hashing/verification |
| `sample_autodiscover.py` | sample: auto-discover proxies, then download |
| `sample_proxylist.py` | sample: given a proxy list, then download |

## Reusing the logic

The two libraries are importable — `catalog_resolve` (obtaining the target) and `downloader`
(getting the bytes):

```python
import catalog_resolve as cr
import downloader as dl

# 1) Resolve a signature / package / url -> {url, hash, hash_algo, resolved}
target = cr.resolve_target(signature=3128)          # or package="<uuid>", or url="https://..."

# 2) Find which proxy already has it (cached first, then list order; unreachable dropped)
ordered, any_cached = dl.rank_proxies(["http://192.168.0.6:8080"], target["url"], None, 5)

# 3) Try each proxy in order, else direct — verified against the catalog hash
result = dl.download_ordered(target["url"], "winrar.exe", ordered,
                             expected_hash=target["hash"], hash_algo=target["hash_algo"])
print(result["method"], result["proxy"], result["cache"], result["hash"])

# convenience wrapper (rank + download in one call):
result = dl.fetch(target["url"], "winrar.exe", proxies=["http://192.168.0.6:8080"],
                  expected_hash=target["hash"], hash_algo=target["hash_algo"])

# discover proxies on the LAN yourself
for p in dl.discover_proxies():
    print(p["base_url"], "secure=", p["secure"])
```

## See also
- [../simple-proxy](../simple-proxy) — the proxy itself (build, run, security model, C ABI).
- [../catalog-lookup](../catalog-lookup) — inspect signatures, packages, patches in the catalog.

_Created by Chris Seiler — OPSWAT OEM Field CTO_
