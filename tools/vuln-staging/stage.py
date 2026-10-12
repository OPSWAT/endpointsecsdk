#!/usr/bin/env python3
###############################################################################################
##  Vuln Staging — stage (runs on the test box, as Administrator)
##
##  Installs every fetched installer silently through the OESIS SDK, pins each app's
##  auto-update so the old version stays put, then asks the SDK what it now sees and writes
##  the ground-truth manifest.
##
##  Per app:
##      GetVersion (100)          what is installed now, per signature
##      GetPackages (50306)       SDK view of the planned patch_uuid (logged, not required)
##      InstallPackage (50305)    package_uuid + local path + expected_installer_sha256;
##                                the SDK supplies the silent-install switches
##      auto-update pin           registry policy / service / task / updater folder
##      GetVersion (100)          confirm the target version is what is installed
##      GetProductVulnerability   (50505, v2mod.dat via 50520) the CVEs the SDK reports,
##                                compared with the CVEs the catalog says to expect
##      GetLatestInstaller (50300) what the SDK says is the missing patch
##
##  Any failure on one app is recorded and the run moves on to the next app. Data on the
##  endpoint comes from the OESIS API only.
##
##  Usage (from the bundle folder):
##      python stage.py [--apps k1,k2] [--replace-newer] [--no-pin] [--verify-only]
##
##      --replace-newer   uninstall (SDK 40000) a NEWER installed copy first; without it an app
##                        that is already newer than the target is skipped
##      --verify-only     install nothing; re-check what is installed against the manifest
##                        (use later to catch an updater that slipped through)
##
##  Elevated from a non-admin shell:
##      Start-Process cmd.exe -ArgumentList '/c cd /d C:\vuln-staging && python stage.py
##          > stage.log 2>&1' -Verb RunAs -Wait
##
##  Output: results\manifest.json and results\manifest.md in the bundle folder.
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import ctypes
import os
import sys
from datetime import datetime, timezone

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)

import _staging as st                            # noqa: E402
try:
    from sdk_wrapper import OESISWrapper, SDKError   # noqa: E402
    from platform_utils import get_lib_filename      # noqa: E402
except ImportError:
    print(f"ERROR: sdk_wrapper.py / platform_utils.py not found next to stage.py ({HERE}).")
    print("       Run stage.py from the bundle fetch.py builds (default C:\\vuln-staging),")
    print("       not from the repo's tools\\vuln-staging folder -- copy the whole bundle,")
    print("       including sdk\\, installers\\ and staging.json.")
    sys.exit(2)

METHOD_GET_VERSION          = 100
METHOD_UNINSTALL            = 40000
METHOD_GET_LATEST_INSTALLER = 50300
METHOD_LOAD_PATCH_DATABASE  = 50302
METHOD_INSTALL_PACKAGE      = 50305
METHOD_GET_PACKAGES         = 50306
METHOD_GET_VULNERABILITY    = 50505
METHOD_CONSUME_VMOD         = 50520


def parse_args(argv):
    opts = {"apps": None, "replace_newer": False, "pin": True, "verify_only": False,
            "bundle": HERE}
    it = iter(argv[1:])
    for a in it:
        if a == "--apps":
            opts["apps"] = [k.strip() for k in next(it).split(",") if k.strip()]
        elif a == "--replace-newer":
            opts["replace_newer"] = True
        elif a == "--no-pin":
            opts["pin"] = False
        elif a == "--verify-only":
            opts["verify_only"] = True
        elif a == "--bundle":
            opts["bundle"] = os.path.abspath(next(it))
        else:
            print(f"Unknown option {a}")
            print("Usage: python stage.py [--apps k1,k2] [--replace-newer] [--no-pin] "
                  "[--verify-only] [--bundle DIR]")
            sys.exit(1)
    return opts


def is_admin():
    try:
        return bool(ctypes.windll.shell32.IsUserAnAdmin())
    except Exception:
        return False


