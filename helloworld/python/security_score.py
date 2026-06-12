#!/usr/bin/env python3
###############################################################################################
##  Sample Code for Security Score
##  Reference Implementation using OESIS Framework
##
##  Calculates and prints the OPSWAT Security Score for this device using
##  GetSecurityScore (method 111). Prints the overall score and a per-category
##  breakdown to the console and writes the full result to a JSON output file.
##
##  Usage:
##      python security_score.py            # force a fresh refresh (default)
##      python security_score.py cached     # use cached values, no refresh
##
##  GetSecurityScore returns:
##      total_score   -- overall device security score (0-100)
##      score_status  -- 'good', 'warning', or 'poor'
##      categories[]  -- per-category score, max_score, status, and details
##
##  https://software.opswat.com/OESIS_V4/html/c_method.html -> method 111
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import json
import os
import sys

from sdk_wrapper import OESISWrapper, SDKError
from platform_utils import validate_sdk_environment
from platform_utils import get_lib_filename

# Hardcoded SDK directory relative to this script
SDK_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "sdk")


def initialize_framework():
    # Load the SDK and initialize with the pass_key.txt in the sdk directory
    # https://software.opswat.com/OESIS_V4/html/c_sdk.html
    pass_key_path = os.path.join(SDK_DIR, "pass_key.txt")

    if not os.path.isfile(pass_key_path):
        print("Could not find pass_key.txt. Make sure the license is in the sdk directory.")
        raise Exception("License pass_key.txt file not found")

    sdk = OESISWrapper(os.path.join(SDK_DIR, get_lib_filename()))
    sdk.load()
    sdk.setup(os.path.join(SDK_DIR, "license.cfg"), pass_key_path)
    return sdk


def consume_offline_vmod_database(sdk, database_file):
    # Loads the offline CVE vulnerability database (v2mod.dat) into the SDK so the
    # Vulnerabilities category of the security score can be evaluated offline.
    # Without this, the Vulnerabilities category reports 0 / poor.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50520
    rc, result = sdk.invoke(50520, dat_input_source_file=database_file)
    if rc < 0:
        raise Exception(f"ConsumeOfflineVmodDatabase failed (rc={rc}): {result}")


def get_security_score(sdk, force_refresh=True):
    # Calculate and return the OPSWAT Security Score for the device.
    # force_refresh=True makes fresh API calls; False uses cached values.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 111
    rc, result = sdk.invoke(111, force_refresh=force_refresh)
    if rc < 0:
        raise Exception(f"GetSecurityScore failed (rc={rc}): {result}")
    return result.get("result", {})


def print_score_report(score):
    # Print the overall score and a per-category breakdown to the console.
    total      = score.get("total_score", "Unknown")
    status     = score.get("score_status", "Unknown")
    cache_ok   = score.get("cache_valid")
    timestamp  = score.get("timestamp", "Unknown")

    print(f"\n  OPSWAT Security Score")
    print("-" * 60)
    print(f"  Total Score   : {total} / 100")
    print(f"  Score Status  : {status}")
    print(f"  Timestamp     : {timestamp}")
    print(f"  Cache Valid   : {cache_ok}")

    categories = score.get("categories", [])
    if not categories:
        print("\n  No category breakdown returned.")
        return

    print(f"\n  Category Breakdown")
    print("-" * 60)
    for c in categories:
        # Friendly label from the raw category name (e.g. anti_malware -> Anti Malware)
        name      = c.get("name", "unknown").replace("_", " ").title()
        score_val = c.get("score", 0)
        max_val   = c.get("max_score", 0)
        cat_status = c.get("status", "unknown")
        print(f"  {name:<24}  {str(score_val) + '/' + str(max_val):<10}  {cat_status}")


def main(force_refresh=True):
    # Accept an optional "cached" argument to skip the refresh and use cached values
    if len(sys.argv) > 1:
        if sys.argv[1].lower() == "cached":
            force_refresh = False
        else:
            print(f"Invalid argument '{sys.argv[1]}'.")
            print("Usage: python security_score.py [cached]   (default: force refresh)")
            return

    if not validate_sdk_environment(SDK_DIR):
        return

    sdk = None
    try:
        sdk = initialize_framework()

        # Load the offline vulnerability database (v2mod.dat) so the
        # Vulnerabilities category is scored against real CVE data.
        consume_offline_vmod_database(sdk, os.path.join(SDK_DIR, "v2mod.dat"))

        mode = "fresh refresh" if force_refresh else "cached values"
        print(f"\nCalculating OPSWAT Security Score ({mode})...")

        score = get_security_score(sdk, force_refresh)
        print_score_report(score)

        # Write full results to a JSON output file
        output_file = os.path.join(os.getcwd(), "security_score.json")
        with open(output_file, "w", encoding="utf-8") as f:
            json.dump(score, f, indent=2, default=str)
        print(f"\n  Full results written to: {output_file}")

    except Exception as e:
        print(f"Received an Exception: {e}")
    finally:
        if sdk:
            try:
                sdk.teardown()
            except SDKError:
                pass


if __name__ == "__main__":
    main()
