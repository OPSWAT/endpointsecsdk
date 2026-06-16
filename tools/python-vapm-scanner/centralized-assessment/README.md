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
python scan-ca-endpoint.py    # orchestrator: runs osdetails + third-party scans
python scan-ca-osdetails.py   # OS info + missing + installed patches -> scan-ca-osdetails-result.json
python scan-ca-third-party.py # detected products + versions (DetectProducts / GetVersion)

# After scan-ca-osdetails.py has produced its result file, map it to CVEs:
python map_ca_osdetails.py    # missing patches + CVEs from the Analog offline catalog
```

## Files

- `copysdk.py` — stages the SDK client binaries and license files into a local `sdk/` directory (resolves the repo root via the `sdkroot` marker).
- `vapm_scanner.py` — the combined centralized patch + vulnerability assessment (stub).
- `scan-ca-endpoint.py` — orchestrator that runs `scan-ca-osdetails.py` and `scan-ca-third-party.py` in turn (as subprocesses) and prints a summary.
- `scan-ca-osdetails.py` — collects OS details (`GetOSInfo`, method 1) and, per patch-management product (Windows: signature 1103), the missing patches (`GetMissingPatches`, 1013) and installed patches (`GetInstalledPatches`, 1023); writes `scan-ca-osdetails-result.json`.
- `scan-ca-third-party.py` — detects installed products (`DetectProducts`, method 0) and resolves each product's version (`GetVersion`, method 100); writes `ca_third_party.json`.
- `map_ca_osdetails.py` — maps `scan-ca-osdetails-result.json` against the Analog offline catalog (`OPSWAT-SDK/extract/analog/server/vuln_system_associations.json` + `cves.json`) to produce a list of missing patches and the CVEs each remediates; writes `map-ca-osdetails-result.json`. (Follows the Windows approach in the Analog ruby sample `get_system_vuln.rb`.)
- `sdk_wrapper.py` — `ctypes` wrapper around the OESIS `libwaapi` native library.
- `platform_utils.py` — platform/architecture detection and SDK environment validation.

## Planned assessment flow

- Take endpoint inventory collected elsewhere (e.g. produced by `CollectDeviceInventory` or an agent), then resolve each product's vulnerabilities and available patches against the OESIS offline catalogs — without performing a live scan of the local machine.
