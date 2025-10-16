# 🚀 OPSWAT SDK – Build & Run Guide

Welcome to the **OPSWAT SDK Setup Guide**.
This document walks you through installing the prerequisites, configuring your environment, and running the Go sample project.

> ⚙️ **Supports:** Windows (✅)

---

## 🪟 Windows Setup Instructions

### 🧩 1. Install Go

* Navigate to [https://go.dev/doc/install](https://go.dev/doc/install)
* Download and install the correct version for your architecture.
  Example for **x64**:

  ```text
  go1.24.5.windows-amd64.msi
  ```

  > 📝 *Note: Version may change based on latest Go release.*

---

### 🧰 2. Install GCC (MinGW)

* Go to: [https://github.com/niXman/mingw-builds-binaries/releases/tag/15.1.0-rt_v12-rev0](https://github.com/niXman/mingw-builds-binaries/releases/tag/15.1.0-rt_v12-rev0)
* Download the correct build for your platform.
  **Example:**
  `x86_64-15.1.0-release-posix-seh-ucrt-rt_v12-rev0.7z`
* Extract to a known location (e.g. `C:\gcc`)
* Add to your system path:

  ```text
  C:\gcc\mingw64\bin
  ```

---

### 🔐 3. Place License and Token Files

Ensure the following files exist in the **`%source-root%\eval-license`** directory:

```
license.cfg
pass_key.txt
download_token.txt
```

---

### 💾 4. Download the SDK

* Navigate to:

  ```
  %source-root%\OPSWAT-SDK-Downloader\C-Sharp\bin
  ```
* Run:

  ```powershell
  .\SDKDownloader.exe
  ```

---

### 🧱 5. Build with PowerShell

1. Open **PowerShell**
2. Navigate to your extracted SDK root directory
3. Run the build script:

   ```powershell
   .\build.ps1
   ```

---

### ▶️ 6. Run the Sample Code

1. Navigate to:

   ```
   %extracted_root%\build
   ```
2. Run the sample executable:

   ```powershell
   .\go-sample.exe
   ```

> 💡 **Sample Source:** `%extracted_root%\src\main.go`

---

> 🧠 **Tip:** If your build fails, confirm your `gcc` path and license files are correct.
> You can verify Go installation with:
>
> ```powershell
> go version
> ```

---

### 🏁 Author Notes

This guide was generated to ensure a clear and visually friendly onboarding for engineers integrating OPSWAT SDK in Go across Windows and macOS environments.
