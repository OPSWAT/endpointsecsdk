#!/usr/bin/env python3
###############################################################################################
##  Vuln Staging — shared helpers
##
##  The app list, version comparison and the auto-update pinning table shared by plan.py,
##  fetch.py and stage.py. Standard library only: this file is copied into the offline bundle
##  and imported on the test box, where the catalog and the repo are not available.
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import json
import os
import re
import subprocess
import sys

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

# Windows product ids are < 100000 (100000-199999 is macOS, >= 200000 is Linux).
WINDOWS_PID_LIMIT = 100000

# key -> Windows v4_pid in patch_aggregation_v2.json, plus how to stop the app updating itself.
# Pin kinds (applied by stage.py after install, each one best-effort and reported):
#   policy_reg   : HKLM registry values           {"key": ..., "values": {name: (type, data)}}
#   service      : stop + disable a service        {"name": ...}
#   task         : disable scheduled tasks         {"match": substring of the task path}
#   rename_dir   : rename a folder out of the way  {"path": ...}
#   mozilla      : distribution\policies.json DisableAppUpdate under the install dir
#   note         : nothing to do; the app only notifies (never installs on its own)
PF = os.environ.get("ProgramFiles", r"C:\Program Files")

# Chosen from what the catalog can actually deliver: every Windows product with an older,
# opswat-sourced, fresh-installable x64 version that falls inside at least one CVE range.
# Deliberately left out: Chrome (old versions do not stay installed), Edge (OS component,
# cannot be downgraded), Firefox ESR (same install folder as Firefox). PuTTY, WinSCP,
# FileZilla, Git, Node.js, GIMP, KeePass and Thunderbird were tried and have no older version
# in the catalog with any CVE in range -- the catalog only keeps a short window of versions.
DOTNET_BLOCK_MU = {"kind": "policy_reg", "key": r"SOFTWARE\Microsoft\.NET",
                   "values": {"BlockMU": ("REG_DWORD", "1")}}

APPS = [
    {"key": "firefox",       "name": "Mozilla Firefox", "pid": 44,
     "pin": [{"kind": "policy_reg", "key": r"SOFTWARE\Policies\Mozilla\Firefox",
              "values": {"DisableAppUpdate": ("REG_DWORD", "1")}},
             {"kind": "mozilla", "dir": os.path.join(PF, "Mozilla Firefox")},
             {"kind": "service", "name": "MozillaMaintenance"},
             {"kind": "task", "match": "Mozilla"}]},
    {"key": "acrobatreader", "name": "Adobe Acrobat Reader DC", "pid": 2966,
     "pin": [{"kind": "policy_reg",
              "key": r"SOFTWARE\Policies\Adobe\Acrobat Reader\DC\FeatureLockDown",
              "values": {"bUpdater": ("REG_DWORD", "0")}},
             {"kind": "service", "name": "AdobeARMservice"},
             {"kind": "task", "match": "Adobe Acrobat Update"}]},
    {"key": "foxitreader",   "name": "Foxit Reader", "pid": 394,
     "pin": [{"kind": "service", "name": "FoxitReaderUpdateService"},
             {"kind": "task", "match": "Foxit"}]},
    {"key": "foxiteditor",   "name": "Foxit PDF Editor", "pid": 3417,
     "pin": [{"kind": "service", "name": "FoxitPDFEditorUpdateService"},
             {"kind": "task", "match": "Foxit"}]},
    {"key": "virtualbox",    "name": "VirtualBox", "pid": 205,
     "pin": [{"kind": "note", "text": "update check notifies only"}]},
    {"key": "wireshark",     "name": "Wireshark", "pid": 207,
     "pin": [{"kind": "note", "text": "update check notifies only"}]},
    {"key": "libreoffice",   "name": "LibreOffice", "pid": 838,
     "pin": [{"kind": "note", "text": "update check notifies only"}]},
    {"key": "vscode",        "name": "Visual Studio Code", "pid": 3087,
     "pin": [{"kind": "policy_reg", "key": r"SOFTWARE\Policies\Microsoft\VSCode",
              "values": {"UpdateMode": ("REG_SZ", "none")}}]},
    {"key": "notepadpp",     "name": "Notepad++", "pid": 298,
     "pin": [{"kind": "rename_dir", "path": os.path.join(PF, "Notepad++", "updater")}]},
    {"key": "dotnet9",       "name": ".NET Runtime 9.0", "pid": 3603,
     "pin": [DOTNET_BLOCK_MU]},
    {"key": "aspnetcore9",   "name": "ASP.NET Core Runtime 9.0", "pid": 3605,
     "pin": [DOTNET_BLOCK_MU]},
    {"key": "java8",         "name": "Java 8", "pid": 111,
     "pin": [{"kind": "policy_reg", "key": r"SOFTWARE\JavaSoft\Java Update\Policy",
              "values": {"EnableJavaUpdate": ("REG_DWORD", "0"),
                         "EnableAutoUpdateCheck": ("REG_DWORD", "0")}},
             {"kind": "policy_reg", "key": r"SOFTWARE\WOW6432Node\JavaSoft\Java Update\Policy",
              "values": {"EnableJavaUpdate": ("REG_DWORD", "0"),
                         "EnableAutoUpdateCheck": ("REG_DWORD", "0")}}]},
    {"key": "python313",     "name": "Python 3.13", "pid": 3527,
     "pin": [{"kind": "note", "text": "no updater"}]},
    {"key": "python314",     "name": "Python 3.14", "pid": 3719,
     "pin": [{"kind": "note", "text": "no updater"}]},
    {"key": "zoom",          "name": "Zoom", "pid": 1844,
     "pin": [{"kind": "policy_reg", "key": r"SOFTWARE\Policies\Zoom\Zoom Meetings\General",
              "values": {"EnableClientAutoUpdate": ("REG_DWORD", "0")}}]},
    {"key": "openvpn",       "name": "OpenVPN", "pid": 1082,
     "pin": [{"kind": "note", "text": "no updater"}]},
    {"key": "7zip",          "name": "7-Zip", "pid": 196,
     "pin": [{"kind": "note", "text": "no updater"}]},
    {"key": "vlc",           "name": "VLC media player", "pid": 184,
     "pin": [{"kind": "note", "text": "update check notifies only"}]},
]


