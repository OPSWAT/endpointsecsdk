#!/usr/bin/env python3
###############################################################################
#  Prepare the DLP engine for the "hello world" scanner
#  Copies the OESIS Endpoint-DLP engine runtime - the scanner libraries, the
#  detector rulepack and the OCR data - into the sdk/ subfolder, where
#  dlp_scan.py loads it from. None of it is committed (see .gitignore).
#
#  Cross-platform: it stages the libraries that match the OS you run it on
#  (.dll on Windows, .dylib on macOS, .so on Linux) from that platform's OESIS
#  DLP package (e.g. OESIS-DLP-*.zip, or an unpacked folder). With no argument it
#  looks for the newest OESIS-DLP package in your Downloads folder.
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

# Shared-library extension for the platform we're staging for.
LIB_EXT = {"win32": ".dll", "darwin": ".dylib"}.get(sys.platform, ".so")

# The library "stems" the DLP scan path needs: the scanner, its dependency chain
# (libwautils -> libwaheap) and pdfium for PDF text. The package also ships
# other-module libraries (libwaapi, libwaresource, ...), but those aren't used
# here, so they are left out. pdfium may be named pdfium or libpdfium.
NEEDED_LIB_STEMS = ("libwadlpscan", "libwautils", "libwaheap", "pdfium", "libpdfium")

# Non-library files that are the same on every platform.
DATA_FILES = ("dlp_rules.dat", "dlp_rules.manifest.json")


def _is_platform_lib(low):
    """True if `low` (a lowercased file name) is a shared library for this OS,
    including Linux's versioned names like libwadlpscan.so.4.3."""
    if sys.platform == "win32":
        return low.endswith(".dll")
    if sys.platform == "darwin":
        return low.endswith(".dylib")
    return low.endswith(".so") or ".so." in low


def want(name):
    """Return the destination (relative to sdk/) for a package file, or None to
    skip it. Only the files the DLP scan path needs on this OS are copied."""
    base = os.path.basename(name.replace("\\", "/"))
    low = base.lower()
    if _is_platform_lib(low):
        return base if any(low.startswith(s) for s in NEEDED_LIB_STEMS) else None
    if low in DATA_FILES:
        return base
    if low.endswith(".traineddata"):                 # OCR language data
        return os.path.join("tessdata", base)
    return None


def _folder_has_engine(folder):
    """True if `folder` holds the DLP engine library for this OS."""
    for f in os.listdir(folder) if os.path.isdir(folder) else []:
        low = f.lower()
        if low.startswith("libwadlpscan") and _is_platform_lib(low):
            return True
    return False


def find_default_source():
    """Newest OESIS DLP package (zip or unpacked folder) in ~/Downloads, or None."""
    downloads = os.path.join(os.path.expanduser("~"), "Downloads")
    candidates = glob.glob(os.path.join(downloads, "OESIS-DLP*.zip"))
    for pattern in ("OESIS-DLP*", os.path.join("OESIS-DLP*", "*")):
        for path in glob.glob(os.path.join(downloads, pattern)):
            if _folder_has_engine(path):
                candidates.append(path)
    return max(candidates, key=os.path.getmtime) if candidates else None


def _dest(rel):
    path = os.path.join(SDK_DIR, rel)
    parent = os.path.dirname(path)
    if parent and not os.path.isdir(parent):
        os.makedirs(parent)
    return path


def stage_from_folder(src):
    """Copy the wanted files out of an unpacked package folder into sdk/."""
    root = src
    if not _folder_has_engine(root):
        for dirpath, _dirs, _files in os.walk(src):
            if _folder_has_engine(dirpath):
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
    """Copy the wanted files straight out of the package .zip into sdk/.

    The OESIS DLP zip may use Windows backslash separators, so names are
    normalized and only the basename is used to decide the destination.
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
    if not _folder_has_engine(SDK_DIR):
        print("  MISSING: libwadlpscan%s (the DLP engine library)" % LIB_EXT)
        ok = False
    for req in DATA_FILES:
        if not os.path.isfile(os.path.join(SDK_DIR, req)):
            print("  MISSING: %s" % req)
            ok = False
    if not glob.glob(os.path.join(SDK_DIR, "tessdata", "*.traineddata")):
        print("  NOTE: no tessdata/*.traineddata - image (OCR) scanning will not work.")
    return ok


def main(argv=None):
    parser = argparse.ArgumentParser(
        description="Copy the OESIS DLP engine into sdk/ for dlp_scan.py.")
    parser.add_argument("--source",
                        help="OESIS DLP package: a .zip or the unpacked folder. "
                             "Default: newest one found in ~/Downloads.")
    args = parser.parse_args(argv)

    source = args.source or find_default_source()
    if not source:
        print("ERROR: no DLP package given and none found in your Downloads folder.")
        print("       python prepare.py --source /path/to/OESIS-DLP-package.zip")
        return 1
    if not os.path.exists(source):
        print("ERROR: source not found: %s" % source)
        return 1

    print("Platform: %s (staging %s libraries)" % (sys.platform, LIB_EXT))
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
        print("\nIncomplete - check that --source is the OESIS DLP package for "
              "this platform (it must contain libwadlpscan%s)." % LIB_EXT)
        return 1

    print("\nDone. Try:")
    print("  python make_samples.py            # create the sample files")
    print("  python dlp_scan.py samples/sensitive/employee_record.pdf")
    return 0


if __name__ == "__main__":
    sys.exit(main())
