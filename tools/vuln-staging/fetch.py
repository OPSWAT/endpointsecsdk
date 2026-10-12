#!/usr/bin/env python3
###############################################################################################
##  Vuln Staging — fetch
##
##  Downloads the installers named in plan.json and builds a self-contained staging bundle
##  that can be copied to an offline test box (or used on this machine).
##
##  For each app it tries the planned version first, then each older vulnerable fallback,
##  and each download link of each one. A 404, timeout, TLS failure or SHA-256 mismatch
##  moves on to the next link / next version; an app with nothing downloadable is recorded
##  as skipped and the run carries on with the next app. Every attempt is logged.
##
##  Every file is checked against the catalog SHA-256; a mismatch is deleted and refused.
##  Re-running is safe: an installer already on disk with the right hash is reused.
##
##  TLS: Python on Windows does not use the OS certificate store, so verification often
##  fails. Downloads use an unverified context; integrity comes from the SHA-256 check here
##  and again from InstallPackage (expected_installer_sha256) on the box.
##
##  Bundle layout (default C:\vuln-staging — must be outside the repo):
##      plan.json              from plan.py
##      staging.json           plan + the version actually fetched per app + every attempt
##      installers\<app>\<version>\<file>
##      stage.py, cleanup.py, _staging.py, sdk_wrapper.py, platform_utils.py
##      sdk\                   SDK DLLs, license.cfg, pass_key.txt, patchv2.dat, v2mod.dat
##
##  Usage:
##      python fetch.py [--bundle DIR] [--apps k1,k2] [--no-sdk] [--timeout SEC]
##
##  Examples:
##      python fetch.py                          # uses C:\vuln-staging\plan.json
##      python fetch.py --apps firefox,7zip      # just these
##      python fetch.py --no-sdk                 # installers only, copy SDK files yourself
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import hashlib
import os
import shutil
import ssl
import sys
import time
import urllib.error
import urllib.request
from datetime import datetime, timezone

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)

import _staging as st                           # noqa: E402

DEFAULT_BUNDLE = r"C:\vuln-staging"
USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) vuln-staging/1.0"
NO_RETRY_HTTP = {400, 401, 403, 404, 410, 451}

# Portable (embeddable) Python for the box -- not installed, so it cannot collide with the
# Python 3.13 / 3.14 being staged. Hash pinned from a download over the Windows cert store.
PY_EMBED_ZIP = "python-3.12.10-embed-amd64.zip"
PY_EMBED_URL = f"https://www.python.org/ftp/python/3.12.10/{PY_EMBED_ZIP}"
PY_EMBED_SHA256 = "4acbed6dd1c744b0376e3b1cf57ce906f9dc9e95e68824584c8099a63025a3c3"

# AcmeScanner (tools/vapm-scanner/bin) needs the .NET 9 *Windows Desktop* runtime. Its installer
# also lays down Microsoft.NETCore.App of the same version, so it is pinned to the version the
# .NET Runtime 9.0 app is staged at -- a newer one would put a patched .NET 9 beside the staged
# vulnerable one. URLs + SHA-512 from Microsoft's release metadata
# (builds.dotnet.microsoft.com/dotnet/release-metadata/9.0/releases.json).
DESKTOP_RUNTIME = {
    "9.0.17": ("https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/9.0.17/windowsdesktop-runtime-9.0.17-win-x64.exe",
               "f43d591aa6ea5376e6db2c6d984b62cea61103fbe41df8ec43e5fc24e86b3d4921edf3924fed4e3d3124f9d6cd5fd3a90b37e977cb807a783c2f0c81fcf781ce"),
    "9.0.18": ("https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/9.0.18/windowsdesktop-runtime-9.0.18-win-x64.exe",
               "330e22da74e05cf602c8164a560203f2e3f1916eaf63691eb15a260242bd29d543ea83a288d874131bbe427bdcd5dbf9912ecab58935270c8b40dcc786f9e074"),
    "9.0.19": ("https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/9.0.19/windowsdesktop-runtime-9.0.19-win-x64.exe",
               "47d82ec79834d1cc032292411960bcc42a3955947e999c2afd02a93ffa9c2928ff62ffbb4e7dc4572e3fbce5bbfd6ae9ec5c3ebf99979f8bd07fd375b71a6a53"),
}
DESKTOP_RUNTIME_DEFAULT = "9.0.19"
# Engine DB files AcmeScanner loads from its working directory (VAPMAdapter UpdateDBFiles).
SCANNER_DATS = ("patch.dat", "patchv2.dat", "v2mod.dat", "vmod.dat", "wuo.dat", "wiv-lite.dat",
                "ap_checksum.dat", "patch_driver_firmware.dat")
