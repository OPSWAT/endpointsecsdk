# GenChanges

Analyzes changes between two versions of the OESIS Framework catalog and generates a detailed report of additions, modifications, and deprecations.

## Purpose

GenChanges helps security teams track the evolution of the vulnerability and patch catalog. It identifies:

- New CVEs introduced in the latest catalog
- New signature associations and patch availability
- Deprecated or removed signatures
- Changes in product or patch status
- Impact on existing security detection coverage

This is essential for understanding what has changed between updates and ensuring that security operations workflows remain current.

## Overview

By comparing two catalog versions, GenChanges produces a structured report that highlights all differences. This helps organizations:

- Validate that expected CVEs and patches are in the new catalog
- Identify newly covered vulnerabilities
- Understand the scope of catalog maintenance and updates
- Plan updates to dependent systems (SIEMs, patch management, etc.)
- Maintain audit trails of security catalog changes

## Usage

### Basic Usage

Compare two catalog directories:
```bash
python3 GenChanges.py --old-db /path/to/old/catalog --new-db /path/to/new/catalog
```

### Options

- `--old-db <path>` - Path to previous catalog data directory (required)
- `--new-db <path>` - Path to new catalog data directory (default: current directory)
- `--output <file>` - Output filename (default: `catalog_changes.json`)
- `--format <format>` - Output format: json, text, csv (default: json)
- `--summary-only` - Show only summary statistics, not individual changes
- `--filter <type>` - Filter by change type: added, modified, removed (default: all)
- `--include-products` - Include detailed product-level changes
- `--help` - Display usage information

### Examples

Generate changes report:
```bash
python3 GenChanges.py --old-db /backup/catalog_v1 --new-db /current/catalog
```

Generate text summary only:
```bash
python3 GenChanges.py --old-db /backup/catalog_v1 --new-db /current/catalog --summary-only --format text
```

Show only new CVEs:
```bash
python3 GenChanges.py --old-db /backup/catalog_v1 --new-db /current/catalog --filter added
```

## Output Format

### JSON Output (Default)

```json
{
  "summary": {
    "total_cves_old": 50000,
    "total_cves_new": 52000,
    "cves_added": 2500,
    "cves_removed": 500,
    "cves_modified": 3200,
    "total_signatures_added": 5000,
    "total_signatures_removed": 1200,
    "products_added": 15,
    "products_modified": 45
  },
  "changes": {
    "added_cves": [
      {
        "cve_id": "CVE-2024-5000",
        "products": ["Microsoft Office 2024"],
        "signatures": [9001, 9002],
        "has_patch": true
      }
    ],
    "modified_cves": [
      {
        "cve_id": "CVE-2024-1234",
        "changes": {
          "new_products": ["New Product v2"],
          "new_signatures": [9003],
          "patch_status": {
            "old": false,
            "new": true
          }
        }
      }
    ],
    "removed_cves": [
      {
        "cve_id": "CVE-2020-0001",
        "reason": "No longer applicable"
      }
    ]
  }
}
```

### Text Summary Output

```
Catalog Changes Report
=======================

Summary Statistics:
  CVEs (Old): 50000
  CVEs (New): 52000
  Added: 2500
  Removed: 500
  Modified: 3200

Signatures:
  Added: 5000
  Removed: 1200

Products:
  Added: 15
  Modified: 45

New CVEs: 2500
Modified CVEs: 3200
Removed CVEs: 500
```

## Key Features

- **Comprehensive Comparison**: Compares all aspects of catalog data
- **Multiple Output Formats**: JSON for tools, text for reports
- **Detailed Change Tracking**: Shows exactly what changed for each CVE
- **Filter Options**: Focus on specific types of changes
- **Summary Statistics**: Quick overview of catalog evolution

## Common Tasks

### Find All New CVEs

```bash
python3 GenChanges.py --old-db /backup/catalog --new-db /current/catalog | jq '.changes.added_cves'
```

### Export Changes for Audit Trail

```bash
python3 GenChanges.py --old-db /backup/catalog --new-db /current/catalog --format text > audit_log.txt
```

### Get Summary Statistics Only

```bash
python3 GenChanges.py --old-db /backup/catalog --new-db /current/catalog --summary-only
```

### Find CVEs with New Patches

```bash
python3 GenChanges.py --old-db /backup/catalog --new-db /current/catalog | \
  jq '.changes.modified_cves[] | select(.changes.patch_status.old==false and .changes.patch_status.new==true)'
```

## Troubleshooting

**"Catalog directory not found"**
- Verify paths to both old and new catalog directories
- Ensure directories contain valid OESIS Framework data files

**"Incompatible catalog versions"**
- Both catalogs must be in AnalogV2 format
- Ensure both directories have all required data files

**"No changes detected"**
- Catalogs may be identical or missing required comparison files
- Verify both directories contain different versions of the catalog

## Workflow Integration

### Automated Catalog Validation

```bash
#!/bin/bash
OLD_CATALOG="/backup/last_week"
NEW_CATALOG="/data/current"

python3 GenChanges.py --old-db $OLD_CATALOG --new-db $NEW_CATALOG \
  --format json > changes.json

# Alert if too many CVEs were removed (possible data issue)
REMOVED=$(jq '.summary.cves_removed' changes.json)
if [ $REMOVED -gt 1000 ]; then
  echo "WARNING: $REMOVED CVEs removed - verify catalog integrity"
fi
```

### Version Control Integration

```bash
# Save changes to version control
python3 GenChanges.py --old-db /old/catalog --new-db /new/catalog \
  --format text > catalog_changes_v2.1.txt
git add catalog_changes_v2.1.txt
git commit -m "Catalog v2.1 changes report"
```

## Related Utilities

- **GetListOfChangedSigs** - Extract specific signature ID changes
- **GenCVEToSig** - Generate complete CVE-to-signature mappings
- **FindCVE** - Search for specific CVEs and details

## Performance

- Comparison time: typically 2-10 seconds for large catalogs
- Memory usage: scales with the number of changes
- Output file size: typically 1-10MB depending on extent of changes

## Support

For questions or issues:
- Review inline script documentation
- Check the main repository README for setup instructions
- Contact OPSWAT support: oem@opswat.com
