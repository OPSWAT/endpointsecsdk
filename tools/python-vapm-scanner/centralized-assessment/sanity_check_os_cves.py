#!/usr/bin/env python3
###############################################################################################
##  VAPM Centralized Assessment — OS CVE Applicability Sanity Check (OPTIONAL, ONLINE)
##
##  The offline catalog maps OS CVEs to the endpoint by os_id. Occasionally a catalog record
##  stamps the wrong os_id on a CVE (e.g. an old Windows 10 CVE wrongly associated with a
##  current Windows 11 24H2 cumulative KB). This tool cross-checks each reported OS CVE
##  against the NVD CPE applicability data and FLAGS any CVE whose NVD record does not list
##  the endpoint's OS product — a likely catalog os_id over-association.
##
##  This is a diagnostic aid, NOT part of the offline assessment: it requires internet access
##  to NVD. The mapper (map_ca_osdetails.py) remains fully offline.
##
##  Usage:
##      python3 sanity_check_os_cves.py                  # check every OS CVE in the result
##      python3 sanity_check_os_cves.py --cves CVE-2020-17103,CVE-2026-45585
##      python3 sanity_check_os_cves.py --limit 20 --strict
##      set NVD_API_KEY=...   (optional; raises the NVD rate limit 5->50 per 30s)
##
##  Reads:  results/ca-result.json (os section) or map-ca-osdetails-result.json
##  Writes: sanity-check-os-cves-result.json
##
##  Created by Chris Seiler
##  OPSWAT OEM Solutions Architect
###############################################################################################

import argparse
import json
import os
import sys
import time
import urllib.request
import urllib.error

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
CA_RESULT = os.path.join(SCRIPT_DIR, "results", "ca-result.json")
OS_MAP_RESULT = os.path.join(SCRIPT_DIR, "map-ca-osdetails-result.json")
OUTPUT_FILE = os.path.join(SCRIPT_DIR, "sanity-check-os-cves-result.json")
CACHE_FILE = os.path.join(SCRIPT_DIR, ".nvd-cpe-cache.json")

NVD_API = "https://services.nvd.nist.gov/rest/json/cves/2.0?cveId="

# Windows 11 build -> NVD release token (for --strict matching).
WIN11_BUILD_RELEASE = {
    "22000": "21h2", "22621": "22h2", "22631": "23h2",
    "26100": "24h2", "26200": "25h2",
}


# ---------------------------------------------------------------------------
# Load the OS CVEs + endpoint OS identity from the assessment result
# ---------------------------------------------------------------------------

def load_os_section():
    if os.path.isfile(CA_RESULT):
        with open(CA_RESULT, "r", encoding="utf-8") as f:
            data = json.load(f)
        osr = data.get("os") or {}
        return {
            "name":    osr.get("name"),
            "version": osr.get("version"),
            "os_id":   osr.get("os_id"),
            "cves":    list(osr.get("cves", [])),
            "source":  os.path.basename(CA_RESULT),
        }
    if os.path.isfile(OS_MAP_RESULT):
        with open(OS_MAP_RESULT, "r", encoding="utf-8") as f:
            data = json.load(f)
        info = data.get("os_info", {})
        return {
            "name":    info.get("name"),
            "version": info.get("version"),
            "os_id":   info.get("os_id"),
            "cves":    [c.get("cve") for c in data.get("cves", []) if c.get("cve")],
            "source":  os.path.basename(OS_MAP_RESULT),
        }
    return None


def derive_target(os_name, os_version, strict):
    """Return (family_token, strict_token_or_None) for matching against NVD CPE products.

    family_token: a substring all acceptable products must contain (e.g. 'windows_11').
    strict_token: an exact product token (e.g. 'windows_11_24h2') when --strict and the
    release can be derived from the build; otherwise None.
    """
    name = (os_name or "").lower()
    version = str(os_version or "")
    build = version.split(".")[-1] if version else ""

    if "windows 11" in name:
        family = "windows_11"
    elif "windows 10" in name:
        family = "windows_10"
    elif "server" in name:
        family = "windows_server"
    elif "windows" in name:
        family = "windows"
    else:
        family = name.replace(" ", "_") or "windows"

    strict_token = None
    if strict and family == "windows_11":
        rel = WIN11_BUILD_RELEASE.get(build)
        if rel:
            strict_token = f"windows_11_{rel}"
    return family, strict_token


# ---------------------------------------------------------------------------
# NVD lookup (cached, rate-limited)
# ---------------------------------------------------------------------------

def load_cache():
    if os.path.isfile(CACHE_FILE):
        try:
            with open(CACHE_FILE, "r", encoding="utf-8") as f:
                return json.load(f)
        except (OSError, ValueError):
            return {}
    return {}


def save_cache(cache):
    try:
        with open(CACHE_FILE, "w", encoding="utf-8") as f:
            json.dump(cache, f)
    except OSError:
        pass