def vkey(version):
    """'8.9.6' -> (8, 9, 6). Trailing zeros dropped so '8.0' == '8'."""
    parts = [int(x) for x in re.findall(r"\d+", str(version or ""))]
    while len(parts) > 1 and parts[-1] == 0:
        parts.pop()
    return tuple(parts)


def load_json(path):
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def save_json(path, data):
    os.makedirs(os.path.dirname(os.path.abspath(path)), exist_ok=True)
    tmp = path + ".tmp"
    with open(tmp, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2)
    os.replace(tmp, path)


# --- auto-update pinning (stage.py, on the box) ---------------------------------------------

def _run(cmd):
    p = subprocess.run(cmd, capture_output=True, text=True)
    return p.returncode, (p.stdout + p.stderr).strip()


# sc.exe qc START_TYPE number -> the word sc.exe config start= takes.
SERVICE_START = {2: "auto", 3: "demand", 4: "disabled", 0: "boot", 1: "system"}


def _reg_read(key, name):
    """(type, data) of an HKLM value, or None when the key or value does not exist."""
    import winreg
    try:
        with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, key, 0,
                            winreg.KEY_READ | winreg.KEY_WOW64_64KEY) as k:
            data, rtype = winreg.QueryValueEx(k, name)
            return rtype, data
    except OSError:
        return None


def _reg_key_exists(key):
    import winreg
    try:
        winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, key, 0,
                       winreg.KEY_READ | winreg.KEY_WOW64_64KEY).Close()
        return True
    except OSError:
        return False


def _service_start_type(name):
    """Current start type ('auto', 'demand', ...) of a service, or None if it is not there."""
    rc, txt = _run(["sc.exe", "qc", name])
    if rc != 0:
        return None
    m = re.search(r"START_TYPE\s*:\s*(\d+)", txt)
    return SERVICE_START.get(int(m.group(1))) if m else None


def _tasks(match):
    """{task path: enabled?} for scheduled tasks whose line mentions `match`."""
    rc, txt = _run(["schtasks.exe", "/Query", "/FO", "CSV", "/NH"])
    found = {}
    for line in txt.splitlines():
        if match.lower() not in line.lower():
            continue
        cols = [c.strip('"') for c in line.split('","')]
        if cols and cols[0].startswith("\\"):
            found[cols[0]] = found.get(cols[0], False) or (len(cols) < 3 or cols[-1] != "Disabled")
    return found


