import json
import os
import xml.etree.ElementTree as ET
from datetime import datetime, timezone


def _generate_filename(scan_type, hostname, fmt):
    timestamp = datetime.now(timezone.utc).strftime("%Y%m%d_%H%M%S")
    return f"{scan_type}_{hostname}_{timestamp}.{fmt}"


def write_json(data, output_dir, scan_type, hostname):
    filename = _generate_filename(scan_type, hostname, "json")
    filepath = os.path.join(output_dir, filename)
    with open(filepath, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2, default=str)
    return filepath

def write_output(data, output_dir, scan_type, hostname):
    """Write scan results in the requested formats. Returns list of file paths."""
    os.makedirs(output_dir, exist_ok=True)

    # Add timestamp to the data
    data["timestamp"] = datetime.now(timezone.utc).isoformat()

    paths = []
    paths.append(write_json(data, output_dir, scan_type, hostname))
    return paths
