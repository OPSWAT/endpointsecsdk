# Python Scanner

A cross-platform endpoint vulnerability and patch scanner written in Python. It runs on an endpoint and uses the OESIS Framework SDK to produce a snapshot of the installed products, their known vulnerabilities (CVEs), and missing OS/system patches. It runs on Windows, macOS, and Linux.

## Purpose

The python-scanner utility enables security teams to:

- Detect installed software on an endpoint and its vulnerability status
- Query CVE/vulnerability data for each detected product
- Detect patch management agents and enumerate missing OS/system patches
- Identify the latest available installer/patch for vulnerable products
- Write structured JSON scan results that can be collected and analyzed

## Overview

This scanner is a thin client around the OESIS Framework SDK (`libwaapi`). It loads the native SDK library, initializes it with a license, feeds it the offline vulnerability and patch DAT files, and then drives the SDK through a series of scans. All vulnerability and patch intelligence comes from the SDK and its DAT files; this tool orchestrates the calls and serializes the results.

## Prerequisites

Before running the scanner you need three sets of inputs in place: the SDK libraries, the DAT files, and a license. The SDK is obtained by running the repository's `sdk-downloader` first, which populates an `OPSWAT-SDK/` directory at the repository root.

1. Run the repo-root `sdk-downloader`. This creates `OPSWAT-SDK/client/<os>/<arch>/` containing `libwaapi.*` and the DAT files for your platform.
2. Make a license available: `license.cfg` and `pass_key.txt`.

The scanner can use these inputs in two ways:

- Place them directly under the tool directory in `./sdk`, `./dat`, and `./license` (the default paths), or point the corresponding flags at them.
- Leave `./sdk` empty and let the scanner locate the SDK automatically. If `libwaapi.*` is not found in `--sdk-path`, the scanner searches up to two directory levels above for `OPSWAT-SDK/client/<os>/<arch>/`, then copies the matching SDK files into `--sdk-path` for use. If `OPSWAT-SDK` is not found, it logs an error telling you to run the SDK downloader first.

Similarly, if `license.cfg` / `pass_key.txt` are missing from `--license-path`, the scanner searches up to two levels above for an `eval-license/` directory containing both files and copies them into the license path.

## Requirements

- Python 3.6 or later (standard library only; no third-party packages required)
- The OESIS Framework SDK (`libwaapi.dll` / `.so` / `.dylib`) and matching DAT files
- A valid license (`license.cfg`, `pass_key.txt`)
- Administrator/root privileges recommended for complete detection

## Usage

The entry point is `vuln_scanner.py`. With the SDK, DAT, and license already in the default `./sdk`, `./dat`, and `./license` locations, a full scan is simply:

```bash
python3 vuln_scanner.py
```

To specify locations explicitly:

```bash
python3 vuln_scanner.py \
  --sdk-path ./sdk \
  --dat-path ./dat \
  --license-path ./license \
  --output-dir ./output
```

### Options

- `--sdk-path <dir>` - Directory containing the SDK libraries (`libwaapi.*`). Default: `./sdk` relative to the script (falls back to the script directory if `sdk/` does not exist).
- `--dat-path <dir>` - Directory containing the DAT files (`v2mod.dat`, `patch.dat`, etc.). Default: `./dat` relative to the script.
- `--license-path <dir>` - Directory containing `license.cfg` and `pass_key.txt`. Default: `./license` relative to the script.
- `--output-dir <dir>` - Directory where output files are written. Default: `./output`.
- `--scan <type> [<type> ...]` - Which scan(s) to run. One or more of `product`, `system`, `all`. Default: `all`.
- `--debug-log <path>` - Optional path for the SDK's own debug log output.
- `--help` - Display usage information.

### Examples

Run only the product scan:

```bash
python3 vuln_scanner.py --scan product
```

Run both scans explicitly with custom input/output locations:

```bash
python3 vuln_scanner.py \
  --scan product system \
  --sdk-path /opt/opswat/sdk \
  --dat-path /opt/opswat/dat \
  --license-path /opt/opswat/license \
  --output-dir /var/log/opswat-scan
```

Run a system scan and capture the SDK debug log:

```bash
python3 vuln_scanner.py --scan system --debug-log ./output/sdk_debug.log
```

## Scans

The `--scan` flag selects which scans run. `all` (the default) runs both.

