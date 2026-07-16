//! simple-proxy — a secure, cross-platform caching download proxy for patch content.
//!
//! This crate builds three ways:
//!   * an **rlib** the demo binary (`src/main.rs`) links against;
//!   * a **cdylib** (`simple_proxy.dll` / `libsimple_proxy.so` / `libsimple_proxy.dylib`) exposing
//!     a small C ABI (see [`ffi`]) so an existing process can embed the proxy;
//!   * (via the bin target) the `simple-proxy` executable for standalone/demo use.
//!
//! The core server lives in [`server`]; [`server::run`] takes a config and a shutdown future so
//! both the executable (Ctrl-C/SIGTERM) and the embedded library (a stop signal) share one code
//! path.
//!
//! Created by Chris Seiler — OPSWAT OEM Field CTO

pub mod cache;
pub mod config;
pub mod discovery;
pub mod ffi;
pub mod firewall;
pub mod logging;
pub mod security;
pub mod server;

pub use config::Config;
pub use server::run;
