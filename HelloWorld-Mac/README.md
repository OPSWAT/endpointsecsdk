# OPSWAT MetaDefender Security Endpoint SDK - Mac Sample Applications

This repository provides sample applications that demonstrate how to integrate with the OPSWAT MetaDefender Security Endpoint SDK for macOS. The examples are written in C++ and cover various endpoint security use cases such as product detection.

---

## Getting Started

### Prerequisites
These examples use clang++ for compiling and using the SDK. You will need [Homebrew](https://brew.sh/) installed to manage dependencies.

1.  **Install Homebrew (if not already installed)**

2.  **Install Required Packages using Homebrew:**
    Run the following command in your terminal:
    ```bash
    brew install nlohmann-json tinyxml2 curl pkg-config
    ```

3.  **License File:**
    Ensure you have placed your `license.cfg` file inside the `src/license/` directory.

### Building and Running the Samples

1.  **Download the SDK:**
    First, run the SDK downloader script. This will download the necessary OPSWAT SDK files into the `src/sdk` directory.
    ```bash
    cd src
    chmod +x build_sdk_downloader.sh
    ./build_sdk_downloader.sh
    ```
    *Note: This script requires the license file (`src/license/license.cfg`) to be present.* 

2.  **Run an Example:**
    Use the `run_example.sh` script to build and run a specific example. Provide the name of the example directory as an argument.
    ```bash
    chmod +x run_example.sh
    ./run_example.sh <ExampleName>
    ```
    Replace `<ExampleName>` with the name of the example you want to run (e.g., `DetectProducts`, `GetMissingPatches`, `GetOSInfo`).

    If you run the script without an example name, it will list the available examples:
    ```bash
    ./run_example.sh 
    ```

### Notes
- The `build_sdk_downloader.sh` script downloads the latest SDK files into `src/sdk`. It uses the `SDKDownloader` utility located in `src/SDKDownloader`.
---

## Sample Applications
Each sample project demonstrates a different feature of the SDK.

### DetectProducts List Installed Products
Scans the system for installed security products (e.g., firewalls) and checks if they are active. Outputs the status of each detected product.

**To Run:**
```bash
cd src
./run_example.sh DetectProducts
```

**Files:**
- `DetectProducts.cpp`: Core logic of product detection.
- `Utils.cpp`: Utility functions for string processing.
- `SDKInit.cpp`: Contains functions that setup the SDK configuration

---

### GetMissingPatches List missing patches found on the endpoint
This will scan for each patch product and list the missing patches on the macOS system

**To Run:**
```bash
cd src
./run_example.sh GetMissingPatches
```

**Files:**
- `GetMissingPatches.cpp`: Core logic of GetMissingPatches.
- `Utils.cpp`: Utility functions for string processing.
- `SDKInit.cpp`: Contains functions that setup the SDK configuration

---

### GetOSInfo List OS information
This will return the information of the OS

**To Run:**
```bash
cd src
./run_example.sh GetOSInfo
```

**Files:**
- `GetOSInfo.cpp`: Core logic of GetOSInfo.
- `Utils.cpp`: Utility functions for string processing.
- `SDKInit.cpp`: Contains functions that setup the SDK configuration

---

## M1 Mac Compatibility
This project has been configured to work on Apple Silicon (M1) Macs. The makefiles and build scripts are set up to handle the arm64 architecture.

---

For detailed SDK usage, refer to the official OPSWAT documentation or contact support if needed. The documentation is found here: https://software.opswat.com/OESIS_V4/html/index.html 