# 🔐 OESIS Framework – Evaluation License Setup

Welcome to the **OESIS Framework Evaluation License** directory.  
This folder contains the license and token files required to activate and validate your OESIS Framework evaluation.

---

## 📁 Folder Contents

| File | Description |
|------|--------------|
| **license.cfg** | Main license configuration file used by the SDK. |
| **pass_key.txt** | Encrypted pass key that unlocks your evaluation license. |
| **download_token.txt** | Token file used by the SDK Downloader to fetch SDK packages and updates. |

These files are critical for initializing the SDK and downloading evaluation components.

---

## ⚙️ How to Use

1. Place this `eval-license` directory at the **repository root** (next to the `sdkroot` marker file).  
   Example structure:
   ```
   endpointsecsdk/
   ├── sdkroot
   ├── eval-license/
   │   ├── license.cfg
   │   ├── pass_key.txt
   │   └── download_token.txt
   ├── sdk-downloader/        # downloads + organizes the SDK into OPSWAT-SDK/
   ├── OPSWAT-SDK/            # created by the downloader (client/, inc/, libs/, ...)
   └── helloworld/           # sample projects (windows-cpp, mac-cpp, python, ...)
   ```

2. When you run the **SDK Downloader** or any SDK sample project, the tools automatically read from this directory:
   ```powershell
   cd sdk-downloader\windows-csharp\bin
   .\SDKDownloader.exe
   ```

3. Once complete, your SDK samples will have access to the appropriate libraries and license keys.

---

## 🧰 Troubleshooting

| Issue | Solution |
|--------|-----------|
| `License not found` | Ensure this folder is named exactly **`eval-license`** and located at the repository root. |
| `Invalid token` | Verify that the contents of `download_token.txt` are correct and not expired. |
| `Permission denied` | If running on Windows, open PowerShell as Administrator; on macOS/Linux, use `sudo`. |

---

## 📧 Need a License?

If you have not received an evaluation package or need an updated license, contact the OPSWAT OEM team:

> 📩 **[OEM@OPSWAT.com](mailto:OEM@OPSWAT.com)**

They can provide a trial license ZIP containing all required files for evaluation.

---

## 🧩 Additional Resources

For full setup and integration instructions, refer to the main SDK README:  
👉 [Root README.md](../README.md)

---

**Author:** OPSWAT OEM Solutions Team  
**Website:** [https://www.opswat.com](https://www.opswat.com)  
**Last Updated:** October 2025
