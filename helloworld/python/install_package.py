#!/usr/bin/env python3
###############################################################################################
##  Sample Code for Install Package
##  Reference Implementation using OESIS Framework
##
##  Installs a product using the package-based (v2) patching flow:
##
##      patch_uuid  --->  GetPackages (50306)  --->  InstallPackage (50305)
##
##  This is the newer alternative to patch.py's flow (GetLatestInstaller -> InstallFromFiles).
##  The differences that matter:
##
##      * Works from patchv2.dat, the same database rollback uses.
##      * The SDK evaluates every package for THIS endpoint and reports
##        evaluation_status (applicable / not_applicable / not_evaluated), so the
##        caller does not have to reason about architecture or language itself.
##      * Install is by package_uuid, with an expected_installer_sha256 the SDK
##        verifies before running the installer, and force_close_processes to
##        terminate anything blocking the install.
##      * Any version in the database can be installed, not only the latest --
##        GetPackages resolves whatever patch_uuid it is given.
##
##  The one thing the SDK cannot do is turn a signature into a patch_uuid. When a
##  signature is given, the patch_uuid is looked up in the Analog server catalog
##  (patch_aggregation_v2.json). When a patch_uuid is given directly, the catalog is
##  not needed at all -- that is the pure-SDK path.
##
##  Usage:
##      python install_package.py <patch_uuid>                       [--yes] [--download-only]
##      python install_package.py --signature <id> [--version <v>]   [--yes] [--download-only]
##
##  Examples:
##      python install_package.py --signature 3241                    # Notepad++ x64, latest
##      python install_package.py --signature 3241 --version 8.9.6.4  # a specific version
##      python install_package.py eeeaba57-c17b-570d-8e64-f66295cfd570
##      python install_package.py --signature 3241 --download-only    # stop before install
##
##  !! NOTE: requires patchv2.dat in the sdk directory                     !!
##  !!       (copy from OPSWAT-SDK/extract/analog/client/patchv2.dat)      !!
##  !! NOTE: The install step requires Administrator / root access         !!
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import hashlib
import json
import os
import ssl
import sys
import urllib.request

from sdk_wrapper import OESISWrapper, SDKError
from platform_utils import validate_sdk_environment
from platform_utils import get_lib_filename


# Hardcoded SDK directory relative to this script
SDK_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "sdk")

# OESIS method IDs
METHOD_GET_VERSION         = 100
METHOD_LOAD_PATCH_DATABASE = 50302
METHOD_INSTALL_PACKAGE     = 50305
METHOD_GET_PACKAGES        = 50306


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


def load_patch_database(sdk):
    # GetPackages and InstallPackage read patchv2.dat. The v1 patch.dat does not
    # carry package UUIDs, so it cannot be used for this flow.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50302
    patchv2 = os.path.join(SDK_DIR, "patchv2.dat")
    if not os.path.isfile(patchv2):
        raise Exception(
            "patchv2.dat not found in the sdk directory. Copy it from "
            "OPSWAT-SDK/extract/analog/client/patchv2.dat")

    rc, result = sdk.invoke(METHOD_LOAD_PATCH_DATABASE, dat_input_source_file=patchv2)
    if rc < 0:
        raise Exception(f"LoadPatchDatabase failed (rc={rc}): {result}")


# ---------------------------------------------------------------------------
# Analog catalog -- turning a signature into a patch_uuid
# ---------------------------------------------------------------------------

def find_catalog():
    """Walk up from this script looking for the Analog server catalog."""
    search_root = os.path.dirname(os.path.abspath(__file__))
    for _ in range(4):
        search_root = os.path.dirname(search_root)
        candidate = os.path.join(search_root, "OPSWAT-SDK", "extract",
                                 "analog", "server", "patch_aggregation_v2.json")
        if os.path.isfile(candidate):
            return candidate
    return None


def resolve_patch_uuid(signature_id, version=None):
    """Return (patch_uuid, product_name, version) for a signature from the catalog.

    With no version, the record flagged is_latest is used. The SDK has no call that
    performs this lookup -- GetPackages needs the patch_uuid handed to it -- so the
    server-side catalog is the only source.
    """
    catalog_path = find_catalog()
    if not catalog_path:
        raise Exception("Could not find OPSWAT-SDK/extract/analog/server/"
                        "patch_aggregation_v2.json -- run the SDK downloader, or "
                        "pass a patch_uuid directly.")

    with open(catalog_path, "r", encoding="utf-8") as fh:
        data = json.load(fh)

    records = []
    for section in data.get("oesis", []):
        found = section.get("patch_aggregation_v2")
        if found is not None:
            records = list(found.values()) if isinstance(found, dict) else found
            break

    for record in records:
        product = record.get("product", {})
        if signature_id not in product.get("v4_signatures", []):
            continue
        if version is None and not record.get("is_latest"):
            continue
        if version is not None and record.get("version") != version:
            continue
        return record["patch_uuid"], product.get("name", "Unknown"), record.get("version")

    wanted = f"version {version}" if version else "the latest version"
    raise Exception(f"No catalog record for signature {signature_id} at {wanted}.")