def apply_pin(pin):
    """Apply one pin action. Returns {"kind", "result": applied|absent|failed|note, "detail",
    "previous"}. `previous` is what the pin changed, so undo_pin() can put it back exactly."""
    kind = pin["kind"]
    out = {"kind": kind}
    try:
        if kind == "note":
            out.update(result="note", detail=pin["text"])

        elif kind == "policy_reg":
            import winreg
            previous = {"key_existed": _reg_key_exists(pin["key"]),
                        "values": {name: _reg_read(pin["key"], name) for name in pin["values"]}}
            with winreg.CreateKeyEx(winreg.HKEY_LOCAL_MACHINE, pin["key"], 0,
                                    winreg.KEY_SET_VALUE | winreg.KEY_WOW64_64KEY) as k:
                for name, (rtype, data) in pin["values"].items():
                    if rtype == "REG_DWORD":
                        winreg.SetValueEx(k, name, 0, winreg.REG_DWORD, int(data))
                    else:
                        winreg.SetValueEx(k, name, 0, winreg.REG_SZ, str(data))
            out.update(result="applied", detail=f"HKLM\\{pin['key']} {pin['values']}",
                       previous=previous)

        elif kind == "service":
            start = _service_start_type(pin["name"])
            if start is None:
                out.update(result="absent", detail=f"service {pin['name']} not present")
            else:
                _run(["sc.exe", "stop", pin["name"]])
                rc, txt = _run(["sc.exe", "config", pin["name"], "start=", "disabled"])
                out.update(result="applied" if rc == 0 else "failed",
                           detail=f"service {pin['name']} disabled" if rc == 0 else txt,
                           previous={"start": start})

        elif kind == "task":
            tasks = _tasks(pin["match"])
            if not tasks:
                out.update(result="absent", detail=f"no task matching '{pin['match']}'")
            else:
                names = sorted(tasks)
                done = [n for n in names
                        if _run(["schtasks.exe", "/Change", "/TN", n, "/Disable"])[0] == 0]
                out.update(result="applied" if done else "failed",
                           detail=f"disabled {done}" if done else f"could not disable {names}",
                           previous={"enabled": sorted(n for n in done if tasks[n])})

        elif kind == "rename_dir":
            src = pin["path"]
            if not os.path.isdir(src):
                out.update(result="absent", detail=f"{src} not present")
            else:
                dst = src + ".disabled"
                if os.path.isdir(dst):
                    import shutil
                    shutil.rmtree(dst, ignore_errors=True)
                os.replace(src, dst)
                out.update(result="applied", detail=f"{src} -> {dst}")

        elif kind == "mozilla":
            if not os.path.isdir(pin["dir"]):
                out.update(result="absent", detail=f"{pin['dir']} not present")
            else:
                path = os.path.join(pin["dir"], "distribution", "policies.json")
                data = {}
                existed = os.path.isfile(path)
                if existed:
                    try:
                        data = load_json(path)
                    except ValueError:
                        data = {}
                had = "DisableAppUpdate" in data.get("policies", {})
                previous = {"file_existed": existed,
                            "value": data["policies"]["DisableAppUpdate"] if had else None,
                            "had_value": had}
                data.setdefault("policies", {})["DisableAppUpdate"] = True
                save_json(path, data)
                out.update(result="applied", detail=f"{path} DisableAppUpdate=true",
                           previous=previous)
        else:
            out.update(result="failed", detail=f"unknown pin kind {kind}")
    except Exception as exc:                       # pinning never stops the run
        out.update(result="failed", detail=f"{type(exc).__name__}: {exc}")
    return out


