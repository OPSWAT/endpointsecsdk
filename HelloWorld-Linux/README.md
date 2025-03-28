# OPSWAT MetaDefender Security Endpoint SDK - Sample Applications

This repository provides sample applications that demonstrate how to integrate with the OPSWAT MetaDefender Security Endpoint SDK. The examples are written in C++ and cover various endpoint security use cases such as product detection.

---

## Getting Started

### Prerequisites
These are using g++ for compiling and using the SDK.  There is a script there will install the corresponding libraries for downloading, parsing and running the SDK.

### Compiling the Samples
1. First run the Prerequisites script to install the necessary dependencies. This can be done by running the following command in your terminal:
   ```bash
   src/install-pre-req
   ```
   This will install all the required libraries and dependencies for the SDK.

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
- The `SDKDownloader` utility downloads the latest SDK files into `HelloWorld-Linux/src/sdk`.  This will also be built and when you run with the install-pre-req script.
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

## Coming Soon
Additional examples and advanced use cases will be added to this repository over time.

---

For detailed SDK usage, refer to the official OPSWAT documentation or contact support if needed.  The documentation is found here: https://software.opswat.com/OESIS_V4/html/index.html?_gl=1*amlpzo*_ga_B4377JYKYJ*MTY4MTIwMTQ3Ny42LjAuMTY4MTIwMTQ3Ny42MC4wLjA.
