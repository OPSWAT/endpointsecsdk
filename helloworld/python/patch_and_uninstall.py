#!/usr/bin/env python3
###############################################################################################
##  Patch and Uninstall Runner
##  Reference Implementation using OESIS Framework
##
##  Reads moby.json and iterates over all Windows signatures that have BOTH:
##      support_app_remover == true
##      fresh_installable   == true
##
##  For each matching signature:
##      1. Runs patch.py  (install latest version)
##      2. Waits 15 seconds
##      3. Runs uninstall.py (remove the product)
##
##  Produces a final report of successes and failures for both operations.
##
##  Usage:
##      python patch_and_uninstall.py [moby.json]
##
##  The moby.json path defaults to ./analog/moby.json if not supplied.
##
##  !! NOTE: patch.py and uninstall.py must be in the same directory as this script !!
##  !! NOTE: This operation requires Administrator / root access                    !!
###############################################################################################

import json
import os
import subprocess
import sys
import time
from dataclasses import dataclass, field
from typing import Optional


# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------

SCRIPT_DIR        = os.path.dirname(os.path.abspath(__file__))
DEFAULT_MOBY_PATH = os.path.join(SCRIPT_DIR, "analog", "moby.json")
PATCH_SCRIPT      = os.path.join(SCRIPT_DIR, "patch.py")
UNINSTALL_SCRIPT  = os.path.join(SCRIPT_DIR, "uninstall.py")
WAIT_SECONDS      = 1


# ---------------------------------------------------------------------------
# Data structures
# ---------------------------------------------------------------------------

@dataclass
class SignatureInfo:
    signature_id: int
    name:         str
    vendor:       str


@dataclass
class OperationResult:
    success:     bool
    return_code: int
    stdout:      str
    stderr:      str
    error:       Optional[str] = None


@dataclass
class SignatureResult:
    sig:       SignatureInfo
    install:   Optional[OperationResult] = None
    uninstall: Optional[OperationResult] = None

    @property
    def install_ok(self) -> bool:
        return self.install is not None and self.install.success

    @property
    def uninstall_ok(self) -> bool:
        return self.uninstall is not None and self.uninstall.success

    @property
    def fully_successful(self) -> bool:
        return self.install_ok and self.uninstall_ok


# ---------------------------------------------------------------------------
# moby.json parsing
# ---------------------------------------------------------------------------

def load_matching_signatures(moby_path: str) -> list[SignatureInfo]:
    """
    Return all Windows signatures from moby.json where both
    support_app_remover and fresh_installable are True.
    """
    if not os.path.isfile(moby_path):
        raise FileNotFoundError(f"moby.json not found: {moby_path}")

    with open(moby_path, "r", encoding="utf-8") as fh:
        data = json.load(fh)

    products = data.get("products", {})
    matches: list[SignatureInfo] = []

    for _prod_key, platforms in products.items():
        windows_data = platforms.get("WINDOWS") or platforms.get("windows")
        if not windows_data:
            continue

        for sig in windows_data.get("signatures", []):
            if sig.get("support_app_remover") and sig.get("fresh_installable"):
                matches.append(SignatureInfo(
                    signature_id=sig["signature_id"],
                    name=sig.get("signature_name", "Unknown"),
                    vendor=sig.get("vendor_name", "Unknown"),
                ))

    # Stable ordering by signature_id
    matches.sort(key=lambda s: s.signature_id)
    return matches


# ---------------------------------------------------------------------------
# Sub-process helpers
# ---------------------------------------------------------------------------

def run_script(script_path: str, *args: str,
               input_text: Optional[str] = None) -> OperationResult:
    """
    Run a Python script in a subprocess and capture its output.
    Returns an OperationResult regardless of whether the script succeeds.
    """
    cmd = [sys.executable, script_path, *args]
    try:
        proc = subprocess.run(
            cmd,
            capture_output=True,
            text=True,
            input=input_text,
        )
        success = proc.returncode == 0
        return OperationResult(
            success=success,
            return_code=proc.returncode,
            stdout=proc.stdout.strip(),
            stderr=proc.stderr.strip(),
        )
    except Exception as exc:
        return OperationResult(
            success=False,
            return_code=-1,
            stdout="",
            stderr="",
            error=str(exc),
        )