### Product scan (`product`)

- Loads the vulnerability DAT files into the SDK: `v2mod.dat` (third-party applications, all platforms) plus the platform-specific system DAT (`wiv-lite.dat` on Windows, `mav.dat` on macOS, `liv.dat` on Linux).
- Loads the patch database (`patch.dat` / `patch_mac.dat` / `patch_linux.dat`, plus `wuov2.dat` on Windows).
- Detects all installed products, then for each product queries the SDK for CVE data and the latest available installer/patch.
- Produces a per-product breakdown plus a summary (total products detected, products with vulnerabilities, total CVEs, products with available patches).

### System scan (`system`)

- Detects patch management agents (e.g. Windows Update, apt/yum, macOS Software Update).
- Queries each agent for missing OS/system patches.
- Queries the SDK for CVEs affecting OS-level components.
- Produces a list of missing system patches and system vulnerabilities, plus a summary.

## Output

All output is written into `--output-dir` (created if it does not exist):

- A log file `scanner_<timestamp>.log` capturing the full run (DEBUG to file, INFO to console). This is created on every run regardless of which scans are selected.
- One JSON file per scan that runs, named `<scan_type>_<hostname>_<UTC-timestamp>.json`:
  - `product_scan_<hostname>_<timestamp>.json`
  - `system_scan_<hostname>_<timestamp>.json`

Each JSON file contains the structured scan result with a `scan_type`, `hostname`, OS info, the detected data, a `summary` block, and a `timestamp`. JSON is the only output format; there is no CSV or HTML output.

### Product scan JSON (shape)

```json
{
  "scan_type": "product",
  "hostname": "WORKSTATION-01",
  "os": { },
  "detected_products": [
    {
      "signature_id": 1234,
      "product_name": "Example Product",
      "vendor_name": "Example Vendor",
      "raw_detection": { },
      "vulnerabilities": { "has_vulnerability": true, "cves": [] },
      "patch_info": { "url": "...", "version": "..." }
    }
  ],
  "summary": {
    "total_products_detected": 45,
    "products_with_vulnerabilities": 8,
    "total_cves": 17,
    "products_with_available_patches": 10
  },
  "timestamp": "2026-06-12T10:30:00+00:00"
}
```

### System scan JSON (shape)

```json
{
  "scan_type": "system",
  "hostname": "WORKSTATION-01",
  "os": { },
  "missing_system_patches": [
    { "source_product": "Windows Update", "signature_id": 1103, "patches": [] }
  ],
  "system_vulnerabilities": [],
  "summary": {
    "total_patch_management_products": 1,
    "total_missing_patch_sources": 1,
    "total_system_vuln_sources": 0
  },
  "timestamp": "2026-06-12T10:30:00+00:00"
}
```

## Troubleshooting

**"OPSWAT-SDK directory not found" / "Could not find libwaapi"**
- Run the repo-root `sdk-downloader` first to populate `OPSWAT-SDK/`.
- Either place `libwaapi.*` directly in `--sdk-path`, or run the scanner from within the repo so it can locate `OPSWAT-SDK/client/<os>/<arch>/` two levels up and copy the files in.

**"License path not found" / missing license files**
- Ensure `license.cfg` and `pass_key.txt` exist in `--license-path`, or provide an `eval-license/` directory (with both files) up to two levels above the license path so the scanner can copy them.

**"DAT path not found"**
- Point `--dat-path` at the directory containing the DAT files for your platform (`v2mod.dat`, the platform patch DAT, etc.). Missing individual DAT files are skipped with a warning rather than aborting the scan.

**No products or no vulnerabilities reported**
- Confirm the correct DAT files for your platform are present in `--dat-path`.
- Check the `scanner_<timestamp>.log` file in the output directory for per-step detail.
- Run with administrator/root privileges for complete detection.

## Supported Platforms

- Windows
- macOS
- Linux

Architecture (x64 / x86 / win32 / arm64) is detected automatically when locating the SDK in `OPSWAT-SDK/client/`.

## Related Tools

- **vapm-scanner** - Vulnerability and patch management
- **Catalog Scripts** - CVE and signature lookup

## Support

For questions or issues:
- Review the inline module documentation (`vuln_scanner.py`, `scanner.py`, `platform_utils.py`, `output_formatter.py`)
- Contact OPSWAT support: oem@opswat.com