def nvd_lookup(cve, api_key):
    """Return {'found': bool, 'products': [vendor:product...]} for a CVE from NVD."""
    req = urllib.request.Request(NVD_API + cve, headers={"User-Agent": "vapm-sanity-check"})
    if api_key:
        req.add_header("apiKey", api_key)
    for attempt in range(4):
        try:
            with urllib.request.urlopen(req, timeout=90) as resp:
                data = json.loads(resp.read().decode("utf-8"))
            break
        except urllib.error.HTTPError as e:
            if e.code in (403, 429):       # rate limited — back off and retry
                time.sleep(8 * (attempt + 1))
                continue
            return {"found": False, "products": [], "error": f"HTTP {e.code}"}
        except OSError as e:
            # URLError, socket.timeout, TimeoutError, connection resets — back off and retry.
            time.sleep(4 * (attempt + 1))
            if attempt == 3:
                return {"found": False, "products": [], "error": str(e)}
    else:
        return {"found": False, "products": [], "error": "rate-limited"}

    if data.get("totalResults", 0) < 1:
        return {"found": False, "products": []}
    cve_obj = data["vulnerabilities"][0]["cve"]
    products = set()
    for cfg in cve_obj.get("configurations", []) or []:
        for node in cfg.get("nodes", []) or []:
            for match in node.get("cpeMatch", []) or []:
                parts = match.get("criteria", "").split(":")
                if len(parts) > 4:
                    products.add(f"{parts[3]}:{parts[4]}")
    return {"found": True, "products": sorted(products)}


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    ap = argparse.ArgumentParser(description="NVD-CPE applicability sanity check for OS CVEs.")
    ap.add_argument("--cves", help="comma-separated CVE ids to check (default: all in the result)")
    ap.add_argument("--limit", type=int, default=0, help="check at most N CVEs (0 = no limit)")
    ap.add_argument("--strict", action="store_true",
                    help="require the exact release token (e.g. windows_11_24h2), not just the family")
    ap.add_argument("--api-key", default=os.environ.get("NVD_API_KEY"),
                    help="NVD API key (or set NVD_API_KEY); raises the rate limit")
    args = ap.parse_args()

    osr = load_os_section()
    if not osr:
        print("ERROR: no OS result found. Run scan-ca.py (or map_ca_osdetails.py) first.")
        return

    cves = [c.strip() for c in args.cves.split(",")] if args.cves else sorted(set(osr["cves"]))
    if args.limit:
        cves = cves[:args.limit]

    family, strict_token = derive_target(osr["name"], osr["version"], args.strict)
    target = strict_token or family
    delay = 0.6 if args.api_key else 6.5     # respect NVD rate limits (50 vs 5 per 30s)

    print("VAPM — OS CVE Applicability Sanity Check (NVD CPE cross-check)")
    print("=" * 70)
    print(f"  Endpoint OS : {osr['name']} ({osr['version']}), os_id={osr['os_id']}")
    print(f"  Result src  : {osr['source']}")
    print(f"  Match target: '{target}'  ({'strict release' if strict_token else 'OS family'})")
    print(f"  CVEs to check: {len(cves)}   (NVD {'key' if args.api_key else 'no key — slow'}, "
          f"~{delay}s/req)")
    print("-" * 70)

    cache = load_cache()
    flagged, not_in_nvd, errors, ok = [], [], [], 0

    for i, cve in enumerate(cves, 1):
        info = cache.get(cve)
        if info is None:
            info = nvd_lookup(cve, args.api_key)
            if "error" not in info:
                cache[cve] = info
                save_cache(cache)
            time.sleep(delay)

        if info.get("error"):
            errors.append({"cve": cve, "error": info["error"]})
            continue
        if not info.get("found"):
            not_in_nvd.append(cve)
            print(f"  [{i}/{len(cves)}] {cve:<18} NOT FOUND in NVD")
            continue

        products = info["products"]
        applies = any(target in p for p in products)
        if applies:
            ok += 1
        else:
            win_products = sorted({p for p in products if "windows" in p})
            flagged.append({"cve": cve, "nvd_products": products,
                            "windows_products": win_products})
            print(f"  [{i}/{len(cves)}] {cve:<18} FLAG — no '{target}' in NVD CPE; "
                  f"NVD lists: {', '.join(win_products) or '(no windows products)'}")

    print("-" * 70)
    print(f"  Applicable (target present in NVD CPE) : {ok}")
    print(f"  FLAGGED (target absent — likely catalog os_id over-association): {len(flagged)}")
    print(f"  Not found in NVD                       : {len(not_in_nvd)}")
    print(f"  Lookup errors                          : {len(errors)}")
    if flagged:
        print("\n  Flagged CVEs (catalog claims this OS, NVD does not):")
        for f in flagged:
            print(f"    {f['cve']:<18} NVD windows products: "
                  f"{', '.join(f['windows_products']) or '(none)'}")

    output = {
        "endpoint_os":   {k: osr[k] for k in ("name", "version", "os_id")},
        "match_target":  target,
        "strict":        bool(strict_token),
        "total_checked": len(cves),
        "applicable":    ok,
        "flagged":       flagged,
        "not_in_nvd":    not_in_nvd,
        "errors":        errors,
    }
    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        json.dump(output, f, indent=2, default=str)
    print(f"\n  Full results written to: {OUTPUT_FILE}")


if __name__ == "__main__":
    main()
