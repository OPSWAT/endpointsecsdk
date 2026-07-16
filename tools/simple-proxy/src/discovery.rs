//! UDP self-discovery responder.
//!
//! When enabled (`--discovery`), the proxy listens on a UDP port for a small probe datagram and
//! replies with a JSON descriptor so clients on the LAN can find it without being pre-configured
//! with its address. It answers both **broadcast** (`255.255.255.255`) and **multicast**
//! (`239.255.42.98`) probes — the multicast group is the modern, IPv4-router-friendly equivalent
//! of a broadcast.
//!
//! The reply never contains the token; it only advertises whether a token is *required*
//! (`"secure": true|false`) so discovery works both with and without a security token — the
//! client learns it must authenticate but obtains the token out of band.
//!
//! Reply JSON:
//!   {"service":"simple-proxy","version":"0.1.0","proxy_port":8080,"scheme":"http","secure":true}
//!
//! Created by Chris Seiler — OPSWAT OEM Field CTO

use std::collections::HashMap;
use std::net::{IpAddr, Ipv4Addr};
use std::sync::Arc;
use std::time::{Duration, Instant};

use tokio::net::UdpSocket;

use crate::config::Config;

/// Probe payload a client must send to get a reply.
pub const DISCOVERY_MAGIC: &str = "SIMPLE-PROXY-DISCOVER v1";

/// Administratively-scoped multicast group used for discovery.
pub const DISCOVERY_MULTICAST: Ipv4Addr = Ipv4Addr::new(239, 255, 42, 98);

/// Per-source reply rate limit: at most this many replies to a given source IP per window.
/// Bounds UDP reflection/amplification — in a spoofed-source flood every request appears to come
/// from the victim's IP, so this caps how much reflected traffic any one victim can receive.
/// Legitimate clients send only a handful of probes per discovery, so this is generous.
const RATE_MAX_REPLIES: u32 = 20;
const RATE_WINDOW: Duration = Duration::from_secs(10);
/// Cap on tracked source IPs so the table can't grow without bound.
const MAX_TRACKED_SOURCES: usize = 8192;

/// Fixed-window per-source rate check. Returns true if a reply to `ip` is allowed right now.
fn allow_reply(buckets: &mut HashMap<IpAddr, (Instant, u32)>, ip: IpAddr, now: Instant) -> bool {
    // Bound memory: if the table is oversized, drop entries whose window has expired.
    if buckets.len() > MAX_TRACKED_SOURCES {
        buckets.retain(|_, (start, _)| now.duration_since(*start) < RATE_WINDOW);
    }
    let entry = buckets.entry(ip).or_insert((now, 0));
    if now.duration_since(entry.0) >= RATE_WINDOW {
        *entry = (now, 0); // window elapsed -> reset
    }
    entry.1 += 1;
    entry.1 <= RATE_MAX_REPLIES
}

/// Run the discovery responder until the task is aborted. Best-effort: bind/join failures are
/// logged and the responder simply stops (the HTTP proxy keeps running).
pub async fn run_responder(cfg: Arc<Config>) {
    let port = cfg.discovery_port;
    let socket = match UdpSocket::bind((Ipv4Addr::UNSPECIFIED, port)).await {
        Ok(s) => s,
        Err(e) => {
            tracing::warn!("discovery: failed to bind UDP 0.0.0.0:{port}: {e}");
            return;
        }
    };
    if let Err(e) = socket.set_broadcast(true) {
        tracing::debug!("discovery: could not enable broadcast: {e}");
    }
    if let Err(e) = socket.join_multicast_v4(DISCOVERY_MULTICAST, Ipv4Addr::UNSPECIFIED) {
        tracing::debug!("discovery: multicast join failed (broadcast still works): {e}");
    }

    tracing::info!(
        "discovery responder on UDP {} (multicast {}); advertising proxy port {} (secure={})",
        port,
        DISCOVERY_MULTICAST,
        cfg.port,
        cfg.token.is_some()
    );

    let reply = build_reply(&cfg);
    let mut buf = vec![0u8; 1024];
    // Per-source reply counters (this loop is the sole owner, so no locking needed).
    let mut buckets: HashMap<IpAddr, (Instant, u32)> = HashMap::new();
    loop {
        match socket.recv_from(&mut buf).await {
            Ok((n, from)) => {
                if String::from_utf8_lossy(&buf[..n]).trim() == DISCOVERY_MAGIC {
                    if !allow_reply(&mut buckets, from.ip(), Instant::now()) {
                        tracing::debug!("discovery: rate-limited {from}");
                        continue;
                    }
                    match socket.send_to(reply.as_bytes(), from).await {
                        Ok(_) => tracing::debug!("discovery: replied to {from}"),
                        Err(e) => tracing::debug!("discovery: reply to {from} failed: {e}"),
                    }
                }
            }
            Err(e) => {
                tracing::debug!("discovery: recv error: {e}");
                // Avoid a tight error loop if the socket is in a bad state momentarily.
                tokio::time::sleep(std::time::Duration::from_millis(200)).await;
            }
        }
    }
}

fn build_reply(cfg: &Config) -> String {
    serde_json::json!({
        "service": "simple-proxy",
        "version": env!("CARGO_PKG_VERSION"),
        "proxy_port": cfg.port,
        "scheme": "http",
        "secure": cfg.token.is_some(),
    })
    .to_string()
}
