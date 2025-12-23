# SDK Downloader

The **SDK Downloader** is a cross-platform utility for downloading and organizing the latest **OESIS Framework** client binaries and supporting files for all platforms.  
It ensures local sample applications (such as those in `/helloworld`) always reference the correct and most current SDK build.

---

## 📂 Folder Structure

| Folder | Description |
|:-------|:-------------|
| **`script/src`** | Python-based downloader for Linux and macOS. |
| **`windows-csharp`** | C# .NET version of the downloader for Windows, including a pre-compiled executable. |

Both implementations retrieve the SDK client libraries from OPSWAT’s distribution endpoint and organize them into the `/OPSWAT-SDK/client/<platform>/<architecture>` directory.

More information on the resulting file layout can be found in the [file-layout](../file-layout/README.md) documentation.

---

## ⚙️ Prerequisites

1. Ensure your repository root contains the **`sdkroot`** marker file.  
2. Verify that the following folder exists:
   ```
   eval-license/download-token.txt
   ```
   This file must contain your **valid download token** — required for authentication when retrieving SDK packages.  
   If you need a token or evaluation license, contact **oem@opswat.com**.

---

## 🚀 Usage Guide

### 🪟 Windows (C# Downloader)

You can use the **pre-compiled executable** provided in the repository to download the Windows SDK libraries.

```powershell
cd sdk-downloader\windows-csharp\bin\Debug
.\SDKDownloader.exe
```

This executable will:
- Detect the SDK root automatically using `sdkroot`.
- Authenticate using the token in `eval-license/download-token.txt`.
- Download or update the Windows SDK binaries (`x64`, `win32`, `arm64`).
- Place them under:
  ```
  OPSWAT-SDK\client\windows\<architecture>\
  ```

The C# project source code is also provided for developers who wish to modify, rebuild, or integrate the downloader into their own workflows.

---

### 🐍 Linux / macOS (Python Downloader)

For Linux and macOS users, the downloader is implemented as a Python script.

```bash
cd sdk-downloader/script/src
python3 sdk_downloader.py
```

The script will:
- Locate the SDK root.
- Use `eval-license/download-token.txt` for authentication.
- Download and organize client binaries for your current platform.
- Output files to:
  ```
  OPSWAT-SDK/client/linux/<architecture>/
  OPSWAT-SDK/client/macos/<architecture>/
  ```

---

## 🧩 Integration Flow

1. **Ensure `eval-license/download-token.txt` is present.**  
2. **Run the appropriate downloader** (Python or Windows executable) for your platform.  
3. The SDK binaries will populate the `/OPSWAT-SDK/client` directory.  
4. The `/helloworld` and other sample projects automatically locate these files using the `sdkroot` file.  
5. Build and run your samples — they will link to the freshly downloaded SDK libraries.

---

## 🧠 Notes

- The Windows executable and Python script produce the same directory layout for consistency across platforms.  
- If `download-token.txt` is missing or invalid, the downloader will fail authentication.  
- All downloader source code is included in this directory for developer reference and customization.

---

## 🧰 Troubleshooting

| Issue | Likely Cause | Resolution |
|--------|---------------|-------------|
| “SDK root not found” | Missing `sdkroot` file | Ensure you run from repository root. |
| “Authentication failed” | Missing or invalid `download-token.txt` | Verify token exists in `eval-license/`. |
| “Access denied” | Permission issue | Run as administrator (Windows) or with `sudo` (Linux/macOS). |
| Incorrect architecture downloaded | Wrong parameter or environment | Specify `-Architecture x64`, `win32`, or `arm64` as needed. |

---

## 📬 Support

For evaluation keys, download tokens, or SDK assistance, contact:  
**📧 oem@opswat.com**

For repository updates, documentation, or contributing improvements, see the [main project README](https://github.com/OPSWAT/endpointsecsdk).
