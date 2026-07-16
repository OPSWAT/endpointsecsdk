//! Request/URL security: scheme + domain allow-listing, SSRF (private-IP) protection, and
//! bearer-token auth. The proxy fetches attacker-influenceable URLs, so this is the core of the
//! "very secure" requirement.
//!
//! Created by Chris Seiler — OPSWAT OEM Field CTO

use std::net::{IpAddr, Ipv4Addr, Ipv6Addr, SocketAddr};

use crate::config::Config;

#[derive(Debug)]
pub enum UrlError {
    Parse(String),
    Scheme(String),
    NoHost,
    DomainNotAllowed(String),
}

impl std::fmt::Display for UrlError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            UrlError::Parse(s) => write!(f, "invalid URL: {s}"),
            UrlError::Scheme(s) => write!(f, "scheme not allowed: {s} (only https / http with --allow-http)"),
            UrlError::NoHost => write!(f, "URL has no host"),
            UrlError::DomainNotAllowed(h) => write!(f, "host not in --allow-domain list: {h}"),
        }
    }
}

/// A validated upstream URL that the proxy is cleared to fetch.
pub struct SafeUrl {
    pub full: String,
}

/// True if `host` matches an allow-list entry (exact host or a dot-suffix, e.g. "microsoft.com"
/// matches "download.microsoft.com").
fn host_allowed(host: &str, allow: &[String]) -> bool {
    if allow.is_empty() {
        return true;
    }
    let h = host.trim_end_matches('.').to_ascii_lowercase();
    allow.iter().any(|d| {
        let d = d.trim().trim_start_matches('.').to_ascii_lowercase();
        h == d || h.ends_with(&format!(".{d}"))
    })
}

/// Reject loopback, private, link-local, unspecified, multicast, and IPv4-mapped equivalents.
pub fn is_blocked_ip(ip: IpAddr) -> bool {
    match ip {
        IpAddr::V4(v4) => is_blocked_v4(v4),
        IpAddr::V6(v6) => {
            // Unwrap IPv4-mapped / -compatible addresses and check as v4.
            if let Some(v4) = v6.to_ipv4() {
                return is_blocked_v4(v4);
            }
            is_blocked_v6(v6)
        }
    }
}

fn is_blocked_v4(v4: Ipv4Addr) -> bool {
    v4.is_loopback()
        || v4.is_private()
        || v4.is_link_local()
        || v4.is_unspecified()
        || v4.is_broadcast()
        || v4.is_documentation()
        || v4.octets()[0] == 0
        // 100.64.0.0/10 carrier-grade NAT
        || (v4.octets()[0] == 100 && (v4.octets()[1] & 0xc0) == 64)
        // 169.254.0.0/16 already covered by is_link_local; 192.0.0.0/24 IETF protocol assignments
        || (v4.octets()[0] == 192 && v4.octets()[1] == 0 && v4.octets()[2] == 0)
}

fn is_blocked_v6(v6: Ipv6Addr) -> bool {
    let seg = v6.segments();
    v6.is_loopback()
        || v6.is_unspecified()
        || v6.is_multicast()
        // fc00::/7 unique local
        || (seg[0] & 0xfe00) == 0xfc00
        // fe80::/10 link-local
        || (seg[0] & 0xffc0) == 0xfe80
}

/// Validate a user-supplied URL's scheme and host allow-list. Returns a `SafeUrl` the proxy may
/// fetch, or a `UrlError`.
///
/// Note: private/loopback/link-local IP blocking is **not** done here. It is enforced at connect
/// time by [`SecureResolver`], which is the reqwest DNS resolver used for every connection (the
/// initial request and every redirect hop). Doing the block at resolve/connect time — rather than
/// resolving here and discarding the result — is what makes it robust against DNS rebinding
/// (TOCTOU) and redirects to hostnames that resolve to internal addresses.
pub fn validate_url(raw: &str, cfg: &Config) -> Result<SafeUrl, UrlError> {
    let url = reqwest::Url::parse(raw).map_err(|e| UrlError::Parse(e.to_string()))?;

    let scheme = url.scheme().to_ascii_lowercase();
    let is_https = scheme == "https";
    if !(is_https || (scheme == "http" && cfg.allow_http)) {
        return Err(UrlError::Scheme(scheme));
    }

    let host = url.host_str().ok_or(UrlError::NoHost)?.to_string();
    if !host_allowed(&host, &cfg.allow_domains) {
        return Err(UrlError::DomainNotAllowed(host));
    }

    Ok(SafeUrl {
        full: url.to_string(),
    })
}

