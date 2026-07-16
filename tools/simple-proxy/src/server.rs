//! HTTP server: routing, request auth, cache serving, and the tee-download path. The public
//! entry point is [`run`], which takes a `Config` and a shutdown future so the executable and the
//! embedded library share one implementation.
//!
//! Created by Chris Seiler — OPSWAT OEM Field CTO

use std::collections::HashMap;
use std::future::Future;
use std::sync::Arc;

use axum::{
    body::Body,
    extract::{Query, State},
    http::{header, HeaderMap, HeaderValue, StatusCode},
    response::{IntoResponse, Response},
    routing::get,
    Json, Router,
};
use futures_util::StreamExt;
use serde_json::json;
use tokio::io::AsyncWriteExt;
use tokio_util::io::ReaderStream;

use crate::cache::{self, Cache, PartialGuard};
use crate::config::Config;
use crate::{firewall, security};

/// Header used for the "secure mode" token, in addition to `Authorization: Bearer <token>`.
pub const TOKEN_HEADER: &str = "x-proxy-token";

#[derive(Clone)]
pub struct AppState {
    pub cfg: Arc<Config>,
    pub cache: Arc<Cache>,
    pub client: reqwest::Client,
}

/// Build the reqwest client used for upstream fetches, with redirect validation + timeouts.
pub fn build_client(cfg: Arc<Config>) -> anyhow::Result<reqwest::Client> {
    let redirect_cfg = cfg.clone();
    let client = reqwest::Client::builder()
        // Enforce private/loopback/link-local IP blocking at CONNECT time for every connection,
        // including redirect hops. This is the authoritative SSRF control (see SecureResolver).
        .dns_resolver(std::sync::Arc::new(security::SecureResolver {
            allow_private: cfg.allow_private,
        }))
        .redirect(reqwest::redirect::Policy::custom(move |attempt| {
            if attempt.previous().len() >= 5 {
                return attempt.error("too many redirects");
            }
            // Scheme + host allow-list check per hop; the IP-level SSRF block is enforced by the
            // DNS resolver above when the redirect connection is actually made.
            if security::redirect_target_ok(attempt.url(), &redirect_cfg) {
                attempt.follow()
            } else {
                attempt.error("redirect to disallowed target blocked")
            }
        }))
        .connect_timeout(std::time::Duration::from_secs(cfg.upstream_timeout_seconds))
        .read_timeout(std::time::Duration::from_secs(cfg.upstream_timeout_seconds))
        .user_agent(concat!("simple-proxy/", env!("CARGO_PKG_VERSION")))
        .build()?;
    Ok(client)
}

/// Assemble the axum router for a given state.
pub fn build_router(state: AppState) -> Router {
    Router::new()
        .route("/health", get(health))
        .route("/cached", get(cached))
        .route("/download", get(download))
        .with_state(state)
}

