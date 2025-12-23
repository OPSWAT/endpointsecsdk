# Key Datasets (.dat) – What They Represent

This page documents common datasets found under `extract/analog/client/`.

## Device Posture Scripts
- `compliance/**/libwaresource.*` – This file contains device posture scripts used for compliance checks across various platforms.

## Checksums
- `ap_checksum.dat` – Checksums for patch content
- `ap_checksum_mac.dat` – Checksums for patch content on macOS

## Linux Vulnerability Intelligence
- `liv.dat` – Vulnerability scripts and data for all Linux platforms
- `liv_alma.dat` – Alma Linux vulnerability data
- `liv_amazon.dat` – Amazon Linux vulnerability data
- `liv_debian.dat` – Debian vulnerability data
- `liv_mint.dat` – Linux Mint vulnerability data
- `liv_oracle.dat` – Oracle Linux vulnerability data
- `liv_redhat.dat` – Red Hat vulnerability data
- `liv_rocky.dat` – Rocky Linux vulnerability data
- `liv_suse.dat` – SUSE vulnerability data
- `liv_ubuntu.dat` – Ubuntu vulnerability data

## Patch Intelligence
- `patch_linux.dat` – Patch scripts and data for Linux OS patches
- `patch_mac.dat` – Patch scripts and data for macOS
- `patch.dat` – Patch scripts and data for 3rd-party applications on Windows

## macOS Vulnerability / Patch Intelligence
- `mav.dat` – Vulnerability data for macOS OS patches

## Windows OS Vulnerability Intelligence
- `wiv-lite.dat` – Windows OS vulnerability intelligence

## 3rd-Party Vulnerability Intelligence
- `v2mod.dat` – Vulnerability data for 3rd-party applications **including** descriptions and titles
- `v2mod-vuln-oft.dat` – Newer vulnerability dataset **without** descriptions or titles
- `vmod.dat` – **Deprecated** legacy vulnerability dataset for 3rd-party applications
- `vmod-vuln-oft.dat` – **Deprecated** legacy vulnerability dataset without descriptions or titles
