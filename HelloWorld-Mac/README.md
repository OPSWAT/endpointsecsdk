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

## Building and Running the Samples

You can build and run the samples using either Xcode (recommended) or the terminal scripts.

### Method 1: Using Xcode (Recommended)

This repository includes an Xcode workspace with projects for the SDK Downloader and all examples, making it easy to build and debug directly within Xcode.

1.  **Open the Xcode Workspace:**
    Open `HelloWorld-Mac.xcworkspace` in Xcode.

2.  **Download the SDK First:**
    Before running any examples, you must first build and run the SDK Downloader:
    - In Xcode, select the `SDKDownloader` scheme from the scheme selector
    - Click the Run button (or press ⌘R) to build and run
    - This will download the necessary SDK files to the `src/sdk` directory

3.  **Run an Example:**
    After the SDK has been downloaded, you can build and run any of the examples:
    - Select the desired example scheme (e.g., `DetectProducts`, `GetOSInfo`, or `GetMissingPatches`)
    - Click the Run button (or press ⌘R) to build and run the selected example

4.  **Debugging:**
    You can set breakpoints in the code and use Xcode's debugging tools to step through the execution.

*Note: The Xcode projects are configured to use the SDK files in the `src/sdk` directory, so make sure to run the SDK Downloader first.*

### Method 2: Using Terminal Scripts

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

### Important: macOS Security Permissions

When running any of the examples (via Xcode or terminal), you will likely see multiple permission requests from macOS. **You must accept these permission dialogs** for the examples to work properly. 

The SDK needs to scan your system for installed applications and security products, which requires access to various folders on your Mac. You may see prompts requesting access to:

- Files and Folders
- System Events
- Automation permissions

These permission requests are normal and necessary for the SDK to function correctly. After accepting these permissions, the examples will be able to properly detect installed products, missing patches, and other system information.

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
This project has been configured to work on Apple Silicon (M1) Macs. The makefiles, build scripts, and Xcode projects are set up to handle the arm64 architecture.

---

For detailed SDK usage, refer to the official OPSWAT documentation or contact support if needed. The documentation is found here: https://software.opswat.com/OESIS_V4/html/index.html 