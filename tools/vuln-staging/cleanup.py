#!/usr/bin/env python3
###############################################################################################
##  Vuln Staging — cleanup (runs on the test box, as Administrator)
##
##  Undoes what stage.py did, from its manifest (results\manifest.json):
##
##      uninstall                 UninstallProduct (40000) for every app stage.py installed,
##                                on each signature it was detected on
##      auto-update pins          each pin reversed: registry values put back (or deleted when
##                                stage.py created them), services back to their start type,
##                                tasks re-enabled, the renamed updater folder renamed back,
##                                Firefox's DisableAppUpdate policy removed
##      GetVersion (100)          confirm what is still installed afterwards
##
##  An app is uninstalled only when stage.py installed it. An app that was already on the box
##  before staging (stage.py found it at the target version, or skipped it as newer) is left
##  installed unless --include-preexisting is given; its pins are still undone. A newer copy
##  that stage.py removed with --replace-newer cannot be put back: it is listed in the report.
##
##  Only pins the manifest records as applied are undone. They restore the exact previous
##  setting when the manifest recorded it (stage.py does this from this version on). For an
##  older manifest each setting goes back to the app's default: policy values deleted, services
##  set to demand-start, matching tasks re-enabled.
##
##  Usage (from the bundle folder):
##      python cleanup.py [--apps k1,k2] [--keep-apps] [--keep-pins] [--include-preexisting]
##                        [--dry-run] [--bundle DIR]
##
##      --keep-apps            undo the pins only; leave every app installed
##      --keep-pins            uninstall only; leave the auto-update pins in place
##      --include-preexisting  also uninstall apps that were on the box before staging
##      --dry-run              print what would be done and change nothing
##
##  Elevated from a non-admin shell:
##      Start-Process cmd.exe -ArgumentList '/c cd /d C:\vuln-staging && python cleanup.py
##          > cleanup.log 2>&1' -Verb RunAs -Wait
##
##  Output: results\cleanup.json in the bundle folder. The manifest itself is left as it was.
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import os
import sys
from datetime import datetime, timezone

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)

import _staging as st                            # noqa: E402
from stage import init_sdk, versions, uninstall, is_admin   # noqa: E402
from sdk_wrapper import SDKError                 # noqa: E402

USAGE = ("Usage: python cleanup.py [--apps k1,k2] [--keep-apps] [--keep-pins] "
         "[--include-preexisting] [--dry-run] [--bundle DIR]")


def parse_args(argv):
    opts = {"apps": None, "uninstall": True, "unpin": True, "preexisting": False,
            "dry_run": False, "bundle": HERE}
    it = iter(argv[1:])
    for a in it:
        if a == "--apps":
            opts["apps"] = [k.strip() for k in next(it).split(",") if k.strip()]
        elif a == "--keep-apps":
            opts["uninstall"] = False
        elif a == "--keep-pins":
            opts["unpin"] = False
        elif a == "--include-preexisting":
            opts["preexisting"] = True
        elif a == "--dry-run":
            opts["dry_run"] = True
        elif a == "--bundle":
            opts["bundle"] = os.path.abspath(next(it))
        else:
            print(f"Unknown option {a}")
            print(USAGE)
            sys.exit(1)
    return opts


def pin_id(pin):
    """Two apps can carry the same pin (.NET runtime and ASP.NET Core share BlockMU)."""
    return (pin["kind"], pin.get("key") or pin.get("name") or pin.get("match")
            or pin.get("path") or pin.get("dir") or pin.get("text"))


def installed_by_stage(entry):
    """True when stage.py installed this app (InstallPackage, or the direct silent install it
    falls back to on a fresh box) and it succeeded."""
    return (entry.get("install") or {}).get("rc", -1) >= 0


def signatures_of(entry, plan_app):
    sigs = list(entry.get("signatures_detected") or [])
    if not sigs and plan_app:                      # never detected: fall back to the plan's list
        sigs = list(plan_app.get("signatures") or [])
    return [int(s) for s in sigs]


