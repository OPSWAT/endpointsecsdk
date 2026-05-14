# GetListOfChangedSigs

Extracts a list of signature IDs that have changed between two versions of the OESIS Framework catalog.

## Purpose

GetListOfChangedSigs is a specialized utility for security operations teams that need to understand which detection signatures have been added, modified, or removed. This is particularly useful for:

- Updating SIEM rules and detection configurations
- Validating security product signature databases
- Tracking coverage changes for specific threats
- Generating reports of signature-level updates
- Planning detection system updates

## Overview

By comparing two catalog versions at the signature level, this utility produces a focused list of signature ID changes without the broader CVE and product context. This makes it ideal for:

- Rapid integration into update scripts
- Bulk import into security tools
- Verification of signature coverage
- Audit trails of detection capability changes

## Usage

### Basic Usage

Extract signature changes:
```bash
python3 GetListOfChangedSigs.py --old-db /path/to/old/catalog --new-db /path/to/new/catalog
```

### Options

- `--old-db <path>` - Path to previous catalog data directory (required)
- `--new-db <path>` - Path to new catalog data directory (default: current directory)
- `--output <file>` - Output filename (default: `changed_sigs.txt`)
- `--format <format>` - Output format: list, json, csv (default: list)
- `--type <type>` - Filter by change type: added, removed, all (default: all)
- `--by-product` - Group signature changes by product
- `--help` - Display usage information

### Examples

Get all changed signatures:
```bash
python3 GetListOfChangedSigs.py --old-db /backup/catalog --new-db /current/catalog
```

Get only newly added signatures:
```bash
python3 GetListOfChangedSigs.py --old-db /backup/catalog --new-db /current/catalog --type added
```

Export as JSON for tool integration:
```bash
python3 GetListOfChangedSigs.py --old-db /backup/catalog --new-db /current/catalog --format json
```

Group by product:
```bash
python3 GetListOfChangedSigs.py --old-db /backup/catalog --new-db /current/catalog --by-product
```

## Output Format

### Simple List (Default)

```
# Added Signatures
1001
1002
1003
5001
5002

# Removed Signatures
500
501
502
```

### JSON Format

```json
{
  "added": [1001, 1002, 1003, 5001, 5002],
  "removed": [500, 501, 502],
  "total_added": 5,
  "total_removed": 3
}
```

### CSV Format

```
Signature ID,Change Type
1001,added
1002,added
1003,added
5001,added
5002,added
500,removed
501,removed
502,removed
```

### By Product Format

```
Microsoft Office 2019:
  Added: 1001, 1002, 1003
  Removed: 500, 501

Windows Defender:
  Added: 5001, 5002
  Removed: 502
```

## Key Features

- **Focused Output**: Signature-level changes without broader context
- **Multiple Formats**: List, JSON, CSV for different use cases
- **Flexible Filtering**: Show all changes or specific types
- **Product Grouping**: Organize signatures by product if needed
- **Easy Integration**: Simple output formats for scripts and tools

## Common Tasks

### Update Security Product Signatures

```bash
# Get new signatures to add to detection system
python3 GetListOfChangedSigs.py --old-db /old --new-db /new --type added > new_sigs.txt

# Process for your security tool
while read sig_id; do
  security_tool add-signature $sig_id
done < new_sigs.txt
```

### Export for SIEM Integration

```bash
python3 GetListOfChangedSigs.py --old-db /old --new-db /new --format json > sig_changes.json

# Parse and import into SIEM
jq '.added[]' sig_changes.json | while read sig; do
  siem_import_signature $sig
done
```

### Generate Audit Report

```bash
python3 GetListOfChangedSigs.py --old-db /old --new-db /new --format csv > audit_signature_changes.csv
# Commit to version control for tracking
git add audit_signature_changes.csv
git commit -m "Signature changes report for catalog v2.1"
```

### Find Signatures Removed (Deprecated)

```bash
python3 GetListOfChangedSigs.py --old-db /old --new-db /new --type removed > deprecated_sigs.txt

# Verify these are no longer needed
cat deprecated_sigs.txt
```

## Troubleshooting

**"No changes detected"**
- Verify both catalogs are different versions
- Ensure both directories have valid OESIS Framework data files

**"Catalog directory not found"**
- Check paths to old and new catalog directories
- Verify the directories contain the required signature data

**"Output file permission denied"**
- Ensure write permissions in the output directory
- Use a different output path with `--output` option

## Integration Examples

### Bash Script Integration

```bash
#!/bin/bash
NEW_SIGS=$(python3 GetListOfChangedSigs.py --old-db /old --new-db /new --type added)
for sig in $NEW_SIGS; do
  echo "Importing signature: $sig"
  # your import logic here
done
```

### Python Script Integration

```python
import json
import subprocess

result = subprocess.run(
    ["python3", "GetListOfChangedSigs.py", "--old-db", "/old", "--new-db", "/new", "--format", "json"],
    capture_output=True
)
changes = json.loads(result.stdout)

for sig_id in changes["added"]:
    update_detection_system(sig_id)
```

## Performance

- Signature comparison: typically < 5 seconds
- Output generation: immediate for most catalog sizes
- Memory usage: typically < 200MB

## Related Utilities

- **GenChanges** - Comprehensive catalog changes report
- **GenCVEToSig** - Complete CVE-to-signature mappings
- **FindCVE** - Search for specific CVE signatures

## Support

For questions or issues:
- Review inline script documentation
- Check the main repository README for setup instructions
- Contact OPSWAT support: oem@opswat.com
