#!/usr/bin/env python3
"""
OPSWAT Endpoint Vulnerability Scanner

Runs on an endpoint to produce a snapshot of what the OESIS SDK detects:
installed products, their vulnerabilities, and missing patches.

Usage:
    python3 endpoint_vuln_scanner.py \
        --sdk-path ./sdk \
        --dat-path ./dat \
        --license-path ./license \
        --output-dir ./output \
"""

import argparse
import logging
import os
from pathlib import Path
import sys
import traceback
from datetime import datetime, timezone

from platform_utils import resolve_sdk_lib_path, get_hostname, get_os_type
from sdk_wrapper import OESISWrapper, SDKError
from scanner import product_scan, system_scan
from output_formatter import write_output

logger = logging.getLogger("endpoint_vuln_scanner")


def setup_logging(output_dir):
    """Configure logging to both console and a log file in the output directory."""
    os.makedirs(output_dir, exist_ok=True)
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    log_file = os.path.join(output_dir, f"scanner_{timestamp}.log")

    root_logger = logging.getLogger("endpoint_vuln_scanner")
    root_logger.setLevel(logging.DEBUG)

    # File handler — captures everything including DEBUG
    fh = logging.FileHandler(log_file, encoding="utf-8")
    fh.setLevel(logging.DEBUG)
    fh.setFormatter(logging.Formatter(
        "%(asctime)s [%(levelname)s] %(message)s", datefmt="%Y-%m-%d %H:%M:%S"
    ))
    root_logger.addHandler(fh)

    # Console handler — INFO and above
    ch = logging.StreamHandler()
    ch.setLevel(logging.INFO)
    ch.setFormatter(logging.Formatter("%(message)s"))
    root_logger.addHandler(ch)

    return log_file


def find_default_path(name):
    """Look for a subdirectory relative to the script location.

    Falls back to the script's own directory if the subdirectory doesn't exist,
    so everything can be co-located in a single folder.
    """
    script_dir = os.path.dirname(os.path.abspath(__file__))
    candidate = os.path.join(script_dir, name)
    if os.path.isdir(candidate):
        return candidate
    # Fall back to the script directory itself (flat layout)
    return script_dir


def parse_args():
    parser = argparse.ArgumentParser(
        description="OPSWAT Endpoint Vulnerability Scanner — "
                    "Scans the endpoint for installed products, vulnerabilities, and missing patches."
    )
    parser.add_argument(
        "--sdk-path",
        default=find_default_path("sdk"),
        help="Path to directory containing SDK libraries (libwaapi.*). "
             "Defaults to ./sdk relative to script."
    )
    parser.add_argument(
        "--dat-path",
        default=find_default_path("dat"),
        help="Path to directory containing DAT files (v2mod.dat, patch.dat, etc.). "
             "Defaults to ./dat relative to script."
    )
    parser.add_argument(
        "--license-path",
        default=find_default_path("license"),
        help="Path to directory containing license.cfg and pass_key.txt. "
             "Defaults to ./license relative to script."
    )
    parser.add_argument(
        "--output-dir",
        default=os.path.join(os.path.dirname(os.path.abspath(__file__)), "output"),
        help="Directory for output files. Defaults to ./output."
    )
    parser.add_argument(
        "--scan",
        nargs="+",
        choices=["product", "system", "all"],
        default=["all"],
        help="Which scan(s) to run. Defaults to all."
    )
    parser.add_argument(
        "--debug-log",
        default=None,
        help="Path for SDK debug log output. Optional."
    )
    return parser.parse_args()

def find_license_files_in_parent(license_path):
    """Walk up from license_path looking for an eval-license directory.
    
    Searches up to 2 levels above license_path for an eval-license folder
    containing both required license files. Returns the path if found, else None.
    """
    required_files = ["pass_key.txt", "license.cfg"]
    search_path = os.path.abspath(license_path)

    for _ in range(2):
        search_path = os.path.dirname(search_path)
        candidate = os.path.join(search_path, "eval-license")
        if os.path.isdir(candidate):
            if all(os.path.isfile(os.path.join(candidate, f)) for f in required_files):
                return candidate

    return None


def validate_license_files(license_path, errors):
    """Check that required license files exist in the license directory.
    
    If files are missing, searches for an eval-license directory up to 2 levels
    above the license path. If found, copies the files into the license directory.
    Otherwise appends an error for each missing file.
    """
    import shutil

    if not (license_path and os.path.isdir(license_path)):
        return

    required_files = ["pass_key.txt", "license.cfg"]
    missing = [f for f in required_files
               if not os.path.isfile(os.path.join(license_path, f))]

    if not missing:
        return

    # Some files are missing — try to find them in an eval-license directory
    eval_dir = find_license_files_in_parent(license_path)
    if eval_dir:
        logger.info(f"License files not found in {license_path}, "
                    f"copying from {eval_dir}")
        for filename in missing:
            src = os.path.join(eval_dir, filename)
            dst = os.path.join(license_path, filename)
            shutil.copy2(src, dst)
            logger.info(f"  Copied {filename}")
    else:
        for filename in missing:
            errors.append(f"{filename} not found in {license_path} "
                          f"and no eval-license directory found in parent paths")

