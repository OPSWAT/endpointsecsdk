//! C ABI for embedding the proxy in an existing process via the dynamic library
//! (`simple_proxy.dll` / `libsimple_proxy.so` / `libsimple_proxy.dylib`).
//!
//! The host process calls [`simple_proxy_start`] with an **initialization token**; the proxy then
//! runs on its own background thread + Tokio runtime and requires that same token (via the
//! `X-Proxy-Token` or `Authorization: Bearer` header) on every request. [`simple_proxy_stop`]
//! signals a graceful shutdown and joins the thread. See `include/simple_proxy.h`.
//!
//! All functions are panic-safe (they never unwind across the FFI boundary) and null-tolerant.
//!
//! Created by Chris Seiler — OPSWAT OEM Field CTO

use std::ffi::{c_char, c_void, CStr};
use std::net::IpAddr;
use std::path::PathBuf;

use crate::config::Config;

/// Opaque handle returned by [`simple_proxy_start`] and freed by [`simple_proxy_stop`].
pub struct ProxyHandle {
    shutdown: Option<tokio::sync::oneshot::Sender<()>>,
    thread: Option<std::thread::JoinHandle<()>>,
}

/// Convert a C string into an owned `String`, treating null/empty as `None`.
///
/// # Safety
/// `ptr` must be null or a valid NUL-terminated C string.
unsafe fn cstr_opt(ptr: *const c_char) -> Option<String> {
    if ptr.is_null() {
        return None;
    }
    match CStr::from_ptr(ptr).to_str() {
        Ok(s) if !s.is_empty() => Some(s.to_string()),
        _ => None,
    }
}

/// Returns a static version string, e.g. `"simple-proxy 0.1.0"`. Do not free.
#[no_mangle]
pub extern "C" fn simple_proxy_version() -> *const c_char {
    concat!("simple-proxy ", env!("CARGO_PKG_VERSION"), "\0").as_ptr() as *const c_char
}

/// Spawn the proxy on a background thread + runtime for a fully-formed `Config`, returning an
/// opaque handle (or NULL on failure). Shared by both start entry points.
fn start_with_config(cfg: Config) -> *mut c_void {
    // Console by default, or the size-capped log file if configured (idempotent).
    crate::logging::init(&cfg);

    let (tx, rx) = tokio::sync::oneshot::channel::<()>();
    let thread = std::thread::Builder::new()
        .name("simple-proxy".to_string())
        .spawn(move || {
            let rt = match tokio::runtime::Builder::new_multi_thread()
                .enable_all()
                .build()
            {
                Ok(rt) => rt,
                Err(e) => {
                    eprintln!("simple-proxy: failed to create runtime: {e}");
                    return;
                }
            };
            rt.block_on(async move {
                let shutdown = async move {
                    let _ = rx.await;
                };
                if let Err(e) = crate::server::run(cfg, shutdown).await {
                    eprintln!("simple-proxy: server error: {e}");
                }
            });
        });

    match thread {
        Ok(thread) => {
            let handle = Box::new(ProxyHandle {
                shutdown: Some(tx),
                thread: Some(thread),
            });
            Box::into_raw(handle) as *mut c_void
        }
        Err(e) => {
            eprintln!("simple-proxy: failed to spawn thread: {e}");
            std::ptr::null_mut()
        }
    }
}

