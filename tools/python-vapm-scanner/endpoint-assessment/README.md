# VAPM Scanner — Endpoint Assessment (traditional workflow)

This is the **traditional** VAPM workflow: the OESIS database files are copied **onto the endpoint**, and the endpoint resolves **all** vulnerability and patch detail locally via the SDK. `copysdk.py` stages the SDK runtime **and** the database files (`v2mod.dat`, `wuov2.dat`, `wiv-lite.dat`, …) into `./sdk`; the scan scripts then load those databases and produce the complete result without any server involvement.

Simple to deploy, but every endpoint carries the full (potentially large) OESIS database files and does the matching work. If on-endpoint database size is a concern, use the [centralized workflow](../centralized-assessment/README.md) instead — it does a minimal scan here and the mapping on a server, producing the **same** final result (identical schema in `results/ea-result.json` vs `results/ca-result.json`).

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

Both workflows are designed to produce the **same** product-centric result. The difference is *where the work and the database files live*:

- **Endpoint (this folder):** database files are on the endpoint; the endpoint resolves everything locally.
- **Centralized:** the endpoint runs a minimal scan with **no** database files; a server maps the minimal data against the catalog.

Because they use different data sources, coverage can differ in practice — for example, the lite `wiv-lite.dat` shipped for the endpoint may report fewer OS CVEs than the server catalog's `vuln_system_associations`.
