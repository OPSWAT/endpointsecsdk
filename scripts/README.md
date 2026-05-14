# Scripts

This directory contains Python-based utilities and scripts for working with OESIS Framework data, including patch and vulnerability catalog management, data merging, and analysis tools.

## Overview

The scripts in this directory are designed to help build, combine, and analyze patch and vulnerability datasets. They provide examples for:

- Merging analog and patch data for product mapping
- Generating CVE-to-signature mappings
- Tracking and analyzing changes in patch and vulnerability data
- Creating and maintaining catalog associations

## Folder Structure

| Folder | Description |
|--------|-------------|
| catalog-appcentric | Utilities for mapping CVEs to signature IDs and managing application-centric patch and vulnerability data. |

## Getting Started

### Prerequisites

- Python 3.6 or later
- Required Python libraries (see individual script documentation for specific requirements)
- Access to OESIS Framework SDK and data files

### Running the Scripts

Each script in this directory (and its subdirectories) includes inline documentation and usage examples. To run a script:

```bash
python3 script_name.py --help
```

This will display the script's purpose, usage, and available command-line options.

## Catalog-AppCentric

The `catalog-appcentric` subdirectory contains specialized utilities for managing application-centric patch and vulnerability catalogs. These tools help map CVEs to signature IDs, track patch availability, and generate reports on vulnerability remediation options.

See `catalog-appcentric/README.md` for detailed information on the utilities and workflows available in that directory.

## Notes

- All scripts are Python-based and cross-platform (Windows, macOS, Linux)
- Output formats are typically JSON for easy integration with other tools
- Most scripts are designed to work with OESIS Framework AnalogV2 data format
- Detailed logging and error handling help troubleshoot data processing issues

## Support

For questions about specific scripts or data format issues, please refer to:

- Individual script documentation (headers and comments within the scripts)
- The main repository README for SDK setup information
- OPSWAT documentation: https://software.opswat.com/OESIS_V4/html/index.html

For additional assistance, contact:
📧 oem@opswat.com