/// A reqwest DNS resolver that enforces SSRF protection at **connect time**: it resolves the
/// hostname and drops every address that [`is_blocked_ip`] rejects (unless `allow_private`).
///
/// Because reqwest connects to exactly the addresses this resolver returns — for the initial
/// request *and* every redirect hop — the address that was checked is the address that is
/// connected to. This closes both SSRF gaps that a resolve-then-discard scheme leaves open:
///   * DNS rebinding / TOCTOU (a hostname that flips to a private IP between check and connect),
///   * redirects to a hostname that resolves to a private/loopback/link-local address.
#[derive(Clone)]
pub struct SecureResolver {
    pub allow_private: bool,
}

impl reqwest::dns::Resolve for SecureResolver {
    fn resolve(&self, name: reqwest::dns::Name) -> reqwest::dns::Resolving {
        let allow_private = self.allow_private;
        Box::pin(async move {
            let host = name.as_str().to_owned();
            // Port is irrelevant for IP-policy filtering; use 0.
            let resolved = tokio::net::lookup_host((host.as_str(), 0u16)).await?;
            let filtered: Vec<SocketAddr> = resolved
                .filter(|sa| allow_private || !is_blocked_ip(sa.ip()))
                .collect();
            if filtered.is_empty() {
                let err: Box<dyn std::error::Error + Send + Sync> = format!(
                    "refusing to connect: '{host}' resolved only to blocked/non-public addresses \
                     (SSRF protection)"
                )
                .into();
                return Err(err);
            }
            let addrs: reqwest::dns::Addrs = Box::new(filtered.into_iter());
            Ok(addrs)
        })
    }
}

/// Best-effort validation of a redirect target inside the reqwest redirect policy (which is a
/// synchronous context, so DNS cannot be resolved here). Blocks disallowed schemes, IP-literal
/// hosts in private ranges, and hosts outside the allow-list. Full DNS re-validation happens for
/// the initial URL in `validate_url`; combined with a small redirect cap this bounds SSRF via
/// redirects. For maximum hardening, use --allow-domain.
pub fn redirect_target_ok(url: &reqwest::Url, cfg: &Config) -> bool {
    let scheme = url.scheme().to_ascii_lowercase();
    if !(scheme == "https" || (scheme == "http" && cfg.allow_http)) {
        return false;
    }
    let host = match url.host_str() {
        Some(h) => h,
        None => return false,
    };
    if !host_allowed(host, &cfg.allow_domains) {
        return false;
    }
    if !cfg.allow_private {
        if let Ok(ip) = host.parse::<IpAddr>() {
            if is_blocked_ip(ip) {
                return false;
            }
        }
    }
    true
}

/// Constant-time-ish comparison for a token (length-independent early return is unavoidable, but
/// the byte comparison itself does not short-circuit).
pub fn tokens_match(a: &str, b: &str) -> bool {
    let (a, b) = (a.as_bytes(), b.as_bytes());
    if a.len() != b.len() {
        return false;
    }
    let mut diff = 0u8;
    for i in 0..a.len() {
        diff |= a[i] ^ b[i];
    }
    diff == 0
}

/// Returns true if the request is authorized (either no token configured, or a matching
/// `Authorization: Bearer <token>` header was supplied).
pub fn authorized(cfg: &Config, auth_header: Option<&str>) -> bool {
    match &cfg.token {
        None => true,
        Some(expected) => match auth_header {
            Some(h) => {
                let h = h.trim();
                let provided = h.strip_prefix("Bearer ").or_else(|| h.strip_prefix("bearer "));
                match provided {
                    Some(p) => tokens_match(p.trim(), expected),
                    None => false,
                }
            }
            None => false,
        },
    }
}
