# simple-proxy

A small, **secure, cross-platform caching download proxy** for patch content, written in Rust.

Point your downloader at simple-proxy instead of directly at the internet. The proxy fetches each
URL **once**, streams it back to you while caching it to disk, and serves every later request for
that same URL straight from the local cache. This lets several machines (or repeated runs) share a
single download of large patch/installer content instead of each pulling it over the WAN.

```
             ┌───────────────┐        first time         ┌──────────────┐
 downloader ─┤  simple-proxy  ├── HEAD + GET (tee) ──────▶│  internet     │
   (you)    ◀┤   :8080 cache  │◀── stream + write ───────┤  (patch host) │
             └───────────────┘                            └──────────────┘
             │  later: served from cache, no upstream call │
```

It ships **two ways to run**:
- a **dynamic library** (`simple_proxy.dll` / `libsimple_proxy.so` / `libsimple_proxy.dylib`) with
  a small **C ABI**, so an existing process can embed the proxy and drive it directly, and
- a **demo executable** (`simple-proxy-demo`) for standalone use / testing — which itself
  **loads the dynamic library at runtime** (via `dlopen`/`LoadLibrary`) and drives the same C ABI,
  so it doubles as a working example of embedding. It therefore requires the library to be present
  next to it (it is, in [`bin/`](bin)) or pointed at with `--lib`.

## Endpoints

| Method & path | Purpose |
|---|---|
| `GET /download?url=<url>` | Return the content for `url`. Cache hit → served from disk. Cache miss → fetched upstream and streamed back to you **while** being written to the cache (the connection is kept alive for the whole initial download). |
| `GET /cached?url=<url>` | Ask whether `url` is currently cached. Returns `{"cached":true,"size":...,"content_type":...,"cached_at_epoch":...}` or `{"cached":false}`. No download is performed. |
| `GET /health` | Liveness + cache stats (entry count, bytes used, limits, whether secure mode is on). |

`url` must be percent-encoded when it contains query strings of its own.

### Examples
```bash
# Is it cached yet?
curl "http://127.0.0.1:8080/cached?url=https%3A%2F%2Fdownload.microsoft.com%2F...%2Fpatch.msu"

# Download (streamed; cached for next time)
curl -o patch.msu "http://127.0.0.1:8080/download?url=https%3A%2F%2Fdownload.microsoft.com%2F...%2Fpatch.msu"

# Secure mode — pass the token in a header (either form works)
curl -H "X-Proxy-Token: $TOKEN"        -o patch.msu "http://127.0.0.1:8080/download?url=https%3A%2F%2F..."
curl -H "Authorization: Bearer $TOKEN" -o patch.msu "http://127.0.0.1:8080/download?url=https%3A%2F%2F..."
```