# --- SDK -------------------------------------------------------------------------------------

def init_sdk(sdk_dir):
    sdk = OESISWrapper(os.path.join(sdk_dir, get_lib_filename()))
    sdk.load()
    sdk.setup(os.path.join(sdk_dir, "license.cfg"), os.path.join(sdk_dir, "pass_key.txt"))
    rc, res = sdk.invoke(METHOD_LOAD_PATCH_DATABASE,
                         dat_input_source_file=os.path.join(sdk_dir, "patchv2.dat"))
    if rc < 0:
        raise Exception(f"LoadPatchDatabase(patchv2.dat) failed rc={rc}")
    rc, res = sdk.invoke(METHOD_CONSUME_VMOD,
                         dat_input_source_file=os.path.join(sdk_dir, "v2mod.dat"))
    if rc < 0:
        raise Exception(f"ConsumeOfflineVmodDatabase(v2mod.dat) failed rc={rc}")
    return sdk


def get_version(sdk, sig):
    # GetVersion answers rc=0 / "0.0" for a signature that is not installed -- treat as absent.
    rc, res = sdk.invoke(METHOD_GET_VERSION, signature=sig)
    version = (res.get("result") or {}).get("version") if rc >= 0 else None
    return version if version and st.vkey(version) not in ((), (0,)) else None


def versions(sdk, sigs):
    return {sig: get_version(sdk, sig) for sig in sigs}


def sdk_cves(sdk, sig):
    rc, res = sdk.invoke(METHOD_GET_VULNERABILITY, signature=sig)
    if rc < 0:
        return None, rc
    out = []
    for c in (res.get("result") or {}).get("cves") or []:
        cid = c.get("cve") if isinstance(c, dict) else c
        if cid:
            out.append(cid)
    return sorted(set(out)), rc


def sdk_latest(sdk, sig):
    rc, res = sdk.invoke(METHOD_GET_LATEST_INSTALLER, signature=sig)
    if rc < 0:
        return {"rc": rc}
    r = res.get("result") or {}
    return {"rc": rc, "version": r.get("version") or r.get("latest_version"),
            "title": r.get("title"), "url": r.get("url"), "patch_id": r.get("patch_id")}


def package_status(sdk, patch_uuid, package_uuid):
    rc, res = sdk.invoke(METHOD_GET_PACKAGES, patch_uuid=patch_uuid, package_limit="all")
    if rc < 0:
        return f"GetPackages rc={rc}"
    for p in (res.get("result") or {}).get("packages") or []:
        if p.get("package_uuid") == package_uuid:
            return p.get("evaluation_status") or "unknown"
    return "package_uuid not returned by GetPackages"


def install(sdk, package_uuid, path, sha256):
    rc, res = sdk.invoke(METHOD_INSTALL_PACKAGE, package_uuid=package_uuid,
                         path=path.replace("\\", "/"), expected_installer_sha256=sha256,
                         force_close_processes=True)
    return rc, (res.get("result") if rc >= 0 else res.get("error")) or {}


def uninstall(sdk, sig):
    rc, res = sdk.invoke(METHOD_UNINSTALL, signature=sig, type="auto")
    return rc


# --- per app ---------------------------------------------------------------------------------

