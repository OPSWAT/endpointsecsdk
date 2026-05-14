# VAPM Scanner

Vulnerability and Patch Management Scanner. Tracks vulnerability and patch status across systems, identifies remediation opportunities, and generates prioritized recommendations for security teams.

## Purpose

The VAPM scanner helps organizations manage vulnerabilities and patches by:

- Tracking vulnerability status by product and platform
- Identifying available patches and their remediation coverage
- Prioritizing vulnerabilities by severity and exploitability
- Generating remediation roadmaps and action plans
- Providing trend analysis and metrics tracking
- Supporting patch deployment planning and validation

## Overview

This tool combines vulnerability detection with patch management capabilities to provide comprehensive remediation guidance. It helps security teams:

- Plan and prioritize patch deployments
- Understand patch coverage and effectiveness
- Track remediation progress over time
- Meet compliance requirements for vulnerability management
- Reduce time-to-remediation for critical vulnerabilities

## Usage

### Basic Vulnerability and Patch Scan

```bash
python3 scan_patches.py
```

### Scan with Options

```bash
python3 scan_patches.py --output results.json --format json
```

### Options

- `--output <file>` - Output filename (default: vapm_results.json)
- `--format <format>` - Output format: json, csv, html (default: json)
- `--severity <level>` - Filter by severity: critical, high, medium, low
- `--product <name>` - Focus on specific product
- `--remediation-plan` - Generate prioritized remediation recommendations
- `--trend-analysis` - Compare with previous scans
- `--export-kb` - Include knowledge base articles
- `--help` - Display usage information

### Generate Remediation Plan

```bash
python3 remediation_plan.py --output plan.json
```

### Remediation Plan Options

- `--severity <level>` - Filter by severity for plan
- `--max-items <number>` - Limit number of recommendations
- `--exclude-product <name>` - Exclude specific products from plan
- `--timeline <days>` - Show remediation timeline (default: 30 days)

### Examples

Basic scan:
```bash
python3 scan_patches.py --output today_scan.json
```

Find critical patches needed:
```bash
python3 scan_patches.py --severity critical --output critical_patches.json
```

Generate remediation plan:
```bash
python3 remediation_plan.py --severity critical,high --output remediation_plan.json
```

Track trends:
```bash
python3 scan_patches.py --trend-analysis --compare previous_scan.json
```

## Output Format

### JSON Scan Results

```json
{
  "scan_date": "2024-05-14T10:30:00Z",
  "summary": {
    "total_vulnerabilities": 25,
    "critical": 2,
    "high": 5,
    "medium": 10,
    "low": 8,
    "total_patches_available": 18,
    "patch_coverage": 72
  },
  "products": [
    {
      "name": "Microsoft Office 2019",
      "vulnerabilities": 8,
      "patches_available": 6,
      "coverage": 75,
      "details": [
        {
          "cve_id": "CVE-2024-1234",
          "severity": "critical",
          "patch_available": true,
          "kb_article": "KB5044284"
        }
      ]
    }
  ],
  "remediation_opportunities": [
    {
      "product": "Microsoft Office 2019",
      "fixes": 6,
      "coverage": "75%",
      "estimated_time": "2 hours"
    }
  ]
}
```

### Remediation Plan

```json
{
  "plan_date": "2024-05-14",
  "timeline": 30,
  "total_recommendations": 8,
  "by_priority": {
    "critical": [
      {
        "priority": 1,
        "cve": "CVE-2024-1234",
        "product": "Microsoft Office 2019",
        "patch": "KB5044284",
        "risk": "Critical remote code execution",
        "remediation": "Apply security patch immediately",
        "estimated_effort": "30 minutes"
      }
    ],
    "high": [
      {
        "priority": 2,
        "cve": "CVE-2024-5678",
        "product": "Windows Defender",
        "patch": "Monthly update (May 2024)",
        "risk": "Privilege escalation",
        "remediation": "Install latest definitions",
        "estimated_effort": "15 minutes"
      }
    ]
  },
  "deployment_phases": {
    "immediate": {
      "target_days": "1-2",
      "items": 2,
      "total_patches": 2
    },
    "urgent": {
      "target_days": "3-7",
      "items": 3,
      "total_patches": 3
    },
    "planned": {
      "target_days": "8-30",
      "items": 3,
      "total_patches": 3
    }
  }
}
```

