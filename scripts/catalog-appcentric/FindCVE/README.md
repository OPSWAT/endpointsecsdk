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

### Basic Search

Find a specific CVE:
```bash
python3 FindCVE.py CVE-2024-1234
```

### Search Options

- `CVE-ID` - CVE identifier to search for
- `--product <name>` - Filter results to a specific product
- `--format <format>` - Output format: json, text, csv (default: text)
- `--include-patches` - Show available patches for the CVE
- `--include-kb` - Show associated knowledge base articles
- `--db-dir <path>` - Path to OESIS Framework data directory
- `--help` - Display usage information

### Examples

Search for a CVE:
```bash
python3 FindCVE.py CVE-2024-1234
```

Search with JSON output:
```bash
python3 FindCVE.py CVE-2024-1234 --format json
```

Find CVEs affecting a specific product:
```bash
python3 FindCVE.py CVE-2024-1234 --product "Microsoft Office"
```

Include patch and KB information:
```bash
python3 FindCVE.py CVE-2024-1234 --include-patches --include-kb
```

## Output Format

### Text Output (Default)

```
CVE-2024-1234
  Description: [CVE description]
  CVSS Score: 7.5
  Products Affected:
    - Microsoft Office 2019 (Product ID: 100)
      Signatures: 1001, 1002, 1003
      Patches: Yes (3rd-party)
    - Microsoft Office 365 (Product ID: 101)
      Signatures: 1004, 1005
      Patches: Yes (System)
  Knowledge Base: KB5044284, KB5044285
```

### JSON Output

```json
{
  "cve_id": "CVE-2024-1234",
  "description": "...",
  "cvss_score": 7.5,
  "products": [
    {
      "id": 100,
      "name": "Microsoft Office 2019",
      "signatures": [1001, 1002, 1003],
      "has_patch": true,
      "patch_type": "third_party"
    }
  ],
  "knowledge_base": ["KB5044284", "KB5044285"]
}
```

### CSV Output

```
CVE ID,Product ID,Product Name,Signature IDs,Has Patch,Patch Type,KB Articles
CVE-2024-1234,100,Microsoft Office 2019,"1001,1002,1003",true,third_party,"KB5044284,KB5044285"
```

## Key Features

- **Fast Lookup**: Indexed search for quick results
- **Multi-Format Output**: JSON, CSV, and human-readable text
- **Comprehensive Details**: Product, signature, patch, and KB information in one query
- **Flexible Filtering**: Search by CVE, product, or other criteria
- **Cross-Platform**: Works on Windows, macOS, and Linux

## Common Tasks

### Check if a CVE has Patches

```bash
python3 FindCVE.py CVE-2024-1234 --include-patches | grep -i "patch"
```

### Export CVE Data to CSV

```bash
python3 FindCVE.py CVE-2024-1234 --format csv > cve_data.csv
```

### Find All CVEs Affecting a Product

```bash
python3 FindCVE.py --product "Microsoft Office" --format json
```

### Get JSON Output for Integration

```bash
python3 FindCVE.py CVE-2024-1234 --format json | jq '.products[] | .id'
```

## Troubleshooting

**"CVE not found"**
- Verify the CVE ID format (should be CVE-YYYY-NNNNN)
- Check that catalog data is up-to-date
- The CVE may not be in the current dataset

**"No products found"**
- The CVE may exist but have no associated product signatures in this catalog
- Try searching with different products or check KB information

**"Database not found"**
- Ensure you're in the correct directory or use `--db-dir` to specify the data location
- Verify OESIS Framework data files are present

## Integration

### With Other Tools

Export JSON for processing in other security tools:

```bash
# Get CVE data and pipe to another tool
python3 FindCVE.py CVE-2024-1234 --format json | process_cve_data.py
```

### With Scripts

Use FindCVE output in bash scripts:

```bash
#!/bin/bash
for cve in CVE-2024-1234 CVE-2024-5678; do
  python3 FindCVE.py $cve --format json >> all_cves.json
done
```

## Performance

- Single CVE lookup: typically < 100ms
- Large product filters: may take 1-5 seconds depending on catalog size
- Memory usage: scales with catalog size (typically < 500MB)

## Related Utilities

- **GenCVEToSig** - Generate complete CVE-to-signature mappings
- **GenChanges** - Track changes between catalog versions
- **AppCentricFile** - Manage and validate catalog data

## Support

For questions or issues:
- Review inline script documentation
- Check the main repository README for setup instructions
- Contact OPSWAT support: oem@opswat.com
