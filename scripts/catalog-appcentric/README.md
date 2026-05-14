# Catalog AppCentric

This directory contains utilities for mapping CVEs to signature IDs and managing application-centric patch and vulnerability data. These tools help organizations understand which security signatures are associated with specific vulnerabilities and how patches remediate them.

## Overview

The catalog-appcentric utilities provide a suite of Python scripts that work together to:

- Map CVEs to their corresponding signature IDs
- Track patch availability and remediation status
- Analyze changes in patch and vulnerability data over time
- Generate reports on vulnerability coverage and remediation options
- Maintain and update application-centric catalog associations

## Available Utilities

| Utility | Description |
|---------|-------------|
| AppCentricFile | Manages and validates application-centric catalog files, including data format conversion and schema validation. |
| FindCVE | Searches for CVEs in the catalog and retrieves associated metadata, patch information, and signature IDs. |
| GenCVEToSig | Generates mappings between CVEs and their corresponding signature IDs, producing structured output for integration with security tools. |
| GenChanges | Identifies and reports changes between catalog versions, tracking new vulnerabilities, patches, and signature associations. |
| GetListOfChangedSigs | Extracts lists of signature IDs that have changed between catalog versions for targeted analysis and updates. |

## Getting Started

### Prerequisites

- Python 3.6 or later
- OESIS Framework SDK data files (cves.json, patch_associations.json, etc.)
- Access to the SDK downloader (see main repository setup instructions)

### Basic Usage

Each utility can be run independently and provides help documentation:

```bash
python3 GenCVEToSig.py --help
python3 FindCVE.py --help
python3 GenChanges.py --help
```

### Typical Workflow

1. **Prepare Data**: Use AppCentricFile to validate and prepare your catalog data
2. **Generate Mappings**: Run GenCVEToSig to create CVE-to-signature mappings
3. **Find Vulnerabilities**: Use FindCVE to search for specific CVEs and their remediation options
4. **Track Changes**: Run GenChanges and GetListOfChangedSigs to understand what has evolved between catalog updates

## Output Formats

Most utilities produce JSON output for easy parsing and integration:

- CVE mappings include product IDs, signature IDs, patch availability, and KB articles
- Change reports show additions, modifications, and deprecations
- Signature lists are formatted for bulk import into security management systems

## Key Concepts

- **CVE (Common Vulnerabilities and Exposures)**: Standardized identifiers for known security vulnerabilities
- **Signature ID**: OPSWAT's internal identifier for a security detection rule or patch
- **Product ID**: Identifier for a specific software product or application
- **Patch**: Software update that remediates one or more vulnerabilities
- **KB Article**: Knowledge base article with remediation guidance

## Troubleshooting

**"Data file not found"**
- Ensure all required SDK data files are in the expected location
- Run the SDK downloader from the main repository first

**"Invalid JSON output"**
- Check that input data files are not corrupted
- Verify the catalog data matches the expected AnalogV2 schema

**"Missing CVE in results"**
- Some CVEs may not have associated signatures or patches in your current catalog
- Check catalog version and availability of the specific CVE

## Notes

- All utilities work with OESIS Framework AnalogV2 data format
- Output files are typically large JSON documents; consider using JSON processors (jq) for parsing
- Catalog updates may add, modify, or deprecate signature associations; use GenChanges to stay aware of updates
- Most scripts include inline comments and docstrings for development reference

## Support

For questions about specific utilities or workflows:

- Review the main repository README for SDK setup
- Consult inline script documentation for parameter details
- Contact OPSWAT support at oem@opswat.com

## Related Documentation

- [OESIS Framework Documentation](https://software.opswat.com/OESIS_V4/html/index.html)
- [SDK Downloader](../../sdk-downloader/README.md)
- [Hello World Samples](../../helloworld/README.md)