def stage_app(sdk, app, bundle, opts, previous):
    cand = app["candidates"][app["selected"]]
    inst = app["installer"]
    target = cand["version"]
    sigs = app["signatures"]
    entry = {
        "key": app["key"], "name": app["name"], "v4_pid": app["pid"],
        "target_version": target, "release_date": cand.get("release_date"),
        "installer": {"file": inst["path"], "sha256": inst["sha256"], "url": inst["url"],
                      "package_uuid": inst["package_uuid"], "patch_uuid": inst["patch_uuid"]},
        "expected_patch_version": cand["expected_patch_version"],
        "catalog_newest_version": app.get("newest_version"),
        "expected_cves": cand["expected_cves"],
        "limit_equal_cves": cand.get("limit_equal_cves", []),
        "cve_details": cand.get("cve_details", {}),
        "fallbacks_skipped": [a for a in app.get("attempts", []) if not a["ok"]],
        "steps": [],
    }
    log = entry["steps"].append

    before = versions(sdk, sigs)
    entry["versions_before"] = {str(k): v for k, v in before.items() if v}
    print(f"    installed now : {entry['versions_before'] or '(not detected)'}")

    if not opts["verify_only"]:
        present = [v for v in before.values() if v]
        if any(st.vkey(v) == st.vkey(target) for v in present):
            log("already at target version; install skipped")
        else:
            # Checked before anything is uninstalled, so a missing file never leaves the app gone.
            installer_path = os.path.join(bundle, inst["path"])
            if not os.path.isfile(installer_path):
                entry["status"] = "INSTALLER_MISSING"
                log(f"installer not in the bundle: {installer_path}")
                print(f"    INSTALLER MISSING: {inst['path']} -- re-copy the bundle; continuing")
                return entry
            newer = [s for s, v in before.items() if v and st.vkey(v) > st.vkey(target)]
            if newer and not opts["replace_newer"]:
                entry["status"] = "SKIPPED_NEWER_INSTALLED"
                log(f"newer version installed on {newer}; rerun with --replace-newer")
                return entry
            for s in newer:
                rc = uninstall(sdk, s)
                log(f"uninstall newer (sig {s}) rc={rc}")
            status = package_status(sdk, inst["patch_uuid"], inst["package_uuid"])
            log(f"GetPackages evaluation_status={status}")
            print(f"    installing    : {target}  ({status})")
            rc, result = install(sdk, inst["package_uuid"], os.path.join(bundle, inst["path"]),
                                 inst["sha256"])
            entry["install"] = {"rc": rc, "installer_exit_code": result.get("installer_exit_code"),
                                "reported_version": result.get("version"),
                                "terminated_processes": result.get("terminated_processes")}
            log(f"InstallPackage rc={rc}")
            if rc < 0:
                entry["status"] = "INSTALL_FAILED"
                entry["install"]["error"] = result
                print(f"    INSTALL FAILED rc={rc} -- continuing with the next app")
                return entry
        if opts["pin"]:
            entry["autoupdate"] = [st.apply_pin(p) for p in app["pin"]]
            for p in entry["autoupdate"]:
                print(f"    pin {p['kind']:<11}: {p['result']} - {p.get('detail')}")

    after = versions(sdk, sigs)
    detected = {s: v for s, v in after.items() if v}
    entry["signatures_detected"] = sorted(detected)
    entry["installed_version"] = sorted(set(detected.values()), key=st.vkey)
    if len(entry["installed_version"]) > 1:
        log(f"more than one installed version across signatures: {detected}")
    if not detected:
        entry["status"] = "NOT_DETECTED"
        print("    NOT DETECTED after install")
        return entry
    on_target = [s for s, v in detected.items() if st.vkey(v) == st.vkey(target)]
    # One install can match two signatures (e.g. Notepad++ exe x64 3241 and msi x64 4127): the
    # entry is per product, and the CVE comparison uses the signatures on the target version.
    check_sigs = on_target or sorted(detected)

    reported, rcs = set(), {}
    for s in check_sigs:
        cves, rc = sdk_cves(sdk, s)
        rcs[str(s)] = rc
        reported.update(cves or [])
    expected = set(cand["expected_cves"])
    entry["sdk_vulnerability_rc"] = rcs
    entry["sdk_cves"] = sorted(reported)
    entry["cves_matched"] = sorted(expected & reported)
    entry["cves_expected_not_reported"] = sorted(expected - reported)
    entry["cves_reported_not_expected"] = sorted(reported - expected)
    entry["sdk_latest_installer"] = {str(s): sdk_latest(sdk, s) for s in check_sigs}

    if not on_target:
        entry["status"] = "VERSION_MISMATCH"
    elif not reported:
        entry["status"] = "NO_CVES_REPORTED"
    elif not entry["cves_matched"]:
        entry["status"] = "CVES_DIFFER"
    else:
        entry["status"] = "CONFIRMED"
    prev = previous.get(app["key"])
    if opts["verify_only"] and prev and prev.get("status") == "CONFIRMED" and not on_target:
        entry["status"] = "DRIFTED"
    print(f"    detected      : {detected}")
    print(f"    CVEs          : expected {len(expected)}, SDK reports {len(reported)}, "
          f"matched {len(entry['cves_matched'])}  -> {entry['status']}")
    return entry


