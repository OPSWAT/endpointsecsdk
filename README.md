# MetaDefender Endpoint SDK – Sample and Utility Repository

This repository provides example projects, utilities, and data management scripts to support development and integration with the **MetaDefender Endpoint SDK**.  
It serves as a reference base for both partners and internal developers working with OPSWAT’s Device Trust, Vulnerability, and Patch Management modules.

---

## 📁 Repository Overview

### **helloworld/**
Sample projects demonstrating basic SDK usage across multiple platforms and programming languages.  
Each sample shows how to initialize the SDK, run a basic scan, and query endpoint posture data.  
Examples include:
- Windows (C#, C++)
- macOS (C++)
- Linux (C++)

### **tools/**
Utilities and extended sample applications built on top of the SDK.  
These provide practical workflows such as:
- Endpoint posture evaluation  
- Patch and vulnerability scanning  
- System remediation testing  
- SDK API feature demonstrations  

These tools are intended as **advanced integration examples** showing how to apply the SDK in real-world device management and compliance scenarios.

### **scripts/**
Python scripts used for data catalog management and automation tasks.  
They support:
- Merging and normalizing catalog data files (`patch.dat`, `checksum.dat`, etc.)  
- Generating SDK reference data for patching and vulnerability scanning  
- Supporting offline mode operations and testing  

Scripts are primarily used to prepare and maintain content that feeds into SDK and tool samples.

---

## 🧩 Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/OPSWAT/endpointsecsdk.git