SCANNER_SECRETS = ("license.cfg", "pass_key.txt", "download_token.txt")


def parse_args(argv):
    opts = {"bundle": DEFAULT_BUNDLE, "apps": None, "sdk": True, "timeout": 120, "retries": 2}
    it = iter(argv[1:])
    for a in it:
        if a == "--bundle":
            opts["bundle"] = next(it)
        elif a == "--apps":
            opts["apps"] = [k.strip() for k in next(it).split(",") if k.strip()]
        elif a == "--no-sdk":
            opts["sdk"] = False
        elif a == "--timeout":
            opts["timeout"] = int(next(it))
        else:
            print(f"Unknown option {a}")
            print("Usage: python fetch.py [--bundle DIR] [--apps k1,k2] [--no-sdk] [--timeout SEC]")
            sys.exit(1)
    return opts


def find_repo_root():
    current = HERE
    while True:
        if os.path.isfile(os.path.join(current, "sdkroot")):
            return current
        parent = os.path.dirname(current)
        if parent == current:
            return None
        current = parent


def inside(path, root):
    path, root = os.path.normcase(os.path.abspath(path)), os.path.normcase(os.path.abspath(root))
    return path == root or path.startswith(root + os.sep)


def sha256_of(path, algo="sha256"):
    h = hashlib.new(algo)
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


def filename_for(url, package_uuid):
    name = url.rsplit("/", 1)[-1].split("?", 1)[0].split("#", 1)[0]
    name = urllib.request.url2pathname(name).replace("\\", "_").replace("/", "_")
    return name if name and "." in name else f"{package_uuid}.bin"


def download(url, dest, expected_sha, timeout, retries, algo="sha256"):
    """Returns (ok, detail). Never raises. expected_sha is checked with hashlib algo."""
    ctx = ssl._create_unverified_context() if sys.platform == "win32" else ssl.create_default_context()
    part = dest + ".part"
    last = None
    for attempt in range(1, retries + 2):
        try:
            req = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
            with urllib.request.urlopen(req, context=ctx, timeout=timeout) as resp, \
                    open(part, "wb") as out:
                shutil.copyfileobj(resp, out, 1 << 20)
            actual = sha256_of(part, algo)
            if actual != expected_sha:
                os.remove(part)
                return False, f"{algo} mismatch (got {actual})"
            os.replace(part, dest)
            return True, f"ok ({os.path.getsize(dest):,} bytes)"
        except urllib.error.HTTPError as exc:
            last = f"HTTP {exc.code}"
            if exc.code in NO_RETRY_HTTP:
                break
        except Exception as exc:
            last = f"{type(exc).__name__}: {exc}"
        if os.path.exists(part):
            os.remove(part)
        if attempt <= retries:
            time.sleep(3 * attempt)
    return False, last


def fetch_app(app, bundle, opts):
    """Try candidates in order. Mutates app with 'selected', 'installer', 'attempts'."""
    app["attempts"] = []
    app["selected"] = None
    for idx, cand in enumerate(app.get("candidates") or []):
        pkg = cand["package"]
        for url in pkg.get("download_links") or []:
            rel = os.path.join("installers", app["key"], cand["version"],
                              filename_for(url, pkg["package_uuid"]))
            dest = os.path.join(bundle, rel)
            os.makedirs(os.path.dirname(dest), exist_ok=True)
            if os.path.isfile(dest) and sha256_of(dest) == pkg["sha256"]:
                ok, detail = True, "already downloaded, sha256 ok"
            else:
                print(f"    {cand['version']:<16} {url}")
                ok, detail = download(url, dest, pkg["sha256"], opts["timeout"], opts["retries"])
            app["attempts"].append({"version": cand["version"], "url": url, "ok": ok,
                                    "detail": detail})
            print(f"    {'':<16} -> {detail}")
            if ok:
                app["selected"] = idx
                app["installer"] = {"path": rel, "sha256": pkg["sha256"], "url": url,
                                    "package_uuid": pkg["package_uuid"],
                                    "patch_uuid": cand["patch_uuid"]}
                return True
    return False


