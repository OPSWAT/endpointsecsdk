# FindCVE

Searches the OESIS Framework catalog for CVEs and retrieves associated vulnerability data, patch information, and signature mappings.

## Purpose

FindCVE is a command-line utility for querying the vulnerability catalog. It allows security teams to:

- Quickly locate specific CVEs in the catalog
- View associated products and signature IDs
- Check patch availability and remediation status
- Access knowledge base articles and remediation guidance
- Filter results by product, severity, or other criteria

## Overview

This utility reads OESIS Framework catalog data and provides a fast, flexible search interface for finding CVE information and understanding the scope of vulnerabilities across products and platforms.

## Usage

FindCVE searches the consolidated app-centric file (`app_centric.json`) for a CVE
and reports the products that reference it. If the app-centric file is missing or
older than two hours, it is regenerated first (which downloads and extracts the
catalog). Results are printed to the console and also written to a timestamped
`results_<cve>_<timestamp>.txt` file in the current directory.

### Basic Search

Find a specific CVE:
```bash
python3 FindCVE.py --cve CVE-2024-1234
```

### Options

- `--cve <CVE-ID>` - CVE identifier to search for (default: `UNKNOWN`)
- `--appcentric <file>` - Path to the app-centric JSON file (default: `app_centric.json`)
- `--catalogdir <path>` - Path to the extracted catalog directory (default: `./CatalogExtract`)
- `--tokenfile <file>` - Path to the download token file (default: `download_token.txt`)
- `--help` - Display usage information

### Examples

Search for a CVE:
```bash
python3 FindCVE.py --cve CVE-2024-1234
```

Search using a specific app-centric file:
```bash
python3 FindCVE.py --cve CVE-2024-1234 --appcentric app_centric.json
```

Search using a specific catalog directory and token file:
```bash
python3 FindCVE.py --cve CVE-2024-1234 --catalogdir ./CatalogExtract --tokenfile download_token.txt
```

## Output Format

### Console / Text Output

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

- **App-Centric Search**: Looks up CVEs in the consolidated `app_centric.json` file
- **Auto-Refresh**: Regenerates the app-centric file if it is missing or over two hours old
- **Product Details**: Reports product name, signature ID, vendor, and product line
- **Vulnerability and Patch Hits**: Shows both vulnerability matches and patch matches for the CVE
- **Results File**: Writes a timestamped results file alongside console output

## Common Tasks

### Search for a CVE and Capture Results

```bash
python3 FindCVE.py --cve CVE-2024-1234
# Results are also written to results_CVE-2024-1234_<timestamp>.txt
```

### Search Multiple CVEs

```bash
#!/bin/bash
for cve in CVE-2024-1234 CVE-2024-5678; do
  python3 FindCVE.py --cve $cve
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

- **GenCVEToSig** - Generate complete CVE-to-signature mappings
- **GenChanges** - Filter CVEs by cutoff date
- **AppCentricFile** - Build and refresh the app-centric catalog data

## Support

For questions or issues:
- Review inline script documentation
- Check the main repository README for setup instructions
- Contact OPSWAT support: oem@opswat.com
