# OPSWAT Endpoint SDK Samples

This repository contains example integrations, sample applications, and utilities demonstrating how to use the **OPSWAT Endpoint Security SDK**. These samples provide reference implementations for device posture, patch management, and vulnerability scanning.  

All samples and programs here require a license of the MetaDefender Endpoint SDK.  To obtain an evaluation copy email OEM@OPSWAT.com.

---

## 📁 Repository Structure

| Folder                                                   | Purpose / Use Case           | Description                                                                                                                          |
| -------------------------------------------------------- | ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| **HelloWorld-CPP**                                       | C++ Sample                   | Minimal example showing basic SDK initialization and product query.                                                                  |
| **HelloWorld-Linux**                                     | Linux Native Sample          | Linux version of HelloWorld demonstrating platform build differences.                                                                |
| **HelloWorld-Mac**                                       | macOS Native Sample          | macOS build configuration and usage example.                                                                                         |
| **HelloWorld**                                           | Cross-Platform Wrapper       | General HelloWorld sample that uses the SDK Downloader to fetch required libs.                                                       |
| **OPSWAT-SDK-Downloader**                                | SDK Download Utility         | C# utility to automatically download the SDK components and place them in the expected structure (e.g., `libs` and `inc`).           |
| **VAPM-Scanner**                                         | Vulnerability & Patch Module | Demonstrates how to scan systems for missing patches and vulnerabilities. Supports both online and offline patch catalog operations. |
| **opswat-posture**                                       | Device Posture Example       | Illustrates how to collect device posture data and evaluate compliance policies.                                                     |
| **language-examples/go**                                 | Go Integration Sample        | Shows how to call the SDK from Go using `cgo` bindings. Includes `build.ps1` automation.                                             |
| **catalog-scripts/AppCentricFile**                       | Patch Catalog Tools          | Scripts to build and update the patch catalog and JSON schemas. Useful for backend catalog generation or validation.                 |
| **download-patch**                                       | Patch Download Example       | Example for downloading and applying SDK-provided patches.                                                                           |
| **simple-samples/vulnerability/scan3rdParty-cSharp/src** | C# Vulnerability Sample      | Simple 3rd-party scanning sample demonstrating vulnerability API usage.                                                              |

---

## ⚙️ Typical Workflow

1. **Download the SDK Libraries**
   Run the downloader to fetch all SDK binaries and header files.

   ```powershell
   cd OPSWAT-SDK-Downloader\C-Sharp\bin
   .\SDKDownloader.exe
   ```

2. **Pick a Sample**
   Choose a relevant sample depending on your target language or platform. Examples:

   * `HelloWorld-CPP` → for C++
   * `VAPM-Scanner` → for vulnerability scanning
   * `language-examples/go` → for Go

3. **Build the Sample**

   * Windows: run PowerShell build scripts (`build.ps1`)
   * Linux/macOS: use provided Makefiles or `cmake`

4. **Run and Test**
   Execute the built sample to validate SDK setup and integration.

---

## 🔑 Key Features Demonstrated

* **Device Trust** – Detecting and reporting endpoint posture (AV, EDR, firewall, encryption, etc.)
* **Patch & Vulnerability Management** – Identifying missing patches, mapping CVEs, and performing offline scans
* **Cross-Platform SDK Integration** – Using the same core API on Windows, macOS, and Linux
* **Evaluation License & Token Handling** – Using license files and tokens for SDK activation

---

## 🧱 Folder Relationships

* `OPSWAT-SDK-Downloader` → Downloads core SDK binaries
* `HelloWorld-*` → Entry-level integrations
* `VAPM-Scanner` & `opswat-posture` → Feature demonstrations
* `catalog-scripts` → Backend data processing tools
* `language-examples` → Multi-language bindings (C++, C#, Go)

---

## 🧰 Prerequisites

* **Windows:** Visual Studio or GCC (MinGW)
* **macOS:** Xcode command-line tools
* **Linux:** GCC and Make
* **Go:** Version 1.22 or newer (for Go bindings)

Ensure your environment variables and license files are properly configured before running samples.

---

## 🧠 Troubleshooting

| Issue                          | Possible Fix                                                         |
| ------------------------------ | -------------------------------------------------------------------- |
| SDK not found                  | Verify SDKDownloader completed successfully and `libs` folder exists |
| Permission denied              | Run PowerShell as Administrator or use `sudo` on Unix systems        |
| Build fails on missing headers | Check `inc` folder placement and environment paths                   |

---

## 🧩 Contributing

Pull requests are welcome! If you’d like to add new samples or update an existing one, please follow these steps:

1. Fork the repository
2. Create a feature branch
3. Submit a PR with clear description and test results

---

## 📄 License

This repository is provided for demonstration purposes under the OPSWAT SDK evaluation terms. Redistribution or public hosting of SDK binaries requires an active OPSWAT license agreement.

---


**Author:** OPSWAT OEM Solutions Team
**Contact:** [https://www.opswat.com](https://www.opswat.com)
**Last Updated:** October 2025
