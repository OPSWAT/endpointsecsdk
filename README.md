# MetaDefender Endpoint SDK – Sample and Utility Repository

This repository provides example projects, utilities, and data management scripts to support development and integration with the **MetaDefender Endpoint SDK**.  
It serves as a reference base for both partners and internal developers working with OPSWAT’s Device Trust, Vulnerability, and Patch Management modules.

---

## 📁 Repository Overview

### **helloworld/**
Sample projects demonstrating basic SDK usage across multiple platforms and programming languages.  
Each example shows how to initialize the SDK, perform a scan, and query endpoint posture data.

**Available samples:**
- **Windows:** C++, C#, and Go examples demonstrating SDK setup, detection, and product discovery workflows  
- **macOS:** C++ sample built with Xcode showing cross-platform SDK integration  
- **Linux:** C++ sample demonstrating patch and vulnerability queries  

These projects provide the most direct way to learn the SDK API structure and core integration sequence.

---

### **tools/**
Utility projects and extended sample applications built on top of the SDK.  
These examples demonstrate broader endpoint management use cases, including:

- Posture evaluation workflows  
- Patch and vulnerability scanning  
- Remediation testing and compliance validation  
- SDK feature coverage across multiple modules  

These tools are designed as **advanced integration examples** and can serve as the foundation for OEM partner implementations.

---

### **scripts/**
Python scripts for data catalog processing, offline patch preparation, and SDK automation tasks.  
They provide functions for:

- Merging and normalizing catalog data (`patch.dat`, `checksum.dat`, etc.)  
- Building unified aggregation files (`patch_aggregation.json`, `vuln_associations.json`)  
- Supporting offline scanning, delta updates, and data validation workflows  

These scripts are primarily used on the **server side** to prepare data consumed by the SDK and tool samples.

---

### **eval-license/**
Contains instructions and supporting files for **extracting and placing your evaluation license**.  
When you receive the evaluation ZIP from OPSWAT, extract its contents into this directory.  
This allows all samples to load and validate the license automatically during initialization.

If you do not yet have a license, see the section below for requesting one.

---

## 🧩 Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/OPSWAT/endpointsecsdk.git
   ```
2. **Navigate to a sample directory**
   ```bash
   cd endpointsecsdk/helloworld/windows
   ```
3. **Build using your platform toolchain**
   - Windows: Visual Studio (C++, C#, or Go)
   - macOS: Xcode (C++)
   - Linux: GCC/Clang (C++)
4. **Run the sample** to confirm SDK initialization and license validation.

---

## 🔧 Requirements

- MetaDefender Endpoint SDK evaluation or production license  
- Supported build environments:
  - Visual Studio (Windows)
  - Xcode (macOS)
  - GCC/Clang (Linux)
  - Go 1.21+ (for Windows Go example)
- Python 3.9+ (for `scripts/` utilities)

---

## 📄 License and Use

All samples, scripts, and tools are provided for demonstration and integration purposes only.  
Redistribution or production use requires permission under your OPSWAT OEM license agreement.

---

## ✉️ Request an Evaluation License

To request an evaluation key or additional information about the MetaDefender Endpoint SDK,  
please email: **[oem@opswat.com](mailto:oem@opswat.com)**.

---

© OPSWAT Inc. – “Trust No File, Trust No Device.”
