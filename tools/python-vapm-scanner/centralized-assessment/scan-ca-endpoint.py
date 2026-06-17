#!/usr/bin/env python3
###############################################################################################
##  VAPM Centralized Assessment — Endpoint Scan (Orchestrator)
##  Reference Implementation using OESIS Framework
##
##  Runs the full centralized endpoint assessment by invoking the individual scans in turn:
##      scan-ca-osdetails.py    -> missing patches (GetMissingPatches / 1013)
##      scan-ca-third-party.py  -> detected products + versions (DetectProducts / GetVersion)
##
##  Each scan is a standalone script (it stages nothing itself — run copysdk.py first) and
##  is executed as a subprocess so its own SDK setup/teardown is self-contained.
##
##  Usage:
##      python3 copysdk.py            # stage the SDK + license into ./sdk first
##      python3 scan-ca-endpoint.py
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import json
import os
import subprocess
import sys

# Force UTF-8 console output so non-ASCII child output doesn't crash on Windows (cp1252).
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))

# The individual scans this orchestrator runs, in order, with the result file each writes.
SCANS = [
    ("OS Details / Missing Patches", "scan-ca-osdetails.py",   "scan-ca-osdetails-result.json"),
    ("Third-Party Products",         "scan-ca-third-party.py", "scan-ca-third-party-result.json"),
]

# The single, consolidated endpoint scan file that the server-side mappers consume.
COMBINED_RESULT = os.path.join(SCRIPT_DIR, "scan-ca-endpoint-result.json")


def run_scan(label, script_name):
    script_path = os.path.join(SCRIPT_DIR, script_name)
    print(f"\n{'#' * 70}")
    print(f"#  {label}  ({script_name})")
    print(f"{'#' * 70}")

    if not os.path.isfile(script_path):
        print(f"  ERROR: scan script not found: {script_path}")
        return 1

    # Run the scan as a subprocess using the same Python interpreter, from this
    # directory so each scan finds its local sdk/ and shared modules.
    result = subprocess.run([sys.executable, script_path], cwd=SCRIPT_DIR)
    return result.returncode


def load_json(name):
    path = os.path.join(SCRIPT_DIR, name)
    if not os.path.isfile(path):
        return None
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def main():
    print("VAPM Centralized Assessment — Endpoint Scan (orchestrator)")
    print("Running all centralized scans. Make sure 'python copysdk.py' has been run first.")

    results = []
    for label, script_name, _ in SCANS:
        rc = run_scan(label, script_name)
        results.append((label, script_name, rc))

    # Combine the individual scan outputs into a single endpoint scan file. This is the
    # one minimal file the endpoint would hand to the server for mapping.
    combined = {
        "osdetails":   load_json("scan-ca-osdetails-result.json"),
        "third_party": load_json("scan-ca-third-party-result.json"),
    }
    with open(COMBINED_RESULT, "w", encoding="utf-8") as f:
        json.dump(combined, f, indent=2, default=str)

    print(f"\n{'=' * 70}")
    print("  Endpoint Scan Summary")
    print(f"{'=' * 70}")
    for label, script_name, rc in results:
        status = "ok" if rc == 0 else f"FAILED (exit {rc})"
        print(f"  {label:<34} {script_name:<26} {status}")
    print(f"\n  Endpoint scan written to: {COMBINED_RESULT}")

    # Non-zero overall exit if any scan failed
    if any(rc != 0 for _, _, rc in results):
        sys.exit(1)


if __name__ == "__main__":
    main()