def clean_app(sdk, entry, plan_app, opts, pins_done):
    out = {"key": entry["key"], "name": entry["name"], "steps": []}
    log = out["steps"].append
    preexisting = bool(entry.get("versions_before"))
    replaced = [s for s in (entry.get("steps") or []) if s.startswith("uninstall newer")]
    if replaced:
        out["not_restorable"] = replaced
        log(f"stage.py removed a newer copy ({'; '.join(replaced)}); it is not reinstalled")
    elif preexisting and installed_by_stage(entry):
        before = ", ".join(sorted(set(entry["versions_before"].values())))
        out["not_restorable"] = [f"was {before} before staging; uninstalling does not bring it back"]

    # --- uninstall --------------------------------------------------------------------------
    sigs = signatures_of(entry, plan_app)
    want_uninstall = opts["uninstall"] and (installed_by_stage(entry)
                                            or (opts["preexisting"] and preexisting))
    if not opts["uninstall"]:
        out["uninstall"] = "kept (--keep-apps)"
    elif not want_uninstall:
        out["uninstall"] = ("kept: on the box before staging (use --include-preexisting)"
                            if preexisting else "kept: stage.py did not install it")
    elif not sigs:
        out["uninstall"] = "no signatures to uninstall by"
    else:
        before = versions(sdk, sigs) if sdk else {}
        present = [s for s, v in before.items() if v]
        out["versions_before_cleanup"] = {str(s): v for s, v in before.items() if v}
        if not present:
            out["uninstall"] = "not installed"
        elif opts["dry_run"]:
            out["uninstall"] = f"would uninstall signatures {present}"
        else:
            rcs = {}
            for s in present:
                if versions(sdk, [s])[s]:          # one product can match two signatures
                    rcs[str(s)] = uninstall(sdk, s)
                    log(f"UninstallProduct (40000) sig {s} rc={rcs[str(s)]}")
            out["uninstall_rc"] = rcs
            out["uninstall"] = "done" if all(rc >= 0 for rc in rcs.values()) else "failed"

    # --- auto-update pins ------------------------------------------------------------------
    if opts["unpin"] and plan_app:
        # stage.py writes one autoupdate record per pin, in the plan's pin order. Only pins it
        # actually applied are undone: a pin it never applied (app skipped, install failed,
        # --no-pin, service absent) is a setting cleanup has no business touching.
        records = {pin_id(p): r for p, r in
                   zip(plan_app.get("pin") or [], entry.get("autoupdate") or [])}
        out["autoupdate"] = []
        for pin in plan_app.get("pin") or []:
            pid = pin_id(pin)
            record = records.get(pid)
            if pin["kind"] == "note":
                continue
            if not record or record.get("result") != "applied":
                out["autoupdate"].append({"kind": pin["kind"], "result": "nothing_to_do",
                                          "detail": "stage.py did not apply this pin"})
                continue
            if pid in pins_done:
                out["autoupdate"].append({"kind": pin["kind"], "result": "nothing_to_do",
                                          "detail": f"already undone for {pins_done[pid]}"})
                continue
            pins_done[pid] = entry["key"]
            if opts["dry_run"]:
                out["autoupdate"].append({"kind": pin["kind"], "result": "would_restore",
                                          "detail": str(pid[1])})
            else:
                out["autoupdate"].append(st.undo_pin(pin, record))
    elif opts["unpin"]:
        out["autoupdate"] = [{"kind": "-", "result": "failed",
                              "detail": "app not in staging.json; pins unknown"}]

    # --- what is left ----------------------------------------------------------------------
    if sdk and sigs and not opts["dry_run"]:
        after = versions(sdk, sigs)
        out["versions_after"] = {str(s): v for s, v in after.items() if v}
    if out.get("uninstall") == "done" and out.get("versions_after"):
        out["uninstall"] = "incomplete: still detected"
    return out


def main():
    opts = parse_args(sys.argv)
    bundle = opts["bundle"]
    if not opts["dry_run"] and not is_admin():
        print("ERROR: run as Administrator (uninstall and services need elevation). "
              "--dry-run does not.")
        sys.exit(2)
    manifest_path = os.path.join(bundle, "results", "manifest.json")
    state_path = os.path.join(bundle, "staging.json")
    if not os.path.isfile(manifest_path):
        print(f"ERROR: {manifest_path} not found -- nothing was staged from this bundle.")
        sys.exit(2)
    manifest = st.load_json(manifest_path)
    plan = {a["key"]: a for a in st.load_json(state_path).get("apps", [])} \
        if os.path.isfile(state_path) else {}
    if not plan:
        print(f"WARNING: {state_path} not found -- pins cannot be undone, apps can.")

    report = {
        "host": os.environ.get("COMPUTERNAME"),
        "cleaned": datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
        "staged": manifest.get("staged"),
        "mode": "dry-run" if opts["dry_run"] else "cleanup",
        "options": {k: v for k, v in opts.items() if k != "bundle"},
        "apps": [],
    }
    pins_done = {}
    sdk = None
    try:
        if opts["uninstall"]:
            sdk = init_sdk(os.path.join(bundle, "sdk"))
        print(f"Vuln staging cleanup ({report['mode']}) on {report['host']} -- staged {manifest.get('staged')}")
        for entry in manifest.get("apps", []):
            if opts["apps"] and entry["key"] not in opts["apps"]:
                continue
            print(f"\n  {entry['name']}")
            try:
                res = clean_app(sdk, entry, plan.get(entry["key"]), opts, pins_done)
            except Exception as exc:               # one bad app never stops the cleanup
                res = {"key": entry["key"], "name": entry["name"],
                       "error": f"{type(exc).__name__}: {exc}"}
                print(f"    ERROR: {res['error']} -- continuing with the next app")
            report["apps"].append(res)
            if "uninstall" in res:
                print(f"    uninstall     : {res['uninstall']}")
            for p in res.get("autoupdate") or []:
                if p["result"] != "note":
                    print(f"    pin {p['kind']:<11}: {p['result']} - {p.get('detail')}")
            if res.get("versions_after"):
                print(f"    still detected: {res['versions_after']}")
            for n in res.get("not_restorable") or []:
                print(f"    NOT RESTORED  : {n}")
    finally:
        if sdk:
            try:
                sdk.teardown()
            except SDKError:
                pass

    if not opts["dry_run"]:
        st.save_json(os.path.join(bundle, "results", "cleanup.json"), report)

    print(f"\n  {'App':<26} {'Uninstall':<34} {'Pins':<22} Still installed")
    print("  " + "-" * 98)
    for r in report["apps"]:
        pins = [p["result"] for p in r.get("autoupdate") or [] if p["result"] != "note"]
        summary = ", ".join(f"{n} {k}" for k, n in sorted(
            {k: pins.count(k) for k in pins}.items())) or "-"
        print(f"  {r['name']:<26} {str(r.get('uninstall', r.get('error', '-')))[:34]:<34} "
              f"{summary[:22]:<22} {', '.join(sorted(set((r.get('versions_after') or {}).values()))) or '-'}")
    if not opts["dry_run"]:
        print(f"\n  Report: {os.path.join(bundle, 'results', 'cleanup.json')}")


if __name__ == "__main__":
    main()
