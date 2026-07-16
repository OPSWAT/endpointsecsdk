//! On-disk content cache with a TTL and size-bounded, oldest-first eviction.
//!
//! Each cached object lives in a file named after the SHA-256 of its URL. An in-memory index
//! (persisted to `index.json`) tracks metadata. Access is serialized through a Mutex so the
//! bookkeeping stays consistent under concurrent requests.
//!
//! Created by Chris Seiler — OPSWAT OEM Field CTO

use std::collections::HashMap;
use std::path::{Path, PathBuf};
use std::sync::atomic::{AtomicU64, Ordering};
use std::sync::Arc;
use std::time::{SystemTime, UNIX_EPOCH};

use serde::{Deserialize, Serialize};
use sha2::{Digest, Sha256};
use tokio::sync::Mutex;

#[derive(Clone, Debug, Serialize, Deserialize)]
pub struct Entry {
    pub url: String,
    /// File name (relative to cache dir) holding the content.
    pub file: String,
    pub size: u64,
    pub content_type: Option<String>,
    /// Unix seconds when the entry was written.
    pub created_at: u64,
}

#[derive(Default, Serialize, Deserialize)]
struct Index {
    entries: HashMap<String, Entry>,
}

pub struct Cache {
    dir: PathBuf,
    ttl_seconds: u64,
    max_size: u64,
    inner: Mutex<Index>,
    /// Monotonic counter to give each in-progress download a unique temp file, so concurrent
    /// downloads of the SAME url don't write to the same `.part` file.
    temp_counter: AtomicU64,
}

/// SHA-256 hex of a URL; used as the cache key and on-disk file name.
pub fn key_for(url: &str) -> String {
    let mut h = Sha256::new();
    h.update(url.as_bytes());
    format!("{:x}", h.finalize())
}

fn now_secs() -> u64 {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .map(|d| d.as_secs())
        .unwrap_or(0)
}

impl Cache {
    /// Open (or create) a cache rooted at `dir`, loading any existing index.
    pub async fn open(dir: PathBuf, ttl_seconds: u64, max_size: u64) -> anyhow::Result<Arc<Self>> {
        tokio::fs::create_dir_all(&dir).await?;
        let index_path = dir.join("index.json");
        let index = match tokio::fs::read(&index_path).await {
            Ok(bytes) => serde_json::from_slice::<Index>(&bytes).unwrap_or_default(),
            Err(_) => Index::default(),
        };
        let cache = Arc::new(Self {
            dir,
            ttl_seconds,
            max_size,
            inner: Mutex::new(index),
            temp_counter: AtomicU64::new(0),
        });
        // Reconcile the loaded index with what's actually on disk, dropping stale/expired items.
        cache.reconcile().await?;
        // Enforce the size cap immediately, in case an existing cache was larger than the current
        // --max-cache-size (e.g. the cap was lowered between runs). Otherwise the cap would only
        // take effect lazily on the next write. ensure_room(0) evicts oldest-first until used<=cap.
        cache.ensure_room(0).await?;
        Ok(cache)
    }

    fn index_path(&self) -> PathBuf {
        self.dir.join("index.json")
    }

    pub fn path_for(&self, file: &str) -> PathBuf {
        self.dir.join(file)
    }

    fn is_expired(&self, e: &Entry, now: u64) -> bool {
        self.ttl_seconds > 0 && now.saturating_sub(e.created_at) >= self.ttl_seconds
    }

    /// Drop index entries whose files are missing or expired, and delete orphaned/expired files.
    async fn reconcile(&self) -> anyhow::Result<()> {
        let now = now_secs();
        let mut idx = self.inner.lock().await;
        let mut drop_keys = Vec::new();
        for (k, e) in idx.entries.iter() {
            let p = self.dir.join(&e.file);
            let missing = tokio::fs::metadata(&p).await.is_err();
            if missing || self.is_expired(e, now) {
                drop_keys.push(k.clone());
            }
        }
        for k in drop_keys {
            if let Some(e) = idx.entries.remove(&k) {
                let _ = tokio::fs::remove_file(self.dir.join(&e.file)).await;
            }
        }
        self.persist(&idx).await?;
        Ok(())
    }

