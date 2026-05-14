# Tools

This directory contains higher-level utilities and scanning tools that demonstrate advanced use of the OESIS Framework SDK. These tools provide practical solutions for endpoint security assessment, patch management, vulnerability tracking, and compliance reporting.

## Overview

The tools in this directory build on the SDK capabilities to provide:

- **Endpoint Security Assessment**: Scan systems for installed products and active protections
- **Patch Management**: Identify missing patches and plan remediation
- **Vulnerability Scanning**: Detect and track known vulnerabilities
- **Compliance Reporting**: Generate reports on security posture and coverage
- **Data Analysis**: Aggregate and analyze security data across endpoints

## Available Tools

| Tool | Description |
|------|-------------|
| posture | Comprehensive endpoint posture assessment and reporting tool. Scans for installed products, missing patches, and vulnerable software; generates detailed compliance and risk reports. |
| python-scanner | Multi-platform Python-based vulnerability and patch scanner. Provides cross-platform scanning for Windows, macOS, and Linux systems. |
| vapm-scanner | Vulnerability and patch management scanner. Tracks vulnerability status, patch availability, and remediation recommendations. |

## Common Workflows

### Quick Security Assessment

```bash
# Scan local system for security posture
python3 tools/posture/run_assessment.py

# Generate compliance report
python3 tools/posture/generate_report.py --output compliance_report.html
```

### Cross-Platform Scanning

```bash
# Run scanner on multiple platforms
python3 tools/python-scanner/scan.py --platform windows --output win_results.json
python3 tools/python-scanner/scan.py --platform macos --output mac_results.json
python3 tools/python-scanner/scan.py --platform linux --output linux_results.json
```

### Vulnerability and Patch Management

```bash
# Identify missing patches
python3 tools/vapm-scanner/scan_patches.py --output patches_needed.json

# Generate remediation plan
python3 tools/vapm-scanner/remediation_plan.py --severity high --output urgent_fixes.txt
```

## Getting Started

### Prerequisites

- Python 3.6 or later
- OESIS Framework SDK (download using sdk-downloader)
- Required Python libraries (see individual tool README for details)
- Access to OESIS Framework data files

### Installation

Each tool is standalone and can be run independently:

```bash
# Navigate to the specific tool
cd tools/posture/

# View help documentation
python3 run_assessment.py --help

# Run the tool
python3 run_assessment.py
```

## Tool-Specific Documentation

### Posture Tool

The posture assessment tool provides comprehensive analysis of endpoint security:

- Installed products inventory
- Active protection status
- Missing patches and vulnerabilities
- Compliance status reporting

See `posture/README.md` for detailed documentation.

### Python Scanner

Multi-platform scanning utility:

- Windows, macOS, and Linux support
- Detailed vulnerability and patch reports
- JSON and HTML output formats
- Integration with SIEM and reporting systems

See `python-scanner/README.md` for detailed documentation.

### VAPM Scanner

Vulnerability and patch management scanner:

- Track vulnerability status by product
- Identify available patches
- Generate remediation priorities
- Trend analysis and historical tracking

See `vapm-scanner/README.md` for detailed documentation.

## Output Formats

Most tools support multiple output formats:

- **JSON**: For integration with other tools and systems
- **CSV**: For spreadsheet analysis
- **HTML**: For executive reports and compliance documentation
- **Text**: For command-line viewing and logging

## Integration

### With Monitoring Systems

Export JSON data for SIEM and monitoring platform import:

```bash
python3 tools/posture/run_assessment.py --format json --output endpoint_posture.json

# Import into your monitoring system
siem_import endpoint_posture.json
```

### With Compliance Reporting

Generate reports for compliance documentation:

```bash
python3 tools/posture/generate_report.py \
  --template compliance \
  --output_format pdf \
  --output compliance_report.pdf
```

### With Patch Management

Export patch data for patch management system:

```bash
python3 tools/vapm-scanner/scan_patches.py \
  --format csv \
  --output missing_patches.csv

# Import into patch management platform
patch_manager import missing_patches.csv
```

## Performance Considerations

- Full endpoint assessment: typically 5-15 minutes depending on system complexity
- Scanning multiple systems: consider parallel execution with batching
- Report generation: typically 1-5 minutes for detailed compliance reports

## Troubleshooting

**"SDK not found"**
- Run the SDK downloader from the main repository
- Verify sdkroot marker file exists at repository root

**"Permission denied"**
- Some tools require elevated privileges for full access
- Run with appropriate permissions (sudo on Linux/macOS, Administrator on Windows)

**"No data found"**
- Verify OESIS Framework data files are present
- Check that SDK was successfully downloaded

## Security Considerations

- These tools access sensitive system information
- Run on systems with appropriate access controls
- Store output reports securely
- Follow organizational policies for sensitive data

## Recommended Practices

1. **Regular Assessment**: Run assessments periodically to track security posture over time
2. **Baseline Comparison**: Compare results to organizational baselines
3. **Automation**: Integrate tools into automated security scanning workflows
4. **Documentation**: Keep assessment results for compliance and audit purposes
5. **Remediation**: Use reports to prioritize and track remediation activities

## Related Resources

- [SDK Downloader](../sdk-downloader/README.md) - Get the latest SDK libraries
- [Hello World Samples](../helloworld/README.md) - SDK integration examples
- [Catalog Scripts](../scripts/README.md) - Vulnerability and patch catalog utilities

## Support

For questions about specific tools:

- Review tool-specific README documentation
- Check inline script documentation and help text
- Contact OPSWAT support: oem@opswat.com

For SDK integration questions:

- Review the SDK documentation: https://software.opswat.com/OESIS_V4/html/index.html
- Check Hello World sample implementations
- Contact OPSWAT support: oem@opswat.com