# ---------------------------------------------------------------------------
# Core workflow
# ---------------------------------------------------------------------------

def process_signature(sig: SignatureInfo, index: int, total: int) -> SignatureResult:
    result = SignatureResult(sig=sig)
    label  = f"[{index}/{total}] {sig.name} (ID {sig.signature_id}, {sig.vendor})"

    print(f"\n{'='*70}")
    print(f"  {label}")
    print(f"{'='*70}")

    # --- Step 1: Patch (install) ---
    print(f"\n  >> INSTALL: running patch.py {sig.signature_id} ...")
    result.install = run_script(PATCH_SCRIPT, str(sig.signature_id))

    if result.install.error:
        print(f"  !! Install raised an exception: {result.install.error}")
    elif result.install.success:
        print(f"  ++ Install succeeded (rc={result.install.return_code})")
    else:
        print(f"  -- Install FAILED (rc={result.install.return_code})")

    if result.install.stdout:
        _indent_print(result.install.stdout, "     ")
    if result.install.stderr:
        _indent_print(result.install.stderr, "  ERR ")

    # --- Step 2: Wait ---
    print(f"\n  .. Waiting {WAIT_SECONDS} seconds before uninstall ...")
    time.sleep(WAIT_SECONDS)

    # --- Step 3: Uninstall ---
    print(f"\n  >> UNINSTALL: running uninstall.py {sig.signature_id} ...")
    # uninstall.py prompts "Proceed? [y/N]" — feed "y\n" automatically
    result.uninstall = run_script(UNINSTALL_SCRIPT, str(sig.signature_id),
                                  input_text="y\n")

    if result.uninstall.error:
        print(f"  !! Uninstall raised an exception: {result.uninstall.error}")
    elif result.uninstall.success:
        print(f"  ++ Uninstall succeeded (rc={result.uninstall.return_code})")
    else:
        print(f"  -- Uninstall FAILED (rc={result.uninstall.return_code})")

    if result.uninstall.stdout:
        _indent_print(result.uninstall.stdout, "     ")
    if result.uninstall.stderr:
        _indent_print(result.uninstall.stderr, "  ERR ")

    return result


def _indent_print(text: str, prefix: str) -> None:
    for line in text.splitlines():
        print(f"{prefix}{line}")


# ---------------------------------------------------------------------------
# Reporting
# ---------------------------------------------------------------------------

def print_report(results: list[SignatureResult]) -> None:
    successes         = [r for r in results if r.fully_successful]
    install_failures  = [r for r in results if not r.install_ok]
    uninstall_failures= [r for r in results if r.install_ok and not r.uninstall_ok]

    width = 70
    print(f"\n\n{'#'*width}")
    print(f"  FINAL REPORT")
    print(f"{'#'*width}")
    print(f"  Total processed : {len(results)}")
    print(f"  Full successes  : {len(successes)}")
    print(f"  Install failures: {len(install_failures)}")
    print(f"  Uninstall fails : {len(uninstall_failures)}")

    # --- Successes ---
    if successes:
        print(f"\n{'─'*width}")
        print(f"  ✔  SUCCESSES ({len(successes)})")
        print(f"{'─'*width}")
        for r in successes:
            print(f"  [{r.sig.signature_id:>5}]  {r.sig.name}  ({r.sig.vendor})")

    # --- Install failures ---
    if install_failures:
        print(f"\n{'─'*width}")
        print(f"  ✘  INSTALL FAILURES ({len(install_failures)})")
        print(f"{'─'*width}")
        for r in install_failures:
            print(f"\n  [{r.sig.signature_id:>5}]  {r.sig.name}  ({r.sig.vendor})")
            if r.install:
                rc = r.install.return_code
                print(f"          Return code : {rc}")
                if r.install.error:
                    print(f"          Exception   : {r.install.error}")
                elif r.install.stdout:
                    # Show last meaningful line of stdout as the failure reason
                    last = _last_nonempty_line(r.install.stdout)
                    print(f"          Detail      : {last}")
                if r.install.stderr:
                    last = _last_nonempty_line(r.install.stderr)
                    print(f"          Stderr      : {last}")

    # --- Uninstall failures ---
    if uninstall_failures:
        print(f"\n{'─'*width}")
        print(f"  ✘  UNINSTALL FAILURES ({len(uninstall_failures)})")
        print(f"  (install succeeded but uninstall did not)")
        print(f"{'─'*width}")
        for r in uninstall_failures:
            print(f"\n  [{r.sig.signature_id:>5}]  {r.sig.name}  ({r.sig.vendor})")
            if r.uninstall:
                rc = r.uninstall.return_code
                print(f"          Return code : {rc}")
                if r.uninstall.error:
                    print(f"          Exception   : {r.uninstall.error}")
                elif r.uninstall.stdout:
                    last = _last_nonempty_line(r.uninstall.stdout)
                    print(f"          Detail      : {last}")
                if r.uninstall.stderr:
                    last = _last_nonempty_line(r.uninstall.stderr)
                    print(f"          Stderr      : {last}")

    print(f"\n{'#'*width}\n")


