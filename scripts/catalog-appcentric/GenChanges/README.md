────────────────────────────────────────────────────────────
README.md  (included first so the file is self-documenting)
────────────────────────────────────────────────────────────

# OPSWAT CVE Filter Tool (`gen-changes.py`)

This tool parses the `cves.json` file from the OPSWAT Analog Patch & Vulnerability Catalog and produces a filtered version that only includes CVEs newer than a specified cutoff date.

It is designed for use cases such as:
- Generating delta CVE data for partners
- Reducing catalog size for lightweight agents
- Validating only newly introduced vulnerabilities

---

## ✅ What the Script Does

| Function | Description |
|----------|-------------|
| Downloads catalog (optional) | If a valid OPSWAT token is provided, the script will automatically download and extract `analog.zip` |
| Loads `cves.json` | Parses the main CVE dataset used by the Endpoint Security SDK |
| Filters by date | Keeps only CVEs where `published_epoch` or `last_modified_epoch` is **greater than or equal to** the cutoff |
| Preserves structure | Output JSON remains in the same format (`oesis → [header, cves]`) |
| Writes new JSON file | Saves results to a path defined by `--out` |

---

## 🛠️ Requirements

- Python **3.8 or higher**
- Extracted OPSWAT Patch Catalog (`analog.zip`) OR a valid token for auto-download
- The catalog folder must contain:

```
<CATALOG_DIR>/
  analog/
    server/
      cves.json
```

---

## 🚀 Usage

### Filter by published date (default)

```bash
python gen-changes.py \
  --catalogdir ./CatalogExtract \
  --cutoff 2025-11-01 \
  --epochfield published_epoch \
  --out cve_delta.json
```

### Filter using `last_modified_epoch` instead

```bash
python gen-changes.py \
  --catalogdir ./CatalogExtract \
  --cutoff 2024-06-01 \
  --epochfield last_modified_epoch \
  --out modified_delta.json
```

### Auto-download the analog catalog before filtering

```bash
python gen-changes.py \
  --catalogdir ./CatalogExtract \
  --tokenfile download_token.txt \
  --cutoff 2025-11-01
```

---

## 🔧 CLI Arguments

| Argument | Default | Description |
|----------|---------|-------------|
| `--catalogdir` | `./CatalogExtract` | Directory where catalog is extracted |
| `--tokenfile` | `download_token.txt` | Token file for downloading `analog.zip` |
| `--cutoff` | `2025-11-01` | Cutoff date (`YYYY-MM-DD`), inclusive |
| `--epochfield` | `last_modified_epoch` | Field to filter on (`published_epoch` or `last_modified_epoch`) |
| `--out` | `cve_delta.json` | Output file path |

---

## 📄 Output Format Example

```json
{
  "oesis": [
    { "header": { ... } },
    {
      "cves": {
        "CVE-2025-12345": { ... },
        "CVE-2025-99887": { ... }
      }
    }
  ]
}
```

---

## ⚠️ Notes

- CVEs missing the selected epoch field are skipped
- Script does **not** modify the full catalog, only writes a filtered output file
- Auto-download requires helper module `download_catalog.py` in `../AppCentricFile/`