# --- report ----------------------------------------------------------------------------------

def write_markdown(path, manifest):
    lines = [f"# Vuln staging manifest — {manifest['host']}", "",
             f"Staged {manifest['staged']} · plan {manifest['plan_generated']} · "
             f"catalog {manifest['catalog_modified']} · range rule `{manifest['range_rule']}`", "",
             "| App | Installed | Expected patch | Expected CVEs | SDK CVEs | Matched | Status |",
             "|---|---|---|---:|---:|---:|---|"]
    for e in manifest["apps"]:
        lines.append(f"| {e['name']} | {', '.join(e.get('installed_version') or []) or '-'} | "
                     f"{e.get('expected_patch_version') or '-'} | {len(e.get('expected_cves') or [])} | "
                     f"{len(e.get('sdk_cves') or [])} | {len(e.get('cves_matched') or [])} | "
                     f"{e.get('status')} |")
    lines += ["", "## Installers", "", "| App | Version | SHA-256 | Signatures |", "|---|---|---|---|"]
    for e in manifest["apps"]:
        ins = e.get("installer") or {}
        lines.append(f"| {e['name']} | {e.get('target_version') or '-'} | `{ins.get('sha256', '-')}` | "
                     f"{', '.join(str(s) for s in e.get('signatures_detected') or []) or '-'} |")
    with open(path, "w", encoding="utf-8") as f:
        f.write("\n".join(lines) + "\n")


def preflight(plan, bundle, opts):
    """Report every problem up front. Returns False only when nothing could be done at all."""
    keys = [a["key"] for a in plan.get("apps", [])]
    if opts["apps"]:
        unknown = [k for k in opts["apps"] if k not in keys]
        if unknown:
            print(f"ERROR: unknown app key(s) {unknown}. This bundle has: {', '.join(keys)}")
            return False
    chosen = [a for a in plan.get("apps", []) if not opts["apps"] or a["key"] in opts["apps"]]
    not_fetched = [a for a in chosen if a.get("selected") is None]
    missing = [(a, os.path.join(bundle, a["installer"]["path"])) for a in chosen
               if a.get("selected") is not None
               and not os.path.isfile(os.path.join(bundle, a["installer"]["path"]))]
    if not_fetched:
        print(f"NOTE: {len(not_fetched)} app(s) have no installer in this bundle and will be skipped:")
        for a in not_fetched:
            print(f"    {a['name']:<26} {a.get('fetch_status') or a.get('skipped_reason') or 'not fetched'}")
    if missing and not opts["verify_only"]:
        print(f"WARNING: {len(missing)} installer file(s) named in staging.json are not on disk "
              "(bundle copied partially?). Those apps will be skipped:")
        for a, path in missing:
            print(f"    {a['name']:<26} {path}")
        print("    Fix: copy the whole installers\\ folder from the build machine again.")
    if not_fetched or missing:
        print()
    usable = len(chosen) - len(not_fetched) - (0 if opts["verify_only"] else len(missing))
    if usable <= 0:
        print("ERROR: nothing to do -- no app in this bundle has an installer on disk.")
        print("       Run fetch.py on the build machine and copy the whole bundle again.")
        return False
    return True


