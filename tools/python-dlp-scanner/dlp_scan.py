#!/usr/bin/env python3
###############################################################################
#  Sample Code for OESIS Endpoint-DLP  -  "hello world"
#  The simplest possible example: scan ONE file for sensitive data (SSNs, credit
#  cards, IBANs, ... ~2,000 detectors) using the OESIS DLP engine. Runs on
#  Windows, macOS and Linux.
#
#  The engine runtime (libraries, rulepack, OCR data) is loaded from the sdk/
#  folder. Run "python prepare.py" once to stage it from the OESIS DLP package.
#
#  Created by Chris Seiler
#  OPSWAT OEM Solutions Architect
###############################################################################
"""Scan a single file for sensitive data with the OESIS Endpoint-DLP engine.

The DLP engine (libwadlpscan.dll) exposes a small JSON-in / JSON-out C ABI that
mirrors the main OESIS API. Every string is wide (wchar_t); setup/invoke write a
result pointer the caller must release with wa_dlpscan_free:

    int  wa_dlpscan_setup(const wchar_t* json_config, wchar_t** json_out)
    int  wa_dlpscan_invoke(const wchar_t* json_in,     wchar_t** json_out)
    int  wa_dlpscan_free(wchar_t* json_out)
    int  wa_dlpscan_teardown(void)

The return code is >= 0 on success, < 0 on failure (OESIS error codes).

This sample makes the three calls a scan needs, in order:

    setup                      initialize the engine and apply the license
    invoke method 130002       load the detector rulepack, set the confidence floor
    invoke method 130001       scan the file and return the detectors that fired
    teardown                   shut the engine down

Usage:
    python dlp_scan.py <file>

The engine is loaded from the sdk/ subfolder. Set it up once with:
    python prepare.py

Runs on Windows, macOS and Linux (x64), loading the matching engine library
(.dll / .dylib / .so). For confidence floors, batch/folder scanning, and other
options, see the more advanced configuration sample.
"""

import ctypes
import json
import os
import sys

# The engine runtime (libraries, rulepack, OCR data) lives in the sdk/ subfolder,
# staged there by prepare.py.
HERE = os.path.dirname(os.path.abspath(__file__))
SDK_DIR = os.path.join(HERE, "sdk")

# Shared-library naming for this platform. The OESIS libraries keep their "libwa"
# prefix on every OS; only the extension changes.
_LIB_EXT = {"win32": ".dll", "darwin": ".dylib"}.get(sys.platform, ".so")
ENGINE_LIB = "libwadlpscan" + _LIB_EXT

# DLP invoke method IDs.
METHOD_CONFIGURE = 130002   # load rulepack + set confidence floor
METHOD_SCAN = 130001        # scan a file

# Detections below this confidence (0-100) are dropped. 80 keeps high-value
# detectors (SSN, credit card, IBAN, ...) while ignoring loose, low-value ones.
CONFIDENCE_FLOOR = 80


