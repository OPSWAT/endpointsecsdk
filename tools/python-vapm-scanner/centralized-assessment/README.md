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

# Recommended — full pipeline (gather -> map -> final consolidated report):
python scan-ca.py             # -> results/ca-result.json (intermediate files cleaned up)

python vapm_scanner.py        # combined centralized assessment (stub)

# Or run an individual scan:
python scan-ca-endpoint.py    # orchestrator: runs osdetails + third-party scans
python scan-ca-osdetails.py   # OS info + missing + installed patches -> scan-ca-osdetails-result.json
python scan-ca-third-party.py # detected products + versions (DetectProducts / GetVersion)

# After the scan-ca-*.py result files exist, map them to CVEs via the Analog catalog:
python map_ca_osdetails.py    # missing patches + CVEs (OS / system)
python map_ca_third_party.py  # detected products -> CVEs (third-party apps)

# Or run both mappers and merge into a single combined result:
python map-ca.py              # -> map-ca-result.json
```

## Files

- `copysdk.py` — stages the SDK client binaries and license files into a local `sdk/` directory (resolves the repo root via the `sdkroot` marker).
- `vapm_scanner.py` — the combined centralized patch + vulnerability assessment (stub).
- `scan-ca-endpoint.py` — orchestrator that runs `scan-ca-osdetails.py` and `scan-ca-third-party.py` in turn (as subprocesses) and prints a summary.
- `scan-ca-osdetails.py` — collects OS details (`GetOSInfo`, method 1) and, per patch-management product (Windows: signature 1103), the missing patches (`GetMissingPatches`, 1013) and installed patches (`GetInstalledPatches`, 1023); writes `scan-ca-osdetails-result.json`.
- `scan-ca-third-party.py` — detects installed products (`DetectProducts`, method 0) and resolves each product's version (`GetVersion`, method 100); writes `scan-ca-third-party-result.json` (including `product_id` and `os_type` for mapping).
- `map_ca_osdetails.py` — maps `scan-ca-osdetails-result.json` against the Analog offline catalog (`OPSWAT-SDK/extract/analog/server/vuln_system_associations.json` + `cves.json`) to produce a list of missing patches and the CVEs each remediates. CVEs already covered by **installed** patches are subtracted, so the result is the *net* exposure. Writes `map-ca-osdetails-result.json`. (Follows the Windows approach in the Analog ruby sample `get_system_vuln.rb`.)
- `map_ca_third_party.py` — maps `scan-ca-third-party-result.json` against the Analog catalog to produce, per detected product: the CVEs it is affected by (`vuln_associations.json` + `cves.json`, matched by product id, signature, and version range) and the latest available version with a `patch_missing` flag (`patch_associations.json` + `patch_aggregation.json`). Writes `map-ca-third-party-result.json`. (Follows the Analog ruby samples `get_vuln.rb` and `get_latest_installer.rb`.)
- `map-ca.py` — runs both mappers (`map_ca_osdetails.py` + `map_ca_third_party.py`) and merges their output into a single `map-ca-result.json`, including a unified de-duplicated CVE count across the OS and third-party assessments.
- `scan-ca.py` — **full pipeline** entry point: runs the endpoint scan (`scan-ca-endpoint.py`), then `map-ca.py`, then derives a final consolidated report at **`results/ca-result.json`** (endpoint summary, missing OS patches, vulnerable/outdated products, and a unified CVE list tagged by source). It then removes the intermediate `*-result.json` files so `results/ca-result.json` is the single, clear output.

> **Output:** the full pipeline's only deliverable is `results/ca-result.json`. The individual `scan-ca-*` / `map_ca_*` scripts still write their own `*.json` in this directory when run on their own; `scan-ca.py` cleans those up at the end.
- `sdk_wrapper.py` — `ctypes` wrapper around the OESIS `libwaapi` native library.
- `platform_utils.py` — platform/architecture detection and SDK environment validation.

## Planned assessment flow

- Take endpoint inventory collected elsewhere (e.g. produced by `CollectDeviceInventory` or an agent), then resolve each product's vulnerabilities and available patches against the OESIS offline catalogs — without performing a live scan of the local machine.
