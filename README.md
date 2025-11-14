# OESIS Framework Samples and Utilities

This repository contains sample applications, scripts, and supporting utilities for evaluating and integrating the **OESIS Framework (OESIS SDK)** across Windows, macOS, and Linux.  
It includes everything needed to get started with vulnerability, patch, and compliance detection, as well as SDK integration examples.

---

## 📦 Repository Overview

| Folder | Description |
|:-------|:-------------|
| **`helloworld/`** | Sample “Hello World” projects for multiple languages and platforms (Windows C++, Windows C#, Windows Go, Linux C++, macOS C++). Demonstrates basic SDK initialization, scanning, and result retrieval. |
| **`scripts/`** | Python-based data merge and catalog utilities used for building, combining, or analyzing patch and vulnerability datasets. Includes examples for merging analog data, generating associations, and producing JSON outputs. |
| **`tools/`** | Broader utilities for patch scanning, catalog analysis, and endpoint posture reporting using the SDK. These scripts showcase higher-level posture logic and aggregation. |
| **`eval-license/`** | Directory containing evaluation and licensing artifacts. To use the SDK, place your **evaluation key** and **download token** here. |
| **`sdk-downloader/`** | Cross-platform downloader utility that retrieves and organizes SDK client binaries. Supports **Windows**, **Linux**, and **macOS**. |

---

## ⚙️ Getting Started

### 1. Obtain an Evaluation License  
Request an evaluation key and download token by contacting:  
**📧 oem@opswat.com**

### 2. Prepare the Environment  
- Extract the provided evaluation SDK package at the repository root.  
- Ensure the root includes the file:  
  ```
  sdkroot
  ```
- Place your license and token in:  
  ```
  eval-license/
    ├── eval_license.key
    └── download-token.txt
  ```

### 3. Download the SDK Libraries  
Use the `sdk-downloader` utility to download the correct binaries for your platform.

---

## 🚀 SDK Downloader

The **sdk-downloader** automates the process of retrieving platform-specific client libraries for the OESIS Framework.

| Platform | Downloader Type | Location | Output |
|-----------|-----------------|-----------|---------|
| **Windows** | C# .NET Executable (pre-compiled) | `sdk-downloader/windows-csharp/bin/Debug/SDKDownloader.exe` | `OPSWAT-SDK/client/windows/<architecture>/` |
| **Linux/macOS** | Python Script | `sdk-downloader/script/src/sdk_downloader.py` | `OPSWAT-SDK/client/<platform>/<architecture>/` |

### 🪟 Windows Example
```powershell
cd sdk-downloader\windows-csharp\bin\Debug
.\SDKDownloader.exe
```

### 🐍 Linux/macOS Example
```bash
cd sdk-downloader/script/src
python3 sdk_downloader.py
```

### Requirements
- `eval-license/download-token.txt` must contain your **valid SDK download token**.  
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
eval_license.key
download-token.txt
```
These are required to run the downloader and initialize the SDK.

---

### `sdk-downloader/`
Automates the setup of SDK client libraries for all supported platforms.  
Each platform’s downloader retrieves the latest SDK binaries compatible with **AnalogV2** format.

**Notes:**
- A valid download token is required in `eval-license/download-token.txt`.  
- The Windows executable and Python script produce the same standardized folder structure for SDK clients.  
- The C# source code is included for developers who wish to rebuild or extend the downloader.

---

## 🧰 Troubleshooting

| Issue | Likely Cause | Resolution |
|--------|---------------|-------------|
| “SDK root not found” | Missing `sdkroot` marker file | Ensure the root contains the file `sdkroot`. |
| “Authentication failed” | Invalid or missing token | Verify `eval-license/download-token.txt` contains a valid token. |
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