    async fn persist(&self, idx: &Index) -> anyhow::Result<()> {
        let bytes = serde_json::to_vec_pretty(idx)?;
        let tmp = self.index_path().with_extension("json.tmp");
        tokio::fs::write(&tmp, &bytes).await?;
        tokio::fs::rename(&tmp, self.index_path()).await?;
        Ok(())
    }

    /// Return a live (non-expired, present-on-disk) entry for `url`, or None.
    pub async fn get(&self, url: &str) -> Option<Entry> {
        let key = key_for(url);
        let now = now_secs();
        let mut idx = self.inner.lock().await;
        if let Some(e) = idx.entries.get(&key).cloned() {
            if self.is_expired(&e, now) {
                idx.entries.remove(&key);
                let _ = tokio::fs::remove_file(self.dir.join(&e.file)).await;
                let _ = self.persist(&idx).await;
                return None;
            }
            if tokio::fs::metadata(self.dir.join(&e.file)).await.is_err() {
                idx.entries.remove(&key);
                let _ = self.persist(&idx).await;
                return None;
            }
            return Some(e);
        }
        None
    }

    /// How much room a new object of `incoming` bytes needs; evicts oldest entries until it fits
    /// (or the cache is empty). Call before committing a freshly downloaded file.
    pub async fn ensure_room(&self, incoming: u64) -> anyhow::Result<()> {
        if self.max_size == 0 {
            return Ok(()); // unbounded
        }
        let mut idx = self.inner.lock().await;
        let mut used: u64 = idx.entries.values().map(|e| e.size).sum();
        // Evict oldest-first until the incoming object fits under the ceiling.
        while used.saturating_add(incoming) > self.max_size && !idx.entries.is_empty() {
            // Find the oldest entry.
            let oldest = idx
                .entries
                .iter()
                .min_by_key(|(_, e)| e.created_at)
                .map(|(k, e)| (k.clone(), e.file.clone(), e.size));
            match oldest {
                Some((k, file, size)) => {
                    idx.entries.remove(&k);
                    let _ = tokio::fs::remove_file(self.dir.join(&file)).await;
                    used = used.saturating_sub(size);
                    tracing::info!("cache evicted oldest entry {} ({} bytes)", file, size);
                }
                None => break,
            }
        }
        self.persist(&idx).await?;
        Ok(())
    }

    /// Register a freshly written file (already located at `path_for(file)`).
    pub async fn insert(
        &self,
        url: &str,
        file: String,
        size: u64,
        content_type: Option<String>,
    ) -> anyhow::Result<Entry> {
        let entry = Entry {
            url: url.to_string(),
            file,
            size,
            content_type,
            created_at: now_secs(),
        };
        let mut idx = self.inner.lock().await;
        idx.entries.insert(key_for(url), entry.clone());
        self.persist(&idx).await?;
        Ok(entry)
    }

    /// Total bytes currently accounted for in the cache.
    pub async fn used_bytes(&self) -> u64 {
        self.inner.lock().await.entries.values().map(|e| e.size).sum()
    }

    /// Number of live entries.
    pub async fn len(&self) -> usize {
        self.inner.lock().await.entries.len()
    }

    /// True if the cache currently holds no entries.
    pub async fn is_empty(&self) -> bool {
        self.inner.lock().await.entries.is_empty()
    }

    /// A unique temp file path (in the cache dir) for staging an in-progress download. Includes a
    /// monotonic counter so two concurrent downloads of the same URL never share a `.part` file.
    pub fn temp_path(&self, key: &str) -> PathBuf {
        let n = self.temp_counter.fetch_add(1, Ordering::Relaxed);
        self.dir.join(format!("{key}.{n}.part"))
    }
}

/// Guard that removes a partial download file unless explicitly disarmed on success.
pub struct PartialGuard {
    path: Option<PathBuf>,
}

impl PartialGuard {
    pub fn new(path: PathBuf) -> Self {
        Self { path: Some(path) }
    }
    pub fn disarm(&mut self) {
        self.path = None;
    }
    pub fn path(&self) -> &Path {
        self.path.as_ref().expect("guard already disarmed")
    }
}

impl Drop for PartialGuard {
    fn drop(&mut self) {
        if let Some(p) = self.path.take() {
            // Best-effort synchronous cleanup of the abandoned partial file.
            let _ = std::fs::remove_file(p);
        }
    }
}