class DlpEngine:
    """Minimal ctypes wrapper around libwadlpscan's wa_dlpscan_* C ABI.

    Call load() -> setup() -> configure() -> scan_file() -> teardown().
    All runtime files (engine DLLs, rulepack, OCR data, license) are read from
    the script's own folder.
    """

    def __init__(self):
        self._lib = None

    def load(self):
        """Load the DLP engine library and declare its C signatures.

        Windows loads libwadlpscan.dll (WinDLL / __stdcall); macOS
        libwadlpscan.dylib and Linux libwadlpscan.so (CDLL). The dependency
        libraries (libwautils, libwaheap, pdfium) sit beside it in sdk/.
        """
        lib = os.path.join(SDK_DIR, ENGINE_LIB)
        if not os.path.isfile(lib):
            raise FileNotFoundError(
                "%s not found in the sdk folder.\n"
                "Run 'python prepare.py' first to stage the DLP engine into sdk/."
                % ENGINE_LIB)

        if sys.platform == "win32":
            # Let Windows resolve the dependent DLLs sitting beside the engine.
            os.environ["PATH"] = SDK_DIR + os.pathsep + os.environ.get("PATH", "")
            if hasattr(os, "add_dll_directory"):
                os.add_dll_directory(SDK_DIR)
            self._lib = ctypes.WinDLL(lib)
        else:
            # On macOS/Linux the dynamic linker won't discover sibling libraries
            # from an env var set after the process starts, so preload the
            # dependency chain (leaf first) with global visibility, then load the
            # engine itself.
            for dep in ("libwaheap", "libwautils", "pdfium", "libpdfium"):
                dep_path = os.path.join(SDK_DIR, dep + _LIB_EXT)
                if os.path.isfile(dep_path):
                    try:
                        ctypes.CDLL(dep_path, mode=ctypes.RTLD_GLOBAL)
                    except OSError:
                        pass
            self._lib = ctypes.CDLL(lib)

        for name in ("wa_dlpscan_setup", "wa_dlpscan_invoke"):
            fn = getattr(self._lib, name)
            fn.argtypes = [ctypes.c_wchar_p, ctypes.POINTER(ctypes.c_wchar_p)]
            fn.restype = ctypes.c_int
        self._lib.wa_dlpscan_free.argtypes = [ctypes.c_wchar_p]
        self._lib.wa_dlpscan_free.restype = ctypes.c_int
        self._lib.wa_dlpscan_teardown.argtypes = []
        self._lib.wa_dlpscan_teardown.restype = ctypes.c_int

    def _call(self, func_name, payload):
        """Send a JSON payload to setup/invoke; return (rc, parsed_json)."""
        fn = getattr(self._lib, func_name)
        out = ctypes.c_wchar_p()
        rc = fn(json.dumps(payload), ctypes.byref(out))

        result = None
        if out.value is not None:
            result = out.value                     # copy out, then free the
            self._lib.wa_dlpscan_free(out)         # engine-allocated buffer
        if rc < 0:
            raise RuntimeError("%s failed (rc=%d): %s" % (func_name, rc, result or ""))
        return json.loads(result) if result else {}

    def _read(self, filename):
        """Return the stripped contents of a file in the sdk folder, or None."""
        path = os.path.join(SDK_DIR, filename)
        if not os.path.isfile(path):
            return None
        with open(path, "r", encoding="utf-8") as f:
            return f.read().strip().replace("\r", "")

    def setup(self):
        """Initialize the engine (wa_dlpscan_setup).

        This dev build of the engine runs without an external license file, so
        none is required. If a build you use needs one, drop pass_key.txt (or the
        offline license_bytes.txt + license_key.txt pair) into sdk/ and it is
        picked up here automatically.
        """
        config = {
            "passkey_string": self._read("pass_key.txt") or "",
            "database_location": SDK_DIR,
            "component_location": SDK_DIR,
            "license_location": SDK_DIR,
            "cache_location": SDK_DIR,
            "online_mode": False,
            "caching": False,
            "license_update": True,
            "limits": {"ocr_languages": ["eng"]},   # needs tessdata/ for images
        }
        # Use the offline license pair only if someone placed it in sdk/.
        license_bytes = self._read("license_bytes.txt")
        license_key = self._read("license_key.txt")
        if license_bytes and license_key:
            config["license_bytes"] = license_bytes
            config["license_key_bytes"] = license_key
        self._call("wa_dlpscan_setup", {"config": config})

    def configure(self):
        """Load the detector rulepack and set the confidence floor (method 130002).

        The rulepack's expected SHA-256 (from dlp_rules.manifest.json) is passed
        so the engine can verify integrity; a mismatch is rejected with -48.
        """
        inp = {"method": METHOD_CONFIGURE,
               "confidence_floor": CONFIDENCE_FLOOR,
               "rulepack_path": os.path.join(SDK_DIR, "dlp_rules.dat")}
        manifest = self._read("dlp_rules.manifest.json")
        if manifest:
            try:
                sha = json.loads(manifest).get("sha256")
                if sha:
                    inp["rulepack_sha256"] = sha
            except ValueError:
                pass
        self._call("wa_dlpscan_invoke", {"input": inp})

    def scan_file(self, path):
        """Scan one file (method 130001) and return the parsed response."""
        item = {"type": "file_path",
                "label": os.path.basename(path),
                "path": os.path.abspath(path)}
        return self._call("wa_dlpscan_invoke",
                          {"input": {"method": METHOD_SCAN, "items": [item]}})

    def teardown(self):
        if self._lib is not None:
            self._lib.wa_dlpscan_teardown()


def print_findings(resp):
    """Print the detectors that fired. Returns 0 if clean, 2 if anything fired."""
    # The engine returns result.items[]; each item lists what fired in
    # .violations[] (detector name, confidence, match_count).
    items = resp.get("result", {}).get("items", [])
    violations = items[0].get("violations", []) if items else []

    if not violations:
        print("RESULT: CLEAN  (no sensitive data at or above confidence %d)"
              % CONFIDENCE_FLOOR)
        return 0

    print("RESULT: SENSITIVE DATA FOUND\n")
    for v in violations:
        count = v.get("match_count", 1)
        cnt = "" if count == 1 else " x%d" % count
        print("  - %s (confidence %s)%s" % (v.get("detector"), v.get("confidence"), cnt))
    return 2


def main(argv=None):
    argv = sys.argv[1:] if argv is None else argv
    if len(argv) != 1 or argv[0] in ("-h", "--help"):
        print("usage: python dlp_scan.py <file>")
        return 0 if argv[:1] in (["-h"], ["--help"]) else 1

    path = argv[0]
    if not os.path.isfile(path):
        print("File not found: %s" % path, file=sys.stderr)
        return 1

    engine = DlpEngine()
    try:
        engine.load()
    except (FileNotFoundError, OSError) as ex:
        print("Failed to load the DLP engine: %s" % ex, file=sys.stderr)
        return 1

    try:
        engine.setup()
        engine.configure()
        print("Scanning: %s\n" % os.path.abspath(path))
        resp = engine.scan_file(path)
        return print_findings(resp)
    except RuntimeError as ex:
        print(str(ex), file=sys.stderr)
        return 1
    finally:
        engine.teardown()


if __name__ == "__main__":
    sys.exit(main())
