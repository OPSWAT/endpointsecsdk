# OESIS Framework – Go Sample (Windows)

This document walks you through installing the prerequisites, configuring your environment, and building/running the Go sample project on Windows.

> **Supported platform:** Windows

---

## Windows Setup Instructions

### 1. Install Go

* Navigate to [https://go.dev/doc/install](https://go.dev/doc/install)
* Download and install the correct version for your architecture.
  Example for **x64**:

  ```text
  go1.24.5.windows-amd64.msi
  ```

  > *Note: Version may change based on the latest Go release.*

---

### 2. Install GCC (MinGW)

CGO requires a C compiler. Install MinGW-w64:

* Go to: [https://github.com/niXman/mingw-builds-binaries/releases/tag/15.1.0-rt_v12-rev0](https://github.com/niXman/mingw-builds-binaries/releases/tag/15.1.0-rt_v12-rev0)
* Download the correct build for your platform.
  **Example:** `x86_64-15.1.0-release-posix-seh-ucrt-rt_v12-rev0.7z`
* Extract to a known location (e.g. `C:\gcc`)
* Add to your system PATH:

  ```text
  C:\gcc\mingw64\bin
  ```

---

### 3. Place License and Token Files

Ensure the following files exist in the `%source-root%\eval-license` directory:

```
license.cfg
pass_key.txt
download_token.txt
```

---

### 4. Download the SDK

The SDK downloader must be run before building — it populates `OPSWAT-SDK\`.

* Navigate to:

  ```
  %source-root%\sdk-downloader\windows-csharp\bin
  ```
* Run:

  ```powershell
  .\SDKDownloader.exe
  ```

  Or use the cross-platform Python downloader: `python %source-root%\sdk-downloader\script\src\main.py`.

---

### 5. Build with PowerShell

1. Open **PowerShell**
2. Navigate to `%source-root%\helloworld\windows-go`
3. Run the build script (it stages the SDK from `OPSWAT-SDK\` and builds the sample):

   ```powershell
   .\build.ps1
   ```

   If script execution is blocked by policy, bypass the signature check:

   ```powershell
   powershell -ExecutionPolicy Bypass -File .\build.ps1
   ```

---

### 6. Run the Sample Code

1. Navigate to the build output:

   ```
   %source-root%\helloworld\windows-go\build
   ```
2. Run the sample executable:

   ```powershell
   .\go-sample.exe
   ```

> **Sample source:** `%source-root%\helloworld\windows-go\src\main.go`

---

> **Tip:** If your build fails, confirm your `gcc` path and license files are correct.
> You can verify your Go installation with:
>
> ```powershell
> go version
> ```
