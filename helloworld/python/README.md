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

The package-flow samples (`show_packages.py`, `install_package.py`, `Rollback.py`) additionally need the **v2 patch database**, which `copy_sdk_files.py` does not copy yet:

```bash
cp ../../OPSWAT-SDK/extract/analog/client/patchv2.dat sdk/
```

Those three scripts also read the server-side catalog at `OPSWAT-SDK/extract/analog/server/patch_aggregation_v2.json` (produced by the SDK downloader) to turn a signature into a `patch_uuid`. The SDK has no call that does this lookup.

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
├── invoke.py               # Invoke any SDK method from a JSON request (--json or --file)
├── detect_products.py      # List all installed applications on the endpoint
├── product_detail.py       # Full detail report for a single product by signature ID
├── patch_status.py         # Missing and installed patches for all patch management agents
├── uninstall_product.py    # Uninstall a product by signature ID
├── show_packages.py        # List every installable version of a product and which apply here (read-only)
├── install_package.py      # Install a specific version via the package (v2) flow
├── Rollback.py             # Roll a product back to an OPSWAT-approved earlier version
├── security_score.py       # OPSWAT device security score with per-category breakdown
├── collect_device_inventory.py     # Collect system/OS/BIOS/device inventory (Windows only)
└── detect_driver_firmware_patches.py  # Detect driver/firmware patches (Windows only)
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

### `invoke.py`

A general-purpose tool for invoking **any** OESIS SDK method from a raw JSON request — useful for ad-hoc calls and experimentation without writing a dedicated script. The request is read from either a `--json` string or a `--file`; the script prints the input request and the raw JSON response. Use `--debug` for diagnostics (paths, Python/platform info, request method) and `--trace` for a full Python traceback on error.

```bash
python invoke.py --file request.json                       # uses the bundled request.json (DetectProducts)
python invoke.py --json "{ \"input\": { \"method\": 0 } }"   # inline JSON (escape quotes on Windows)
python invoke.py --file request.json --debug               # with diagnostics
```

