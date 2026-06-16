# VAPM Scanner — Centralized Assessment

Assesses **externally-collected endpoint inventory** for vulnerabilities and missing patches against the OESIS Framework offline catalogs — a centralized / server-side model, rather than scanning the local machine live.

> **Status:** stub — the assessment logic is not yet implemented. The script initializes the SDK and outlines the intended workflow.

## Prerequisites

- The SDK downloader has been run so `OPSWAT-SDK/` exists (see [sdk-downloader](../../../sdk-downloader/README.md)).
- License files are in [`eval-license/`](../../../eval-license/README.md) at the repository root.
- Python 3.7+.

## Usage

```bash
python copysdk.py        # stage SDK binaries + license into ./sdk
python vapm_scanner.py   # run the centralized assessment (stub)
```

## Files

- `copysdk.py` — stages the SDK client binaries and license files into a local `sdk/` directory (resolves the repo root via the `sdkroot` marker).
- `vapm_scanner.py` — the centralized patch + vulnerability assessment (stub).
- `sdk_wrapper.py` — `ctypes` wrapper around the OESIS `libwaapi` native library.
- `platform_utils.py` — platform/architecture detection and SDK environment validation.

## Planned assessment flow

- Take endpoint inventory collected elsewhere (e.g. produced by `CollectDeviceInventory` or an agent), then resolve each product's vulnerabilities and available patches against the OESIS offline catalogs — without performing a live scan of the local machine.
