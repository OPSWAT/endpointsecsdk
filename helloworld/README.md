# OESIS Framework — HelloWorld

The **HelloWorld** directory contains minimal, cross-platform examples that demonstrate how to integrate and use the **OESIS Framework (OESIS)**.

Each sample shows how to:
- Initialize the SDK  
- Load and validate an evaluation license  
- Enumerate endpoint security products  
- Retrieve patch and vulnerability data  

---

## 📁 Folder Structure

| Folder | Description |
|--------|--------------|
| **linux-cpp** | C++ HelloWorld sample for Linux |
| **mac-cpp** | C++ HelloWorld sample for macOS |
| **python** | Python HelloWorld samples — cross-platform (Windows, macOS, Linux) |
| **windows-cpp** | C++ HelloWorld sample for Windows |
| **windows-csharp** | .NET (C#) HelloWorld sample for Windows |
| **windows-go** | Go (Golang) HelloWorld sample for Windows |

---

## 🚀 Getting Started

### 1. Request Evaluation Access
To get started, email **[oem@opswat.com](mailto:oem@opswat.com)** to request:
- An **evaluation SDK package**  
- A **license key**

You’ll receive a ZIP file containing the SDK metadata and licensing information.

---

### 2. Extract the SDK

Extract the evaluation ZIP into the **root** of this repository.

After extraction, you should see a file named `sdkroot` at the project root — this tells helper scripts and examples where the SDK base directory resides.

---

### 3. Place Your License

Copy your evaluation license files into `eval-license/` at the repository root:
```
eval-license/
  ├── license.cfg
  ├── pass_key.txt
  └── download_token.txt
```

The SDK Downloader (next step) reads `download_token.txt` to authenticate, and all HelloWorld samples reference these files when initializing the SDK. See [eval-license/README.md](../eval-license/README.md) for details.

---

### 4. Run the SDK Downloader

The SDK Downloader retrieves the latest OPSWAT SDK client binaries for your platform and architecture and organizes them under `OPSWAT-SDK/`.

#### 🪟 **Windows**

Run the precompiled downloader executable:

```powershell
cd sdk-downloader\windows-csharp\bin
.\SDKDownloader.exe
```

This will create the following structure:
```
OPSWAT-SDK/
  └── client/
      └── windows/
          ├── x64/
          ├── win32/
          └── arm64/
```

---

#### 🐧 **Linux** and 🍎 **macOS**

Run the Python version of the SDK Downloader:

```bash
cd sdk-downloader/script/src
python3 main.py
```

This will populate:
```
OPSWAT-SDK/
  └── client/
      ├── linux/
      └── mac/
```

> **Note:** The Python downloader requires Python 3 and the `requests` package:
> ```bash
> pip3 install requests
> ```

---

## 🧱 Building and Running the Samples

### 🪟 **Windows**

#### C++  
1. Open `windows-cpp` in **Visual Studio**  
2. Build for your desired architecture (`x64`, `win32`, or `arm64`)  
3. Run the generated binary from:
   ```
   helloworld\windows-cpp\bin\<arch>\(Debug|Release)
   ```

#### C#  
1. Open `windows-csharp` in **Visual Studio**  
2. Build (Debug/Release)  
3. Run from:
   ```
   helloworld\windows-csharp\bin\<arch>\(Debug|Release)
   ```

#### Go  
```powershell
cd windows-go
go run .
# or build
go build -o bin\helloworld.exe
```

Ensure the correct binaries exist under:
```
OPSWAT-SDK\client\windows\<architecture>\
```

---

### 🐧 **Linux**

```bash
cd linux-cpp
make
./bin/helloworld
```

Make sure the SDK `.so` libraries are in your library path:
```bash
export LD_LIBRARY_PATH=../../OPSWAT-SDK/client/linux/x64:$LD_LIBRARY_PATH
```

---

### 🍎 **macOS**

```bash
cd mac-cpp
make
./bin/helloworld
```

If required, set the library path:
```bash
export DYLD_LIBRARY_PATH=../../OPSWAT-SDK/client/mac:$DYLD_LIBRARY_PATH
```

---

### 🐍 **Python** (cross-platform)

```bash
cd python
python copy_sdk_files.py     # stage SDK binaries + license into ./sdk
python detect_products.py    # then run any sample
```

The Python samples run on Windows, macOS, and Linux. `copy_sdk_files.py` stages the
correct SDK binaries from `OPSWAT-SDK/` and your license files into a local `sdk/`
directory. See [python/README.md](python/README.md) for the full list of samples and
usage. Python 3.7+ is required; the samples use only the standard library.

---

## 🧠 What Each Sample Demonstrates

- **SDK Initialization** and **License Activation**  
- **Product Enumeration** (e.g., AV, Patch, Firewall, Encryption)  
- **Patch & Vulnerability Detection**  
- **Result Serialization** (console or JSON output)

These examples serve as the foundation for OEM partners to build integrations with OESIS for **Device Trust**, **Patch Management**, and **Vulnerability Assessment**.

---

## ⚠️ Common Issues

| Issue | Cause | Fix |
|--------|--------|------|
| **SDK not found** | Downloader not executed | Run the downloader for your platform (binary or Python) |
| **License not found** | Missing files | Copy license files into `eval-license/` |
| **Illegal characters in path (Windows)** | Trailing backslash in post-build output path | Use `"$(TargetDir)."` in post-build event |
| **Architecture mismatch** | SDK binaries don’t match build target | Build with the same arch as the SDK (`x64`, `win32`, `arm64`) |
| **Python: SDK / license not found** | `copy_sdk_files.py` not run | In `python/`, run `python copy_sdk_files.py` to stage the SDK + license before running a sample |

---

## 📬 Support

**OPSWAT OEM Team**  
📧 [oem@opswat.com](mailto:oem@opswat.com)  
🌐 [www.opswat.com/oem](https://www.opswat.com/oem)
