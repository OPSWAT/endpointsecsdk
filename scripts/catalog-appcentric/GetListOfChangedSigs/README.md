# GetListOfChangedSigs

Looks up a CVE in the OESIS Framework app-centric catalog and reports the products
(and their signature IDs) that reference it.

## Purpose

GetListOfChangedSigs is a command-line utility for finding the products and signature
IDs associated with a specific CVE. The entry point is `get-delta-sigs.py`. It is
useful for:

- Identifying which detection signatures cover a given CVE
- Listing the products affected by a CVE along with their signature IDs
- Capturing per-CVE results to a file for later review

## Overview

GetListOfChangedSigs reads the consolidated app-centric file (`app_centric.json`). If
that file is missing or older than two hours, it is regenerated first (which downloads
and extracts the catalog). The script then searches for the requested CVE and reports
each matching product with its signature ID, vendor, vulnerability hits, and patch
hits. Results are printed to the console and also written to a timestamped
`results_<cve>_<timestamp>.txt` file in the current directory.

## Usage

### Basic Usage

Look up a CVE:
```bash
python3 get-delta-sigs.py --cve CVE-2024-1234
```

### Options

- `--cve <CVE-ID>` - CVE identifier to search for (default: `UNKNOWN`)
- `--appcentric <file>` - Path to the app-centric JSON file (default: `app_centric.json`)
- `--catalogdir <path>` - Path to the extracted catalog directory (default: `./CatalogExtract`)
- `--tokenfile <file>` - Path to the download token file (default: `download_token.txt`)
- `--help` - Display usage information

### Examples

Look up a CVE:
```bash
python3 get-delta-sigs.py --cve CVE-2024-1234
```

Use a specific app-centric file:
```bash
python3 get-delta-sigs.py --cve CVE-2024-1234 --appcentric app_centric.json
```

Use a specific catalog directory and token file:
```bash
python3 get-delta-sigs.py --cve CVE-2024-1234 --catalogdir ./CatalogExtract --tokenfile download_token.txt
```

## Output Format

Both the console output and the generated results file use the same human-readable
format:

```
Products referencing CVE-2024-1234:
------------------------------------------------------------
Product: Microsoft Office 2019  (sig_id: 100)
Vendor:  Microsoft
Line:    Office
Vulnerabilities:
  CVE: CVE-2024-1234, CPE: cpe:/a:microsoft:office, Ranges: [...]
Patch Hits:
  Patch: Microsoft Office 2019, Version: ..., Bulletin: KB5044284, Release Date: ..., Reboot Required: ..., Architectures: ...

Catalog release date (local time): 2024-10-10 00:46:31
Results written to: results_CVE-2024-1234_20240612_120000.txt
```

## Key Features

- **App-Centric Lookup**: Finds products and signature IDs for a CVE in `app_centric.json`
- **Auto-Refresh**: Regenerates the app-centric file if it is missing or over two hours old
- **Product and Signature Detail**: Reports product name, signature ID, vendor, and product line
- **Vulnerability and Patch Hits**: Shows both vulnerability matches and patch matches for the CVE
- **Results File**: Writes a timestamped results file alongside console output

## Common Tasks

### Look Up a CVE and Capture Results

```bash
python3 get-delta-sigs.py --cve CVE-2024-1234
# Results are also written to results_CVE-2024-1234_<timestamp>.txt
```

### Look Up Multiple CVEs

```bash
#!/bin/bash
for cve in CVE-2024-1234 CVE-2024-5678; do
  python3 get-delta-sigs.py --cve $cve
done
```

## Troubleshooting

**"No products reference <CVE>"**
- Verify the CVE ID format (should be CVE-YYYY-NNNNN)
- The CVE may not be associated with any product in the current catalog
- Check that the app-centric file is up to date

**"Could not find app_centric.json"**
- Ensure the token file exists so the app-centric file can be generated
- Verify the path passed to `--appcentric`

**Download or extraction errors during refresh**
- Confirm the token in `--tokenfile` is valid
- Verify network connectivity to the download URL

## Related Utilities

- **GenChanges** - Filter CVEs by cutoff date
- **GenCVEToSig** - Complete CVE-to-signature mappings
- **FindCVE** - Search for specific CVEs and details

## Support

For questions or issues:
- Review inline script documentation
- Check the main repository README for setup instructions
- Contact OPSWAT support: oem@opswat.com
