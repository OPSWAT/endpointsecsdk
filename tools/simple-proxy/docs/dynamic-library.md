# Using the simple-proxy dynamic library

This guide shows how to embed **simple-proxy** in an existing process by loading its dynamic
library and driving the C ABI. The library starts the proxy on its own background thread + async
runtime, so your process keeps running normally; you just get a local caching download endpoint.

- Header: [`include/simple_proxy.h`](../include/simple_proxy.h) (also copied into [`bin/`](../bin))
- Library file per platform:

| Platform | Library to load | Link/import artifact |
|---|---|---|
| Windows | `simple_proxy.dll` | `simple_proxy.dll.lib` (import library) |
| Linux | `libsimple_proxy.so` | link with `-lsimple_proxy` |
| macOS | `libsimple_proxy.dylib` | link with `-lsimple_proxy` |

Build them with [`build.ps1`](../build.ps1) (Windows) or [`build.sh`](../build.sh) (macOS/Linux);
artifacts land in `bin/`.

> **Reference implementations of loading this library:** the bundled `simple-proxy-demo`
> executable ([`src/main.rs`](../src/main.rs)) loads it from Rust via `libloading` and calls
> `simple_proxy_start_json`; [`examples/embed_ctypes.py`](../examples/embed_ctypes.py) does the
> same from Python via `ctypes`. Both are runnable and exercise the exact `.dll`/`.so`/`.dylib`
> an external process would.

## The C ABI

```c
// Returns a static version string, e.g. "simple-proxy 0.1.0". Do not free.
const char *simple_proxy_version(void);

// Start the proxy on a background thread. Returns an opaque handle, or NULL on failure.
void *simple_proxy_start(const char *bind,              // e.g. "127.0.0.1"; NULL -> "127.0.0.1"
                         uint16_t    port,              // e.g. 8080
                         const char *cache_dir,         // NULL -> "simple-proxy-cache"
                         const char *token,             // init token; NULL/empty -> auth disabled
                         uint64_t    max_cache_bytes,   // 0 -> unbounded
                         uint64_t    ttl_seconds,       // 0 -> no expiry
                         uint64_t    max_download_bytes // 0 -> unlimited
);

// Graceful shutdown: signal, join the thread, free the handle. NULL-safe; never call twice.
void simple_proxy_stop(void *handle);

// Like simple_proxy_start but takes the FULL config as a JSON object string, exposing every
// option (allow_domains, allow_http, open_firewall, upstream_timeout_seconds, discovery,
// discovery_port, log_file, log_max_size, ...). Missing fields use defaults. This is what the
// bundled demo executable uses.
//   {"bind":"127.0.0.1","port":8080,"cache_dir":"cache","token":"my-secret-token",
//    "max_cache_size":5000000000,"ttl_seconds":86400,"max_download":2000000000,
//    "allow_domains":["microsoft.com"]}
void *simple_proxy_start_json(const char *config_json);
```

### Semantics & guarantees
- **Non-blocking.** `simple_proxy_start` spawns a thread and returns immediately. Give the listener
  a brief moment (tens of ms) before the first request, or just retry the first connection.
- **Secure mode.** Whatever you pass as `token` becomes the **initialization token**; every request
  must then present it as `X-Proxy-Token: <token>` **or** `Authorization: Bearer <token>`. Pass
  `NULL`/empty to disable auth (only sensible for loopback).
- **Secure defaults.** Embedded instances are HTTPS-only, keep SSRF protection on (private/loopback
  targets refused), and never touch the firewall.
- **Lifecycle.** The handle is owned by your process. Call `simple_proxy_stop` exactly once to shut
  down and reclaim it. It is safe to call with `NULL`.
- **Panic-safe.** The FFI never unwinds across the boundary; failures return `NULL` (start) or log
  to stderr.
- **Threads.** One proxy per handle; you may start several on different ports if needed.

### Then make requests
Once started, your process (or any other) issues normal HTTP GETs to the local port:

```
GET http://<bind>:<port>/download?url=<percent-encoded-url>      -> content (streamed)
GET http://<bind>:<port>/cached?url=<percent-encoded-url>        -> {"cached":true|false,...}
GET http://<bind>:<port>/health                                  -> stats + {"secure_mode":bool}
```
sending header `X-Proxy-Token: <token>` when secure mode is on.

---

## C / C++

```c
#include "simple_proxy.h"
#include <stdio.h>

int main(void) {
    printf("%s\n", simple_proxy_version());

    void *proxy = simple_proxy_start(
        "127.0.0.1", 8080, "cache",
        "my-secret-token",   // init token; clients must send it back
        5000000000ULL,       // 5 GB cache
        86400ULL,            // 24h TTL
        0ULL);               // no per-download cap
    if (!proxy) {
        fprintf(stderr, "failed to start proxy\n");
        return 1;
    }

    // ... your process runs; issue GETs to http://127.0.0.1:8080/download?url=...
    //     with header "X-Proxy-Token: my-secret-token" ...

    simple_proxy_stop(proxy);
    return 0;
}
```

