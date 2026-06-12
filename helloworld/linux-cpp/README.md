# OESIS Framework - Sample Applications

This repository provides sample applications that demonstrate how to integrate with the OESIS Framework. The examples are written in C++ and cover various endpoint security use cases such as product detection.

---

## Getting Started

### Prerequisites
These are using g++ for compiling and using the SDK. There is a script that will install the corresponding libraries for downloading, parsing and running the SDK.

> **The SDK downloader must be run before the samples can build.** The `install-pre-req` script runs it for you (`sdk-downloader/script/src/main.py`) and then stages the downloaded headers and libraries into `src/sdk`. If the downloader has not produced the SDK files, `install-pre-req` stops with a clear error (exit code `2`) telling you to run it first.

### Compiling the Samples
1. First run the Prerequisites script to install the necessary dependencies. This can be done by running the following command in your terminal:
   ```bash
   src/install-pre-req
   ```
   This installs the required build dependencies, **runs the SDK downloader** to populate `OPSWAT-SDK/`, and stages the SDK headers and libraries into `src/sdk` (`src/sdk/inc` and `src/sdk/lib/x64`) where the sample Makefiles expect them.

2. Try running the Detect Products example.  Navidate to the DetectProducts Folder and type 'make' in the terminal. This will compile the DetectProducts example and create an executable file.

   ```bash
   cd src/HelloWorldCPP/DetectProducts
   make
   ```

3. Run detect_prodcuts by typing the following command in the terminal:
   ```bash
   ./detect_products
   ```

### Notes
- The SDK files are downloaded by the cross-platform downloader at the repo root (`sdk-downloader/script/src/main.py`), which the `install-pre-req` script runs for you. It resolves the repo root via the `sdkroot` marker and populates `OPSWAT-SDK/`.
---

## Sample Applications
Each sample project demonstrates a different feature of the SDK.

### DetectProducts List Installed Products
Scans the system for installed security products (e.g., firewalls) and checks if they are active. Outputs the status of each detected product.

**Files:**
- `DetectProducts.cpp`: Core logic of product detection.
- `Utils.cpp`: Utility functions for string processing.
- `SDKInit.cpp`: Contains functions that setup the SDK configuration

---

### GetMissingPatches List missing patches found on the endpoint
This will scan for each patch product and list the missing patches on the linux system

**Files:**
- `GetMissingPatches.cpp`: Core logic of GetMissingPatches.
- `Utils.cpp`: Utility functions for string processing.
- `SDKInit.cpp`: Contains functions that setup the SDK configuration

---

### GetOSInfo List missing patches found on the endpoint
This will return the information of the OS

**Files:**
- `GetOSInfo.cpp`: Core logic of GetOSInfo.
- `Utils.cpp`: Utility functions for string processing.
- `SDKInit.cpp`: Contains functions that setup the SDK configuration

---


## Coming Soon
Additional examples and advanced use cases will be added to this repository over time.

---

For detailed SDK usage, refer to the official OPSWAT documentation or contact support if needed.  The documentation is found here: https://software.opswat.com/OESIS_V4/html/index.html?_gl=1*amlpzo*_ga_B4377JYKYJ*MTY4MTIwMTQ3Ny42LjAuMTY4MTIwMTQ3Ny42MC4wLjA.