/// Start the proxy on a background thread and return an opaque handle (NULL on failure).
///
/// Parameters (all size values are bytes):
///  * `bind`               listen address, e.g. "127.0.0.1" or "0.0.0.0". NULL → "127.0.0.1".
///  * `port`               TCP port (e.g. 8080).
///  * `cache_dir`          cache directory. NULL → "simple-proxy-cache".
///  * `token`              secure-mode initialization token. NULL/empty → auth disabled.
///  * `max_cache_bytes`    total cache ceiling; 0 → unbounded.
///  * `ttl_seconds`        entry TTL; 0 → no expiry.
///  * `max_download_bytes` per-download cap; 0 → unlimited.
///
/// Embedded instances use secure defaults for everything else (HTTPS-only, SSRF protection on,
/// no firewall changes). The returned handle must be released with [`simple_proxy_stop`].
///
/// # Safety
/// The string pointers must each be NULL or a valid NUL-terminated C string, valid for the
/// duration of the call.
#[no_mangle]
pub unsafe extern "C" fn simple_proxy_start(
    bind: *const c_char,
    port: u16,
    cache_dir: *const c_char,
    token: *const c_char,
    max_cache_bytes: u64,
    ttl_seconds: u64,
    max_download_bytes: u64,
) -> *mut c_void {
    let result = std::panic::catch_unwind(|| {
        let bind_str = cstr_opt(bind).unwrap_or_else(|| "127.0.0.1".to_string());
        let bind_ip: IpAddr = match bind_str.parse() {
            Ok(ip) => ip,
            Err(_) => {
                eprintln!("simple-proxy: invalid bind address '{bind_str}'");
                return std::ptr::null_mut();
            }
        };
        let cache_dir =
            cstr_opt(cache_dir).unwrap_or_else(|| "simple-proxy-cache".to_string());
        let token = cstr_opt(token);

        let cfg = Config {
            bind: bind_ip,
            port,
            cache_dir: PathBuf::from(cache_dir),
            max_cache_size: max_cache_bytes,
            ttl_seconds,
            max_download: if max_download_bytes == 0 {
                u64::MAX
            } else {
                max_download_bytes
            },
            token,
            allow_http: false,
            allow_domains: Vec::new(),
            allow_private: false,
            open_firewall: false,
            upstream_timeout_seconds: 60,
            discovery: false,
            discovery_port: 8099,
            log_file: None,
            log_max_size: 10_000_000,
        };

        start_with_config(cfg)
    });

    result.unwrap_or_else(|_| {
        eprintln!("simple-proxy: panic during start");
        std::ptr::null_mut()
    })
}

/// Start the proxy from a JSON-serialized `Config`, giving full control over every option
/// (allow-domain, allow-http, open-firewall, upstream timeout, ...) — not just the subset
/// [`simple_proxy_start`] exposes. Missing JSON fields fall back to the built-in defaults.
/// Returns an opaque handle (NULL on failure); release with [`simple_proxy_stop`].
///
/// This is what the bundled demo executable uses to drive the library.
///
/// # Safety
/// `config_json` must be NULL or a valid NUL-terminated UTF-8 C string containing a JSON object.
#[no_mangle]
pub unsafe extern "C" fn simple_proxy_start_json(config_json: *const c_char) -> *mut c_void {
    let result = std::panic::catch_unwind(|| {
        let json = match cstr_opt(config_json) {
            Some(s) => s,
            None => {
                eprintln!("simple-proxy: null/empty config JSON");
                return std::ptr::null_mut();
            }
        };
        let cfg: Config = match serde_json::from_str(&json) {
            Ok(c) => c,
            Err(e) => {
                eprintln!("simple-proxy: invalid config JSON: {e}");
                return std::ptr::null_mut();
            }
        };
        start_with_config(cfg)
    });

    result.unwrap_or_else(|_| {
        eprintln!("simple-proxy: panic during start_json");
        std::ptr::null_mut()
    })
}

/// Signal a graceful shutdown, join the proxy thread, and free the handle. Safe to call with NULL
/// (no-op) but never call twice on the same non-NULL handle.
///
/// # Safety
/// `handle` must be NULL or a handle previously returned by [`simple_proxy_start`] and not yet
/// passed to this function.
#[no_mangle]
pub unsafe extern "C" fn simple_proxy_stop(handle: *mut c_void) {
    if handle.is_null() {
        return;
    }
    let _ = std::panic::catch_unwind(|| {
        let mut h = Box::from_raw(handle as *mut ProxyHandle);
        if let Some(tx) = h.shutdown.take() {
            let _ = tx.send(());
        }
        if let Some(t) = h.thread.take() {
            let _ = t.join();
        }
    });
}
