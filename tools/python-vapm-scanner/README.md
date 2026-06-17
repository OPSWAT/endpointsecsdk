# Python VAPM Scanner

A Python Vulnerability And Patch Management (VAPM) sample built on the OESIS Framework SDK. It demonstrates **two ways to assess an endpoint for missing patches and CVEs**, and shows that both workflows can produce the **same final result** — letting you choose where the cost of the large OESIS database files lands.

Both workflows produce a product-centric result with an **identical schema** (`results/ea-result.json` and `results/ca-result.json`): a list of products, each with `signature_id`, `product_id`, `name`, `version`, `latest_version`, and the vulnerable `cves` / `cpes`.

## The two workflows

| Workflow | Folder | Where the database files live | What runs where |
|----------|--------|-------------------------------|-----------------|
| **Endpoint (traditional)** | [`endpoint-assessment/`](endpoint-assessment/README.md) | **On the endpoint** | The endpoint loads the OESIS database files (`v2mod.dat`, `wuov2.dat`, `wiv-lite.dat`, …) and resolves **all** vulnerability/patch detail locally. |
| **Centralized** | [`centralized-assessment/`](centralized-assessment/README.md) | **On the server** | The endpoint runs a **minimal scan with no database files** (just product/OS detection). The minimal results are sent to a **cloud or on-premise server**, which does the heavy **mapping** against the catalog database files. |

### Why two workflows?

- **Endpoint / traditional** is simplest to deploy, but every endpoint must carry the full OESIS database files (which can be large) and do the matching work.
- **Centralized** keeps the endpoint footprint tiny — no catalog database files are copied to the endpoint at all. The endpoint only collects a small amount of detail (detected products + versions, OS info, patch lists); the server holds the database files and performs the CVE/patch mapping. This lets organizations concerned with on-endpoint database size **take that hit on the server side** instead.

The key point of the sample: **the end result matches through either workflow**, so you can pick the deployment model that fits your constraints without changing the outcome.

## Prerequisites

1. **Run the SDK downloader first** so the `OPSWAT-SDK/` directory is populated — see [sdk-downloader/README.md](../../sdk-downloader/README.md).
2. Place your license files (`license.cfg`, `pass_key.txt`, `download_token.txt`) in [`eval-license/`](../../eval-license/README.md) at the repository root.
3. Python 3.7+ (standard library only).

## Quick start

```bash
# Traditional endpoint workflow:
cd endpoint-assessment
python copysdk.py        # stage the SDK (incl. database files) onto the endpoint
python scan-ea.py        # -> results/ea-result.json

# Centralized workflow:
cd ../centralized-assessment
python copysdk.py        # stage the SDK runtime + license (no catalog DB needed on the endpoint)
python scan-ca.py        # endpoint scan + server-side mapping -> results/ca-result.json
```

See each folder's README for the script-by-script breakdown. The endpoint folder runs everything locally; the centralized folder separates its scripts into the **endpoint scan** (no database files) and the **server-side mapping** (uses the catalog database files).
