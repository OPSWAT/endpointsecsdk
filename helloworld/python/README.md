# OPSWAT OESIS SDK — Python Sample Code

A collection of Python sample scripts demonstrating how to integrate the [OPSWAT OESIS SDK](https://software.opswat.com/OESIS_V4/html/index.html) to detect installed software, query vulnerability and patch data, and manage applications on an endpoint.

These scripts are a direct Python translation of the official C# sample code, sharing the same SDK API calls and logic.

---

## Prerequisites

### 1. Download the OESIS SDK Libraries

For Mac, Linux and Windows you can use the python script to download the build
Navigate to %samples_root%\sdk-downloader\script\src
**run "python3 main.py"**

OR

On Windows there is also a native executable for downloading the build 
Navigate to %samples_root%\sdk-downloader\windows-csharp\bin
**run SDKDownloader.exe**

The downloader will populate the following structure:

```
OPSWAT-SDK/
  client/
    linux/
      arm64/   x64/   x86/
    mac/
    windows/
      arm64/   win32/   x64/
```




### 2. License Files

A valid OPSWAT evaluation license is required. Place the two license files in an `eval-license/` directory at the repo root:

```
<repo_root>/
  eval-license/
    license.cfg       # JSON file with license and license_key fields
    pass_key.txt      # Plain text passkey string
  sdkroot             # Empty marker file used to locate the repo root
  <your_project>/
    *.py
```

### 3. Prepare the Environment

Before running any sample script, run `copy_sdk_files.py` once to copy the correct SDK binaries and license files into the local `sdk/` directory:

```bash
python copy_sdk_files.py
```

Every sample will check that the `sdk/` directory is ready and exit with a clear message if it is not.

### 4. Python Version

Python 3.7 or later is required. No third-party packages are needed — all dependencies are from the standard library.

---

## Project Structure

```
.
├── sdk/                    # Populated by copy_sdk_files.py — SDK binaries + license files
├── copy_sdk_files.py       # Environment setup — run this first
├── platform_utils.py       # Shared platform detection and SDK path utilities
├── sdk_wrapper.py          # ctypes wrapper around the OESIS libwaapi native library
├── compliance.py           # Check firewall product state
├── vulnerability.py        # Scan all products for CVE vulnerabilities
├── patch.py                # Download and install the latest patch for a product
├── inline_license.py       # SDK initialisation using inline license bytes
├── detect_products.py      # List all installed applications on the endpoint
├── product_detail.py       # Full detail report for a single product by signature ID
├── patch_status.py         # Missing and installed patches for all patch management agents
└── uninstall_product.py    # Uninstall a product by signature ID
```

---

## Environment Setup

### `copy_sdk_files.py`

Copies the correct OESIS SDK binaries and license files into the local `sdk/` directory. Detects the current OS and architecture automatically and resolves the repo root by walking up from the script looking for an `sdkroot` marker file.

```bash
python copy_sdk_files.py
```

**Supported platforms:** Windows (x64, win32, arm64), macOS, Linux (x64, x86, arm64)

---

## Shared Utilities

### `platform_utils.py`

Contains shared helper functions used by all sample scripts:

- `validate_sdk_environment(sdk_dir)` — checks that `libwaapi`, `license.cfg`, and `pass_key.txt` are all present in the `sdk/` directory before any script proceeds
- `get_lib_filename()` — returns the correct library filename for the current OS (`libwaapi.dll` / `.dylib` / `.so`)
- `get_dat_files(dat_path)` — returns platform-specific DAT file paths
- `resolve_sdk_lib_path(sdk_path)` — locates the SDK library, falling back to the repo structure if needed
- `get_os_type()`, `get_architecture()`, `get_hostname()`

### `sdk_wrapper.py`

A `ctypes` wrapper around the OESIS `libwaapi` native library that exposes three SDK operations as Python methods:

| Method | Description |
|---|---|
| `sdk.setup(license_cfg, pass_key)` | Initialise the SDK with license credentials |
| `sdk.invoke(method_id, **params)` | Call any SDK method by ID, returns `(rc, result_dict)` |
| `sdk.teardown()` | Deinitialise the SDK |

Return codes follow the [OESIS return code specification](https://software.opswat.com/OESIS_V4/html/c_return_codes.html) — any value `>= 0` is a success.

---

## Sample Scripts

### `compliance.py`

Detects all firewall products installed on the endpoint (category 7) and reports whether each one is currently running.

```bash
python compliance.py
```

**SDK methods used:**
- `0` — DetectProducts (category 7 = Firewall)
- `1007` — GetFirewallState

---

### `vulnerability.py`

Loads the offline CVE vulnerability database (`v2mod.dat`), detects all installed products, and reports which ones have known vulnerabilities along with the CVE count. Writes full detail to `vulnerabilityResult.json`.

```bash
python vulnerability.py
```

**SDK methods used:**
- `50520` — ConsumeOfflineVmodDatabase
- `0` — DetectProducts (category 0 = All)
- `50505` — GetProductVulnerability

**Output:**
```
Firefox                                  Vulnerable    12
Google Chrome                            Clean
```

---

### `patch.py`

Downloads and installs the latest available patch for a product. Verifies the downloaded file against its SHA-256 checksum before installing. Defaults to Firefox (signature `4046`).

> **Requires Administrator / root access to install.**

```bash
python patch.py              # Firefox (default)
python patch.py 3039         # Different product by signature ID
```

**SDK methods used:**
- `50302` — LoadPatchDatabase
- `50300` — GetLatestInstaller
- `50301` — InstallFromFiles

---

### `inline_license.py`

Minimal example demonstrating SDK initialisation using inline license bytes read from `license.cfg` rather than relying on a license file present on disk at runtime.

```bash
python inline_license.py
```

---

### `detect_products.py`

Detects all installed applications on the endpoint, sorted alphabetically by product name. Accepts an optional product category to filter results. Writes output to `detected_products_<category>.json`.

```bash
python detect_products.py              # All categories (default)
python detect_products.py 5            # Antimalware only
python detect_products.py 7            # Firewall only
```

**Category reference:**

| ID | Category |
|---|---|
| 0 | All |
| 1 | Public File Sharing |
| 2 | Backup |
| 3 | Disk Encryption |
| 4 | Antiphishing |
| 5 | Antimalware |
| 6 | Browser |
| 7 | Firewall |
| 8 | Instant Messenger |
| 9 | Cloud Storage |
| 10 | Unclassified |
| 11 | Data Loss Prevention |
| 12 | Patch Management |
| 13 | VPN Client |
| 14 | Virtual Machine |
| 15 | Health Agent |
| 16 | Remote Control |
| 17 | Peer to Peer |
| 18 | Web Conference |

**SDK methods used:**
- `0` — DetectProducts
- `100` — GetVersion (called per product for reliable version data)

**Output:**
```
Firefox                                  Mozilla                         131.0.2               sig=3039
Google Chrome                            Google                          130.0.6723.116        sig=4046
```

---

### `product_detail.py`

Produces a full detail report for a single product identified by signature ID. Queries product info, version, vulnerability data, and patch status, printing each section to the console and writing a combined `product_detail_<signature_id>.json` file. Defaults to Firefox (signature `3039`).

```bash
python product_detail.py              # Firefox (default)
python product_detail.py 4046         # Different product by signature ID
```

If the signature ID does not correspond to an installed product the script exits immediately with a clear error message.

**SDK methods used:**
- `3` — GetProductInfo (with `run_detection=True`)
- `100` — GetVersion
- `50505` — GetProductVulnerability
- `50300` — GetLatestInstaller (patch status)

---

### `patch_status.py`

Detects all patch management agents on the endpoint (category 12) and queries each one for its missing and installed patches. Prints a per-agent summary table and writes full detail to `patch_status.json`.

```bash
python patch_status.py
```

**SDK methods used:**
- `0` — DetectProducts (category 12 = Patch Management)
- `50302` — LoadPatchDatabase
- `1013` — GetMissingPatches
- `1014` — GetInstalledPatches

**Output:**
```
============================================================
  Windows Update  (Microsoft)
  Signature ID: 1234
============================================================

  Missing Patches:
    Count: 3
    KB5031455             2023-10 Cumulative Update for Windows 11    [Critical]

  Installed Patches:
    Count: 147
```

---

### `uninstall_product.py`

Uninstalls a product from the endpoint by signature ID. Looks up the product name before proceeding and requires explicit confirmation before running the uninstall. Use `detect_products.py` to find the signature ID for any installed product.

> **Requires Administrator / root access.**

```bash
python uninstall_product.py 3039
```

Exits with a usage message if no signature ID is provided:

```
ERROR: A signature ID is required.

Usage:   python uninstall_product.py <signature_id>
Example: python uninstall_product.py 3039

Run detect_products.py to list all installed products and their signature IDs.
```

**SDK methods used:**
- `3` — GetProductInfo (with `run_detection=True`)
- `50303` — UninstallProduct

---

## Quick Start

```bash
# 1. Download SDK libraries using SDK_Downloader (provided by OPSWAT)
SDK_Downloader

# 2. Place license.cfg and pass_key.txt in <repo_root>/eval-license/

# 3. Create the sdkroot marker file at the repo root
touch sdkroot          # macOS / Linux
echo. > sdkroot        # Windows

# 4. Copy SDK binaries and license files into the local sdk/ directory
python copy_sdk_files.py

# 5. Run any sample
python detect_products.py
python vulnerability.py
python product_detail.py 3039
```

---

## Reference Links

- [OESIS SDK Documentation](https://software.opswat.com/OESIS_V4/html/index.html)
- [SDK Initialisation](https://software.opswat.com/OESIS_V4/html/c_sdk.html)
- [Method Reference](https://software.opswat.com/OESIS_V4/html/c_method.html)
- [Return Codes](https://software.opswat.com/OESIS_V4/html/c_return_codes.html)
