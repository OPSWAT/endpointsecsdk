//! Logging setup: log to the console by default, or to a configured file that is size-capped.
//!
//! When `--log-file <path>` is set, all log output goes to that file instead of the console. The
//! file grows to at most `--log-max-size` (default 10 MB); when the next write would exceed the
//! cap, the current file is rotated to `<path>.1` (replacing any previous `.1`) and a fresh file
//! is started — so the active log never exceeds the cap.
//!
//! Created by Chris Seiler — OPSWAT OEM Field CTO

use std::fs::{File, OpenOptions};
use std::io::{self, Write};
use std::path::{Path, PathBuf};
use std::sync::{Arc, Mutex};

use tracing_subscriber::fmt::MakeWriter;
use tracing_subscriber::EnvFilter;

const DEFAULT_FILTER: &str = "simple_proxy=info,tower_http=warn";

use crate::config::Config;

fn env_filter() -> EnvFilter {
    EnvFilter::try_from_default_env().unwrap_or_else(|_| EnvFilter::new(DEFAULT_FILTER))
}

/// Initialize the global tracing subscriber from `cfg`. Idempotent — only the first call in a
/// process takes effect (safe to call from every entry point).
pub fn init(cfg: &Config) {
    match &cfg.log_file {
        Some(path) => match CappedFileWriter::new(path.clone(), cfg.log_max_size) {
            Ok(writer) => {
                let _ = tracing_subscriber::fmt()
                    .with_env_filter(env_filter())
                    .with_ansi(false)
                    .with_writer(CappedMakeWriter(writer))
                    .try_init();
            }
            Err(e) => {
                let _ = tracing_subscriber::fmt().with_env_filter(env_filter()).try_init();
                tracing::warn!(
                    "could not open log file {}: {e}; logging to console instead",
                    path.display()
                );
            }
        },
        None => {
            let _ = tracing_subscriber::fmt().with_env_filter(env_filter()).try_init();
        }
    }
}

/// A `Write` sink backed by a file that is rotated once it reaches a maximum size, so the active
/// file never exceeds the cap.
pub struct CappedFileWriter {
    inner: Mutex<Inner>,
}

struct Inner {
    file: Option<File>,
    path: PathBuf,
    size: u64,
    max: u64,
}

impl CappedFileWriter {
    pub fn new(path: PathBuf, max: u64) -> io::Result<Arc<Self>> {
        let file = OpenOptions::new().create(true).append(true).open(&path)?;
        let size = file.metadata().map(|m| m.len()).unwrap_or(0);
        Ok(Arc::new(Self {
            inner: Mutex::new(Inner {
                file: Some(file),
                path,
                size,
                max: max.max(1),
            }),
        }))
    }

    fn write_bytes(&self, buf: &[u8]) -> io::Result<usize> {
        let mut inner = self.inner.lock().unwrap_or_else(|e| e.into_inner());
        // Rotate if this write would push the file past the cap (but keep at least one write so a
        // single oversized record can't wedge us into an empty-rotate loop).
        if inner.size > 0 && inner.size + buf.len() as u64 > inner.max {
            rotate(&mut inner)?;
        }
        if inner.file.is_none() {
            let f = OpenOptions::new().create(true).append(true).open(&inner.path)?;
            inner.size = f.metadata().map(|m| m.len()).unwrap_or(0);
            inner.file = Some(f);
        }
        let f = inner.file.as_mut().unwrap();
        f.write_all(buf)?;
        inner.size += buf.len() as u64;
        Ok(buf.len())
    }

    fn flush_file(&self) -> io::Result<()> {
        let mut inner = self.inner.lock().unwrap_or_else(|e| e.into_inner());
        match inner.file.as_mut() {
            Some(f) => f.flush(),
            None => Ok(()),
        }
    }
}

/// Rotate `<path>` to `<path>.1`. The current handle is closed first so the rename succeeds on
/// Windows (which won't rename an open file). The new file is reopened lazily on the next write.
fn rotate(inner: &mut Inner) -> io::Result<()> {
    if let Some(mut f) = inner.file.take() {
        let _ = f.flush();
        // `f` is dropped here, closing the OS handle.
    }
    let backup = backup_path(&inner.path);
    let _ = std::fs::remove_file(&backup);
    let _ = std::fs::rename(&inner.path, &backup);
    inner.size = 0;
    Ok(())
}

/// `foo.log` -> `foo.log.1` (append `.1` to the whole path).
fn backup_path(path: &Path) -> PathBuf {
    let mut s = path.as_os_str().to_owned();
    s.push(".1");
    PathBuf::from(s)
}

/// `MakeWriter` handle that hands each log record to the shared capped writer.
#[derive(Clone)]
pub struct CappedMakeWriter(Arc<CappedFileWriter>);

pub struct CappedGuard(Arc<CappedFileWriter>);

impl Write for CappedGuard {
    fn write(&mut self, buf: &[u8]) -> io::Result<usize> {
        self.0.write_bytes(buf)
    }
    fn flush(&mut self) -> io::Result<()> {
        self.0.flush_file()
    }
}

impl<'a> MakeWriter<'a> for CappedMakeWriter {
    type Writer = CappedGuard;
    fn make_writer(&'a self) -> Self::Writer {
        CappedGuard(self.0.clone())
    }
}