# ---------------------------------------------------------------------------
# Package flow
# ---------------------------------------------------------------------------

def get_packages(sdk, patch_uuid):
    # Ask the SDK for every package linked to the patch. Nothing is downloaded;
    # the SDK evaluates each package against this endpoint and sets
    # evaluation_status so the caller can pick the right one.
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50306
    rc, result = sdk.invoke(METHOD_GET_PACKAGES, patch_uuid=patch_uuid,
                            package_limit="all")
    if rc < 0:
        raise Exception(f"GetPackages failed (rc={rc}): {result}")
    return result.get("result", {})


def choose_package(packages):
    """Pick the package the SDK evaluated as applicable to this endpoint."""
    applicable = [p for p in packages if p.get("evaluation_status") == "applicable"]
    if len(applicable) == 1:
        return applicable[0]
    if not applicable:
        return None
    # More than one applicable package (e.g. several languages) -- take the first
    # but say so, since a real integration would let the user choose.
    print(f"  Note: {len(applicable)} applicable packages, using the first.")
    return applicable[0]


def download_valid_file(url, destination, expected_sha256):
    # Download a file and verify its SHA-256 checksum before returning success.
    # On Windows the Python installer does not import the OS certificate store,
    # so SSL verification frequently fails with "unable to get local issuer
    # certificate". We disable SSL verification and compensate with the
    # SHA-256 checksum check that follows -- and InstallPackage checks it again.
    print(f"  Downloading: {url}")

    if sys.platform == "win32":
        print("  Note: SSL certificate verification disabled on Windows "
              "(integrity guaranteed by SHA-256 checksum).")
        ctx = ssl._create_unverified_context()
    else:
        ctx = ssl.create_default_context()

    try:
        with urllib.request.urlopen(url, context=ctx) as response, \
             open(destination, "wb") as out_file:
            out_file.write(response.read())
    except Exception as exc:
        print(f"  Download FAILED: {exc}")
        return False

    print("  Download succeeded")

    if not expected_sha256:
        # Some self-updating products have no stable hash in the database.
        print("  Note: catalog has no SHA-256 for this package; skipping local check.")
        return True

    sha256 = hashlib.sha256()
    with open(destination, "rb") as f:
        for chunk in iter(lambda: f.read(8192), b""):
            sha256.update(chunk)

    actual = sha256.hexdigest()
    if actual.lower() != expected_sha256.lower():
        print("=" * 70)
        print("  ERROR: CHECKSUM VERIFICATION FAILED")
        print(f"    Expected : {expected_sha256.lower()}")
        print(f"    Actual   : {actual.lower()}")
        print("    Install aborted -- the endpoint has not been changed.")
        print("=" * 70)
        return False

    print("  Checksum verified successfully")
    return True


def install_package(sdk, package_uuid, location, expected_sha256):
    # Install the downloaded package.
    #   expected_installer_sha256 -- the SDK hashes the file itself and halts if it
    #                                does not match, so a corrupted download can
    #                                never reach the installer.
    #   force_close_processes     -- terminate anything blocking the install
    #                                (the product itself, typically).
    # !! Requires Administrator / root access !!
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50305
    params = {"package_uuid": package_uuid, "path": location,
              "force_close_processes": True}
    if expected_sha256:
        params["expected_installer_sha256"] = expected_sha256

    rc, result = sdk.invoke(METHOD_INSTALL_PACKAGE, **params)
    if rc < 0:
        raise Exception(f"InstallPackage failed (rc={rc}): {result}")
    return result.get("result", {})


def get_installed_version(sdk, signature_id):
    # https://software.opswat.com/OESIS_V4/html/c_method.html -> method 100
    rc, result = sdk.invoke(METHOD_GET_VERSION, signature=signature_id)
    if rc < 0:
        return None
    return result.get("result", {}).get("version") or None


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

USAGE = """Usage:
  python install_package.py <patch_uuid>                      [--yes] [--download-only]
  python install_package.py --signature <id> [--version <v>]  [--yes] [--download-only]

Examples:
  python install_package.py --signature 3241                    Notepad++ x64, latest
  python install_package.py --signature 3241 --version 8.9.6.4  a specific version
  python install_package.py eeeaba57-c17b-570d-8e64-f66295cfd570"""


