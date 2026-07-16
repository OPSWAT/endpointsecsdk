/*
 * simple_proxy.h — C ABI for embedding the simple-proxy caching download proxy.
 *
 * Link against the dynamic library:
 *   Windows : simple_proxy.dll   (+ simple_proxy.dll.lib import library)
 *   Linux   : libsimple_proxy.so
 *   macOS   : libsimple_proxy.dylib
 *
 * Typical use from a host process:
 *
 *   void* h = simple_proxy_start("127.0.0.1", 8080, "cache", "my-secret-token",
 *                                5000000000ULL, 86400ULL, 2000000000ULL);
 *   // ... issue GET http://127.0.0.1:8080/download?url=... with header
 *   //     "X-Proxy-Token: my-secret-token" (or "Authorization: Bearer my-secret-token")
 *   simple_proxy_stop(h);
 *
 * All functions are panic-safe and null-tolerant.
 *
 * Created by Chris Seiler — OPSWAT OEM Field CTO
 */
#ifndef SIMPLE_PROXY_H
#define SIMPLE_PROXY_H

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

/*
 * Returns a static, NUL-terminated version string (e.g. "simple-proxy 0.1.0").
 * Do NOT free the returned pointer.
 */
const char *simple_proxy_version(void);

/*
 * Start the proxy on a background thread. Returns an opaque handle, or NULL on failure.
 *
 *   bind               Listen address, e.g. "127.0.0.1" or "0.0.0.0". NULL -> "127.0.0.1".
 *   port               TCP port (e.g. 8080).
 *   cache_dir          Cache directory path. NULL -> "simple-proxy-cache".
 *   token              Secure-mode initialization token. Requests must present it via the
 *                      "X-Proxy-Token" or "Authorization: Bearer" header. NULL/empty -> auth off.
 *   max_cache_bytes    Total cache ceiling in bytes; 0 -> unbounded.
 *   ttl_seconds        Entry time-to-live in seconds; 0 -> no expiry.
 *   max_download_bytes Per-download cap in bytes; 0 -> unlimited.
 *
 * Embedded instances use secure defaults for everything else: HTTPS-only, SSRF protection ON,
 * no firewall modification. Release the handle with simple_proxy_stop().
 */
void *simple_proxy_start(const char *bind,
                         uint16_t port,
                         const char *cache_dir,
                         const char *token,
                         uint64_t max_cache_bytes,
                         uint64_t ttl_seconds,
                         uint64_t max_download_bytes);

/*
 * Like simple_proxy_start(), but takes the full configuration as a JSON object string, giving
 * control over every option (bind, port, cache_dir, max_cache_size, ttl_seconds, max_download,
 * token, allow_http, allow_domains, allow_private, open_firewall, upstream_timeout_seconds,
 * discovery, discovery_port, log_file, log_max_size). Missing fields fall back to defaults.
 * Returns an opaque handle, or NULL on failure.
 *
 * Example JSON:
 *   {"bind":"127.0.0.1","port":8080,"cache_dir":"cache","token":"my-secret-token",
 *    "max_cache_size":5000000000,"ttl_seconds":86400,"max_download":2000000000,
 *    "allow_domains":["microsoft.com"]}
 */
void *simple_proxy_start_json(const char *config_json);

/*
 * Signal a graceful shutdown, join the proxy thread, and free the handle.
 * Safe to call with NULL (no-op). Never call twice on the same non-NULL handle.
 */
void simple_proxy_stop(void *handle);

#ifdef __cplusplus
} /* extern "C" */
#endif

#endif /* SIMPLE_PROXY_H */
