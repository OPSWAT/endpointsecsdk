# GenChanges

Filters the OESIS Framework `cves.json` by a cutoff date and produces a delta file of
CVEs that changed (or were published) on or after that date.

## Purpose

GenChanges helps security teams focus on recent activity in the vulnerability catalog.
It downloads and refreshes the catalog as needed, then keeps only the CVEs whose
chosen epoch field (published or last-modified) is at or after a cutoff date. Each
retained CVE is mapped to the products that reference it.

This is useful for:

- Producing a periodic delta of newly published or recently modified CVEs
- Limiting downstream processing to CVEs that changed since the last run
- Mapping recent CVEs to the affected products

## Overview

GenChanges relies on the app-centric file produced by AppCentricFile. If the
app-centric file is missing or older than two hours, it is regenerated first (which
downloads and extracts the catalog). The script then reads `cves.json` from the
extracted catalog, filters by the cutoff, and writes a delta JSON file that preserves
the original `oesis` structure with a filtered `cves` block keyed to the matching
products.

## Usage

### Basic Usage

Filter CVEs by a cutoff date:
```bash
python3 gen-changes.py --catalogdir ./CatalogExtract --cutoff 2025-11-01 --out cve_delta.json
```

### Options

- `--catalogdir <path>` - Extracted catalog directory (default: `./CatalogExtract`)
- `--tokenfile <file>` - Token file used to download analog.zip (default: `download_token.txt`)
- `--cutoff <datetime>` - Cutoff date, inclusive, in UTC. Accepts `YYYY-MM-DD`, `YYYY-MM-DDTHH:MM`, or `YYYY-MM-DDTHH:MM:SS` (default: `2025-11-01T00:00:00`)
- `--epochfield <field>` - Which epoch field to compare against: `published_epoch` or `last_modified_epoch` (default: `last_modified_epoch`)
- `--appcentric <file>` - App-centric JSON path (default: `app_centric.json`)
- `--out <file>` - Output delta JSON path (default: `cve_delta.json`)
- `--help` - Display usage information

### Examples

Filter using the last-modified epoch (default):
```bash
python3 gen-changes.py --catalogdir ./CatalogExtract --cutoff 2025-11-01 --out cve_delta.json
```

Filter using the published epoch:
```bash
python3 gen-changes.py --catalogdir ./CatalogExtract --cutoff 2025-11-01 --epochfield published_epoch --out cve_delta.json
```

Use a specific cutoff time and token file:
```bash
python3 gen-changes.py --cutoff 2025-11-01T12:00:00 --tokenfile download_token.txt
```

## Output Format

The output preserves the original `oesis` structure. The header is annotated with the
cutoff, and the `cves` block contains only CVEs at or after the cutoff, each mapped to
the products that reference it:

```json
{
  "oesis": [
    {
      "header": {
        "cutoff_timestamp": 1761955200,
        "cutoff_time": "2025-11-01T00:00:00"
      }
    },
    {
      "cves": {
        "CVE-2025-5000": ["9001-Microsoft Office 2024"]
      }
    }
  ]
}
```

On completion the script prints a summary line, for example:

```
[OK] 2500 CVEs kept where last_modified_epoch >= 2025-11-01. Wrote: cve_delta.json
```

## Key Features

- **Date-Based Filtering**: Keep only CVEs at or after a cutoff date
- **Selectable Epoch Field**: Compare against published or last-modified epoch
- **Auto-Refresh**: Regenerates the app-centric file if missing or over two hours old
- **Product Mapping**: Maps each retained CVE to the products that reference it
- **Structure-Preserving Output**: Keeps the original `oesis` envelope

## Common Tasks

### Generate a Recent-CVE Delta

```bash
python3 gen-changes.py --catalogdir ./CatalogExtract --cutoff 2025-11-01 --out cve_delta.json
```

### Inspect the Delta with jq

```bash
python3 gen-changes.py --cutoff 2025-11-01 --out cve_delta.json
jq '.oesis[1].cves | keys | length' cve_delta.json
```

## Troubleshooting

**"Could not find cves.json"**
- Verify the path passed to `--catalogdir` points to the extracted catalog
- The expected file is at `<catalogdir>/analog/server/cves.json`

**"Could not find app_centric.json"**
- Ensure the token file exists so the app-centric file can be generated
- Verify the path passed to `--appcentric`

**"Invalid cutoff_date"**
- Use `YYYY-MM-DD`, `YYYY-MM-DDTHH:MM`, or `YYYY-MM-DDTHH:MM:SS`

## Related Utilities

- **GetListOfChangedSigs** - Search the app-centric file for a CVE
- **GenCVEToSig** - Generate complete CVE-to-signature mappings
- **FindCVE** - Search for specific CVEs and details

## Performance

- Filtering time scales with the number of CVEs in the catalog
- Download and extraction time depends on catalog size and network speed

## Support

For questions or issues:
- Review inline script documentation
- Check the main repository README for setup instructions
- Contact OPSWAT support: oem@opswat.com
