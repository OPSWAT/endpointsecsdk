# VAPM Scanner — Centralized Assessment

Assesses **externally-collected endpoint inventory** for vulnerabilities and missing patches against the OESIS Framework offline catalogs — a centralized / server-side model, rather than scanning the local machine live.

> **Status:** stub — the assessment logic is not yet implemented. The script initializes the SDK and outlines the intended workflow.

## Prerequisites

- The SDK downloader has been run so `OPSWAT-SDK/` exists (see [sdk-downloader](../../../sdk-downloader/README.md)).
- License files are in [`eval-license/`](../../../eval-license/README.md) at the repository root.
- Python 3.7+.

## Usage

```bash
python copysdk.py             # stage SDK binaries + license into ./sdk first

python vapm_scanner.py        # combined centralized assessment (stub)

# Or run an individual scan:
python scan-ca-endpoint.py    # overall endpoint product inventory (stub)
python scan-ca-osdetails.py   # missing patches per patch-management product (GetMissingPatches / 1013)
python scan-ca-third-party.py # third-party application vuln/patch scan (stub)
```

## Files

- `copysdk.py` — stages the SDK client binaries and license files into a local `sdk/` directory (resolves the repo root via the `sdkroot` marker).
- `vapm_scanner.py` — the combined centralized patch + vulnerability assessment (stub).
- `scan-ca-endpoint.py` — endpoint product-inventory scan (stub).
- `scan-ca-osdetails.py` — detects patch-management products (category 12) and calls `GetMissingPatches` (method 1013) for each, reporting missing patches; writes `ca_missing_patches.json`.
- `scan-ca-third-party.py` — third-party (non-OS) application vulnerability/patch scan (stub).
- `sdk_wrapper.py` — `ctypes` wrapper around the OESIS `libwaapi` native library.
- `platform_utils.py` — platform/architecture detection and SDK environment validation.

## Planned assessment flow

- Take endpoint inventory collected elsewhere (e.g. produced by `CollectDeviceInventory` or an agent), then resolve each product's vulnerabilities and available patches against the OESIS offline catalogs — without performing a live scan of the local machine.
