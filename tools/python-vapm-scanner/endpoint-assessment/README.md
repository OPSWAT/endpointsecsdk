# VAPM Scanner — Endpoint Assessment

Scans **this endpoint** (the local machine) for vulnerabilities and missing patches using the OESIS Framework SDK.

> **Status:** stub — the scan logic is not yet implemented. The script initializes the SDK and outlines the intended workflow.

## Prerequisites

- The SDK downloader has been run so `OPSWAT-SDK/` exists (see [sdk-downloader](../../../sdk-downloader/README.md)).
- License files are in [`eval-license/`](../../../eval-license/README.md) at the repository root.
- Python 3.7+.

## Usage

```bash
python copysdk.py        # stage SDK binaries + license into ./sdk
python vapm_scanner.py   # run the endpoint scan (stub)
```

## Files

- `copysdk.py` — stages the SDK client binaries and license files into a local `sdk/` directory (resolves the repo root via the `sdkroot` marker).
- `vapm_scanner.py` — the endpoint patch + vulnerability scanner (stub).
- `sdk_wrapper.py` — `ctypes` wrapper around the OESIS `libwaapi` native library.
- `platform_utils.py` — platform/architecture detection and SDK environment validation.

## Planned scan flow

- **Vulnerabilities:** consume the offline vmod database (method `50520`), detect installed products (method `0`), then query `GetProductVulnerability` (method `50505`) per product.
- **Patches:** load the patch database (method `50302`), detect patch-management agents (category `12`), then query missing/installed patches (methods `1013` / `1014`).
