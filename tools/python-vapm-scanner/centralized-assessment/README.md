# VAPM Scanner — Centralized Assessment

The **centralized** VAPM workflow keeps the endpoint footprint minimal: the endpoint runs a small scan that **does not load the large OESIS catalog/vulnerability database files**, and the resulting minimal detail is mapped to CVEs and patches on a **cloud or on-premise server** that holds those database files. The final result matches the [traditional endpoint workflow](../endpoint-assessment/README.md) — identical schema in `results/ca-result.json` vs `results/ea-result.json` — so you can move the cost of the (large) database files to the server.

The scripts here are deliberately split into the two halves of that workflow:

```
   ENDPOINT (minimal scan, no catalog DB files)          SERVER (has the catalog DB files)
   ───────────────────────────────────────────          ──────────────────────────────────
   scan-ca-osdetails.py    ─┐                            map_ca_osdetails.py   ─┐
   scan-ca-third-party.py  ─┤ scan-ca-*-result.json ───▶ map_ca_third_party.py ─┤ map-ca-result.json
   (scan-ca-endpoint.py)    ┘   (small, no CVE data)     (map-ca.py)             ┘  ──▶ ca-result.json
```

## Prerequisites

- The SDK downloader has been run so `OPSWAT-SDK/` exists (see [sdk-downloader](../../../sdk-downloader/README.md)).
- License files are in [`eval-license/`](../../../eval-license/README.md) at the repository root.
- Python 3.7+.

## Step 1 — On the endpoint (no catalog database files)

These scripts perform only detection and patch queries via the SDK — they do **not** load the offline catalog/vulnerability databases. Their output is small (detected products + versions, OS info, patch lists) and is what gets sent to the server.

```bash
python copysdk.py             # stage the SDK runtime + license into ./sdk
python scan-ca-endpoint.py    # orchestrator: runs both endpoint scans below
#   scan-ca-osdetails.py      # OS info + missing/installed patches -> scan-ca-osdetails-result.json
#   scan-ca-third-party.py    # detected products + versions        -> scan-ca-third-party-result.json
```

## Step 2 — On the server (catalog database files required)

These scripts take the endpoint's scan JSON and map it against the OESIS Analog catalog under `OPSWAT-SDK/extract/analog/server/` — this is where the large database files live and the CVE/patch matching happens.

```bash
python map_ca_osdetails.py    # OS missing patches + net CVEs
python map_ca_third_party.py  # third-party product CVEs + latest version / patch-missing
python map-ca.py              # runs both mappers and merges -> map-ca-result.json
```

## Full demo (both halves on one machine)

For convenience the whole flow can be run end-to-end on a single machine:

```bash
python scan-ca.py             # endpoint scan -> server mapping -> results/ca-result.json
```

## Files

**Endpoint scan (Step 1 — no catalog DB):**
- `copysdk.py` — stages the SDK runtime + license into `sdk/` (resolves the repo root via the `sdkroot` marker).
- `scan-ca-endpoint.py` — orchestrator that runs the two endpoint scans below (as subprocesses).
- `scan-ca-osdetails.py` — collects OS details (`GetOSInfo`, 1) and, per patch-management product (Windows: signature 1103), missing patches (`GetMissingPatches`, 1013) and installed patches (`GetInstalledPatches`, 1023); writes `scan-ca-osdetails-result.json`.
- `scan-ca-third-party.py` — detects installed products (`DetectProducts`, 0) and resolves versions (`GetVersion`, 100); writes `scan-ca-third-party-result.json` (with `product_id` and `os_type` for mapping).

**Server mapping (Step 2 — uses the catalog DB):**
- `map_ca_osdetails.py` — maps `scan-ca-osdetails-result.json` against the Analog catalog (`vuln_system_associations.json` + `cves.json`) to list missing patches and the CVEs each remediates; CVEs already covered by **installed** patches are subtracted (net exposure). Writes `map-ca-osdetails-result.json`. (Follows the ruby sample `get_system_vuln.rb`.)
- `map_ca_third_party.py` — maps `scan-ca-third-party-result.json` against the catalog to produce per product: the CVEs it is affected by (`vuln_associations.json` + `cves.json`, by product id / signature / version range) and the latest version + `patch_missing` flag (`patch_associations.json` + `patch_aggregation.json`). Writes `map-ca-third-party-result.json`. (Follows `get_vuln.rb` + `get_latest_installer.rb`.)
- `map-ca.py` — runs both mappers and merges into `map-ca-result.json` with a unified de-duplicated CVE count.

**Full pipeline + shared helpers:**
- `scan-ca.py` — runs the endpoint scan, then `map-ca.py`, then derives the final **product-centric** report at **`results/ca-result.json`** — a list of products, each with `signature_id`, `product_id`, `name`, `version`, `latest_version`, and the vulnerable `cves`/`cpes` (the OS is one product, signature 1103). Schema matches the endpoint `results/ea-result.json`. Intermediate `*-result.json` files are cleaned up so `results/ca-result.json` is the single output.
- `sdk_wrapper.py` — `ctypes` wrapper around the OESIS `libwaapi` native library.
- `platform_utils.py` — platform/architecture detection and SDK environment validation.

> In a real deployment, Step 1 runs on each endpoint and Step 2 runs on your server; the scan JSON is transferred between them. Running `scan-ca.py` simply does both locally for demonstration.