def parse_args(argv):
    """Return dict with patch_uuid | signature, version, assume_yes, download_only."""
    opts = {"patch_uuid": None, "signature": None, "version": None,
            "assume_yes": False, "download_only": False}
    args = argv[1:]
    i = 0
    while i < len(args):
        a = args[i]
        if a in ("--yes", "-y"):
            opts["assume_yes"] = True
        elif a == "--download-only":
            opts["download_only"] = True
        elif a == "--signature" and i + 1 < len(args):
            i += 1
            try:
                opts["signature"] = int(args[i])
            except ValueError:
                print(f"ERROR: Invalid signature ID '{args[i]}' -- must be an integer.")
                return None
        elif a == "--version" and i + 1 < len(args):
            i += 1
            opts["version"] = args[i]
        elif not a.startswith("--"):
            opts["patch_uuid"] = a
        else:
            print(f"ERROR: Unknown option '{a}'")
            return None
        i += 1

    if not opts["patch_uuid"] and opts["signature"] is None:
        print(USAGE)
        return None
    return opts


def main():
    opts = parse_args(sys.argv)
    if not opts:
        return

    if not validate_sdk_environment(SDK_DIR):
        return

    print("Install Package")

    # --- Resolve the patch_uuid ----------------------------------------------
    product_name = None
    if opts["patch_uuid"]:
        patch_uuid = opts["patch_uuid"]
        print(f"  Patch UUID : {patch_uuid}  (given)")
    else:
        try:
            patch_uuid, product_name, version = resolve_patch_uuid(
                opts["signature"], opts["version"])
        except Exception as exc:
            print(f"\nERROR: {exc}")
            return
        print(f"  Signature  : {opts['signature']}")
        print(f"  Product    : {product_name}  {version}")
        print(f"  Patch UUID : {patch_uuid}  (from catalog)")

    sdk            = None
    installer_path = None
    download_ok    = False
    try:
        sdk = initialize_framework()
        load_patch_database(sdk)

        # --- Step 1: ask the SDK which packages apply to this endpoint -------
        print(f"\n[1/3] GetPackages")
        info     = get_packages(sdk, patch_uuid)
        packages = info.get("packages", [])
        version  = info.get("version")

        print(f"  Patch version : {version}")
        print(f"  Packages      : {len(packages)}")
        for p in packages:
            arch  = ", ".join(p.get("architectures", []))
            langs = ", ".join(p.get("languages", [])) or "-"
            print(f"    {p.get('package_uuid')}  arch={arch:<8} lang={langs:<6} "
                  f"{p.get('evaluation_status')}")

        package = choose_package(packages)
        if not package:
            print("\nNo package was evaluated as applicable to this endpoint. "
                  "Nothing to install.")
            return

        package_uuid = package["package_uuid"]
        expected_sha = package.get("sha256", "")
        links        = package.get("download_links", [])
        if not links:
            print("\nThe applicable package has no download link (self-updating or "
                  "install-only product). Nothing to install.")
            return
        url = links[0]

        if opts["signature"] is not None:
            current = get_installed_version(sdk, opts["signature"])
            print(f"\n  Installed now : {current or '(not detected)'}")
            print(f"  Will install  : {version}")
            if current == version:
                print(f"\n  Already on {version}. Nothing to do.")
                return

        # --- Step 2: download and verify -----------------------------------
        filename       = url.rsplit("/", 1)[-1].split("?", 1)[0]
        installer_path = os.path.join(os.getcwd(), filename).replace("\\", "/")

        print(f"\n[2/3] Download")
        download_ok = download_valid_file(url, installer_path, expected_sha)
        if not download_ok:
            return

        if opts["download_only"]:
            print(f"\n--download-only: stopping before install. "
                  f"Installer kept at {installer_path}")
            download_ok = False      # keep the file
            return

        # --- Step 3: install -----------------------------------------------
        if not opts["assume_yes"]:
            answer = input(f"\n  Install {product_name or ''} {version} now? [y/N]: ")
            if answer.strip().lower() != "y":
                print("  Install cancelled.")
                return

        print(f"\n[3/3] InstallPackage")
        result = install_package(sdk, package_uuid, installer_path, expected_sha)
        print(json.dumps(result, indent=4, default=str))

        print(f"\n{'='*70}")
        installed = result.get("version")
        if installed == version:
            print(f"  INSTALL SUCCEEDED   {product_name or patch_uuid}: {installed}")
        else:
            print(f"  INSTALL COMPLETED but reported version {installed!r}, "
                  f"expected {version!r}")
        exit_code = result.get("installer_exit_code")
        if exit_code is not None:
            print(f"  Installer exit code : {exit_code}")
        killed = result.get("terminated_processes") or []
        if killed:
            print(f"  Terminated          : {', '.join(str(k) for k in killed)}")
        print(f"{'='*70}")

    except Exception as e:
        print(f"\nReceived an Exception: {e}")
    finally:
        if sdk:
            try:
                sdk.teardown()
            except SDKError:
                pass
        if installer_path and os.path.isfile(installer_path) and download_ok:
            try:
                os.remove(installer_path)
                print(f"\nInstaller deleted: {installer_path}")
            except OSError as e:
                print(f"\nWarning: could not delete installer '{installer_path}': {e}")


if __name__ == "__main__":
    main()
