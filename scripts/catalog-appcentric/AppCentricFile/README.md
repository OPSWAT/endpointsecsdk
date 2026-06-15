# AppCentricFile

Builds a consolidated application-centric catalog file for the OESIS Framework.

## Purpose

AppCentricFile is a utility for building the data structure that maps applications, vulnerabilities, patches, and detection signatures. It helps teams:

- Download and extract the OESIS Framework catalog (analog.zip)
- Consolidate product, patch, and vulnerability data into a single file
- List each product by signature ID with its associated CVEs
- Resolve the patch needed to reach the latest installer for each product
- Produce input data consumed by the other catalog-appcentric utilities

## Overview

This utility is the main entry point and orchestrator for the catalog simplification process. It loads product, patch, and vulnerability data from an extracted catalog directory and writes a single consolidated app-centric JSON file.

## Usage

The entry point is `gen_app_centric_file.py`. It downloads and extracts the OESIS
Framework catalog (analog.zip), then writes a consolidated app-centric JSON file
that lists each product by signature ID along with its associated CVEs and the
patch needed to reach the latest installer.

### Generate the App-Centric File

```bash
python3 gen_app_centric_file.py --dir ./CatalogExtract --out app_centric.json
```

### Options

- `--dir <path>` - Path to the extracted catalog folder (default: `./CatalogExtract`)
- `--out <file>` - Output JSON path (default: `app_centric.json`)
- `--token-file <file>` - Path to the download token file (default: `download_token.txt`)
- `--url-template <url>` - Download URL template containing `{token}` (default: `https://vcr.opswat.com/gw/file/download/analog.zip?type=1&token={token}`)
- `--zip-path <file>` - Where to save the downloaded analog.zip (default: `analog.zip`)
- `--extract-dir <path>` - Where to extract the downloaded files (default: `./CatalogExtract`)
- `--help` - Display usage information

### Examples

Generate with defaults (downloads, extracts, and writes `app_centric.json`):
```bash
python3 gen_app_centric_file.py
```

Generate to a custom output path:
```bash
python3 gen_app_centric_file.py --out office_catalog.json
```

Use a specific token file and extract directory:
```bash
python3 gen_app_centric_file.py --token-file my_token.txt --extract-dir ./CatalogExtract
```

## Key Features

- **Catalog Download**: Fetches and extracts analog.zip using a download token
- **Product Consolidation**: Lists each product by signature ID with its associated CVEs
- **Patch Resolution**: Selects the patch needed to reach the latest installer
- **Single Output File**: Produces one consolidated app-centric JSON document
- **Helper Modules**: Built from reusable helpers (`download_catalog.py`, `patch_classes.py`, `system_patch.py`, `third_party.py`, `util.py`)

## How It Works

The build process:

- Downloads and extracts the OESIS Framework catalog (analog.zip) using the token
- Loads product, patch, and vulnerability data from the extracted `analog/server` directory
- Loads additional product metadata from the Moby compliance data
- Builds third-party product records and the Windows system product record
- Writes the consolidated app-centric JSON with a metadata block and a `products` list

## Troubleshooting

**"Folder not found"**
- Verify the path passed to `--dir` (or `--extract-dir`) points to the extracted catalog
- Ensure the catalog was downloaded and extracted successfully

**"File not found"**
- Confirm the token file exists at the path passed to `--token-file`
- Ensure required files (e.g. `analog/server/products.json`, `analog/header.json`) are present after extraction

**"Download failed"**
- Verify the token in the token file is valid and not expired
- Check the `--url-template` value and network connectivity

## Common Tasks

### Generate Before Deployment

```bash
#!/bin/bash
python3 gen_app_centric_file.py --dir ./CatalogExtract --out app_centric.json

if [ $? -eq 0 ]; then
  echo "App-centric file generated"
  # proceed with deployment
else
  echo "Generation failed"
  exit 1
fi
```

### Generate to a Dated Output File

```bash
python3 gen_app_centric_file.py --out app_centric_$(date +%Y%m%d).json
```

### Use a Custom Download URL Template

```bash
python3 gen_app_centric_file.py \
  --url-template "https://vcr.opswat.com/gw/file/download/analog.zip?type=1&token={token}"
```

## Integration

### CI/CD Pipeline

```bash
#!/bin/bash
# Generate the app-centric file before committing
python3 gen_app_centric_file.py --dir ./CatalogExtract --out app_centric.json || exit 1
echo "App-centric file generated"
```

## Output Examples

### Console Output

```
Loading Moby data for additional product metadata...
Wrote app_centric.json with 2500 signature entries.
```

### Output Sample

```json
{
  "meta": {
    "release_date": 1728520991
  },
  "products": [
    {
      "name": "Microsoft Office 2019",
      "signature_id": 100,
      "vulnerabilities": [
        {
          "cve": "CVE-2024-1234",
          "cpe": "cpe:/a:microsoft:office",
          "ranges": []
        }
      ],
      "patches": [
        {
          "name": "Microsoft Office 2019",
          "bulletin": "KB5044284"
        }
      ]
    }
  ]
}
```

## Related Utilities

- **GenCVEToSig** - Generate CVE-to-signature mappings
- **FindCVE** - Search for specific CVEs
- **GenChanges** - Track catalog changes between versions

## Performance

- Download and extraction time depends on catalog size and network speed
- Build time scales with the number of products and associations in the catalog

## Support

For questions or issues:
- Review inline script documentation
- Check the main repository README for setup instructions
- Contact OPSWAT support: oem@opswat.com
