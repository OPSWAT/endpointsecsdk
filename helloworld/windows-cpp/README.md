# OPSWAT MetaDefender Security Endpoint SDK - Sample Applications

This repository provides sample applications that demonstrate how to integrate with the OPSWAT MetaDefender Security Endpoint SDK. The examples are written in C++ and cover various endpoint security use cases such as product detection, real-time monitoring, and process callbacks.

---

## Getting Started

### Prerequisites
These samples are developed using **Visual Studio 2022 Community Edition**. You can install it via `winget` or download it manually:

winget install --id=Microsoft.VisualStudio.2022.Community -e
> This command installs Visual Studio 2022 Community Edition via the Windows Package Manager (`winget`).

Or download from: [https://visualstudio.microsoft.com/vs/community/](https://visualstudio.microsoft.com/vs/community/)

### Compiling the Samples
1. Open the solution file: `/src/HelloWorldCPP.sln`
2. Copy the following required files into `/src/HelloWorldCPP/license/`:
   - `download_token.txt`
   - `license.cfg`
   - `pass_key.txt`
3. Compile the `DetectProducts` project.

### Notes
- The `SDKDownloader` utility downloads the latest SDK files into `/src/HelloWorldCPP/SDK`.
- These files refresh every 7 days. To force a new download, delete the `SDK` directory and rebuild the project.

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
