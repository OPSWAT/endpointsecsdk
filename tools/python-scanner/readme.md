# Python Scanner

Multi-platform vulnerability and patch scanner using Python. Provides cross-platform scanning capabilities for Windows, macOS, and Linux systems with detailed vulnerability and remediation reporting.

## Purpose

The python-scanner utility enables security teams to:

- Scan endpoints across multiple platforms (Windows, macOS, Linux)
- Identify installed software and its vulnerability status
- Detect missing patches and available updates
- Generate detailed vulnerability reports
- Track remediation recommendations
- Integrate scanning into automated security workflows

## Overview

This scanner uses the OESIS Framework SDK to provide consistent vulnerability detection across operating systems. It helps organizations:

- Maintain visibility of software vulnerabilities
- Plan patch and remediation activities
- Comply with security scanning requirements
- Track vulnerability metrics over time

## Usage

### Basic Scan

```bash
python3 scan.py
```

### Platform-Specific Scan

```bash
python3 scan.py --platform windows
python3 scan.py --platform macos
python3 scan.py --platform linux
```

### With Options

```bash
python3 scan.py --platform windows --output results.json --format json
```

### Options

- `--platform <os>` - Target platform: windows, macos, linux (default: auto-detect)
- `--output <file>` - Output filename (default: scan_results.json)
- `--format <format>` - Output format: json, csv, html (default: json)
- `--verbose` - Detailed output with additional diagnostic information
- `--severity <level>` - Filter vulnerabilities by severity: critical, high, medium, low
- `--compare <file>` - Compare with previous scan results
- `--export-remediations` - Export remediation steps for findings
- `--help` - Display usage information

### Examples

Scan current platform with default settings:
```bash
python3 scan.py --output scan_$(date +%Y%m%d).json
```

Scan Windows system with detailed output:
```bash
python3 scan.py --platform windows --verbose --output win_detailed_scan.json
```

Find only critical vulnerabilities:
```bash
python3 scan.py --severity critical --output critical_vulns.json
```

Generate HTML report:
```bash
python3 scan.py --format html --output vulnerability_report.html
```

Compare with previous scan:
```bash
python3 scan.py --output current_scan.json --compare previous_scan.json
```

## Output Format

### JSON Report

```json
{
  "scan_date": "2024-05-14T10:30:00Z",
  "platform": "windows",
  "hostname": "WORKSTATION-01",
  "scan_summary": {
    "total_installed_products": 45,
    "vulnerabilities_found": 8,
    "critical_vulnerabilities": 2,
    "high_vulnerabilities": 3,
    "missing_patches": 12,
    "remediation_available": 10
  },
  "vulnerabilities": [
    {
      "cve_id": "CVE-2024-1234",
      "severity": "critical",
      "affected_product": "Microsoft Office 2019",
      "installed_version": "16.0.12345",
      "description": "Remote code execution vulnerability",
      "remediation": "Update to version 16.0.12350 or later",
      "kb_article": "KB5044284"
    }
  ],
  "installed_software": [
    {
      "name": "Microsoft Office 2019",
      "version": "16.0.12345",
      "vulnerabilities": 2,
      "missing_patches": 3
    }
  ]
}
```

### HTML Report

Professional report format suitable for sharing with stakeholders and compliance teams.

### CSV Report

Structured data for spreadsheet analysis and bulk processing.

## Key Features

- **Cross-Platform Support**: Consistent scanning on Windows, macOS, and Linux
- **Detailed Reporting**: Comprehensive vulnerability and patch information
- **Multiple Output Formats**: JSON for integration, HTML for reporting, CSV for analysis
- **Severity Filtering**: Focus on critical issues or comprehensive analysis
- **Trend Analysis**: Compare results between scans to track improvements
- **Remediation Guidance**: Specific steps and resources for fixing vulnerabilities
- **Automated Integration**: Easy integration with security workflows

## Common Tasks

### Daily Vulnerability Scanning

```bash
#!/bin/bash
TIMESTAMP=$(date +%Y%m%d)
python3 tools/python-scanner/scan.py --output scan_$TIMESTAMP.json
```

### Generate Management Report

```bash
python3 scan.py \
  --format html \
  --severity critical,high \
  --output weekly_vulnerability_report.html
```

### Cross-Platform Inventory

```bash
# Scan all platforms and combine results
for platform in windows macos linux; do
  python3 scan.py --platform $platform --output scan_$platform.json
done

# Combine results for analysis
combine_scan_results.py scan_*.json --output comprehensive_scan.json
```

### Identify New Vulnerabilities

```bash
python3 scan.py --output current_scan.json --compare previous_scan.json

# Review changes
jq '.new_vulnerabilities' current_scan.json
```

### Export Remediation Plan

```bash
python3 scan.py \
  --severity critical,high \
  --export-remediations \
  --output remediation_plan.json
```

## Troubleshooting

**"No vulnerabilities found"**
- This may be normal for a well-maintained system
- Verify that OESIS Framework data is current
- Run with `--verbose` for diagnostic information

**"Permission denied"**
- Some scanning features require elevated privileges
- Run with administrator/sudo privileges for full scan capability

**"Scan taking too long"**
- Large systems with many installed products may take longer
- Consider using `--severity` filter to speed up initial assessment

**"SDK not found"**
- Run SDK downloader from main repository
- Verify sdkroot marker file exists

## Integration Examples

### Continuous Monitoring

```bash
#!/bin/bash
# Run daily scan and save history
python3 tools/python-scanner/scan.py \
  --output daily_scan_$(date +%Y%m%d).json

# Keep 30 days of history
find . -name "daily_scan_*.json" -mtime +30 -delete
```

### Automated Alerts

```bash
#!/bin/bash
python3 tools/python-scanner/scan.py --format json --output scan.json

# Alert on critical vulnerabilities
CRITICAL=$(jq '.scan_summary.critical_vulnerabilities' scan.json)
if [ "$CRITICAL" -gt 0 ]; then
  send_alert "Found $CRITICAL critical vulnerabilities"
fi
```

### SIEM Integration

```bash
python3 scan.py --format json > scan_results.json

# Send to SIEM
curl -X POST -H "Content-Type: application/json" \
  -d @scan_results.json \
  https://siem.example.com/api/vulnerabilities
```

## Performance

- Initial scan: typically 5-15 minutes depending on system size
- Subsequent scans with caching: typically 2-5 minutes
- Report generation: typically 1-3 minutes
- Memory usage: typically 100-300MB

## Supported Platforms

- **Windows**: 7, 8, 10, 11, Server 2008 R2 and later
- **macOS**: 10.12 (Sierra) and later
- **Linux**: Ubuntu 16.04+, CentOS 7+, Debian 8+, and other distributions

## Related Tools

- **Posture** - Comprehensive endpoint security assessment
- **vapm-scanner** - Vulnerability and patch management
- **Catalog Scripts** - Detailed CVE and signature lookup

## Requirements

- Python 3.6 or later
- OESIS Framework SDK
- Administrator/root privileges recommended
- 500MB+ available disk space for SDK and data files

## Support

For questions or issues:
- Review inline script documentation
- Check the tools directory README
- Contact OPSWAT support: oem@opswat.com