/// Run the proxy until `shutdown` resolves. Opens the firewall on start (if configured) and
/// always closes it again before returning. Shared by the executable and the embedded library.
pub async fn run<F>(cfg: Config, shutdown: F) -> anyhow::Result<()>
where
    F: Future<Output = ()> + Send + 'static,
{
    // Warn if exposed beyond loopback without a token (secure mode strongly recommended there).
    if !cfg.bind.is_loopback() && cfg.token.is_none() {
        tracing::warn!(
            "binding to {} (non-loopback) WITHOUT a token: anyone who can reach this port can \
             drive downloads through this proxy. Set a token and/or an allow-domain list.",
            cfg.bind
        );
    }

    let cache = Cache::open(cfg.cache_dir.clone(), cfg.ttl_seconds, cfg.max_cache_size).await?;
    let cfg = Arc::new(cfg);
    let client = build_client(cfg.clone())?;

    let state = AppState {
        cfg: cfg.clone(),
        cache,
        client,
    };
    let app = build_router(state);

    let addr = std::net::SocketAddr::new(cfg.bind, cfg.port);
    let listener = tokio::net::TcpListener::bind(addr).await?;
    tracing::info!("simple-proxy listening on http://{}", addr);
    tracing::info!(
        "cache: dir={} ttl={}s max={} bytes; secure mode: {}",
        cfg.cache_dir.display(),
        cfg.ttl_seconds,
        cfg.max_cache_size,
        if cfg.token.is_some() { "ON (token required)" } else { "off" }
    );

    // Optionally start the UDP self-discovery responder alongside the HTTP server.
    let discovery_task = if cfg.discovery {
        if cfg.bind.is_loopback() {
            tracing::warn!(
                "discovery is enabled but the proxy is bound to loopback ({}); LAN clients will \
                 discover an address they cannot reach. Bind to 0.0.0.0 for discovery to be useful.",
                cfg.bind
            );
        }
        Some(tokio::spawn(crate::discovery::run_responder(cfg.clone())))
    } else {
        None
    };

    // Optionally open the firewall (TCP proxy port, plus the UDP discovery port when discovery is
    // on); always close whatever we opened on the way out.
    let mut opened_rules: Vec<(u16, firewall::Protocol)> = Vec::new();
    if cfg.open_firewall {
        if firewall::open(cfg.port, firewall::Protocol::Tcp) {
            opened_rules.push((cfg.port, firewall::Protocol::Tcp));
        }
        if cfg.discovery && firewall::open(cfg.discovery_port, firewall::Protocol::Udp) {
            opened_rules.push((cfg.discovery_port, firewall::Protocol::Udp));
        }
    }

    let result = axum::serve(listener, app)
        .with_graceful_shutdown(shutdown)
        .await;

    if let Some(task) = discovery_task {
        task.abort();
    }
    for (port, proto) in opened_rules {
        firewall::close(port, proto);
    }
    tracing::info!("simple-proxy stopped");
    result.map_err(Into::into)
}

/// Resolves when Ctrl-C (all platforms) or SIGTERM (unix) is received.
pub async fn shutdown_signal() {
    let ctrl_c = async {
        let _ = tokio::signal::ctrl_c().await;
    };

    #[cfg(unix)]
    let terminate = async {
        use tokio::signal::unix::{signal, SignalKind};
        if let Ok(mut s) = signal(SignalKind::terminate()) {
            s.recv().await;
        }
    };
    #[cfg(not(unix))]
    let terminate = std::future::pending::<()>();

    tokio::select! {
        _ = ctrl_c => {},
        _ = terminate => {},
    }
    tracing::info!("shutdown signal received");
}

// ---- request auth --------------------------------------------------------------------------

/// Secure-mode check. When a token is configured, the request must present it either as
/// `X-Proxy-Token: <token>` or `Authorization: Bearer <token>`. When no token is configured, all
/// requests are allowed.
fn auth_ok(state: &AppState, headers: &HeaderMap) -> bool {
    let expected = match &state.cfg.token {
        None => return true,
        Some(t) => t,
    };
    // X-Proxy-Token (raw value)
    if let Some(v) = headers.get(TOKEN_HEADER).and_then(|v| v.to_str().ok()) {
        if security::tokens_match(v.trim(), expected) {
            return true;
        }
    }
    // Authorization: Bearer <token>
    let bearer = headers
        .get(header::AUTHORIZATION)
        .and_then(|v| v.to_str().ok());
    security::authorized(&state.cfg, bearer)
}

fn err_json(code: StatusCode, msg: impl Into<String>) -> Response {
    (code, Json(json!({ "error": msg.into() }))).into_response()
}

// ---- handlers ------------------------------------------------------------------------------

async fn health(State(state): State<AppState>) -> Response {
    let used = state.cache.used_bytes().await;
    let count = state.cache.len().await;
    Json(json!({
        "status": "ok",
        "secure_mode": state.cfg.token.is_some(),
        "cache_entries": count,
        "cache_used_bytes": used,
        "cache_max_bytes": state.cfg.max_cache_size,
        "ttl_seconds": state.cfg.ttl_seconds,
    }))
    .into_response()
}

/// GET /cached?url=... — is this URL currently cached? No download is performed.
async fn cached(
    State(state): State<AppState>,
    headers: HeaderMap,
    Query(params): Query<HashMap<String, String>>,
) -> Response {
    if !auth_ok(&state, &headers) {
        return err_json(StatusCode::UNAUTHORIZED, "missing or invalid token");
    }
    let url = match params.get("url") {
        Some(u) => u,
        None => return err_json(StatusCode::BAD_REQUEST, "missing 'url' query parameter"),
    };

    match state.cache.get(url).await {
        Some(e) => Json(json!({
            "cached": true,
            "size": e.size,
            "content_type": e.content_type,
            "cached_at_epoch": e.created_at,
        }))
        .into_response(),
        None => Json(json!({ "cached": false })).into_response(),
    }
}

