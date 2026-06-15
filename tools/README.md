# Tools

This directory contains higher-level utilities that demonstrate advanced use of the OESIS Framework SDK — endpoint posture assessment, vulnerability scanning, and patch management. They build on the same SDK used by the [helloworld](../helloworld/README.md) samples.

> **Mixed languages:** `posture` and `vapm-scanner` are **C#/.NET (Visual Studio) Windows applications**; `python-scanner` is a **Python** command-line tool. See each tool's own README for full details.

## Available Tools

| Tool | Type | Description |
|------|------|-------------|
| [posture](posture/README.md) | C# / .NET (WinForms, Windows) | Endpoint posture/compliance demo — security score, policy validation, geolocation, and browser plugin checks via the OESIS Compliance module. |
| [vapm-scanner](vapm-scanner/README.md) | C# / .NET (WinForms, Windows) | Vulnerability And Patch Management scanner (`AcmeScanner`) — scans installed products for CVEs and missing patches, browses the OESIS catalog, and can auto-patch. Exports CSV. |
| [python-scanner](python-scanner/README.md) | Python (Windows/macOS/Linux) | Cross-platform vulnerability and patch scanner driven by `vuln_scanner.py`. |

## Prerequisites

All tools require the OESIS Framework SDK. **Run the [SDK downloader](../sdk-downloader/README.md) first** so the `OPSWAT-SDK/` directory is populated, and place your license files (`license.cfg`, `pass_key.txt`, `download_token.txt`) in [`eval-license/`](../eval-license/README.md) at the repository root.

| Tool | Additional requirements |
|------|--------------------------|
| posture, vapm-scanner | Windows, Visual Studio 2022 (.NET desktop workload) |
| python-scanner | Python 3.7+ |

## Running the Tools

### posture (C#)
```
Open tools/posture/src/opswat-posture.sln in Visual Studio, build, and run the
opswat-posture startup project.
```
See [posture/README.md](posture/README.md). The app reads `license.cfg` / `pass_key.txt` from its run directory and can auto-download the SDK on startup using `download_token.txt`.

### vapm-scanner (C#)
```
Open tools/vapm-scanner/src/AcmeScanner/AcmeScanner.sln in Visual Studio, build,
and run the AcmeScanner startup project.
```
See [vapm-scanner/README.md](vapm-scanner/README.md). Results can be exported to CSV from the UI.

### python-scanner (Python)
```bash
cd tools/python-scanner
python3 vuln_scanner.py --scan all
# scan subsets: --scan product | --scan system
# override input/output locations: --sdk-path ./sdk --dat-path ./dat --license-path ./license --output-dir ./output
```
See [python-scanner/README.md](python-scanner/README.md) for the full flag reference and output details.

## Troubleshooting

**"SDK not found" / missing libraries**
- Run the SDK downloader from the repository root and verify the `sdkroot` marker file exists.
- For python-scanner, point `--sdk-path` / `--dat-path` at the staged SDK directory if they are not in the defaults.

**"License files missing"**
- Ensure `license.cfg` and `pass_key.txt` are present (in `eval-license/` at the repo root, and/or staged where the tool expects them).

**"Permission denied"**
- Some scans require elevated privileges — run as Administrator on Windows (or with `sudo` on Linux/macOS for python-scanner).

## Related Resources

- [SDK Downloader](../sdk-downloader/README.md) — download and organize the SDK libraries
- [Hello World Samples](../helloworld/README.md) — SDK integration examples in multiple languages
- [Catalog Scripts](../scripts/README.md) — vulnerability and patch catalog utilities

## Support

- Review each tool's README and inline documentation.
- OPSWAT support / evaluation: oem@opswat.com
- SDK documentation: https://software.opswat.com/OESIS_V4/html/index.html