def main():
    opts = parse_args(sys.argv)
    bundle = opts["bundle"]
    if not opts["verify_only"] and not is_admin():
        print("ERROR: run as Administrator (installs need elevation). --verify-only does not.")
        sys.exit(2)
    state_path = os.path.join(bundle, "staging.json")
    if not os.path.isfile(state_path):
        print(f"ERROR: {state_path} not found -- run fetch.py first.")
        sys.exit(2)
    plan = st.load_json(state_path)
    if not preflight(plan, bundle, opts):
        sys.exit(2)

    out_dir = os.path.join(bundle, "results")
    manifest_path = os.path.join(out_dir, "manifest.json")
    previous = {}
    if os.path.isfile(manifest_path):
        previous = {e["key"]: e for e in st.load_json(manifest_path).get("apps", [])}

    manifest = {
        "host": os.environ.get("COMPUTERNAME"),
        "staged": datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
        "mode": "verify-only" if opts["verify_only"] else "install",
        "plan_generated": plan.get("generated"),
        "catalog_modified": plan.get("catalog_modified"),
        "range_rule": plan.get("range_rule"),
        "apps": [],
    }

    sdk = None
    try:
        try:
            sdk = init_sdk(os.path.join(bundle, "sdk"))
        except Exception as exc:
            print(f"ERROR: the OESIS SDK did not start: {exc}")
            print("       Check bundle\\sdk: libwaapi.dll and its DLLs, license.cfg and pass_key.txt")
            print("       (an expired or mismatched license fails here), patchv2.dat and v2mod.dat.")
            print("       Rebuild with copy_sdk_files.py + fetch.py on the build machine if unsure.")
            sys.exit(2)
        print(f"Vuln staging ({manifest['mode']}) on {manifest['host']}")
        for app in plan["apps"]:
            if opts["apps"] and app["key"] not in opts["apps"]:
                continue
            print(f"\n  {app['name']}")
            if app.get("selected") is None:
                reason = app.get("fetch_status") or app.get("skipped_reason") or "not fetched"
                print(f"    SKIPPED: {reason}")
                manifest["apps"].append({"key": app["key"], "name": app["name"],
                                         "v4_pid": app["pid"], "status": "SKIPPED_NO_INSTALLER",
                                         "reason": reason,
                                         "fallbacks_skipped": app.get("attempts", [])})
            else:
                try:
                    manifest["apps"].append(stage_app(sdk, app, bundle, opts, previous))
                except Exception as exc:       # one bad app never stops the run
                    print(f"    ERROR: {exc} -- continuing with the next app")
                    manifest["apps"].append({"key": app["key"], "name": app["name"],
                                             "v4_pid": app["pid"], "status": "ERROR",
                                             "error": f"{type(exc).__name__}: {exc}"})
            # Keep apps from earlier runs that were not part of this one.
            st.save_json(manifest_path, merge(previous, manifest))
    finally:
        if sdk:
            try:
                sdk.teardown()
            except SDKError:
                pass

    final = merge(previous, manifest)
    st.save_json(manifest_path, final)
    write_markdown(os.path.join(out_dir, "manifest.md"), final)

    print(f"\n  {'App':<26} {'Installed':<16} {'Exp':>4} {'SDK':>4} {'Match':>5}  Status")
    print("  " + "-" * 80)
    for e in manifest["apps"]:
        print(f"  {e['name']:<26} {', '.join(e.get('installed_version') or []) or '-':<16} "
              f"{len(e.get('expected_cves') or []):>4} {len(e.get('sdk_cves') or []):>4} "
              f"{len(e.get('cves_matched') or []):>5}  {e['status']}")
    print(f"\n  Manifest: {manifest_path}")


def merge(previous, manifest):
    done = {e["key"] for e in manifest["apps"]}
    out = dict(manifest)
    out["apps"] = manifest["apps"] + [e for k, e in previous.items() if k not in done]
    return out


if __name__ == "__main__":
    main()