def undo_pin(pin, record=None):
    """Reverse one pin action. `record` is what apply_pin() returned (from the manifest); its
    `previous` is restored exactly. Without it (a manifest written before pins recorded their
    previous state) the setting goes back to the app's default: policy values deleted, service
    set to demand-start, matching tasks re-enabled, folder renamed back.
    A recorded `previous` that is the pinned state itself (stage.py run twice, or two apps that
    share a pin such as .NET's BlockMU) says nothing about the original, so it is treated as
    not recorded.
    Returns {"kind", "result": restored|nothing_to_do|failed|note, "detail"}."""
    kind = pin["kind"]
    prev = (record or {}).get("previous")
    out = {"kind": kind}
    try:
        if kind == "note":
            out.update(result="note", detail=pin["text"])

        elif kind == "policy_reg":
            import winreg
            changed = []
            try:
                k = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, pin["key"], 0,
                                   winreg.KEY_ALL_ACCESS | winreg.KEY_WOW64_64KEY)
            except OSError:
                out.update(result="nothing_to_do", detail=f"HKLM\\{pin['key']} not present")
                return out
            with k:
                for name, (_, pinned) in pin["values"].items():
                    old = (prev or {}).get("values", {}).get(name)
                    if old and str(old[1]) == str(pinned):   # recorded the pin itself
                        old = None
                    if old:                                   # it had a value before: put it back
                        rtype, data = old
                        winreg.SetValueEx(k, name, 0, rtype, data)
                        changed.append(f"{name}={data}")
                    else:                                     # it did not exist: remove it
                        try:
                            winreg.DeleteValue(k, name)
                            changed.append(f"{name} deleted")
                        except OSError:
                            pass
                empty = winreg.QueryInfoKey(k)[0] == 0 and winreg.QueryInfoKey(k)[1] == 0
            # A key the pin created, and that holds nothing else now, goes too.
            if empty and prev is not None and not prev.get("key_existed", True):
                try:
                    winreg.DeleteKeyEx(winreg.HKEY_LOCAL_MACHINE, pin["key"],
                                       winreg.KEY_WOW64_64KEY, 0)
                    changed.append("key deleted")
                except OSError:
                    pass
            out.update(result="restored" if changed else "nothing_to_do",
                       detail=f"HKLM\\{pin['key']}: {', '.join(changed) or 'values already absent'}")

        elif kind == "service":
            now = _service_start_type(pin["name"])
            if now is None:
                out.update(result="nothing_to_do", detail=f"service {pin['name']} not present")
            else:
                start = (prev or {}).get("start")
                if start in (None, "disabled"):              # unknown, or recorded the pin itself
                    start = "demand"
                if now == start:
                    out.update(result="nothing_to_do", detail=f"service {pin['name']} already {start}")
                else:
                    rc, txt = _run(["sc.exe", "config", pin["name"], "start=", start])
                    out.update(result="restored" if rc == 0 else "failed",
                               detail=f"service {pin['name']} start={start}"
                               + ("" if prev else " (default; previous state not recorded)")
                               if rc == 0 else txt)

        elif kind == "task":
            # Re-enable exactly the tasks the pin disabled; without a record, every matching task.
            present = _tasks(pin["match"])
            names = (prev or {}).get("enabled")
            if not names:                                    # unknown, or a re-run that found them off
                names = sorted(present)
            names = [n for n in names if n in present]
            if not names:
                out.update(result="nothing_to_do", detail=f"no task matching '{pin['match']}' to re-enable")
            else:
                done = [n for n in names
                        if _run(["schtasks.exe", "/Change", "/TN", n, "/Enable"])[0] == 0]
                out.update(result="restored" if done else "failed",
                           detail=f"enabled {done}" if done else f"could not enable {names}")

        elif kind == "rename_dir":
            src, dst = pin["path"], pin["path"] + ".disabled"
            if not os.path.isdir(dst):
                out.update(result="nothing_to_do", detail=f"{dst} not present")
            elif os.path.isdir(src):
                out.update(result="failed", detail=f"both {src} and {dst} exist; left as is")
            else:
                os.replace(dst, src)
                out.update(result="restored", detail=f"{dst} -> {src}")

        elif kind == "mozilla":
            path = os.path.join(pin["dir"], "distribution", "policies.json")
            if not os.path.isfile(path):
                out.update(result="nothing_to_do", detail=f"{path} not present")
            else:
                data = load_json(path)
                policies = data.get("policies", {})
                if prev and prev.get("had_value") and prev.get("value") is not True:
                    policies["DisableAppUpdate"] = prev.get("value")
                else:
                    policies.pop("DisableAppUpdate", None)
                if not policies and prev is not None and not prev.get("file_existed", True):
                    os.remove(path)
                    out.update(result="restored", detail=f"{path} removed (stage.py created it)")
                else:
                    if not policies:
                        data.pop("policies", None)
                    save_json(path, data)
                    out.update(result="restored", detail=f"{path} DisableAppUpdate removed")
        else:
            out.update(result="failed", detail=f"unknown pin kind {kind}")
    except Exception as exc:                       # cleanup never stops on one setting
        out.update(result="failed", detail=f"{type(exc).__name__}: {exc}")
    return out