def add_portable_python(bundle, opts):
    """Put the embeddable Python into bundle\\python so an offline box needs nothing installed.
    Pinned hash -- keep in step with run-stage.cmd. Returns None or a warning."""
    dest_dir = os.path.join(bundle, "python")
    if os.path.isfile(os.path.join(dest_dir, "python.exe")):
        return None
    zip_path = os.path.join(bundle, PY_EMBED_ZIP)
    if not (os.path.isfile(zip_path) and sha256_of(zip_path) == PY_EMBED_SHA256):
        print(f"\n  Portable Python {PY_EMBED_URL}")
        ok, detail = download(PY_EMBED_URL, zip_path, PY_EMBED_SHA256, opts["timeout"], opts["retries"])
        print(f"    -> {detail}")
        if not ok:
            return f"portable Python not added ({detail}); the box will need internet or the zip"
    import zipfile
    with zipfile.ZipFile(zip_path) as z:
        z.extractall(dest_dir)
    return None


def add_scanner(bundle, plan, opts):
    """bundle\\scanner: AcmeScanner from tools/vapm-scanner/bin, ready to run -- SDK DLLs, the engine
    DB files and the license next to it -- plus the matching .NET Desktop runtime installer in
    bundle\\prereqs. Returns a list of warnings (never raises)."""
    warnings = []
    try:
        root = find_repo_root()
        src = os.path.join(root, "tools", "vapm-scanner", "bin")
        if not os.path.isfile(os.path.join(src, "AcmeScanner.exe")):
            return [f"AcmeScanner not found in {src}; the scanner will not be launched"]
        dst = os.path.join(bundle, "scanner")
        # Copy the tracked binaries only; license files in a developer's bin are not ours to copy.
        shutil.copytree(src, dst, dirs_exist_ok=True,
                        ignore=lambda d, names: [n for n in names if n in SCANNER_SECRETS])
        sdk_dir = os.path.join(bundle, "sdk")
        if os.path.isdir(sdk_dir):
            for name in os.listdir(sdk_dir):
                if name.lower().endswith((".dll", ".exe")) or name in ("license.cfg", "pass_key.txt"):
                    shutil.copy2(os.path.join(sdk_dir, name), dst)
        analog = os.path.join(root, "OPSWAT-SDK", "extract", "analog", "client")
        for name in SCANNER_DATS:
            path = os.path.join(analog, name)
            if os.path.isfile(path):
                shutil.copy2(path, dst)
            elif name in ("patch.dat", "v2mod.dat"):
                warnings.append(f"scanner: {name} not in {analog}; use Download DB in the scanner")
        print(f"\n  AcmeScanner -> {dst}")
    except Exception as exc:
        return [f"scanner not added: {type(exc).__name__}: {exc}"]

    # .NET Desktop runtime pinned to the staged .NET Runtime 9.0 version.
    dotnet = next((a for a in plan["apps"] if a["key"] == "dotnet9"), None)
    version = DESKTOP_RUNTIME_DEFAULT
    if dotnet and dotnet.get("selected") is not None:
        staged = dotnet["candidates"][dotnet["selected"]]["version"]
        if staged in DESKTOP_RUNTIME:
            version = staged
        else:
            warnings.append(f"no pinned Desktop runtime for staged .NET {staged}; using {version}")
    url, sha512 = DESKTOP_RUNTIME[version]
    prereqs = os.path.join(bundle, "prereqs")
    os.makedirs(prereqs, exist_ok=True)
    for old in os.listdir(prereqs):                 # keep exactly one, the matching version
        if old.startswith("windowsdesktop-runtime-") and version not in old:
            os.remove(os.path.join(prereqs, old))
    dest = os.path.join(prereqs, url.rsplit("/", 1)[-1])
    if not (os.path.isfile(dest) and sha256_of(dest, "sha512") == sha512):
        print(f"  .NET Desktop runtime {version} {url}")
        ok, detail = download(url, dest, sha512, opts["timeout"], opts["retries"], algo="sha512")
        print(f"    -> {detail}")
        if not ok:
            warnings.append(f".NET Desktop runtime {version} not downloaded ({detail}); "
                            "AcmeScanner needs it on the box")
    return warnings