/// GET /download?url=... — return the content, from cache if present, else fetch-and-cache while
/// streaming to the caller (connection kept alive throughout the initial download).
async fn download(
    State(state): State<AppState>,
    headers: HeaderMap,
    Query(params): Query<HashMap<String, String>>,
) -> Response {
    if !auth_ok(&state, &headers) {
        return err_json(StatusCode::UNAUTHORIZED, "missing or invalid token");
    }
    let url = match params.get("url") {
        Some(u) => u.clone(),
        None => return err_json(StatusCode::BAD_REQUEST, "missing 'url' query parameter"),
    };

    // 1) Cache hit → stream from disk.
    if let Some(entry) = state.cache.get(&url).await {
        tracing::info!("cache HIT {}", url);
        match serve_from_cache(&state, &entry).await {
            Ok(resp) => return resp,
            Err(e) => {
                tracing::warn!("cache hit but failed to serve ({}); refetching", e);
            }
        }
    }

    // 2) Validate the URL scheme + host allow-list. The private-IP SSRF block is enforced at
    //    connect time by the client's SecureResolver (covers this fetch and any redirects).
    let safe = match security::validate_url(&url, &state.cfg) {
        Ok(s) => s,
        Err(e) => return err_json(StatusCode::FORBIDDEN, e.to_string()),
    };

    // 3) HEAD probe to learn the size, so we can reject over-limit downloads and make room in the
    //    cache before we start filling it.
    let head_len = match state.client.head(&safe.full).send().await {
        Ok(resp) => resp
            .headers()
            .get(header::CONTENT_LENGTH)
            .and_then(|v| v.to_str().ok())
            .and_then(|s| s.parse::<u64>().ok()),
        Err(_) => None, // some servers don't support HEAD; fall through and cap during streaming
    };

    if let Some(len) = head_len {
        if len > state.cfg.max_download {
            return err_json(
                StatusCode::PAYLOAD_TOO_LARGE,
                format!(
                    "content-length {} exceeds max-download {}",
                    len, state.cfg.max_download
                ),
            );
        }
        if let Err(e) = state.cache.ensure_room(len).await {
            tracing::warn!("cache ensure_room failed: {}", e);
        }
    }

    // 4) Fetch and tee: stream to the caller while writing to a temp file. On success, the temp
    //    file is committed to the cache atomically.
    tracing::info!("cache MISS {} — fetching upstream", url);
    let upstream = match state.client.get(&safe.full).send().await {
        Ok(r) => r,
        Err(e) => return err_json(StatusCode::BAD_GATEWAY, format!("upstream error: {e}")),
    };

    if !upstream.status().is_success() {
        return err_json(
            StatusCode::BAD_GATEWAY,
            format!("upstream returned status {}", upstream.status()),
        );
    }

    let content_type = upstream
        .headers()
        .get(header::CONTENT_TYPE)
        .and_then(|v| v.to_str().ok())
        .map(|s| s.to_string());
    let content_length = upstream
        .headers()
        .get(header::CONTENT_LENGTH)
        .and_then(|v| v.to_str().ok())
        .map(|s| s.to_string());

    stream_and_cache(state, url, content_type, content_length, upstream).await
}

/// Serve an existing cache entry as a streamed response.
async fn serve_from_cache(state: &AppState, entry: &cache::Entry) -> anyhow::Result<Response> {
    let path = state.cache.path_for(&entry.file);
    let file = tokio::fs::File::open(&path).await?;
    let stream = ReaderStream::new(file);
    let body = Body::from_stream(stream);

    let mut resp = Response::builder().status(StatusCode::OK);
    resp = resp.header(header::CONTENT_LENGTH, entry.size);
    resp = resp.header("X-Cache", "HIT");
    if let Some(ct) = &entry.content_type {
        if let Ok(v) = HeaderValue::from_str(ct) {
            resp = resp.header(header::CONTENT_TYPE, v);
        }
    }
    Ok(resp.body(body).unwrap())
}

