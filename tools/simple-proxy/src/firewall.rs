//! Optional OS firewall management: open inbound rules on startup and remove them on exit.
//! Cross-platform, best-effort — requires elevated privileges to succeed.
//!
//!  * Windows: `netsh advfirewall firewall add/delete rule`
//!  * Linux:   `firewall-cmd` (firewalld) if present, else `ufw`
//!  * macOS:   the application firewall is app-based, not port-based; we log a note instead.
//!
//! Created by Chris Seiler — OPSWAT OEM Field CTO

use std::process::Command;

/// Transport protocol for a firewall rule.
#[derive(Clone, Copy, Debug)]
pub enum Protocol {
    Tcp,
    Udp,
}

impl Protocol {
    fn upper(self) -> &'static str {
        match self {
            Protocol::Tcp => "TCP",
            Protocol::Udp => "UDP",
        }
    }
    fn lower(self) -> &'static str {
        match self {
            Protocol::Tcp => "tcp",
            Protocol::Udp => "udp",
        }
    }
}

/// Stable rule name so we can find and delete exactly what we created.
fn rule_name(port: u16, proto: Protocol) -> String {
    format!("simple-proxy-{}-{port}", proto.lower())
}

/// Opens the firewall for `port`/`proto`. Returns true if a rule was (probably) added.
pub fn open(port: u16, proto: Protocol) -> bool {
    #[cfg(target_os = "windows")]
    {
        let name = rule_name(port, proto);
        let status = Command::new("netsh")
            .args([
                "advfirewall",
                "firewall",
                "add",
                "rule",
                &format!("name={name}"),
                "dir=in",
                "action=allow",
                &format!("protocol={}", proto.upper()),
                &format!("localport={port}"),
            ])
            .status();
        match status {
            Ok(s) if s.success() => {
                tracing::info!("firewall: opened {} {} (rule {})", proto.upper(), port, name);
                true
            }
            Ok(s) => {
                tracing::warn!("firewall: netsh exited with {} (need admin?)", s);
                false
            }
            Err(e) => {
                tracing::warn!("firewall: failed to run netsh: {}", e);
                false
            }
        }
    }

    #[cfg(target_os = "linux")]
    {
        let p = proto.lower();
        // Prefer firewalld, fall back to ufw.
        if which("firewall-cmd") {
            let ok = Command::new("firewall-cmd")
                .arg(format!("--add-port={port}/{p}"))
                .status()
                .map(|s| s.success())
                .unwrap_or(false);
            if ok {
                tracing::info!("firewall: opened {}/{} via firewall-cmd (runtime)", port, p);
                return true;
            }
        }
        if which("ufw") {
            let ok = Command::new("ufw")
                .args(["allow", &format!("{port}/{p}")])
                .status()
                .map(|s| s.success())
                .unwrap_or(false);
            if ok {
                tracing::info!("firewall: opened {}/{} via ufw", port, p);
                return true;
            }
        }
        tracing::warn!("firewall: no firewalld/ufw available or command failed (need root?)");
        false
    }

    #[cfg(target_os = "macos")]
    {
        let _ = rule_name(port, proto);
        tracing::info!(
            "firewall: macOS uses an application firewall (not port-based); \
             no rule added for {} {}. Allow the binary in System Settings if prompted.",
            proto.upper(),
            port
        );
        false
    }

    #[cfg(not(any(target_os = "windows", target_os = "linux", target_os = "macos")))]
    {
        let _ = (port, proto);
        tracing::warn!("firewall: unsupported platform; not opening port {}", port);
        false
    }
}

/// Closes/removes the firewall rule for `port`/`proto` that `open` created. Best-effort.
pub fn close(port: u16, proto: Protocol) {
    #[cfg(target_os = "windows")]
    {
        let name = rule_name(port, proto);
        let _ = Command::new("netsh")
            .args([
                "advfirewall",
                "firewall",
                "delete",
                "rule",
                &format!("name={name}"),
            ])
            .status();
        tracing::info!("firewall: removed rule {}", name);
    }

    #[cfg(target_os = "linux")]
    {
        let p = proto.lower();
        if which("firewall-cmd") {
            let _ = Command::new("firewall-cmd")
                .arg(format!("--remove-port={port}/{p}"))
                .status();
        }
        if which("ufw") {
            let _ = Command::new("ufw")
                .args(["delete", "allow", &format!("{port}/{p}")])
                .status();
        }
        tracing::info!("firewall: closed {}/{}", port, p);
    }

    #[cfg(target_os = "macos")]
    {
        let _ = (port, proto);
    }

    #[cfg(not(any(target_os = "windows", target_os = "linux", target_os = "macos")))]
    {
        let _ = (port, proto);
    }
}

#[cfg(target_os = "linux")]
fn which(cmd: &str) -> bool {
    Command::new("sh")
        .arg("-c")
        .arg(format!("command -v {cmd} >/dev/null 2>&1"))
        .status()
        .map(|s| s.success())
        .unwrap_or(false)
}