def _last_nonempty_line(text: str) -> str:
    for line in reversed(text.splitlines()):
        if line.strip():
            return line.strip()
    return ""


def save_report_json(results: list[SignatureResult], path: str) -> None:
    """Persist a machine-readable copy of the report."""
    payload = []
    for r in results:
        entry = {
            "signature_id": r.sig.signature_id,
            "name":         r.sig.name,
            "vendor":       r.sig.vendor,
            "install": {
                "success":     r.install.success     if r.install else None,
                "return_code": r.install.return_code if r.install else None,
                "error":       r.install.error       if r.install else None,
                "stdout":      r.install.stdout      if r.install else None,
                "stderr":      r.install.stderr      if r.install else None,
            } if r.install else None,
            "uninstall": {
                "success":     r.uninstall.success     if r.uninstall else None,
                "return_code": r.uninstall.return_code if r.uninstall else None,
                "error":       r.uninstall.error       if r.uninstall else None,
                "stdout":      r.uninstall.stdout      if r.uninstall else None,
                "stderr":      r.uninstall.stderr      if r.uninstall else None,
            } if r.uninstall else None,
        }
        payload.append(entry)

    with open(path, "w", encoding="utf-8") as fh:
        json.dump(payload, fh, indent=2)

    print(f"  JSON report saved to: {path}")


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

def main():
    moby_path = sys.argv[1] if len(sys.argv) > 1 else DEFAULT_MOBY_PATH

    print(f"Patch-and-Uninstall Runner")
    print(f"  moby.json : {moby_path}")
    print(f"  patch.py  : {PATCH_SCRIPT}")
    print(f"  uninstall : {UNINSTALL_SCRIPT}")
    print(f"  wait secs : {WAIT_SECONDS}")

    # Validate helper scripts exist
    for script in (PATCH_SCRIPT, UNINSTALL_SCRIPT):
        if not os.path.isfile(script):
            print(f"\nERROR: Required script not found: {script}")
            sys.exit(1)

    # Load signatures
    try:
        signatures = load_matching_signatures(moby_path)
    except FileNotFoundError as exc:
        print(f"\nERROR: {exc}")
        sys.exit(1)

    if not signatures:
        print("\nNo signatures matched the criteria (support_app_remover + fresh_installable).")
        sys.exit(0)

    print(f"\nFound {len(signatures)} matching signatures (support_app_remover + fresh_installable).\n")

    # Process each signature
    all_results: list[SignatureResult] = []
    for idx, sig in enumerate(signatures, start=1):
        result = process_signature(sig, idx, len(signatures))
        all_results.append(result)

    # Print human-readable report
    print_report(all_results)

    # Save machine-readable JSON report next to this script
    report_path = os.path.join(SCRIPT_DIR, "patch_uninstall_report.json")
    save_report_json(all_results, report_path)


if __name__ == "__main__":
    main()
