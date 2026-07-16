# Catalog Lookup

Quick command‑line lookups against the OPSWAT **Analog** offline catalog
(`OPSWAT-SDK/extract/analog/server/*.json`). Each script finds the catalog automatically by
walking up for the `sdkroot` marker, so run them from anywhere in the repo.

> Run the SDK downloader first so `OPSWAT-SDK/extract/analog/server` exists.

## Scripts

### `find-kb.py <KB> [--no-size]` — is a KB supported?
Searches `kb_info.json` across every OS section and reports, per OS: the build(s) that contain
the KB, whether it's in the supersedence tree (and cumulative), what it supersedes, and the CVEs
it remediates. Then shows **release details** (`patch_system_aggregation.json`): title, **release
date**, severity, category, reboot flag, **description**, KB‑article link, and each download
per architecture with its **URL, SHA1 hash, and file size**.
```
python find-kb.py 5094127
python find-kb.py KB5094127
python find-kb.py 5041580 --no-size    # skip the live download-size lookup (offline)
```
> Size is not stored in the catalog, so it's fetched live via an HTTP HEAD to the download URL.
> Use `--no-size` to skip that when offline or when you only need the catalog fields.

### `find-cve.py <CVE>` — CVE details + patches (with release details) + CPEs
Shows `cves.json` details (CWE, **severity**, **CVSS vector/score**, published date, **description**),
then the **corresponding patches** with **release details**:
- **OS KB(s)** by OS (`kb_info.json`) — each with its **release title, release date, severity,
  category, reboot flag, KB‑article/release‑note link, and download link**
  (`patch_system_aggregation.json`).
- **3rd‑party** affected product(s) with the **version to upgrade to** and **release notes**
  (`patch_associations` + `patch_aggregation`), plus vulnerable version ranges.
- Associated **CPEs** (`vuln_associations.json`).
```
python find-cve.py CVE-2024-38063
```
*(cves.json is large — the first lookup takes a few seconds to load it.)*

### `find-signature.py <application name>` — name → signature id
Given an application name (or any part of one), returns the matching product **signature id(s)**
from `products.json` (matches product name, signature names, and marketing names). Pair the
result with `find-patch-for-signature.py`.
```
python find-signature.py chrome
python find-signature.py ".net runtime"
python find-signature.py "visual studio 2022"
```

### `find-patch-for-signature.py <signature_id>` — patch/installer for a product signature
Shows the product/vendor a signature identifies (`products.json`) and the patch association(s)
(`patch_associations.json`) with the latest available version, release notes, and each download
(`patch_aggregation.json`) with its **URL and SHA256 hash**.
```
python find-patch-for-signature.py 41       # Google Chrome
python find-patch-for-signature.py 3880     # Microsoft .NET Runtime 8.0 x64
```

### `find-cpe.py <cpe or substring>` — what a CPE maps to
Searches `vuln_associations.json` for matching CPE strings and shows the product(s)/signature(s)
they belong to and the CVEs associated with each.
```
python find-cpe.py cpe:/a:microsoft:.net
python find-cpe.py "google:chrome"
```

### `find-patch.py <patch_uuid>` — OS patch by patch UUID
Looks up a patch in `patch_system_aggregation_v2.json` and shows its KB, bulletin id(s), data
source, and every **package** (architecture variant) it contains — each with title, release
info, download **URL + SHA1**, and applicable OS list.
```
python find-patch.py 1f1b6061-3355-4ba6-ac92-5f4e625e2cc0
```

### `find-package.py <package_uuid>` — OS patch package by package UUID
Finds a single package and shows its detail plus the **parent patch** it belongs to
(patch UUID, KB, bulletin id(s)).
```
python find-package.py 50babee7-7339-4c3a-9e75-e6adc041e66c
```

### `find-bulletin.py <bulletin id or substring>` — patches for a bulletin
Finds the patch(es) tied to a security bulletin (e.g. `MS22-0110-5009497`, or a substring like
`MS22-0110` or `5009497`) and lists the patch UUID, KB, and packages for each.
```
python find-bulletin.py MS22-0110-5009497
python find-bulletin.py 5009497
```

## Data sources
| File | Used for |
|---|---|
| `kb_info.json` | KB supersedence / build / KB→CVE (find-kb, find-cve OS patches) |
| `cves.json` | CVE metadata (find-cve) |
| `vuln_associations.json` | 3rd‑party CVE ↔ product ↔ CPE (find-cve, find-cpe) |
| `products.json` | signature/product/vendor names |
| `patch_associations.json` + `patch_aggregation.json` | signature → patch → latest version/downloads/release notes |
| `patch_system_aggregation_v2.json` | OS patch release details (title, date, severity, KB article, download URL+SHA1) and patch/package/bulletin lookups — used by find-kb, find-cve, find-patch, find-package, find-bulletin |
| `os_info.json` | os_id → OS name |

_Created by Chris Seiler — OPSWAT OEM Field CTO_
