# OESIS Framework - Sample Applications

This repository provides sample applications that demonstrate how to integrate with the OESIS Framework. The examples are written in C++ and cover various endpoint security use cases such as product detection, real-time monitoring, and process callbacks.

---

## Getting Started

### Prerequisites
These samples are developed using **Visual Studio 2022 Community Edition**. You can install it via `winget` or download it manually:

```powershell
winget install --id=Microsoft.VisualStudio.2022.Community -e
```
> This command installs Visual Studio 2022 Community Edition via the Windows Package Manager (`winget`).

Or download from: [https://visualstudio.microsoft.com/vs/community/](https://visualstudio.microsoft.com/vs/community/)

### Compiling the Samples
1. Open the solution file: `src/HelloWorldCPP.sln`
2. Place your license files in the `eval-license/` directory at the repository root:
   - `license.cfg`
   - `pass_key.txt`
   - `download_token.txt`

   Each project's post-build step (`copysdkfilescpp.ps1`) stages them — and the SDK binaries — into the build output automatically. See [eval-license/README.md](../../eval-license/README.md).
3. Compile the `DetectProducts` project.

### Notes
- **The SDK downloader must be run before building.** Use the cross-platform downloader at the repo root — `sdk-downloader\windows-csharp\bin\SDKDownloader.exe` (or `sdk-downloader\script\src\main.py`) — which resolves the repo root via the `sdkroot` marker and populates `OPSWAT-SDK\`.
- Each project's post-build step runs `copysdkfilescpp.ps1`, which stages the SDK headers, libraries, and binaries from `OPSWAT-SDK\` (and license files from `eval-license\`) into the build output. If the SDK files are missing, the build stops with an error telling you to run the downloader first.

---

## Sample Applications
Each sample project demonstrates a different feature of the SDK.

### DetectProducts — List Installed Products & Firewall Status
Scans the system for installed security products (e.g., firewalls) and checks if they are active. Outputs the status of each detected product.

**Files:**
- `DetectProducts.cpp`: Core logic of product detection.
- `Utils.cpp`: Utility functions for string processing.

---

### ProcessCallback — Callback for Process Execution
Demonstrates how to register a callback that is triggered whenever a process starts.

**Files:**
- `ProcessCallback.cpp`: Main logic for setting up and handling process callbacks.
- `Utils.cpp`: Utility code for string and logging support.

---

### RealTimeMonitoring — Event-Based Monitoring for System Changes
Watches in real-time for events such as:
- Process start/stop
- Software installs/uninstalls
- Firewall enable/disable

**Files:**
- `RealTimeMonitoring.cpp`: Registers and handles event callbacks.
- `Utils.cpp`: Shared utility functions.

---

## Coming Soon
Additional examples and advanced use cases will be added to this repository over time.

---

For detailed SDK usage, refer to the official OPSWAT documentation or contact support if needed.
