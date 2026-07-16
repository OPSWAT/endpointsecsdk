//! simple-proxy demo executable.
//!
//! This binary demonstrates **consuming the dynamic library**: rather than linking the proxy in
//! statically, it loads `simple_proxy.dll` / `libsimple_proxy.so` / `libsimple_proxy.dylib` at
//! runtime (the same way an external process would) and drives its C ABI —
//! `simple_proxy_start_json(...)` to start and `simple_proxy_stop(...)` to shut down.
//!
//! It parses the normal CLI options, serializes them to JSON, and hands that to the library so the
//! embedded proxy runs with the exact configuration you asked for. By default it loads the library
//! sitting next to this executable (i.e. in `bin/`); override with `--lib <path>`.
//!
//! Created by Chris Seiler — OPSWAT OEM Field CTO

use std::ffi::{c_char, c_void, CStr, CString};
use std::path::PathBuf;

use clap::Parser;
use simple_proxy::config::Config;

/// Demo CLI: all of `Config` plus a `--lib` override for the dynamic-library path.
#[derive(Parser, Debug)]
#[command(name = "simple-proxy-demo", version, about = "Demo that loads the simple-proxy dynamic library")]
struct DemoArgs {
    #[command(flatten)]
    config: Config,

    /// Path to the simple_proxy dynamic library. Defaults to the one next to this executable.
    #[arg(long)]
    lib: Option<PathBuf>,
}

/// Platform-specific default dynamic-library filename.
fn default_lib_name() -> &'static str {
    if cfg!(windows) {
        "simple_proxy.dll"
    } else if cfg!(target_os = "macos") {
        "libsimple_proxy.dylib"
    } else {
        "libsimple_proxy.so"
    }
}

/// Resolve the library path: explicit `--lib`, else next to the current executable.
fn resolve_lib_path(explicit: Option<PathBuf>) -> PathBuf {
    if let Some(p) = explicit {
        return p;
    }
    let name = default_lib_name();
    if let Ok(exe) = std::env::current_exe() {
        if let Some(dir) = exe.parent() {
            return dir.join(name);
        }
    }
    PathBuf::from(name)
}

fn main() -> anyhow::Result<()> {
    let args = DemoArgs::parse();
    let lib_path = resolve_lib_path(args.lib);

    // Hand the full config to the library as JSON so the DLL runs exactly what was requested.
    let config_json = serde_json::to_string(&args.config)?;
    let c_json = CString::new(config_json)?;

    // SAFETY: loading a trusted, co-located library and calling its documented C ABI.
    unsafe {
        let lib = libloading::Library::new(&lib_path).map_err(|e| {
            anyhow::anyhow!(
                "failed to load dynamic library '{}': {e}\n\
                 (build it first with build.ps1/build.sh, or pass --lib <path>)",
                lib_path.display()
            )
        })?;

        let version: libloading::Symbol<unsafe extern "C" fn() -> *const c_char> =
            lib.get(b"simple_proxy_version\0")?;
        let start_json: libloading::Symbol<
            unsafe extern "C" fn(*const c_char) -> *mut c_void,
        > = lib.get(b"simple_proxy_start_json\0")?;
        let stop: libloading::Symbol<unsafe extern "C" fn(*mut c_void)> =
            lib.get(b"simple_proxy_stop\0")?;

        let ver = CStr::from_ptr(version()).to_string_lossy();
        eprintln!("loaded {} ({})", lib_path.display(), ver);

        let handle = start_json(c_json.as_ptr());
        if handle.is_null() {
            anyhow::bail!("simple_proxy_start_json returned NULL (failed to start)");
        }
        eprintln!("proxy started via dynamic library. Press Ctrl-C to stop.");

        // Block until Ctrl-C / SIGTERM using a minimal runtime (the proxy runs on the library's
        // own thread/runtime; this one only waits for the shutdown signal).
        let rt = tokio::runtime::Builder::new_current_thread()
            .enable_all()
            .build()?;
        rt.block_on(simple_proxy::server::shutdown_signal());

        eprintln!("stopping proxy...");
        stop(handle);
        drop(lib);
    }

    Ok(())
}
