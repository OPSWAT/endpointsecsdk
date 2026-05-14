# Posture

Comprehensive endpoint security posture assessment and reporting tool. Provides detailed analysis of installed security products, missing patches, vulnerable software, and overall compliance status.

## Purpose

The posture tool helps organizations understand their endpoint security status by:

- Inventorying installed security products
- Assessing active protection status
- Identifying missing patches and vulnerabilities
- Generating compliance and risk reports
- Tracking security posture over time

## Overview

This tool uses the OESIS Framework SDK to perform comprehensive security assessments on endpoints. It scans for:

- Installed antivirus, firewalls, and security tools
- Active threat protection status
- System vulnerabilities and missing patches
- Compliance with organizational policies
- Security configuration and hardening status

## Usage

### Basic Assessment

```bash
python3 run_assessment.py
```

### With Options

```bash
python3 run_assessment.py --output results.json --format json
```

### Options

- `--output <file>` - Output filename (default: posture_report.json)
- `--format <format>` - Output format: json, csv, html (default: json)
- `--detailed` - Include detailed findings and recommendations
- `--compliance` - Focus on compliance reporting
- `--severity <level>` - Filter by severity: critical, high, medium, low
- `--save-history` - Save results for trend analysis
- `--help` - Display usage information

### Examples

Basic assessment with JSON output:
```bash
python3 run_assessment.py --output today_assessment.json
```

Generate HTML compliance report:
```bash
python3 run_assessment.py --format html --output compliance_report.html --compliance
```

Assessment with detailed findings:
```bash
python3 run_assessment.py --detailed --format json --output detailed_assessment.json
```

Save for historical tracking:
```bash
python3 run_assessment.py --save-history
```

## Output Format

### JSON Report

```json
{
  "assessment_date": "2024-05-14T10:30:00Z",
  "endpoint": {
    "hostname": "WORKSTATION-01",
    "os": "Windows 10 21H2",
    "ip_address": "192.168.1.100"
  },
  "summary": {
    "overall_risk": "medium",
    "products_installed": 5,
    "products_active": 4,
    "missing_patches": 12,
    "vulnerabilities": 8,
    "compliance_score": 78
  },
  "installed_products": [
    {
      "name": "Windows Defender",
      "status": "active",
      "last_update": "2024-05-14T08:00:00Z",
      "version": "1.1.23057.0"
    }
  ],
  "missing_patches": [
    {
      "kb_id": "KB5044284",
      "title": "Windows Security Update",
      "severity": "critical",
      "release_date": "2024-05-01"
    }
  ],
  "vulnerabilities": [
    {
      "cve_id": "CVE-2024-1234",
      "severity": "high",
      "affected_product": "Microsoft Office 2019",
      "remediation": "Update to latest version"
    }
  ],
  "recommendations": [
    "Install 12 missing security patches",
    "Enable real-time file scanning",
    "Update antivirus signatures"
  ]
}
```

### HTML Report

Generated as an interactive, professional report suitable for management and compliance documentation.

### CSV Report

Tab-separated values for spreadsheet analysis and bulk reporting.

## Key Features

- **Comprehensive Assessment**: Full security posture evaluation
- **Multi-Format Output**: JSON for integration, HTML for reporting, CSV for analysis
- **Risk Scoring**: Overall risk assessment with detailed breakdown
- **Compliance Reporting**: Alignment with security policies and standards
- **Historical Tracking**: Compare results over time to measure improvements
- **Detailed Recommendations**: Actionable guidance for remediation

## Common Tasks

### Generate Executive Report

```bash
python3 run_assessment.py --format html --output executive_summary.html --compliance
```

### Scan for Critical Issues Only

```bash
python3 run_assessment.py --severity critical --output critical_findings.json
```

### Track Security Posture Trend

```bash
# Run regularly and save history
python3 run_assessment.py --save-history

# Later, generate trend report
python3 generate_trend_report.py --output trend_analysis.html
```

### Export for SIEM Integration

```bash
python3 run_assessment.py --format json --output endpoint_posture.json
# Import into SIEM system
siem_import endpoint_posture.json
```

### Compliance Audit

```bash
python3 run_assessment.py \
  --compliance \
  --detailed \
  --format html \
  --output compliance_audit_$(date +%Y%m%d).html
```

## Troubleshooting

**"Permission denied"**
- This tool requires elevated privileges to access system security information
- Run with administrator/sudo privileges

**"No products detected"**
- Verify OESIS Framework SDK is properly installed
- Check that SDK data files are present and current

**"Assessment taking too long"**
- Detailed assessment may take 5-15 minutes depending on system complexity
- Use `--severity` filter to focus on critical issues only

**"Invalid output format"**
- Verify output format is supported: json, csv, html
- Check that output directory is writable

## Integration Examples

### Automated Daily Assessment

```bash
#!/bin/bash
# Run daily assessment
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
python3 tools/posture/run_assessment.py \
  --save-history \
  --output daily_assessment_$TIMESTAMP.json

# Keep only last 30 days
find . -name "daily_assessment_*.json" -mtime +30 -delete
```

### SIEM Integration

```bash
#!/bin/bash
python3 tools/posture/run_assessment.py --format json > posture_report.json

# Send to SIEM
curl -X POST \
  -H "Content-Type: application/json" \
  -d @posture_report.json \
  https://siem.example.com/api/events
```

### Compliance Dashboard

```bash
#!/bin/bash
# Generate compliance report for dashboard
python3 tools/posture/run_assessment.py \
  --compliance \
  --format json \
  --output compliance_report.json

# Update dashboard
update_compliance_dashboard compliance_report.json
```

## Performance

- Initial assessment: typically 5-15 minutes
- Reassessment (with caching): typically 2-5 minutes
- Report generation: typically 1-3 minutes
- Memory usage: typically 100-300MB

## Related Tools

- **python-scanner** - Multi-platform vulnerability scanner
- **vapm-scanner** - Vulnerability and patch management
- **Catalog Scripts** - CVE and signature lookup

## Requirements

- Windows 7 or later, macOS 10.12 or later, or Linux (Ubuntu, CentOS, etc.)
- OESIS Framework SDK
- Python 3.6 or later
- Administrator/root privileges for full assessment

## Support

For questions or issues:
- Review inline script documentation
- Check the tools directory README
- Contact OPSWAT support: oem@opswat.com