The bundled `request.json` performs a DetectProducts (`method 0`) call. Edit it — or pass your own `--file`/`--json` — to invoke any method in the [Method Reference](https://software.opswat.com/OESIS_V4/html/c_method.html). The JSON must wrap the call in an `input` object, e.g. `{ "input": { "method": 50505, "signature": 3039 } }`.

**SDK methods used:**
- Any — the method is taken from the `input.method` field in your JSON request.

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

## Package Flow (v2) — Versions, Install and Rollback

`patch.py` uses the original flow: signature → `GetLatestInstaller` → `InstallFromFiles`, which only ever installs the **latest** version. The three scripts below use the package-based flow introduced with `patchv2.dat`:

```
patch_uuid  →  GetPackages (50306)  →  InstallPackage (50305)
```

Any version in the database can be resolved and installed, the SDK evaluates each package for the current endpoint (`evaluation_status`), and installs are verified by `expected_installer_sha256`.

What the SDK **cannot** do, and why these scripts also read `patch_aggregation_v2.json`:

| Need | Source |
|---|---|
| Which versions exist for a product, and their `patch_uuid` | catalog only — `GetPackages` requires a `patch_uuid`; passing a signature returns `-20 INVALID_INPUT_ARGS` |
| Which version is an **approved rollback target** (`is_rollback_target`) | catalog only — not present in any SDK response |
| Package details for a known `patch_uuid` (URL, SHA-256, arch, applicability) | SDK (`patchv2.dat`) |
| Install, with hash verification | SDK |

---

### `show_packages.py`

Read-only, no Administrator rights. For a signature, walks every version in the catalog and calls `GetPackages` on each, printing the SDK's view (package UUID, architecture, `evaluation_status`, SHA-256, download URL) next to the catalog-only flags (`LATEST`, `ROLLBACK TARGET`, `INSTALLED`). With a bare `patch_uuid` it shows a single patch from the SDK alone.

```bash
python show_packages.py --signature 3241            # Notepad++ x64, every version
python show_packages.py --signature 3241 --latest   # current version only
python show_packages.py eeeaba57-c17b-570d-8e64-f66295cfd570   # one patch, SDK only
```

**Output:**
```
  8.9.6.4  released 06/04/2026   ROLLBACK TARGET
    patch_uuid : a2191f8b-66b0-5d11-961b-7dc0a3c21dff
    45f8a080-...  64-bit   -      applicable      sha256=CB902F8A9628324D   rollback=yes

  8.9.8  released 08/23/2026   LATEST  INSTALLED
    patch_uuid : eeeaba57-c17b-570d-8e64-f66295cfd570
    2766faba-...  64-bit   -      applicable      sha256=7B2A949BF460FB37   rollback=no

  Approved rollback targets: 8.9.6.4
```

> The SDK returns only the packages relevant to this endpoint — the catalog's arm64 packages are filtered out on an x64 machine.

**SDK methods used:**
- `50302` — LoadPatchDatabase (`patchv2.dat`)
- `50306` — GetPackages (`package_limit="all"`)
- `100` — GetVersion

---

### `install_package.py`

Installs a specific version of a product through the package flow. Takes a `patch_uuid` directly (pure SDK path) or `--signature` with an optional `--version` (resolved through the catalog; defaults to the latest). Picks the package the SDK evaluated as `applicable`, downloads it, verifies SHA-256 locally, then hands the same hash to `InstallPackage` so the SDK verifies it again before running the installer. `--download-only` stops after the download.

> **The install step requires Administrator / root access.**

```bash
python install_package.py --signature 3241                    # Notepad++ x64, latest
python install_package.py --signature 3241 --version 8.9.6.4  # a specific version
python install_package.py eeeaba57-c17b-570d-8e64-f66295cfd570
python install_package.py --signature 3241 --download-only    # stop before install
```

**SDK methods used:**
- `50302` — LoadPatchDatabase (`patchv2.dat`)
- `50306` — GetPackages
- `50305` — InstallPackage (`expected_installer_sha256`, `force_close_processes`)
- `100` — GetVersion

---

### `Rollback.py`

Rolls a product back to an **earlier** version that OPSWAT has approved as a rollback target. Refuses any version not flagged `is_rollback_target` in the catalog. The flow is:

1. Confirm the target is approved and report the version currently installed
2. Download the older installer and verify its SHA-256 — *before* anything is changed
3. Remove the current version with **AppRemover** (`40000`, `type="auto"`)
4. Install the older version with `InstallFromFiles` using `enable_rollback=true` + `requested_version`
5. Re-detect and confirm the endpoint is on the requested version

Defaults to Notepad++ x64 (signature `3241`) → `8.9.6.4`. `--list` prints every approved rollback target in the catalog (12 products at the time of writing).

> **Requires Administrator / root access** and **OESIS 4.3.6607.0 or newer** (`enable_rollback` was added in that build).

```bash
python Rollback.py --list                 # what can be rolled back?
python Rollback.py                        # Notepad++ x64  8.9.8 -> 8.9.6.4
python Rollback.py 3241 8.9.6.4 --yes     # no confirmation prompt
```

**Output:**
```
[1/3] Downloading 8.9.6.4 installer ....... Checksum verified successfully
[2/3] Removing Notepad++ 8.9.8 with AppRemover ... code 0
[3/3] Installing 8.9.6.4 (enable_rollback=true) .. code 0
======================================================================
  ROLLBACK SUCCEEDED
    Notepad++: 8.9.8  ->  8.9.6.4
======================================================================
```

Things to know before building on this:

- **Rollback is uninstall-and-reinstall, not an in-place restore.** The SDK does not guarantee application data, settings or cache survive, and only the default instance of a multi-instance app is rolled back.
- **`patchv2.dat` is mandatory.** With the v1 database loaded, every step succeeds until the install, which fails with `-1052 WA_VMOD_VERSION_LOCK_NOT_SUPPORTED` — *after* the current version has already been removed. The SDK does not auto-restore. The script refuses to start without `patchv2.dat` for this reason.
- **The application's own updater can undo the rollback.** Notepad++'s updater reinstalled 8.9.8 on the next launch during testing. A production rollback has to suppress the app's self-update (Notepad++ `noUpdate`, browser update policies, etc.); the SDK provides nothing for this today.
- **Eligibility is per package.** Notepad++ 8.9.6.4 is a rollback target for x64 and x86 but not arm64, although an arm64 package exists.

**SDK methods used:**
- `50302` — LoadPatchDatabase (`patchv2.dat`)
- `109` — GetProductInfo (with `run_detection=True`)
- `100` — GetVersion
- `40000` — Uninstall / AppRemover (`type="auto"`)
- `50301` — InstallFromFiles (`enable_rollback=true`, `requested_version`)

---

### `security_score.py`

Calculates the OPSWAT device security score and prints the overall score plus a per-category breakdown (Anti Malware, Antiphishing, Patch Management, Vulnerabilities, Encryption, Firewall, Backup, Unwanted Apps). Loads the offline CVE database first so the Vulnerabilities category is scored against real CVE data. Writes the full result to `security_score.json`.

```bash
python security_score.py            # force a fresh refresh (default)
python security_score.py cached     # use cached values, no refresh
```

> The overall `score_status` reflects the **weakest** category, so a single `poor` category reports the device as `poor` even when the numeric total is high.

**SDK methods used:**
- `50520` — ConsumeOfflineVmodDatabase (so the Vulnerabilities category has CVE data)
- `111` — GetSecurityScore

**Output:**
```
  Total Score   : 90 / 100
  Score Status  : poor

  Anti Malware              30/30       good
  Vulnerabilities           0/10        poor
  Firewall                  10/10       good
```

---

### `collect_device_inventory.py`

> **Windows only.** Requires a license that entitles the driver/firmware feature.

Collects endpoint inventory data (system, OS, BIOS, and hardware devices) used for driver/firmware patch matching. Prints a summary with a per-device-class count and writes the full inventory to `device_inventory.json` (or a custom path).

```bash
python collect_device_inventory.py                 # writes device_inventory.json
python collect_device_inventory.py D:\inv.json     # custom output path
```

**SDK methods used:**
- `50900` — LoadDriverFirmwareDatabase (initializes the driver/firmware vmod; uses `patch_driver_firmware.dat`)
- `50901` — CollectDeviceInventory

**Output:**
```
  System
    Vendor        : Dell Inc.
    Product Name  : Latitude 5450

  Devices: 305 detected
    Net                       16
    USB                       16
    Firmware                  2
```

---

### `detect_driver_firmware_patches.py`

> **Windows only.** Requires a license that entitles the driver/firmware feature.

Detects applicable driver/firmware patches by matching the device inventory against the loaded driver/firmware database. Maps each patch's `reboot_required` code to a label and preserves the full `download_urls` array (URLs + SHA1/256/512/MD5 + size) in `driver_firmware_patches.json`. Optionally accepts a prebuilt inventory file from `collect_device_inventory.py`.

```bash
python detect_driver_firmware_patches.py                      # collect inventory internally
python detect_driver_firmware_patches.py device_inventory.json  # use a prebuilt inventory
```

> If the device model is not in the loaded catalog the SDK returns `WA_VMOD_ERROR_MODEL_NOT_SUPPORTED` (rc `-1067`); the script reports this as a clean "no coverage" result rather than failing.

**SDK methods used:**
- `50900` — LoadDriverFirmwareDatabase
- `50902` — DetectDriverFirmwarePatches

---

## Quick Start

```bash
# 1. Place license.cfg, pass_key.txt, download_token.txt in <repo_root>/eval-license/

# 2. Download the SDK libraries with the repo-root SDK downloader
#    Windows:      sdk-downloader\windows-csharp\bin\SDKDownloader.exe
#    Linux/macOS:  python3 sdk-downloader/script/src/main.py

# 3. Stage the SDK binaries + license files into the local sdk/ directory
python copy_sdk_files.py
#    (for show_packages / install_package / Rollback, also copy the v2 patch database)
cp ../../OPSWAT-SDK/extract/analog/client/patchv2.dat sdk/

# 4. Run any sample
python detect_products.py
python vulnerability.py
python product_detail.py 3039
python show_packages.py --signature 3241
```

---

## Reference Links

- [OESIS SDK Documentation](https://software.opswat.com/OESIS_V4/html/index.html)
- [SDK Initialisation](https://software.opswat.com/OESIS_V4/html/c_sdk.html)
- [Method Reference](https://software.opswat.com/OESIS_V4/html/c_method.html)
- [Return Codes](https://software.opswat.com/OESIS_V4/html/c_return_codes.html)