/// Stream the upstream body to the client while simultaneously writing it to a temp file; commit
/// to the cache when the transfer completes cleanly. Enforces max_download during streaming even
/// when no Content-Length was known up front.
async fn stream_and_cache(
    state: AppState,
    url: String,
    content_type: Option<String>,
    content_length: Option<String>,
    upstream: reqwest::Response,
) -> Response {
    let key = cache::key_for(&url);
    let temp_path = state.cache.temp_path(&key);

    // (tx, rx) tees the body: bytes go to both the caller's response and the temp file writer.
    let (tx, rx) = tokio::sync::mpsc::channel::<Result<bytes::Bytes, std::io::Error>>(16);

    let writer_state = state.clone();
    let writer_url = url.clone();
    let writer_ct = content_type.clone();
    let max_download = state.cfg.max_download;

    // Background task: consume the upstream stream, write to temp file, forward to the channel.
    tokio::spawn(async move {
        let mut guard = PartialGuard::new(temp_path.clone());
        let file = match tokio::fs::File::create(guard.path()).await {
            Ok(f) => f,
            Err(e) => {
                let _ = tx.send(Err(std::io::Error::other(e.to_string()))).await;
                return;
            }
        };
        let mut writer = tokio::io::BufWriter::new(file);
        let mut written: u64 = 0;
        let mut stream = upstream.bytes_stream();
        let mut ok = true;

        while let Some(chunk) = stream.next().await {
            match chunk {
                Ok(bytes) => {
                    written += bytes.len() as u64;
                    if written > max_download {
                        tracing::warn!("download exceeded max-download; aborting {}", writer_url);
                        let _ = tx
                            .send(Err(std::io::Error::other(
                                "download exceeded max-download limit",
                            )))
                            .await;
                        ok = false;
                        break;
                    }
                    if let Err(e) = writer.write_all(&bytes).await {
                        let _ = tx.send(Err(std::io::Error::other(e.to_string()))).await;
                        ok = false;
                        break;
                    }
                    // Forward to the caller. If the receiver is gone (client disconnected), stop.
                    if tx.send(Ok(bytes)).await.is_err() {
                        tracing::info!("client disconnected during download of {}", writer_url);
                        ok = false;
                        break;
                    }
                }
                Err(e) => {
                    let _ = tx.send(Err(std::io::Error::other(e.to_string()))).await;
                    ok = false;
                    break;
                }
            }
        }

        if !ok {
            // guard drops → partial file removed
            return;
        }

        if let Err(e) = writer.flush().await {
            tracing::warn!("failed to flush cache file for {}: {}", writer_url, e);
            return; // guard drops → partial removed
        }
        drop(writer);

        // Commit: make room, move temp → final, register in the index.
        if let Err(e) = writer_state.cache.ensure_room(written).await {
            tracing::warn!("ensure_room failed at commit: {}", e);
        }
        let final_name = key.clone();
        let final_path = writer_state.cache.path_for(&final_name);
        if let Err(e) = tokio::fs::rename(guard.path(), &final_path).await {
            tracing::warn!("failed to commit cache file for {}: {}", writer_url, e);
            return; // guard drops → partial removed
        }
        guard.disarm(); // file now lives at final_path; don't delete it
        if let Err(e) = writer_state
            .cache
            .insert(&writer_url, final_name, written, writer_ct)
            .await
        {
            tracing::warn!("failed to index cache entry for {}: {}", writer_url, e);
        } else {
            tracing::info!("cached {} ({} bytes)", writer_url, written);
        }
    });

    // Response body reads from the channel — streamed to the caller as bytes arrive upstream,
    // keeping the connection alive for the whole initial download.
    let rx_stream = tokio_stream::wrappers::ReceiverStream::new(rx);
    let body = Body::from_stream(rx_stream);

    let mut resp = Response::builder()
        .status(StatusCode::OK)
        .header("X-Cache", "MISS");
    if let Some(ct) = &content_type {
        if let Ok(v) = HeaderValue::from_str(ct) {
            resp = resp.header(header::CONTENT_TYPE, v);
        }
    }
    if let Some(cl) = &content_length {
        if let Ok(v) = HeaderValue::from_str(cl) {
            resp = resp.header(header::CONTENT_LENGTH, v);
        }
    }
    resp.body(body).unwrap()
}
