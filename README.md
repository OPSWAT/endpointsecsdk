# OESIS Framework Samples and Utilities

This repository contains sample applications, scripts, and supporting utilities for evaluating and integrating the **OESIS Framework (OESIS SDK)** across Windows, macOS, and Linux.  
It includes everything needed to get started with vulnerability, patch, and compliance detection, as well as SDK integration examples.

---

## 📦 Repository Overview

| Folder | Description |
|:-------|:-------------|
| [**`helloworld/`**](helloworld/README.md) | Sample “Hello World” projects for multiple languages and platforms (Python, Windows C++, Windows C#, Windows Go, Linux C++, macOS C++). Demonstrates basic SDK initialization, scanning, and result retrieval. |
| [**`scripts/`**](scripts/README.md) | Python-based data merge and catalog utilities used for building, combining, or analyzing patch and vulnerability datasets. Includes examples for merging analog data, generating associations, and producing JSON outputs. |
| [**`tools/`**](tools/README.md) | Broader utilities for patch scanning, catalog analysis, and endpoint posture reporting using the SDK. These scripts showcase higher-level posture logic and aggregation. |
| [**`eval-license/`**](eval-license/README.md) | Directory containing evaluation and licensing artifacts. To use the SDK, place your **license and token files** here. |
| [**`sdk-downloader/`**](sdk-downloader/README.md) | Cross-platform downloader utility that retrieves and organizes SDK client binaries. Supports **Windows**, **Linux**, and **macOS**. |

---

## ⚙️ Getting Started

### Quick start (Windows)

First obtain an evaluation license (see step 1 below) and place the files in `eval-license/`. Then, from the repository root, run the setup helper:

```bat
get-started.bat
```

It verifies your license files, ensures Python is installed (installing it via `winget` if missing), installs the `requests` package, and runs the SDK downloader to populate `OPSWAT-SDK/`. When it finishes, jump to step 4 to run a sample.

### Manual setup

**1. Obtain an evaluation license**  
Request a license key and download token by contacting **oem@opswat.com**.

**2. Prepare the environment**  
- Make sure the `sdkroot` marker file is present at the repository root.  
- Place your license and token files in `eval-license/`:
  ```
  eval-license/
    ├── license.cfg
    ├── pass_key.txt
    └── download_token.txt
  ```
  See [eval-license/README.md](eval-license/README.md) for details.

**3. Download the SDK libraries**  
Run the [`sdk-downloader`](sdk-downloader/README.md) for your platform:
- **Windows:** `sdk-downloader\windows-csharp\bin\SDKDownloader.exe`
- **Linux/macOS:** `python3 sdk-downloader/script/src/main.py`

**4. Run a sample**  
For example, the Python HelloWorld:
```bash
cd helloworld/python
python copy_sdk_files.py     # stage the SDK + license into ./sdk
python detect_products.py
```
See [helloworld/README.md](helloworld/README.md) for all samples and languages.

---

## 🚀 SDK Downloader

The **sdk-downloader** automates the process of retrieving platform-specific client libraries for the OESIS Framework.

| Platform | Downloader Type | Location | Output |
|-----------|-----------------|-----------|---------|
| **Windows** | C# .NET Executable (pre-compiled) | `sdk-downloader/windows-csharp/bin/SDKDownloader.exe` | `OPSWAT-SDK/client/windows/<architecture>/` |
| **Linux/macOS** | Python Script | `sdk-downloader/script/src/main.py` | `OPSWAT-SDK/client/<platform>/<architecture>/` |

### 🪟 Windows Example
```powershell
cd sdk-downloader\windows-csharp\bin
.\SDKDownloader.exe
```

### 🐍 Linux/macOS Example
```bash
cd sdk-downloader/script/src
python3 main.py
```

### Requirements
- `eval-license/download_token.txt` must contain your **valid SDK download token**.  
- `sdkroot` must exist in the repository root.  
- The downloader automatically detects your platform and organizes libraries under:
  ```
  OPSWAT-SDK/client/<platform>/<architecture>/
  ```

---

## 🧠 Folder Details

### `helloworld/`
Contains minimal working examples demonstrating SDK initialization, product scanning, and reporting vulnerability and patch status.  
Includes examples for:
- **Python** (cross-platform)
- **Windows-C++**
- **Windows-C#**
- **Windows-Go**
- **Linux-C++**
- **macOS-C++**

All examples rely on the SDK binaries placed under `/OPSWAT-SDK/client/...` and use the `sdkroot` file for discovery.

---

### `scripts/`
Contains Python utilities for working with SDK output and catalog data.  
Key areas:
- **catalog-appcentric** → merges analog and patch data for product mapping.  
- **data-merging** → combines system and third-party patch data into unified structures.  
- **schema-overviews** → documentation and schema references for generated JSON data.

---

### `tools/`
Includes posture scanning utilities and patch analysis scripts.  
These demonstrate how to extend SDK functionality for compliance, vulnerability, and remediation workflows.

---

### `eval-license/`
Holds all evaluation-related files:
```
license.cfg
pass_key.txt
download_token.txt
```
These are required to run the downloader and initialize the SDK.

---

### `sdk-downloader/`
Automates the setup of SDK client libraries for all supported platforms.  
Each platform’s downloader retrieves the latest SDK binaries compatible with **AnalogV2** format.

**Notes:**
- A valid download token is required in `eval-license/download_token.txt`.  
- The Windows executable and Python script produce the same standardized folder structure for SDK clients.  
- The C# source code is included for developers who wish to rebuild or extend the downloader.

---

## 🧰 Troubleshooting

| Issue | Likely Cause | Resolution |
|--------|---------------|-------------|
| “SDK root not found” | Missing `sdkroot` marker file | Ensure the root contains the file `sdkroot`. |
| “Authentication failed” | Invalid or missing token | Verify `eval-license/download_token.txt` contains a valid token. |
| “Access denied” | Permission issue | Run PowerShell or terminal with elevated privileges. |
| Missing client binaries | Downloader not executed | Run the appropriate downloader for your OS. |

---

## 🧩 Development Notes

- Both downloader variants (Windows `.exe` and Python script) are **AnalogV2 compatible**.  
- SDK data schemas (`patch_aggregation.json`, `vuln_associations.json`, etc.) may change with new releases — check for updates periodically.  
- Contributions or feedback are welcome — please submit pull requests or open issues in this repository.

---

## 📬 Support

For evaluation access, SDK updates, or OEM integration inquiries, contact:  
**📧 oem@opswat.com**

For general information, visit:  
👉 [https://www.opswat.com/products/oesis-framework](https://www.opswat.com/products/oesis-framework)