def copy_bundle_files(bundle, with_sdk):
    root = find_repo_root()
    hello = os.path.join(root, "helloworld", "python")
    for src in (os.path.join(HERE, "stage.py"), os.path.join(HERE, "cleanup.py"),
                os.path.join(HERE, "_staging.py"),
                os.path.join(HERE, "run-stage.cmd"),
                os.path.join(hello, "sdk_wrapper.py"), os.path.join(hello, "platform_utils.py")):
        shutil.copy2(src, bundle)
    if not with_sdk:
        return []
    sdk_src = os.path.join(hello, "sdk")
    analog = os.path.join(root, "OPSWAT-SDK", "extract", "analog", "client")
    sdk_dst = os.path.join(bundle, "sdk")
    os.makedirs(sdk_dst, exist_ok=True)
    missing = []
    if os.path.isdir(sdk_src):
        for name in os.listdir(sdk_src):
            if name.lower().endswith((".dll", ".exe")) or name in ("license.cfg", "pass_key.txt"):
                shutil.copy2(os.path.join(sdk_src, name), sdk_dst)
    for name in ("license.cfg", "pass_key.txt", "libwaapi.dll"):
        if not os.path.isfile(os.path.join(sdk_dst, name)):
            missing.append(f"{name} (run helloworld/python/copy_sdk_files.py first)")
    # Prefer the freshly extracted catalog databases over whatever sits in helloworld/sdk.
    for name in ("patchv2.dat", "v2mod.dat"):
        for src in (os.path.join(analog, name), os.path.join(sdk_src, name)):
            if os.path.isfile(src):
                shutil.copy2(src, sdk_dst)
                break
        else:
            missing.append(name)
    return missing


def main():
    opts = parse_args(sys.argv)
    bundle = os.path.abspath(opts["bundle"])
    root = find_repo_root()
    if root and inside(bundle, root):
        print(f"ERROR: bundle {bundle} is inside the repo ({root}). Installers must never be")
        print("       committed -- choose a folder outside it, e.g. --bundle C:\\vuln-staging")
        sys.exit(2)

    plan_path = os.path.join(bundle, "plan.json")
    if not os.path.isfile(plan_path):
        print(f"ERROR: {plan_path} not found. Run plan.py first (default output is that path).")
        sys.exit(2)
    plan = st.load_json(plan_path)
    state_path = os.path.join(bundle, "staging.json")
    # Resume: keep earlier fetch results for apps not re-fetched, as long as the plan is the same.
    if os.path.isfile(state_path):
        prior = st.load_json(state_path)
        if prior.get("generated") == plan.get("generated"):
            plan = prior
    plan["fetched"] = datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")

    print(f"Vuln staging fetch -> {bundle}")
    for app in plan["apps"]:
        if opts["apps"] and app["key"] not in opts["apps"]:
            continue
        print(f"\n  {app['name']}")
        try:
            if not app.get("candidates"):
                print(f"    SKIPPED: {app.get('skipped_reason')}")
                continue
            if not fetch_app(app, bundle, opts):
                app["fetch_status"] = "no installer could be downloaded"
                print("    SKIPPED: every candidate failed -- continuing with the next app")
            else:
                app["fetch_status"] = "ok"
        except Exception as exc:                   # one bad app never stops the run
            app["fetch_status"] = f"error: {type(exc).__name__}: {exc}"
            print(f"    ERROR: {exc} -- continuing with the next app")
        st.save_json(state_path, plan)             # saved per app, so a crash keeps progress

    missing = copy_bundle_files(bundle, opts["sdk"])
    py_warning = add_portable_python(bundle, opts)
    if py_warning:
        missing.append(py_warning)
    missing.extend(add_scanner(bundle, plan, opts))
    st.save_json(state_path, plan)

    print(f"\n  {'App':<26} {'Version':<16} {'CVEs':>5}  Result")
    print("  " + "-" * 86)
    for app in plan["apps"]:
        if opts["apps"] and app["key"] not in opts["apps"]:
            continue
        sel = app.get("selected")
        if sel is None:
            print(f"  {app['name']:<26} {'-':<16} {'':>5}  "
                  f"{app.get('fetch_status') or app.get('skipped_reason')}")
            continue
        cand = app["candidates"][sel]
        note = "" if sel == 0 else f"fallback #{sel} (newer candidates unavailable)"
        print(f"  {app['name']:<26} {cand['version']:<16} {len(cand['expected_cves']):>5}  ok {note}")
    got = sum(1 for a in plan["apps"] if a.get("selected") is not None)
    print(f"\n  {got} installer(s) ready.  State: {state_path}")
    if missing:
        print(f"  WARNING: SDK files missing from bundle\\sdk: {', '.join(missing)}")
    if not os.environ.get("VULN_STAGING_LAUNCHER"):     # run-stage.cmd carries on by itself
        print("  Next: copy the bundle to the test box and double-click run-stage.cmd")
        print("        (elevates, uses the bundled portable Python, runs stage.py).")


if __name__ == "__main__":
    main()