def validate_paths(args):
    """Validate that required paths exist."""
    errors = []

    if not args.sdk_path or not os.path.isdir(args.sdk_path):
        errors.append(f"SDK path not found: {args.sdk_path}")

    if not args.dat_path or not os.path.isdir(args.dat_path):
        errors.append(f"DAT path not found: {args.dat_path}")

    if not args.license_path or not os.path.isdir(args.license_path):
        errors.append(f"License path not found: {args.license_path}")


    validate_license_files(args.license_path,errors)

    if errors:
        for e in errors:
            logger.error(e)
        sys.exit(1)


def main():
    args = parse_args()

    # Set up logging first so we capture everything
    log_file = setup_logging(args.output_dir)

    try:
        _run(args, log_file)
    except SystemExit:
        raise
    except Exception as e:
        logger.error(f"Fatal error: {e}", exc_info=True)
        sys.exit(1)


def _run(args, log_file):
    validate_paths(args)

    scans_to_run = set(args.scan)
    if "all" in scans_to_run:
        scans_to_run = {"product", "system"}

    hostname = get_hostname()
    logger.info("OPSWAT Endpoint Vulnerability Scanner")
    logger.info(f"Host: {hostname}")
    logger.info(f"Platform: {sys.platform}")
    logger.info(f"Python: {sys.version}")
    logger.info(f"SDK path: {args.sdk_path}")
    logger.info(f"DAT path: {args.dat_path}")
    logger.info(f"License path: {args.license_path}")
    logger.info(f"Output dir: {args.output_dir}")
    logger.info(f"Scans: {', '.join(sorted(scans_to_run))}")
    logger.info(f"Log file: {log_file}")

    # List files in SDK path for debugging
    if args.sdk_path and os.path.isdir(args.sdk_path):
        sdk_files = os.listdir(args.sdk_path)
        logger.debug(f"Files in SDK path: {sdk_files}")

    # List files in DAT path for debugging
    if args.dat_path and os.path.isdir(args.dat_path):
        dat_files = os.listdir(args.dat_path)
        logger.debug(f"Files in DAT path: {dat_files}")

    # Resolve and load the SDK library
    try:
        lib_path = resolve_sdk_lib_path(args.sdk_path)
        logger.info(f"Loading SDK from: {lib_path}")
    except FileNotFoundError as e:
        logger.error(f"{e}")
        sys.exit(1)

    sdk = OESISWrapper(lib_path)
    try:
        sdk.load()
        logger.info("SDK library loaded.")
    except OSError as e:
        logger.error(f"Failed to load SDK library: {e}", exc_info=True)
        sys.exit(1)

    # Initialize the SDK
    license_cfg = os.path.join(args.license_path, "license.cfg")
    pass_key = os.path.join(args.license_path, "pass_key.txt")
    try:
        logger.info("Initializing SDK...")
        setup_result = sdk.setup(license_cfg, pass_key, debug_log_path=args.debug_log)
        logger.info("SDK initialized successfully.")
        logger.debug(f"Setup result keys: {list(setup_result.keys()) if isinstance(setup_result, dict) else 'N/A'}")
    except SDKError as e:
        logger.error(f"SDK initialization failed: {e}")
        sys.exit(1)

    output_files = []
    try:
        logger.info(f"Running scans: {', '.join(sorted(scans_to_run))}")
        # Product scan
        if "product" in scans_to_run:
            product_result = product_scan(sdk, args.dat_path)
            
            if product_result is not None and product_result.values is not None:

                paths = write_output(product_result, args.output_dir,
                                     "product_scan", hostname)
                output_files.extend(paths)
            else:
                logger.info("No product vulnerabilities found")

        # System scan
        if "system" in scans_to_run:
            system_result = system_scan(sdk, args.dat_path)
            paths = write_output(system_result, args.output_dir,
                                 "system_scan", hostname)
            output_files.extend(paths)

    except SDKError as e:
        logger.error(f"SDK error during scan: {e}")
    except Exception as e:
        logger.error(f"Unexpected error during scan: {e}", exc_info=True)
    finally:
        # Always teardown
        try:
            sdk.teardown()
            logger.info("SDK teardown complete.")
        except SDKError:
            pass

    # Summary
    if output_files:
        logger.info("Output files:")
        for f in output_files:
            logger.info(f"  {f}")
    else:
        logger.info("No output files generated.")


if __name__ == "__main__":
    main()
