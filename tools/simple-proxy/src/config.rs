//! Startup configuration for simple-proxy (CLI parsed with clap).
//!
//! Created by Chris Seiler — OPSWAT OEM Field CTO

use std::net::IpAddr;
use std::path::PathBuf;

use clap::Parser;
use serde::{Deserialize, Serialize};

/// Secure caching download proxy for patch content.
///
/// Derives both `clap::Parser` (the executable parses it from the CLI) and serde
/// `Serialize`/`Deserialize` (the demo serializes it to JSON and hands it to the dynamic library
/// via `simple_proxy_start_json`, so the DLL runs with the exact same configuration). Missing
/// JSON fields fall back to [`Config::default`].
#[derive(Parser, Debug, Clone, Serialize, Deserialize)]
#[serde(default)]
#[command(name = "simple-proxy", version, about)]
pub struct Config {
    /// Address to bind the proxy port to. Defaults to loopback for safety; set 0.0.0.0 to expose.
    #[arg(long, default_value = "127.0.0.1")]
    pub bind: IpAddr,

    /// TCP port the proxy listens on.
    #[arg(long, default_value_t = 8080)]
    pub port: u16,

    /// Directory where cached content is stored.
    #[arg(long, default_value = "simple-proxy-cache")]
    pub cache_dir: PathBuf,

    /// Maximum total cache size (e.g. 500MB, 10GB, or a byte count). Oldest entries are evicted
    /// once exceeded.
    #[arg(long, default_value = "5GB", value_parser = parse_size)]
    pub max_cache_size: u64,

    /// Cache entry time-to-live (e.g. 24h, 7d, 3600s, or a plain number of seconds).
    #[arg(long = "ttl", default_value = "24h", value_parser = parse_duration_secs)]
    pub ttl_seconds: u64,

    /// Maximum size of a single download (e.g. 2GB). Larger downloads are rejected.
    #[arg(long, default_value = "2GB", value_parser = parse_size)]
    pub max_download: u64,

    /// Require this bearer token on every request (Authorization: Bearer <token>).
    /// Strongly recommended when binding to anything other than loopback.
    #[arg(long)]
    pub token: Option<String>,

    /// Allow plain http:// upstream URLs. By default only https:// is permitted.
    #[arg(long, default_value_t = false)]
    pub allow_http: bool,

    /// Restrict upstream downloads to these host suffixes (repeatable). If unset, any public
    /// host is allowed (subject to the private/loopback IP blocks).
    #[arg(long = "allow-domain")]
    pub allow_domains: Vec<String>,

    /// Allow upstream URLs that resolve to private/loopback/link-local addresses. OFF by default
    /// (SSRF protection). Only enable for trusted internal testing.
    #[arg(long, default_value_t = false)]
    pub allow_private: bool,

    /// Open the OS firewall for the proxy port on startup and close it again on exit.
    /// Requires administrator/root privileges.
    #[arg(long, default_value_t = false)]
    pub open_firewall: bool,

    /// Per-request upstream connect/read timeout in seconds.
    #[arg(long, default_value_t = 60)]
    pub upstream_timeout_seconds: u64,

    /// Enable the UDP self-discovery responder so clients on the LAN can find this proxy
    /// (works with or without a token; the reply advertises whether a token is required).
    #[arg(long, default_value_t = false)]
    pub discovery: bool,

    /// UDP port the self-discovery responder listens on (broadcast + multicast). Clients must
    /// probe this same port.
    #[arg(long, default_value_t = 8099)]
    pub discovery_port: u16,

    /// Write all logs to this file instead of the console. The file is size-capped and rotated
    /// to <path>.1 (see --log-max-size).
    #[arg(long)]
    pub log_file: Option<PathBuf>,

    /// Maximum log file size before it rotates to <path>.1 (e.g. 10MB, 50MB). Only applies with
    /// --log-file.
    #[arg(long, default_value = "10MB", value_parser = parse_size)]
    pub log_max_size: u64,
}

impl Default for Config {
    /// Must mirror the clap `default_value`s above so CLI and JSON agree.
    fn default() -> Self {
        Config {
            bind: IpAddr::from([127, 0, 0, 1]),
            port: 8080,
            cache_dir: PathBuf::from("simple-proxy-cache"),
            max_cache_size: 5_000_000_000,
            ttl_seconds: 86_400,
            max_download: 2_000_000_000,
            token: None,
            allow_http: false,
            allow_domains: Vec::new(),
            allow_private: false,
            open_firewall: false,
            upstream_timeout_seconds: 60,
            discovery: false,
            discovery_port: 8099,
            log_file: None,
            log_max_size: 10_000_000,
        }
    }
}

/// Parse a human size like "10GB", "500MB", "1024" (bytes). Binary units (KiB/MiB) also accepted.
pub fn parse_size(s: &str) -> Result<u64, String> {
    let t = s.trim().to_ascii_uppercase();
    let (num, mult) = if let Some(n) = t.strip_suffix("GB") {
        (n, 1_000_000_000u64)
    } else if let Some(n) = t.strip_suffix("GIB") {
        (n, 1u64 << 30)
    } else if let Some(n) = t.strip_suffix("MB") {
        (n, 1_000_000)
    } else if let Some(n) = t.strip_suffix("MIB") {
        (n, 1u64 << 20)
    } else if let Some(n) = t.strip_suffix("KB") {
        (n, 1_000)
    } else if let Some(n) = t.strip_suffix("KIB") {
        (n, 1u64 << 10)
    } else if let Some(n) = t.strip_suffix('B') {
        (n, 1)
    } else {
        (t.as_str(), 1)
    };
    let value: f64 = num.trim().parse().map_err(|_| format!("invalid size: {s}"))?;
    if value < 0.0 {
        return Err(format!("size must be non-negative: {s}"));
    }
    Ok((value * mult as f64) as u64)
}

/// Parse a duration like "24h", "7d", "30m", "3600s", or a plain number of seconds.
pub fn parse_duration_secs(s: &str) -> Result<u64, String> {
    let t = s.trim().to_ascii_lowercase();
    let (num, mult) = if let Some(n) = t.strip_suffix('d') {
        (n, 86_400u64)
    } else if let Some(n) = t.strip_suffix('h') {
        (n, 3_600)
    } else if let Some(n) = t.strip_suffix('m') {
        (n, 60)
    } else if let Some(n) = t.strip_suffix('s') {
        (n, 1)
    } else {
        (t.as_str(), 1)
    };
    let value: u64 = num.trim().parse().map_err(|_| format!("invalid duration: {s}"))?;
    Ok(value * mult)
}
