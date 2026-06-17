# VAPM Scanner — Endpoint Assessment

Scans **this endpoint** (the local machine) directly via the OESIS Framework SDK for OS and third-party vulnerabilities and patch status. This is the live / agent-style counterpart to the centralized (offline-catalog) assessment; the scripts emit results in the same shape as the centralized `map-ca-*-result.json` files so the two approaches are directly comparable.

## Prerequisites

- The SDK downloader has been run so `OPSWAT-SDK/` exists (see [sdk-downloader](../../../sdk-downloader/README.md)).
- License files are in [`eval-license/`](../../../eval-license/README.md) at the repository root.
- Python 3.7+.

## Usage

```bash
python copysdk.py              # stage SDK binaries + license into ./sdk first

# Recommended — full pipeline (runs both scans -> single product-centric report):
python scan-ea.py              # -> results/ea-result.json (intermediate files cleaned up)

# Or run the individual scans:
python scan-ea-osdetails.py    # OS details + latest installer + OS CVEs -> scan-ea-osdetails-result.json
python scan-ea-third-party.py  # detected products + CVEs            -> scan-ea-third-party-result.json
```

## Files

- `copysdk.py` — stages the SDK client binaries and license files into a local `sdk/` directory (resolves the repo root via the `sdkroot` marker).
- `scan-ea.py` — **full pipeline**: runs both scans and combines them into a single product-centric report at **`results/ea-result.json`** — a list of products, each with `signature_id`, `product_id`, `name`, `version`, `latest_version`, and the vulnerable `cves`/`cpes`. The OS is included as one product (signature 1103). Schema matches the centralized `results/ca-result.json`. Intermediate `*-result.json` files are cleaned up.
- `scan-ea-osdetails.py` — live OS assessment (like `helloworld/python/os_vulnerability.py`): `GetOSInfo` (1), loads `wuov2.dat`/`wiv-lite.dat`, `GetLatestInstaller` (50300) for the OS patch, and `GetProductVulnerability` (50505) for OS CVEs. Writes `scan-ea-osdetails-result.json` (same schema as `map-ca-osdetails-result.json`).
- `scan-ea-third-party.py` — live third-party scan (like `helloworld/python/vulnerability.py`): loads `v2mod.dat`, `DetectProducts` (0), `GetVersion` (100), `GetProductVulnerability` (50505) per product. Writes `scan-ea-third-party-result.json` (same schema as `map-ca-third-party-result.json`).
- `sdk_wrapper.py` — `ctypes` wrapper around the OESIS `libwaapi` native library.
- `platform_utils.py` — platform/architecture detection and SDK environment validation.

## Endpoint vs centralized

The endpoint scripts use the SDK's own detection on the live machine (so results already reflect installed state), whereas the `centralized-assessment` scripts map collected inventory against the offline Analog catalog. Coverage can differ — for example, the lite `wiv-lite.dat` may report fewer OS CVEs than the catalog's `vuln_system_associations`.