### HTML Report

Professional report format showing vulnerability timeline, patch coverage, and remediation recommendations.

## Key Features

- **Comprehensive Analysis**: Vulnerability and patch status in one tool
- **Prioritization**: Severity-based and risk-adjusted prioritization
- **Coverage Metrics**: Understand what percentage of vulnerabilities have patches
- **Trend Tracking**: Compare results between scans to monitor progress
- **Actionable Recommendations**: Specific remediation steps with effort estimates
- **Timeline Planning**: Phased approach to managing remediation workload
- **Integration Ready**: JSON output for tool integration and automation

## Common Tasks

### Identify Patches That Fix Most Vulnerabilities

```bash
python3 scan_patches.py --format json --output patch_analysis.json | \
  jq '.remediation_opportunities | sort_by(-.coverage) | .[0:5]'
```

### Create 30-Day Remediation Plan

```bash
python3 remediation_plan.py \
  --timeline 30 \
  --output remediation_plan_30days.json
```

### Find Products with No Patch Coverage

```bash
python3 scan_patches.py --format json | \
  jq '.products[] | select(.coverage == 0)'
```

### Generate Executive Dashboard Report

```bash
python3 scan_patches.py --format html --output executive_dashboard.html
```

### Track Remediation Progress

```bash
#!/bin/bash
# Baseline scan
python3 vapm-scanner/scan_patches.py --output baseline.json

# ... implement patches ...

# Progress check
python3 vapm-scanner/scan_patches.py --trend-analysis --compare baseline.json
```

## Troubleshooting

**"No patches found"**
- System may be fully patched (good sign)
- Verify OESIS Framework data is current
- Run with `--verbose` for detailed analysis

**"Remediation plan empty"**
- May indicate all vulnerabilities already have solutions planned
- Check specific products with `--product` option

**"SDK not found"**
- Run SDK downloader from main repository
- Verify all data files are present

## Integration Examples

### Automated Patch Planning

```bash
#!/bin/bash
# Generate weekly remediation plan
python3 tools/vapm-scanner/remediation_plan.py \
  --timeline 7 \
  --output weekly_plan_$(date +%Y%m%d).json

# Notify team
send_notification weekly_plan_$(date +%Y%m%d).json
```

### Patch Deployment Workflow

```bash
#!/bin/bash
# Scan for patches
python3 tools/vapm-scanner/scan_patches.py --output pre_patch_scan.json

# Deploy patches (your script)
deploy_patches.sh

# Verify patching
python3 tools/vapm-scanner/scan_patches.py --trend-analysis --compare pre_patch_scan.json
```

### Compliance Reporting

```bash
# Generate monthly compliance report
python3 tools/vapm-scanner/scan_patches.py \
  --format html \
  --output monthly_compliance_$(date +%B_%Y).html
```

### Management Dashboard

```bash
python3 scan_patches.py --format json --output patch_status.json

# Update dashboard
update_management_dashboard patch_status.json
```

## Performance

- Patch scan: typically 5-10 minutes
- Remediation plan generation: typically 1-2 minutes
- Report generation: typically 1-3 minutes
- Memory usage: typically 100-300MB

## Metrics Explained

- **Patch Coverage**: Percentage of vulnerabilities that have available patches
- **Remediation Opportunity**: Groups of patches that address multiple vulnerabilities
- **Priority Score**: Risk-adjusted severity considering exploitability and impact
- **Estimated Effort**: Time required to plan, test, and deploy patches

## Related Tools

- **Posture** - Endpoint security posture assessment
- **python-scanner** - Multi-platform vulnerability detection
- **Catalog Scripts** - Detailed CVE and patch lookup

## Requirements

- Python 3.6 or later
- OESIS Framework SDK
- 500MB+ available disk space
- Administrator/root privileges for full scanning

## Support

For questions or issues:
- Review inline script documentation
- Check the tools directory README
- Contact OPSWAT support: oem@opswat.com
