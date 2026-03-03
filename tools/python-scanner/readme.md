# OPSWAT Endpoint Vulnerability Scanner

A Python demonstration tool that uses the OPSWAT OESIS SDK to scan an endpoint for installed software, known CVE vulnerabilities, and missing OS patches. It is intended as a reference implementation showing how to integrate the OESIS SDK into a Python workflow.

## What It Does

- Detects all installed products on the endpoint and cross-references them against the OESIS vulnerability database to identify known CVEs
- Identifies missing OS patches by querying platform patch management agents (Windows Update, apt/dpkg, macOS Software Update, etc.)
- Outputs results as timestamped JSON files in a configurable output directory
- Logs all activity to both the console and a log file for easy debugging

## Prerequisites

### 1. SDK Libraries — Run SDK_Downloader First

The OESIS native libraries (`libwaapi` and its dependencies) are **not included** in this repository and must be downloaded before the scanner can run. Use the **SDK_Downloader** tool to fetch them:

```
SDK_Downloader
```

The downloader will populate the following directory structure under `OPSWAT-SDK/`:

```
OPSWAT-SDK/
  client/
    linux/
      arm64/
      x64/
      x86/
    mac/
    windows/
      arm64/
      win32/
      x64/
```

The scanner will automatically locate the correct architecture directory and copy the libraries to the local `sdk/` folder on first run. If the `OPSWAT-SDK` directory is not found, the scanner will prompt you to run SDK_Downloader.

### 2. License Files

A valid OPSWAT evaluation license is required. Place the following two files in the `eval-license/` directory, **two levels above the script**:

```
<repo_root>/
  eval-license/
    license.cfg
    pass_key.txt
  <your_project>/
    vuln_scanner.py
    ...
```

| File | Description |
|---|---|
| `license.cfg` | JSON file containing the `license` and `license_key` fields provided by OPSWAT |
| `pass_key.txt` | Plain text passkey string provided by OPSWAT |

If these files are missing from the configured `--license-path`, the scanner will automatically search for the `eval-license` directory up to two levels above the script and copy the files into place. If neither location contains the license files, the scanner will exit with an error.

> Contact OPSWAT to obtain evaluation license credentials.

## Directory Layout

```
<project>/
  vuln_scanner.py        # Main entry point
  scanner.py             # Scan logic (product and system scans)
  sdk_wrapper.py         # ctypes wrapper around libwaapi
  platform_utils.py      # OS/architecture detection and path resolution
  output_formatter.py    # JSON output writer
  sdk/                   # SDK libraries copied here on first run
  dat/                   # DAT files (vulnerability/patch databases)
  license/               # License files (or auto-copied from eval-license)
  output/                # Scan results and log files written here
```

## Usage

```bash
python3 vuln_scanner.py
```

All paths default to subdirectories relative to the script. Override any of them with the options below:

```bash
python3 vuln_scanner.py \
    --sdk-path     ./sdk \
    --dat-path     ./dat \
    --license-path ./license \
    --output-dir   ./output \
    --scan         all
```

### Arguments

| Argument | Default | Description |
|---|---|---|
| `--sdk-path` | `./sdk` | Directory containing SDK libraries (`libwaapi.*`) |
| `--dat-path` | `./dat` | Directory containing DAT files (`v2mod.dat`, `patch.dat`, etc.) |
| `--license-path` | `./license` | Directory containing `license.cfg` and `pass_key.txt` |
| `--output-dir` | `./output` | Directory where JSON results and log files are written |
| `--scan` | `all` | Which scans to run: `product`, `system`, or `all` |
| `--debug-log` | _(none)_ | Optional path for verbose SDK debug log output |

## Output

Each run produces files in `--output-dir`:

- `product_scan_<hostname>_<timestamp>.json` — per-product CVE and patch data
- `system_scan_<hostname>_<timestamp>.json` — missing OS patches and system-level CVEs
- `scanner_<timestamp>.log` — full debug log of the run

## Supported Platforms

| Platform | Architectures |
|---|---|
| Linux | x64, x86, arm64 |
| macOS | Universal (arm64 + x86_64) |
| Windows | x64, win32, arm64 |

## Troubleshooting

**`OPSWAT-SDK directory not found`**
Run SDK_Downloader to download the SDK libraries before running the scanner.

**`license.cfg / pass_key.txt not found`**
Ensure both files are present in either `--license-path` or the `eval-license/` directory two levels above the script.

**`Failed to load SDK library`**
Ensure SDK_Downloader completed successfully and that the libraries for your OS and architecture are present under `OPSWAT-SDK/client/`.
