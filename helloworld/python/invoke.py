import argparse
import json
import os
import platform
import sys
import traceback
from datetime import datetime

from sdk_wrapper import OESISWrapper
from platform_utils import validate_sdk_environment
from platform_utils import get_lib_filename


SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SDK_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "sdk")
LICENSE_DIR = SDK_DIR


def debug_print(enabled, message):
    if enabled:
        print(f"[DEBUG] {message}")


def print_path_status(debug):
    debug_print(debug, f"Timestamp: {datetime.now().isoformat(timespec='seconds')}")
    debug_print(debug, f"Script path: {__file__}")
    debug_print(debug, f"Script directory: {SCRIPT_DIR}")
    debug_print(debug, f"Current working directory: {os.getcwd()}")
    debug_print(debug, f"Python executable: {sys.executable}")
    debug_print(debug, f"Python version: {sys.version}")
    debug_print(debug, f"Platform: {platform.platform()}")
    debug_print(debug, f"Machine: {platform.machine()}")
    debug_print(debug, f"Pointer size: {8 * (sys.maxsize.bit_length() + 1) // 8} bits")
    debug_print(debug, f"SDK_DIR: {SDK_DIR} exists={os.path.isdir(SDK_DIR)}")
    debug_print(debug, f"LICENSE_DIR: {LICENSE_DIR} exists={os.path.isdir(LICENSE_DIR)}")


def validate_file(path, label, debug):
    exists = os.path.exists(path)
    is_file = os.path.isfile(path)
    size = os.path.getsize(path) if is_file else None
    debug_print(debug, f"{label}: {path} exists={exists} is_file={is_file} size={size}")
    if not exists:
        raise FileNotFoundError(f"{label} not found: {path}")
    if not is_file:
        raise FileNotFoundError(f"{label} is not a file: {path}")

def initialize_framework(debug):
    # Load the SDK and initialize with the pass_key.txt in the sdk directory
    # https://software.opswat.com/OESIS_V4/html/c_sdk.html
    print_path_status(debug)
    pass_key_path = os.path.join(SDK_DIR, "pass_key.txt")

    sdk = OESISWrapper(os.path.join(SDK_DIR, get_lib_filename()))
    sdk.load()
    sdk.setup(os.path.join(SDK_DIR, "license.cfg"), pass_key_path)
    return sdk




def parse_input(args, debug=False):
    if args.json:
        debug_print(debug, "Parsing JSON from --json argument.")
        return json.loads(args.json)

    if args.file:
        input_path = os.path.abspath(args.file)
        debug_print(debug, f"Parsing JSON from file: {input_path}")
        validate_file(input_path, "Input JSON file", debug)
        with open(input_path, "r", encoding="utf-8") as f:
            return json.load(f)

    raise ValueError("You must provide either --json or --file")


def main():
    parser = argparse.ArgumentParser(
        description="Invoke an OESIS SDK method using JSON input",
        epilog="""
Examples:

# From file
python invoke.py --file request.json
Note: Default request.json will do a DetectProducts call

# Simple DetectProducts call
python invoke.py --json "--json "{ "input": {"method": 0}}"

Note: On Windows you will need to escape the " with backslashes on the command line
There is a sample detect.json file included in the code
""",
        formatter_class=argparse.RawTextHelpFormatter,
    )

    parser.add_argument("--json", help="JSON string input")
    parser.add_argument("--file", help="Path to JSON file")
    parser.add_argument("--debug", action="store_true", help="Print detailed diagnostics before and during invocation")
    parser.add_argument("--trace", action="store_true", help="Print full Python traceback when an exception occurs")
    
    args = parser.parse_args()

    # Show help if no parameters
    if not args.json and not args.file:
        parser.print_help()
        sys.exit(0)

    try:
        request = parse_input(args)
        request = parse_input(args, args.debug)

        print("Input JSON:")
        print(json.dumps(request, indent=2))
        print("\nInvoking SDK...\n")

        sdk_wrapper = initialize_framework(args.debug)

        debug_print(args.debug, "Calling sdk_wrapper.invoke(request)")
        debug_print(args.debug, f"Request object type: {type(request).__name__}")
        debug_print(args.debug, f"Request method: {request.get('method') if isinstance(request, dict) else '<not a dict>'}")

        response = sdk_wrapper.invokeJSON(request)

        debug_print(args.debug, f"Response object type: {type(response).__name__}")
        print("Result:")
        print(json.dumps(response, indent=2))

    except Exception as e:
        print(f"Received an Exception: {e}")
        if args.debug or args.trace:
            print("\nTraceback:")
            traceback.print_exc()
        sys.exit(1)


if __name__ == "__main__":
    main()
