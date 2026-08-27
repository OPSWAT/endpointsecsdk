#!/usr/bin/env python3
###############################################################################
#  Prepare the DLP engine for the "hello world" scanner
#  Copies the OESIS Endpoint-DLP engine runtime - the scanner DLLs, the detector
#  rulepack and the OCR data - into the sdk/ subfolder, where dlp_scan.py loads
#  it from. None of it is committed (see .gitignore).
#
#  The engine ships in the OESIS DLP package (e.g. OESIS-DLP-v3-*.zip, which
#  unpacks to an "OESIS-DLP-Demo" folder). Point this at that .zip or the
#  unpacked folder; with no argument it looks for the newest OESIS-DLP package in
#  your Downloads folder.
#
#  Usage:
#      python prepare.py                         # auto-find in ~/Downloads
#      python prepare.py --source <package.zip>
#      python prepare.py --source <unpacked folder>
#
#  Created by Chris Seiler
#  OPSWAT OEM Solutions Architect
###############################################################################

import argparse
import glob
import os
import shutil
import sys
import zipfile

HERE = os.path.dirname(os.path.abspath(__file__))
SDK_DIR = os.path.join(HERE, "sdk")   # engine is staged here; dlp_scan.py reads it

# The one file that proves a folder/zip really contains the DLP engine.
ENGINE_MARKER = "libwadlpscan.dll"

# The only DLLs the DLP scan path needs: the scanner, its dependency chain
# (libwautils -> libwaheap), and pdfium for PDF text. The package also ships
# libwaapi.dll and libwaresource.dll, but those belong to other OESIS modules
# and are not used here, so they are intentionally left out.
NEEDED_DLLS = {"libwadlpscan.dll", "libwautils.dll", "libwaheap.dll", "pdfium.dll"}

# What must exist here afterwards for the scanner to work.
REQUIRED = [ENGINE_MARKER, "dlp_rules.dat", "dlp_rules.manifest.json"]


def want(name):
    """Return the destination (relative to sdk/) for a package file, or None to
    skip it. Only the files the DLP scan path actually needs are copied - demo
    exes, other-module DLLs, wa-dbs-*.dat engine data, license files (this build
    needs none), READMEs and examples are all left behind."""
    base = os.path.basename(name.replace("\\", "/"))
    low = base.lower()
    if low in NEEDED_DLLS:                                    # engine + deps
        return base
    if low in ("dlp_rules.dat", "dlp_rules.manifest.json"):   # rulepack + manifest
        return base
    if low.endswith(".traineddata"):                         # OCR language data
        return os.path.join("tessdata", base)
    return None


def find_default_source():
    """Newest OESIS DLP package (zip or unpacked folder) in ~/Downloads, or None."""
    downloads = os.path.join(os.path.expanduser("~"), "Downloads")
    candidates = glob.glob(os.path.join(downloads, "OESIS-DLP*.zip"))
    for pattern in ("OESIS-DLP*", os.path.join("OESIS-DLP*", "*")):
        for path in glob.glob(os.path.join(downloads, pattern)):
            if os.path.isdir(path) and os.path.isfile(os.path.join(path, ENGINE_MARKER)):
                candidates.append(path)
    return max(candidates, key=os.path.getmtime) if candidates else None


def _dest(rel):
    path = os.path.join(SDK_DIR, rel)
    parent = os.path.dirname(path)
    if parent and not os.path.isdir(parent):
        os.makedirs(parent)
    return path


def stage_from_folder(src):
    """Copy the wanted files out of an unpacked package folder into HERE."""
    root = src
    if not os.path.isfile(os.path.join(root, ENGINE_MARKER)):
        for dirpath, _dirs, files in os.walk(src):
            if ENGINE_MARKER in files:
                root = dirpath
                break
    count = 0
    for dirpath, _dirs, files in os.walk(root):
        for fname in files:
            rel = want(fname)
            if rel:
                shutil.copy2(os.path.join(dirpath, fname), _dest(rel))
                count += 1
    return count


def stage_from_zip(src):
    """Copy the wanted files straight out of the package .zip into HERE.

    The OESIS DLP zip uses Windows backslash separators, so names are normalized
    and only the basename is used to decide the destination.
    """
    count = 0
    with zipfile.ZipFile(src) as zf:
        for info in zf.infolist():
            if info.is_dir():
                continue
            rel = want(info.filename)
            if rel:
                with zf.open(info) as s, open(_dest(rel), "wb") as d:
                    shutil.copyfileobj(s, d)
                count += 1
    return count


def verify():
    ok = True
    for req in REQUIRED:
        if not os.path.isfile(os.path.join(SDK_DIR, req)):
            print("  MISSING: %s" % req)
            ok = False
    if not glob.glob(os.path.join(SDK_DIR, "tessdata", "*.traineddata")):
        print("  NOTE: no tessdata/*.traineddata - image (OCR) scanning will not work.")
    return ok


def main(argv=None):
    parser = argparse.ArgumentParser(
        description="Copy the OESIS DLP engine into this folder for dlp_scan.py.")
    parser.add_argument("--source",
                        help="OESIS DLP package: a .zip or the unpacked folder. "
                             "Default: newest one found in ~/Downloads.")
    args = parser.parse_args(argv)

    source = args.source or find_default_source()
    if not source:
        print("ERROR: no DLP package given and none found in your Downloads folder.")
        print("       python prepare.py --source C:\\path\\to\\OESIS-DLP-v3-2026-08-14.zip")
        return 1
    if not os.path.exists(source):
        print("ERROR: source not found: %s" % source)
        return 1

    print("Source: %s" % os.path.abspath(source))
    print("Staging into: %s\n" % SDK_DIR)
    os.makedirs(SDK_DIR, exist_ok=True)

    try:
        if os.path.isfile(source) and source.lower().endswith(".zip"):
            copied = stage_from_zip(source)
        elif os.path.isdir(source):
            copied = stage_from_folder(source)
        else:
            print("ERROR: --source must be a .zip file or a folder.")
            return 1
    except (OSError, zipfile.BadZipFile) as ex:
        print("ERROR staging the engine: %s" % ex)
        return 1

    print("Copied %d file(s).\n" % copied)
    print("Verifying:")
    if not verify():
        print("\nIncomplete - check that --source is the OESIS DLP package "
              "(it must contain %s)." % ENGINE_MARKER)
        return 1

    print("\nDone. Try:")
    print("  python dlp_scan.py samples/sensitive/employee_record.pdf")
    return 0


if __name__ == "__main__":
    sys.exit(main())