There are ready-made Python examples too — see [Examples](#examples) below.

## Secure token mode

Set a token and the proxy runs in **secure mode**: every request must present that exact token in
a header, or it's rejected with `401`.

- On the executable: `--token <value>`.
- Embedded via the library: the `token` argument to `simple_proxy_start(...)` — the **configured
  initialization token**.
- Clients send it as **`X-Proxy-Token: <token>`** *or* **`Authorization: Bearer <token>`**.
- The comparison does not short-circuit on content (constant-time byte compare).

When no token is configured, secure mode is off and all requests are allowed (fine for a
loopback-only dev setup; not recommended when bound to `0.0.0.0`).

## Build

Requires a recent Rust toolchain (install from <https://rustup.rs>) and a C linker (MSVC build
tools on Windows, `build-essential` on Linux, Xcode CLT on macOS). Built & verified with Rust
1.97.

Use the build script for your platform — it compiles release artifacts and stages them into
[`bin/`](bin):

```powershell
# Windows
./build.ps1
```
```bash
# macOS / Linux
./build.sh
```

Or invoke cargo directly:
```bash
cargo build --release      # produces target/release/{simple-proxy-demo, simple_proxy.<dll|so|dylib>}
```

### `bin/` (checked in)
The compiled artifacts are committed under `bin/` (peer to `src/`) so consumers don't have to
build from source. Per platform:

| Platform | Executable | Dynamic library | Extras |
|---|---|---|---|
| Windows | `simple-proxy-demo.exe` | `simple_proxy.dll` | `simple_proxy.dll.lib` (import lib), `simple_proxy.h` |
| Linux | `simple-proxy-demo` | `libsimple_proxy.so` | `simple_proxy.h` |
| macOS | `simple-proxy-demo` | `libsimple_proxy.dylib` | `simple_proxy.h` |

Run the build script on each target OS to populate its binaries (cross-OS binaries can't be
produced from one host without a cross toolchain).

## Run (executable)

The demo loads `simple_proxy.<dll|so|dylib>` at startup — keep it next to the binary (as in
`bin/`) or pass `--lib <path>`. It forwards all options below to the library via
`simple_proxy_start_json`.

```bash
# Loopback only, 5 GB cache, 24h TTL (all defaults)
bin/simple-proxy-demo

# Expose on the LAN, require a token, open the firewall port, restrict to Microsoft hosts
bin/simple-proxy-demo --bind 0.0.0.0 --port 8080 \
             --token "$(openssl rand -hex 32)" \
             --allow-domain microsoft.com --allow-domain windowsupdate.com \
             --open-firewall \
             --cache-dir /var/cache/simple-proxy \
             --max-cache-size 50GB --ttl 7d --max-download 5GB
```

### Options

| Flag | Default | Meaning |
|---|---|---|
| `--bind <ip>` | `127.0.0.1` | Address to listen on. Loopback by default; use `0.0.0.0` to expose. |
| `--port <n>` | `8080` | TCP port. |
| `--cache-dir <path>` | `simple-proxy-cache` | Where cached content + `index.json` live. |
| `--max-cache-size <size>` | `5GB` | Total cache ceiling. When exceeded, **oldest** entries are evicted first. `0` = unbounded. Accepts `MB/GB/MiB/GiB/...`. |
| `--ttl <dur>` | `24h` | Time-to-live per entry. Expired entries are removed on access and at startup. Accepts `s/m/h/d`. `0` = no expiry. |
| `--max-download <size>` | `2GB` | Reject a single download larger than this (checked via HEAD up front and enforced during streaming). |
| `--token <str>` | *(none)* | Enable secure mode; require this token via `X-Proxy-Token` or `Authorization: Bearer`. |
| `--allow-http` | off | Permit `http://` upstreams. HTTPS-only otherwise. |
| `--allow-domain <suffix>` | *(any)* | Repeatable host allow-list (suffix match, e.g. `microsoft.com` matches `download.microsoft.com`). |
| `--allow-private` | off | Permit upstreams that resolve to private/loopback/link-local IPs. **Leave off** except for trusted internal testing. |
| `--open-firewall` | off | Add an inbound firewall rule for the port on start, remove it on exit. Needs admin/root. |
| `--upstream-timeout-seconds <n>` | `60` | Connect/read timeout to the upstream server. |
| `--discovery` | off | Enable the UDP self-discovery responder (LAN clients can find this proxy). |
| `--discovery-port <n>` | `8099` | UDP port for discovery probes/replies (broadcast + multicast). |
| `--log-file <path>` | *(console)* | Write all logs to this file instead of the console. |
| `--log-max-size <size>` | `10MB` | Max log-file size before rotating to `<path>.1`. Accepts `MB/GB/...`. |
| `--lib <path>` | *(next to exe)* | Path to the dynamic library the demo loads. Demo-only; not passed to the library. |

## Embed (dynamic library / C ABI)

An existing process can load the dynamic library and run the proxy in-process.

> **Full integration guide (C, C#, Python + linking/troubleshooting):**
> [`docs/dynamic-library.md`](docs/dynamic-library.md).

The header is [`include/simple_proxy.h`](include/simple_proxy.h) (also copied into `bin/`):

```c
void *simple_proxy_start(const char *bind, uint16_t port, const char *cache_dir,
                         const char *token, uint64_t max_cache_bytes,
                         uint64_t ttl_seconds, uint64_t max_download_bytes);
void *simple_proxy_start_json(const char *config_json); // full config as JSON
void  simple_proxy_stop(void *handle);
const char *simple_proxy_version(void);
```

The bundled `simple-proxy-demo` executable is itself a working Rust example of this: it loads the
library with `libloading` and calls `simple_proxy_start_json` — see [`src/main.rs`](src/main.rs).

`simple_proxy_start` launches the proxy on its own background thread + runtime and returns an
opaque handle (or `NULL` on failure). The `token` you pass is the initialization token clients
must then present. `simple_proxy_stop` triggers a graceful shutdown, joins the thread, and frees
the handle. Both are panic-safe and null-tolerant. Embedded instances use secure defaults
(HTTPS-only, SSRF protection on, no firewall changes).

Linking:
- **Windows** — link against `simple_proxy.dll.lib` and ship `simple_proxy.dll`.
- **Linux/macOS** — link `-lsimple_proxy` with the `.so`/`.dylib` on the loader path.

A runnable demonstration (via Python `ctypes`, no build step) is in
[`examples/embed_ctypes.py`](examples/embed_ctypes.py).

## Examples

| Script | What it shows |
|---|---|
| [`examples/proxy_download.py`](examples/proxy_download.py) | Download a URL (passed as a parameter) **through** a running proxy; reports cache HIT/MISS. Supports `--token`. |
| [`examples/embed_ctypes.py`](examples/embed_ctypes.py) | **Embed** the proxy by loading the dynamic library via `ctypes`, download through it, then stop it. |
| [`../simple-proxy-patch/`](../simple-proxy-patch) | Sample **patch downloader** that prefers the proxy and **falls back to a direct download** when the proxy is unavailable. |

```bash
# Download through a running proxy (secure mode)
python examples/proxy_download.py https://download.example.com/patch.msu --token "$TOKEN"

# Embed the library and download through it
python examples/embed_ctypes.py https://download.example.com/patch.msu
```

## How caching works

- **Key** — SHA-256 of the exact request URL. Each object is stored as a file named by that hash;
  metadata is tracked in `index.json`.
- **HEAD-before-GET** — before fetching, the proxy issues a HEAD to learn `Content-Length`. This
  is used to (a) reject downloads over `--max-download` and (b) evict oldest entries up front so
  there is room ("how much cache to fill up"). Servers that don't support HEAD still work — the
  size limit is also enforced while streaming.
- **Tee download** — on a miss, bytes are streamed to *you* and written to a temp `.part` file at
  the same time, so you never wait for the whole file before your transfer starts, and the
  connection stays alive throughout. The temp file is committed to the cache atomically only on a
  clean, complete transfer; interrupted downloads are discarded (never cached partially).
- **TTL + size eviction** — entries older than `--ttl` are dropped on access and at startup. When
  a new object would push the cache over `--max-cache-size`, the **oldest** entries are removed
  until it fits.

## Logging

By default the proxy logs to the **console**. Pass `--log-file <path>` to send **all** log output
to a file instead. The file is size-capped at `--log-max-size` (default **10 MB**): when the next
write would exceed the cap, the current file is rotated to `<path>.1` (replacing any previous
`.1`) and a fresh file is started, so the active log never exceeds the cap (at most ~2× the cap is
kept on disk, counting the one rotated backup). Log verbosity honours the `RUST_LOG` environment
variable (e.g. `RUST_LOG=simple_proxy=debug`); the default is `info`.

```bash
bin/simple-proxy-demo --log-file /var/log/simple-proxy.log            # 10 MB cap (default)
bin/simple-proxy-demo --log-file proxy.log --log-max-size 50MB        # custom cap
```

## Concurrency

The server is async (tokio multi-threaded runtime) with no artificial request cap, and comfortably
handles **50+ simultaneous downloads**. Each transfer streams independently; the cache lock is held
only for brief bookkeeping, never during a download. Concurrent downloads of the *same* URL each
use a distinct temp file and commit atomically, so they never corrupt one another.

## Self-discovery (LAN)

With `--discovery`, the proxy answers UDP discovery probes so clients don't need to be pre-configured
with its address. It listens on `--discovery-port` (default `8099`) for the probe
`SIMPLE-PROXY-DISCOVER v1` sent via **broadcast** (`255.255.255.255`) or **multicast**
(`239.255.42.98`, the modern equivalent), and replies with:

```json
{"service":"simple-proxy","version":"0.1.0","proxy_port":8080,"scheme":"http","secure":true}
```

The client learns the proxy's address (the reply's source IP) + `proxy_port`, and whether a token
is required (`secure`). **The token itself is never sent** — discovery works both with and without a
security token; it only advertises that one is needed. Discovery is only useful when the proxy is
exposed (`--bind 0.0.0.0`); it warns if enabled while bound to loopback. The patch sample
([`../simple-proxy-patch`](../simple-proxy-patch)) can auto-discover via `--discover`.

## Security model

Designed to be safe to run as a shared download point.

- **Secure token mode.** `--token` / the init token gates every endpoint (via `X-Proxy-Token` or
  `Authorization: Bearer`); compared in constant time.
- **HTTPS-only by default.** Plain `http://` requires `--allow-http`.
- **SSRF protection (enforced at connect time).** A custom DNS resolver checks every resolved
  address the moment a connection is made and refuses loopback, private (RFC 1918), link-local,
  CGNAT (100.64/10), unspecified, multicast, and IPv4-mapped-IPv6 equivalents. Because the address
  that is checked is the exact address connected to — for the initial request **and every redirect
  hop** — this is robust against DNS rebinding (TOCTOU) and against redirects to a hostname that
  resolves to an internal address. Redirects are additionally scheme/allow-list checked and capped
  at 5 hops.
- **Host allow-list.** `--allow-domain` restricts what can be fetched to named suffixes — the
  strongest control; use it in production.
- **Loopback by default.** You must opt in to `--bind 0.0.0.0`; doing so without a token logs a
  warning.
- **Size caps.** `--max-download` bounds any single fetch; `--max-cache-size` bounds total disk.
- **No partial poisoning.** Only fully-transferred content is cached.
- **Firewall lifecycle.** `--open-firewall` opens the TCP proxy port on start (and the UDP
  discovery port when `--discovery` is set) and **closes them on exit** (Ctrl-C / SIGTERM handled
  for graceful shutdown).

### Firewall support
| OS | Mechanism |
|---|---|
| Windows | `netsh advfirewall firewall add/delete rule` (rule name `simple-proxy-<port>`) |
| Linux | `firewall-cmd` (firewalld) if present, else `ufw` |
| macOS | Application firewall is app-based, not port-based — a note is logged; allow the binary in System Settings if prompted. |

## Files
| Path | Role |
|---|---|
| `src/lib.rs` | Library root; module wiring + re-exports |
| `src/server.rs` | axum server, endpoints, request auth, tee-download + cache serving, `run()` |
| `src/main.rs` | demo executable (`simple-proxy-demo`) — loads the dynamic library at runtime and drives its C ABI |
| `src/ffi.rs` | C ABI (`simple_proxy_start`/`_start_json`/`_stop`/`_version`) for embedding |
| `src/config.rs` | CLI/config (clap) + size/duration parsing |
| `src/security.rs` | URL validation, connect-time SSRF resolver, allow-list, token auth |
| `src/cache.rs` | TTL + size-bounded cache with oldest-first eviction and persisted index |
| `src/discovery.rs` | UDP self-discovery responder (broadcast + multicast) |
| `src/logging.rs` | console / size-capped rotating file logging |
| `src/firewall.rs` | per-OS firewall open/close |
| `include/simple_proxy.h` | C header for embedders |
| `docs/dynamic-library.md` | full embedding guide (C, C#, Python + linking/troubleshooting) |
| `bin/` | committed compiled artifacts (exe + dynamic library + header) |
| `build.ps1` / `build.sh` | build + stage into `bin/` (Windows / macOS+Linux) |
| `examples/` | Python examples (proxy download, ctypes embedding) |

_Created by Chris Seiler — OPSWAT OEM Field CTO_
