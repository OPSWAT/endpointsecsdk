# GenCVEToSig

Generates mappings between CVEs (Common Vulnerabilities and Exposures) and their corresponding signature IDs in the OESIS Framework catalog.

## Purpose

This utility creates a structured JSON document that maps each CVE to its associated signature IDs, product IDs, patch information, and KB articles. This mapping is essential for security operations teams to quickly understand which detection signatures cover specific vulnerabilities and what remediation options are available.

## Overview

GenCVEToSig reads OESIS Framework catalog data (CVEs, patch associations, KB information, and product data) and produces a comprehensive JSON output file that can be:

- Consumed by security information and event management (SIEM) systems
- Imported into patch management platforms
- Used as a reference for vulnerability remediation workflows
- Integrated into custom security tools and dashboards

## Input Data

The utility requires the following OESIS Framework data files:

- `cves.json` - List of known CVEs and their metadata
- `vuln_associations.json` - Associations between CVEs and products
- `patch_associations.json` - Associations between patches and products
- `patch_aggregation.json` - Aggregated patch information
- `patch_system_aggregation.json` - System-level patch aggregation
- `vuln_system_associations.json` - System vulnerability associations
- `kb_info.json` - Knowledge base article references
- `products.json` - Product registry with signature ID references

## Output Format

The output JSON file contains a mapping of CVEs with the following structure:

```json
{
  "map": {
    "CVE-2024-0001": {
      "cpe": ["cpe:/a:vendor/product"],
      "productIds": [
        {
          "id": 100,
          "sigIds": [1001, 1002, 1003]
        }
      ],
      "patch": true,
      "kb": true,
      "kbs": [5044284],
      "names": ["Product Name", "Signature Name"]
    }
  },
  "summary": {
    "cves": {
      "total_in_db": 50000,
      "mapped": 45000,
      "with_patch": 35000,
      "with_kb": 40000
    },
    "mappings": {
      "sig": 125000,
      "sig_with_patch": 95000,
      "kb": 85000,
      "total": 200000
    },
    "patches": {
      "total": 5000,
      "third_party": 3000,
      "system": 2000,
      "by_platform": {
        "windows": 1500,
        "mac": 400,
        "linux": 100
      }
    }
  }
}
```

## Usage

### Basic Usage

```bash
python3 gen-cve-to-sig.py
```

When `--db-dir` is omitted, the script looks for `cves.json` in the current
directory, then walks up the tree looking for
`OPSWAT-SDK/extract/analog/server/`.

### Options

- `--db-dir <path>` - Path to the Analog `server/` directory (default: auto-discovered)
- `--out <file>` - Output JSON file path (default: `cve_remediation_map.json`)
- `--pretty` - Pretty-print JSON with indentation (human readable, larger file)
- `--help` - Display usage information

### Examples

Generate default mapping (auto-discover the db directory):
```bash
python3 gen-cve-to-sig.py
```

Generate with an explicit db directory:
```bash
python3 gen-cve-to-sig.py --db-dir ./server
```

Generate with a custom output path:
```bash
python3 gen-cve-to-sig.py --out my_cve_mapping.json
```

Generate prettified output for easy inspection:
```bash
python3 gen-cve-to-sig.py --pretty --out cve_mapping_pretty.json
```

## Key Features

- **Complete CVE Coverage**: Maps all known CVEs in the catalog
- **Product-Centric**: Groups signatures by product ID for better organization
- **Patch Status**: Indicates whether a CVE has available patches (third-party or system)
- **Knowledge Base Integration**: Links CVEs to KB articles for remediation guidance
- **Performance Optimized**: Efficient processing of large catalogs; one CVE per line by default
- **Summary Statistics**: Provides counts of mapped signatures, patches by platform, and coverage metrics

## Output File Performance

The default output format (one CVE per line) is optimized for:

- Editor compatibility (prevents editor hangs with large files)
- File size equivalence to fully compact JSON
- Valid JSON structure for programmatic parsing
- Easier diff/merge workflows in version control

Use `--pretty` for human-readable inspection, but store large mappings in the default format.

## Troubleshooting

**"Database files not found"**
- Ensure you're running the script from the correct directory
- Verify all required data files are present in the current directory
- Use `--db-dir` to specify the location of OESIS Framework data

**"No CVEs mapped"**
- Check that catalog data is not corrupted
- Verify the data format matches OESIS AnalogV2 schema
- Check the summary statistics to understand what data was found

**"Output file is too large"**
- This is normal for comprehensive catalogs with 50,000+ CVEs
- Use the default format (not `--pretty`) to minimize file size
- Consider post-processing with jq to filter for specific products or vulnerabilities

## Integration

### With SIEM Systems

Export the CVE mapping and import into your SIEM for correlation with detection events:

```bash
# Example: Parse specific CVE data for import
cat cve_remediation_map.json | jq '.map[] | select(.patch==true)'
```

### With Patch Management

Use the patch status and KB information to prioritize patch deployment:

```bash
# Find CVEs with patches and KB articles
cat cve_remediation_map.json | jq '.map[] | select(.patch==true and .kb==true)'
```

### With Custom Tools

The JSON output is straightforward to parse in any programming language:

```python
import json

with open('cve_remediation_map.json') as f:
    data = json.load(f)
    for cve_id, details in data['map'].items():
        print(f"{cve_id}: {len(details.get('productIds', []))} products")
```

## Performance Notes

- Processing time depends on catalog size (typically 1-5 minutes for full catalogs)
- Memory usage scales with the number of CVEs and associations
- Output file size is typically 10-50MB depending on catalog comprehensiveness

## Support

For issues or questions:
- Check the main repository README for SDK setup
- Review script inline documentation for implementation details
- Contact OPSWAT support: oem@opswat.com