Compile/link:
```bash
# Linux
cc app.c -I path/to/include -L path/to/bin -lsimple_proxy -o app
LD_LIBRARY_PATH=path/to/bin ./app

# macOS
cc app.c -I path/to/include -L path/to/bin -lsimple_proxy -o app
DYLD_LIBRARY_PATH=path/to/bin ./app
```
```bat
REM Windows (MSVC): link the import library; keep simple_proxy.dll next to the exe
cl app.c /I path\to\include /link path\to\bin\simple_proxy.dll.lib
```

---

## C# (.NET, P/Invoke)

Relevant if you're embedding from the existing C# tooling. Put `simple_proxy.dll` next to your
executable (or on the DLL search path).

```csharp
using System;
using System.Runtime.InteropServices;

internal static class SimpleProxy
{
    private const string Lib = "simple_proxy"; // resolves simple_proxy.dll / .so / .dylib

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr simple_proxy_version();

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern IntPtr simple_proxy_start(
        string bind, ushort port, string cacheDir, string token,
        ulong maxCacheBytes, ulong ttlSeconds, ulong maxDownloadBytes);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void simple_proxy_stop(IntPtr handle);

    public static string Version() =>
        Marshal.PtrToStringAnsi(simple_proxy_version());

    public static IntPtr Start(string bind, ushort port, string cacheDir, string token,
                               ulong maxCache, ulong ttlSeconds, ulong maxDownload) =>
        simple_proxy_start(bind, port, cacheDir, token, maxCache, ttlSeconds, maxDownload);

    public static void Stop(IntPtr handle) => simple_proxy_stop(handle);
}

class Program
{
    static void Main()
    {
        Console.WriteLine(SimpleProxy.Version());

        IntPtr proxy = SimpleProxy.Start("127.0.0.1", 8080, "cache",
            "my-secret-token", 5_000_000_000UL, 86_400UL, 0UL);
        if (proxy == IntPtr.Zero) { Console.Error.WriteLine("start failed"); return; }

        try
        {
            System.Threading.Thread.Sleep(500); // let the listener come up

            using var http = new System.Net.Http.HttpClient();
            http.DefaultRequestHeaders.Add("X-Proxy-Token", "my-secret-token");
            string url = Uri.EscapeDataString("https://download.example.com/patch.msu");
            var bytes = http.GetByteArrayAsync(
                $"http://127.0.0.1:8080/download?url={url}").Result;
            Console.WriteLine($"downloaded {bytes.Length} bytes");
        }
        finally
        {
            SimpleProxy.Stop(proxy);
        }
    }
}
```

> On Windows, `[DllImport("simple_proxy")]` resolves `simple_proxy.dll`. On Linux/macOS the same
> attribute resolves `libsimple_proxy.so` / `.dylib` when it's on the loader path.

---

## Python (ctypes)

A complete, runnable version ships as [`examples/embed_ctypes.py`](../examples/embed_ctypes.py).
The essence:

```python
import ctypes, platform, os

name = {"Windows": "simple_proxy.dll",
        "Linux":   "libsimple_proxy.so",
        "Darwin":  "libsimple_proxy.dylib"}[platform.system()]
lib = ctypes.CDLL(os.path.join("bin", name))

lib.simple_proxy_version.restype = ctypes.c_char_p
lib.simple_proxy_start.restype = ctypes.c_void_p
lib.simple_proxy_start.argtypes = [ctypes.c_char_p, ctypes.c_uint16, ctypes.c_char_p,
                                   ctypes.c_char_p, ctypes.c_uint64, ctypes.c_uint64,
                                   ctypes.c_uint64]
lib.simple_proxy_stop.argtypes = [ctypes.c_void_p]

h = lib.simple_proxy_start(b"127.0.0.1", 8080, b"cache", b"my-secret-token",
                           5_000_000_000, 86_400, 0)
try:
    ...  # GET http://127.0.0.1:8080/download?url=...  with header X-Proxy-Token
finally:
    lib.simple_proxy_stop(h)
```

Run the shipped example:
```bash
python examples/embed_ctypes.py https://download.example.com/patch.msu
```

---

## Troubleshooting

| Symptom | Likely cause / fix |
|---|---|
| `simple_proxy_start` returns NULL | Bad `bind` address, or the thread/runtime failed to start. Check stderr. |
| First request refused | Listener not up yet — wait briefly and retry the first connection. |
| Every request `401` | Secure mode is on; send `X-Proxy-Token` (or `Authorization: Bearer`) matching the init token. |
| Upstream `403` from the proxy | SSRF/scheme block (private target, or `http://`). Embedded mode is HTTPS-only to public hosts. |
| DLL not found (Windows) | Keep `simple_proxy.dll` beside your exe or on `PATH`. |
| `.so`/`.dylib` not found | Put it on `LD_LIBRARY_PATH` / `DYLD_LIBRARY_PATH`, or install to a standard lib dir. |

_Created by Chris Seiler — OPSWAT OEM Field CTO_
