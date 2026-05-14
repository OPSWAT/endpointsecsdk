# AppCentricFile

Manages, validates, and converts application-centric catalog files for the OESIS Framework.

## Purpose

AppCentricFile is a utility for working with the data structures that map applications, vulnerabilities, patches, and detection signatures. It helps teams:

- Validate catalog file integrity and schema compliance
- Convert between different data formats
- Export specific subsets of catalog data
- Verify product-to-signature associations
- Generate reports on catalog structure and content

## Overview

This utility provides a comprehensive toolkit for managing the core data structures that power the catalog-appcentric utilities. It ensures data consistency, supports migration between formats, and provides insights into catalog organization.

## Usage

### Validate Catalog Files

```bash
python3 AppCentricFile.py --validate --data-dir /path/to/catalog
```

### Convert Between Formats

```bash
python3 AppCentricFile.py --convert --input-format json --output-format csv --input file.json --output file.csv
```

### Export Catalog Subset

```bash
python3 AppCentricFile.py --export --product "Microsoft Office" --output office_catalog.json
```

### Options

- `--validate` - Validate catalog files for schema compliance
- `--data-dir <path>` - Path to catalog data directory
- `--convert` - Convert between file formats
- `--input-format <fmt>` - Input format: json, csv (default: json)
- `--output-format <fmt>` - Output format: json, csv (default: json)
- `--input <file>` - Input file path
- `--output <file>` - Output file path
- `--export` - Export specific catalog data
- `--product <name>` - Product name to export
- `--report` - Generate catalog content report
- `--help` - Display usage information

### Examples

Validate all catalog files:
```bash
python3 AppCentricFile.py --validate --data-dir /data/catalog
```

Generate a validation report:
```bash
python3 AppCentricFile.py --validate --data-dir /data/catalog --report
```

Export data for a specific product:
```bash
python3 AppCentricFile.py --export --product "Windows Defender" --output defender_data.json
```

Convert CSV to JSON:
```bash
python3 AppCentricFile.py --convert --input-format csv --output-format json --input data.csv --output data.json
```

## Key Features

- **Schema Validation**: Ensures catalog data conforms to OESIS AnalogV2 schema
- **Format Conversion**: Convert between JSON and CSV formats
- **Data Export**: Extract subsets of catalog data
- **Integrity Checks**: Verify consistency of product, CVE, and signature associations
- **Reporting**: Generate detailed reports on catalog structure
- **Error Reporting**: Clear diagnostics for invalid data

## Validation

The validation process checks for:

- Required fields in all records
- Valid CVE ID format (CVE-YYYY-NNNNN)
- Valid product and signature ID references
- Data type compliance
- Referential integrity (products referenced in associations must exist)
- Completeness of associations

## Troubleshooting

**"Schema validation failed"**
- Review the detailed error messages
- Check for missing required fields in the data
- Verify that all referenced products and CVEs exist

**"Invalid product reference"**
- A CVE association references a product that doesn't exist in the catalog
- Review the products.json file for the missing product

**"File not found"**
- Verify the path to the catalog data directory
- Ensure all required data files are present

**"Format conversion failed"**
- Verify input file format is valid
- Check that the input file is not corrupted
- Ensure compatibility between source and destination formats

## Common Tasks

### Validate Before Deployment

```bash
#!/bin/bash
python3 AppCentricFile.py --validate --data-dir /staging/catalog

if [ $? -eq 0 ]; then
  echo "Catalog validation passed"
  # proceed with deployment
else
  echo "Catalog validation failed"
  exit 1
fi
```

### Export Product Subset for Third-Party Use

```bash
python3 AppCentricFile.py --export --product "Apache" --output apache_vulnerabilities.json
```

### Generate Audit Report

```bash
python3 AppCentricFile.py --validate --data-dir /data/catalog --report > catalog_audit_$(date +%Y%m%d).txt
```

### Prepare Data for Custom Processing

```bash
# Convert to CSV for spreadsheet processing
python3 AppCentricFile.py --convert --input-format json --output-format csv \
  --input catalog_data.json --output catalog_data.csv
```

## Integration

### CI/CD Pipeline

```bash
#!/bin/bash
# Validate catalog before committing
python3 AppCentricFile.py --validate --data-dir ./catalog_data || exit 1
echo "Catalog validation passed"
```

### Data Migration

```bash
# Export old format, convert to new format
python3 AppCentricFile.py --convert \
  --input-format csv --output-format json \
  --input legacy_catalog.csv --output new_catalog.json
```

## Output Examples

### Validation Report

```
Catalog Validation Report
=========================

Files Checked: 8
Schema Version: AnalogV2

Results:
  CVE Records: 50,000 - OK
  Product Records: 2,500 - OK
  Signature Records: 125,000 - OK
  Associations: 200,000 - OK

Validation Status: PASSED
```

### Export Sample

```json
{
  "product": "Microsoft Office 2019",
  "product_id": 100,
  "vulnerabilities": [
    {
      "cve_id": "CVE-2024-1234",
      "signatures": [1001, 1002],
      "patches": ["KB5044284"]
    }
  ],
  "total_vulnerabilities": 542,
  "total_signatures": 1,245
}
```

## Related Utilities

- **GenCVEToSig** - Generate CVE-to-signature mappings
- **FindCVE** - Search for specific CVEs
- **GenChanges** - Track catalog changes between versions

## Performance

- Validation: typically 10-30 seconds for complete catalogs
- Export: typically < 5 seconds
- Conversion: typically 5-15 seconds depending on data size

## Support

For questions or issues:
- Review inline script documentation
- Check the main repository README for setup instructions
- Contact OPSWAT support: oem@opswat.com
