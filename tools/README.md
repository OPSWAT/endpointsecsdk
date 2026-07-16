# Tools

This directory contains higher-level utilities that demonstrate advanced use of the OESIS Framework SDK — endpoint posture assessment, vulnerability scanning, and patch management. They build on the same SDK used by the [helloworld](../helloworld/README.md) samples.

> **Mixed languages:** `posture` and `vapm-scanner` are **C#/.NET (Visual Studio) Windows applications**; `python-vapm-scanner` and `catalog-lookup` are **Python** command-line tools. See each tool's own README for full details.

## Available Tools

| Tool | Type | Description |
|------|------|-------------|
| [posture](posture/README.md) | C# / .NET (WinForms, Windows) | Endpoint posture/compliance demo — security score, policy validation, geolocation, and browser plugin checks via the OESIS Compliance module. |
| [vapm-scanner](vapm-scanner/README.md) | C# / .NET (WinForms, Windows) | Vulnerability And Patch Management scanner (`AcmeScanner`) — scans installed products for CVEs and missing patches, browses the OESIS catalog, and can auto-patch. Exports CSV. |
| [python-vapm-scanner](python-vapm-scanner/README.md) | Python (Windows/macOS/Linux) | Vulnerability And Patch Management scanner in Python, with `endpoint-assessment` (live SDK) and `centralized-assessment` (offline catalog mapping) modes, plus `scan-all-workflows.py` to run both and diff them. |
| [catalog-lookup](catalog-lookup/README.md) | Python (Windows/macOS/Linux) | Command-line lookups against the OESIS "Analog" offline catalog — find a **KB**, **CVE**, application **signature** (by name), **patch** (by signature, patch UUID, package UUID, or bulletin), or **CPE**, and show the associated details. |

## Prerequisites

All tools require the OESIS Framework SDK. **Run the [SDK downloader](../sdk-downloader/README.md) first** so the `OPSWAT-SDK/` directory is populated, and place your license files (`license.cfg`, `pass_key.txt`, `download_token.txt`) in [`eval-license/`](../eval-license/README.md) at the repository root.

| Tool | Additional requirements |
|------|--------------------------|
| posture, vapm-scanner | Windows, Visual Studio 2022 (.NET desktop workload) |
| python-vapm-scanner, catalog-lookup | Python 3.7+ |

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

### python-vapm-scanner (Python)
```bash
cd tools/python-vapm-scanner
python scan-all-workflows.py      # stage SDK, run endpoint + centralized scans, diff, zip results
# ...or run a single workflow:
cd endpoint-assessment && python copysdk.py && python scan-ea.py
```
See [python-vapm-scanner/README.md](python-vapm-scanner/README.md) for the two assessment modes and details.

### catalog-lookup (Python)
```bash
cd tools/catalog-lookup
python find-kb.py 5094127                     # is a KB supported? OS/build/CVEs
python find-cve.py CVE-2024-38063             # CVE details + OS/3rd-party patches + CPEs
python find-signature.py chrome              # application name -> signature id(s)
python find-patch-for-signature.py 41         # signature id -> patch + latest version
python find-cpe.py cpe:/a:microsoft:.net      # CPE -> products/signatures/CVEs
python find-patch.py <patch_uuid>             # OS patch by patch UUID
python find-package.py <package_uuid>         # OS patch package by package UUID
python find-bulletin.py MS22-0110-5009497     # patches for a security bulletin
```
See [catalog-lookup/README.md](catalog-lookup/README.md). Scripts auto-locate the catalog via the `sdkroot` marker.

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
