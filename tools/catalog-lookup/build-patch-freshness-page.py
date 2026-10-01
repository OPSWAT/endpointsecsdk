#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — build-patch-freshness-page
##
##  Builds a self-contained HTML page showing, for each tracked OESIS v4 signature, the patch
##  the catalog offers and the date that patch was released — i.e. "when did OPSWAT last update
##  its third-party patch for this product".
##
##  Reads exactly three datasets from the Analog snapshot, and only these fields:
##
##    patch_associations.json      match signature against v4_signatures, take the is_latest
##                                 entry, read patch_id
##    patch_aggregation.json       for that patch_id: product_name, latest_version, release_date
##    patch_aggregation_v2.json    same patch, v2 shape: version (not latest_version), and the
##                                 signature mapping is inline on product.v4_signatures
##    products.json                support_3rd_party_patch, to tell "OPSWAT doesn't patch this
##                                 product" apart from "no record found", so the former is not
##                                 reported as a gap
##
##  Everything else in the snapshot (cves.json, vuln_associations.json, bulletin.json, the
##  patch_system_* files) ships in the same zip but is never opened by this routine.
##
##  Usage:
##      python3 build-patch-freshness-page.py
##      python3 build-patch-freshness-page.py --signatures my-sigs.txt --out report.html
##      python3 build-patch-freshness-page.py --vendor-ga vendor_ga.json
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import argparse
import html
import json
import os
import re
import sys
from collections import Counter
from datetime import datetime, timezone


def _helper_dir():
    """Find _catalog.py — beside this script, or in tools/catalog-lookup walking up from it.

    Lets this script run from the repo root as well as from tools/catalog-lookup.
    """
    current = os.path.dirname(os.path.abspath(__file__))
    if os.path.isfile(os.path.join(current, "_catalog.py")):
        return current
    while True:
        candidate = os.path.join(current, "tools", "catalog-lookup")
        if os.path.isfile(os.path.join(candidate, "_catalog.py")):
            return candidate
        parent = os.path.dirname(current)
        if parent == current:
            return None
        current = parent


_HELPERS = _helper_dir()
if not _HELPERS:
    print("ERROR: _catalog.py not found (expected beside this script or in tools/catalog-lookup).")
    sys.exit(2)
sys.path.insert(0, _HELPERS)

import _catalog as cat  # noqa: E402  (path must be set first)

# Files that arrive in the snapshot but are deliberately not read by this routine.
NOT_READ = [
    "cves.json", "vuln_associations.json", "vuln_system_associations.json", "bulletin.json",
    "patch_system_aggregation.json", "patch_system_aggregation_v2.json", "patch_status.json",
    "driver_firmware_patch_aggregation.json", "kb_info.json", "os_info.json",
    "ap_support_chart.json", "ap_support_chart_mac.json",
]


# --- catalog reads ---------------------------------------------------------------------------

def snapshot_stamp(server_dir):
    """Snapshot date from the patch_aggregation.json header block."""
    path = os.path.join(server_dir, "patch_aggregation.json")
    try:
        data = cat.load_json(path)
    except (OSError, ValueError):
        return None, None
    for element in data.get("oesis", []):
        header = element.get("header")
        if isinstance(header, dict):
            ts = header.get("timestamp")
            iso = cat.fmt_epoch(ts) if ts else None
            return iso, header.get("time")
    return None, None


def load_associations(server_dir):
    """signature id -> patch_id, from the is_latest entry in patch_associations.json."""
    latest = {}
    titles = {}
    path = os.path.join(server_dir, "patch_associations.json")
    for rec in cat.read_records(path):
        if not rec.get("is_latest"):
            continue
        patch_id = rec.get("patch_id")
        if patch_id is None:
            continue
        for sig in rec.get("v4_signatures") or []:
            latest[sig] = patch_id
            titles[sig] = rec.get("title")
    return latest, titles


def load_aggregation(server_dir):
    """patch_id -> {product_name, latest_version, release_date} from patch_aggregation.json."""
    agg = {}
    path = os.path.join(server_dir, "patch_aggregation.json")
    for rec in cat.read_records(path):
        pid = rec.get("_id")
        if pid is None:
            continue
        agg[pid] = {
            "product_name":   rec.get("product_name"),
            "latest_version": rec.get("latest_version"),
            "release_date":   rec.get("release_date"),
        }
    return agg


def load_aggregation_v2(server_dir):
    """signature id -> {version, release_date, data_source, name} for is_latest v2 patches.

    In v2 the signature mapping is inline on product.v4_signatures and the version field is
    called 'version' rather than 'latest_version'.
    """
    by_sig = {}
    path = os.path.join(server_dir, "patch_aggregation_v2.json")
    if not os.path.isfile(path):
        return by_sig
    for rec in cat.read_records(path):
        if not rec.get("is_latest"):
            continue
        product = rec.get("product") or {}
        entry = {
            "version":      rec.get("version"),
            "release_date": rec.get("release_date"),
            "data_source":  str(rec.get("data_source") or "unknown").lower(),
            "name":         product.get("name"),
        }
        for sig in product.get("v4_signatures") or []:
            by_sig[sig] = entry
    return by_sig


# --- version + date arithmetic ---------------------------------------------------------------

def parse_release_date(value):
    """Catalog release_date ('MM/DD/YYYY', also tolerates ISO) -> date, or None."""
    if not value:
        return None
    text = str(value).strip()
    for fmt in ("%m/%d/%Y", "%Y-%m-%d", "%d/%m/%Y"):
        try:
            return datetime.strptime(text, fmt).date()
        except ValueError:
            continue
    return None


def version_tuple(value):
    """'151.0.7922.138' -> (151, 0, 7922, 138). Non-numeric noise is dropped."""
    if not value:
        return ()
    return tuple(int(part) for part in re.findall(r"\d+", str(value)))


def compare_versions(left, right):
    """-1 / 0 / 1 comparing two version strings component-wise, or None if either is unparsable."""
    a, b = version_tuple(left), version_tuple(right)
    if not a or not b:
        return None
    width = max(len(a), len(b))
    a += (0,) * (width - len(a))
    b += (0,) * (width - len(b))
    return (a > b) - (a < b)


def age_bucket(days):
    if days is None:
        return "unknown"
    if days <= 7:
        return "0-7"
    if days <= 30:
        return "8-30"
    if days <= 90:
        return "31-90"
    return "90+"


# --- vendor GA side ---------------------------------------------------------------------------

def load_vendor_ga(path):
    """Optional vendor-side GA data, keyed by signature id or exact product name.

    {"generated": "...", "products": {"Google Chrome": {"version": "...",
                                       "release_date": "YYYY-MM-DD", "source_url": "..."}}}

    This is the one AI-assisted input: a daily lookup of each vendor's current broadly-available
    GA release on stable channels only (no beta/dev/canary/insider/staged early-stable), stored
    with the source URL. Nothing here is inferred by this script.
    """
    if not path:
        return None
    with open(path, "r", encoding="utf-8") as f:
        data = json.load(f)
    products = data.get("products") or {}
    return {"generated": data.get("generated"), "products": products}


def vendor_lookup(vendor, sig, product_name):
    if not vendor:
        return None
    products = vendor["products"]
    return products.get(str(sig)) or (products.get(product_name) if product_name else None)


# --- row assembly ------------------------------------------------------------------------------

def build_rows(server_dir, tracked, vendor, today):
    sig_index, _pid_index = cat.load_products(server_dir)
    assoc, assoc_titles = load_associations(server_dir)
    agg = load_aggregation(server_dir)
    agg_v2 = load_aggregation_v2(server_dir)

    if tracked is None:
        tracked = sorted(assoc)

    rows = []
    for sig in tracked:
        product = sig_index.get(sig) or {}
        patch_id = assoc.get(sig)
        v1 = agg.get(patch_id, {}) if patch_id is not None else {}
        v2 = agg_v2.get(sig, {})

        name = (v1.get("product_name") or v2.get("name")
                or product.get("product_name") or assoc_titles.get(sig) or "(unknown)")

        release = parse_release_date(v1.get("release_date")) or parse_release_date(v2.get("release_date"))
        days = (today - release).days if release else None

        # support_3rd_party_patch separates "OPSWAT doesn't patch this" from a genuine gap.
        patchable = product.get("patchable")
        if patch_id is not None and (v1 or v2):
            status = "covered"
        elif patchable is False:
            status = "not-patched"
        else:
            status = "gap"

        v1_version, v2_version = v1.get("latest_version"), v2.get("version")
        drift = compare_versions(v1_version, v2_version)

        row = {
            "signature":    sig,
            "product":      name,
            "vendor":       product.get("vendor_name") or "",
            "patch_id":     patch_id,
            "version":      v1_version or v2_version or "",
            "v2_version":   v2_version or "",
            "release_date": release.isoformat() if release else "",
            "days":         days,
            "bucket":       age_bucket(days) if status == "covered" else "unknown",
            "source":       v2.get("data_source") or ("opswat" if v1 else "unknown"),
            "patchable":    patchable,
            "status":       status,
            "v1_v2_drift":  bool(drift) if drift is not None else False,
        }

        ga = vendor_lookup(vendor, sig, name)
        if ga:
            ga_release = parse_release_date(ga.get("release_date"))
            cmp_result = compare_versions(row["version"], ga.get("version"))
            row["ga_version"] = ga.get("version") or ""
            row["ga_release_date"] = ga_release.isoformat() if ga_release else ""
            row["ga_source_url"] = ga.get("source_url") or ""
            if cmp_result is None:
                row["ga_state"] = "unknown"
            elif cmp_result >= 0:
                row["ga_state"] = "current"
            else:
                row["ga_state"] = "behind"
            # Days the vendor's GA release has been out while the catalog is still on an older
            # version. 0 once the catalog has caught up, blank when either side is unknown.
            if row["ga_state"] == "current":
                row["ga_lag_days"] = 0
            elif row["ga_state"] == "behind" and ga_release:
                row["ga_lag_days"] = (today - ga_release).days
            else:
                row["ga_lag_days"] = None
        rows.append(row)

    rows.sort(key=lambda r: (r["product"].lower(), r["signature"]))
    return rows


def summarize(rows):
    covered = [r for r in rows if r["status"] == "covered"]
    ages = sorted(r["days"] for r in covered if r["days"] is not None)
    buckets = Counter(r["bucket"] for r in covered)
    sources = Counter(r["source"] for r in covered)
    median = ages[len(ages) // 2] if ages else None
    return {
        "tracked":     len(rows),
        "covered":     len(covered),
        "not_patched": sum(1 for r in rows if r["status"] == "not-patched"),
        "gaps":        sum(1 for r in rows if r["status"] == "gap"),
        "dated":       len(ages),
        "median_age":  median,
        "oldest":      ages[-1] if ages else None,
        "newest":      ages[0] if ages else None,
        "buckets":     dict(buckets),
        "sources":     dict(sources),
        "drift":       sum(1 for r in covered if r["v1_v2_drift"]),
        "behind":      sum(1 for r in rows if r.get("ga_state") == "behind"),
        "current":     sum(1 for r in rows if r.get("ga_state") == "current"),
    }


# --- page rendering ------------------------------------------------------------------------------

PAGE = """<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Third-Party Patch Freshness</title>
<style>
:root {
  --bg: #f6f7f9; --panel: #ffffff; --ink: #14181f; --muted: #5c6673; --line: #e2e6ec;
  --accent: #1f5fd0; --ok: #17794a; --warn: #9a6208; --bad: #b3261e; --chip: #eef1f6;
}
@media (prefers-color-scheme: dark) {
  :root:not([data-theme="light"]) {
    --bg: #10141a; --panel: #171c24; --ink: #e7ecf3; --muted: #9aa6b5; --line: #262e39;
    --accent: #6ea8fe; --ok: #4ec98a; --warn: #e0a458; --bad: #f08a82; --chip: #202833;
  }
}
:root[data-theme="dark"] {
  --bg: #10141a; --panel: #171c24; --ink: #e7ecf3; --muted: #9aa6b5; --line: #262e39;
  --accent: #6ea8fe; --ok: #4ec98a; --warn: #e0a458; --bad: #f08a82; --chip: #202833;
}
* { box-sizing: border-box; }
body {
  margin: 0; background: var(--bg); color: var(--ink);
  font: 15px/1.55 -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
}
.wrap { max-width: 1180px; margin: 0 auto; padding: 32px 20px 64px; }
header h1 { font-size: 26px; margin: 0 0 6px; letter-spacing: -0.01em; }
header p.sub { margin: 0; color: var(--muted); font-size: 14px; }
.cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 12px; margin: 24px 0; }
.card { background: var(--panel); border: 1px solid var(--line); border-radius: 10px; padding: 14px 16px; }
.card .n { font-size: 24px; font-weight: 650; letter-spacing: -0.02em; }
.card .l { font-size: 12px; color: var(--muted); text-transform: uppercase; letter-spacing: .04em; margin-top: 2px; }
.panel { background: var(--panel); border: 1px solid var(--line); border-radius: 10px; padding: 18px 20px; margin: 20px 0; }
.panel h2 { font-size: 16px; margin: 0 0 12px; }
.bars { display: grid; gap: 8px; }
.bar { display: grid; grid-template-columns: 110px 1fr 60px; align-items: center; gap: 10px; font-size: 13px; }
.bar .track { display: block; background: var(--chip); border-radius: 5px; height: 10px; overflow: hidden; }
.bar .fill { display: block; height: 100%; min-width: 2px; background: var(--accent); }
.bar .fill.ok { background: var(--ok); } .bar .fill.warn { background: var(--warn); } .bar .fill.bad { background: var(--bad); }
.bar .v { text-align: right; color: var(--muted); font-variant-numeric: tabular-nums; }
.controls { display: flex; flex-wrap: wrap; gap: 10px; align-items: center; margin: 20px 0 10px; }
input[type=search], select {
  font: inherit; font-size: 14px; padding: 8px 10px; border-radius: 8px;
  border: 1px solid var(--line); background: var(--panel); color: var(--ink);
}
input[type=search] { flex: 1 1 260px; min-width: 200px; }
.count { color: var(--muted); font-size: 13px; margin-left: auto; }
.tablewrap { overflow-x: auto; border: 1px solid var(--line); border-radius: 10px; background: var(--panel); }
table { border-collapse: collapse; width: 100%; font-size: 13.5px; }
th, td { text-align: left; padding: 9px 12px; border-bottom: 1px solid var(--line); white-space: nowrap; }
th { position: sticky; top: 0; background: var(--panel); font-size: 12px; text-transform: uppercase;
     letter-spacing: .04em; color: var(--muted); cursor: pointer; user-select: none; z-index: 1; }
th::after { content: ""; }
th.asc::after { content: " ▲"; } th.desc::after { content: " ▼"; }
tbody tr:hover { background: var(--chip); }
td.prod { white-space: normal; min-width: 220px; font-weight: 520; }
td.num { text-align: right; font-variant-numeric: tabular-nums; }
td.ver { font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace; font-size: 12.5px; }
.tag { display: inline-block; font-size: 11.5px; padding: 2px 7px; border-radius: 999px; background: var(--chip); color: var(--muted); }
.tag.ok { color: var(--ok); } .tag.warn { color: var(--warn); } .tag.bad { color: var(--bad); }
.age { font-variant-numeric: tabular-nums; }
.age.b0 { color: var(--ok); } .age.b1 { color: var(--ink); }
.age.b2 { color: var(--warn); } .age.b3 { color: var(--bad); }
.method dl { margin: 0; }
.method dt { font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace; font-size: 13px;
             color: var(--accent); margin-top: 12px; }
.method dd { margin: 4px 0 0; color: var(--muted); font-size: 14px; }
.method code { font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
               background: var(--chip); padding: 1px 5px; border-radius: 4px; font-size: 12.5px; color: var(--ink); }
.notread { display: flex; flex-wrap: wrap; gap: 6px; margin-top: 8px; }
.notread span { font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
                font-size: 12px; background: var(--chip); color: var(--muted);
                padding: 3px 8px; border-radius: 6px; text-decoration: line-through; }
footer { margin-top: 28px; color: var(--muted); font-size: 12.5px; }
.note { border-left: 3px solid var(--accent); padding: 2px 0 2px 12px; color: var(--muted); font-size: 14px; }
</style>
</head>
<body>
<div class="wrap">

<header>
  <h1>Third-Party Patch Freshness</h1>
  <p class="sub">Built from the __SNAPSHOT__ catalog snapshot &middot; __TRACKED__ tracked OESIS v4 signatures &middot; generated __GENERATED__</p>
</header>

<div class="cards">
  <div class="card"><div class="n">__COVERED__</div><div class="l">With patch record</div></div>
  <div class="card"><div class="n">__NOTPATCHED__</div><div class="l">Not patched by OPSWAT</div></div>
  <div class="card"><div class="n">__GAPS__</div><div class="l">No record (gap)</div></div>
  <div class="card"><div class="n">__MEDIAN__</div><div class="l">Median patch age</div></div>
  <div class="card"><div class="n">__OLDEST__</div><div class="l">Oldest patch</div></div>
</div>

<div class="panel">
  <h2>How recently was each patch released?</h2>
  <div class="bars">__BUCKETS__</div>
</div>

__VENDORPANEL__

<div class="controls">
  <input type="search" id="q" placeholder="Filter by product, vendor, signature or version…">
  <select id="fsource">
    <option value="">All sources</option>
    <option value="opswat">OPSWAT-verified</option>
    <option value="winget">winget</option>
  </select>
  <select id="fbucket">
    <option value="">All ages</option>
    <option value="0-7">Updated in last 7 days</option>
    <option value="8-30">8–30 days</option>
    <option value="31-90">31–90 days</option>
    <option value="90+">Over 90 days</option>
  </select>
  <select id="fstatus">
    <option value="">All records</option>
    <option value="covered">With patch record</option>
    <option value="not-patched">Not patched by OPSWAT</option>
    <option value="gap">Gaps only</option>
  </select>
  <span class="count" id="count"></span>
</div>

<div class="tablewrap">
  <table id="t">
    <thead><tr>__HEAD__</tr></thead>
    <tbody></tbody>
  </table>
</div>

<div class="panel method">
  <h2>How this page is built</h2>
  <p class="note">Three files are read from the snapshot, and only specific fields from each. The
  comparison is verbatim field reads plus deterministic version arithmetic — no interpretation.</p>
  <dl>
    <dt>patch_associations.json</dt>
    <dd>The tracked OESIS v4 signature ID is matched against <code>v4_signatures</code>. The entry
        flagged <code>is_latest</code> is taken, and its <code>patch_id</code> read.</dd>
    <dt>patch_aggregation.json / patch_aggregation_v2.json</dt>
    <dd>That patch is looked up to read <code>product_name</code>, <code>latest_version</code>
        (<code>version</code> in v2) and <code>release_date</code>. In v2 the signature mapping is
        inline on <code>product.v4_signatures</code>.</dd>
    <dt>products.json</dt>
    <dd><code>support_3rd_party_patch</code> distinguishes “OPSWAT doesn't patch this product” from
        “no record could be found”, so the former is never reported as a gap.</dd>
  </dl>
  <p style="margin:16px 0 0"><strong>Present in the snapshot but not opened by this routine:</strong></p>
  <div class="notread">__NOTREAD__</div>
  <p style="margin:16px 0 0" class="note">The one AI-assisted step is entirely on the vendor side: a
  daily lookup of each vendor's current broadly-available GA release and its official release date,
  restricted to stable channels — no beta, dev, canary, insider or staged early-stable rollouts —
  stored with the source URL.</p>
</div>

<footer>Catalog snapshot header time: __HEADERTIME__ &middot; ages measured against __TODAY__.</footer>
</div>

<script>
const ROWS = __DATA__;
const HAS_GA = __HASGA__;
const tbody = document.querySelector('#t tbody');
const q = document.getElementById('q');
const fsource = document.getElementById('fsource');
const fbucket = document.getElementById('fbucket');
const fstatus = document.getElementById('fstatus');
const countEl = document.getElementById('count');
let sortKey = 'product', sortDir = 1;

const BUCKET_CLASS = {'0-7':'b0','8-30':'b1','31-90':'b2','90+':'b3'};

function esc(s) {
  return String(s == null ? '' : s).replace(/[&<>"]/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[c]));
}

function statusTag(r) {
  if (r.status === 'covered') return '<span class="tag ok">covered</span>';
  if (r.status === 'not-patched') return '<span class="tag">not patched</span>';
  return '<span class="tag bad">gap</span>';
}

function render() {
  const term = q.value.trim().toLowerCase();
  const src = fsource.value, bkt = fbucket.value, st = fstatus.value;
  const shown = ROWS.filter(r => {
    if (src && r.source !== src) return false;
    if (bkt && r.bucket !== bkt) return false;
    if (st && r.status !== st) return false;
    if (!term) return true;
    return (r.product + ' ' + r.vendor + ' ' + r.signature + ' ' + r.version).toLowerCase().includes(term);
  });
  shown.sort((a, b) => {
    let x = a[sortKey], y = b[sortKey];
    if (x == null) x = sortKey === 'days' ? Infinity : '';
    if (y == null) y = sortKey === 'days' ? Infinity : '';
    if (typeof x === 'string' || typeof y === 'string') {
      x = String(x).toLowerCase(); y = String(y).toLowerCase();
    }
    return (x > y ? 1 : x < y ? -1 : 0) * sortDir;
  });
  tbody.innerHTML = shown.map(r => {
    let cells =
      '<td class="prod">' + esc(r.product) + (r.vendor ? '<br><span class="tag">' + esc(r.vendor) + '</span>' : '') + '</td>' +
      '<td class="num">' + esc(r.signature) + '</td>' +
      '<td class="num">' + esc(r.patch_id == null ? '—' : r.patch_id) + '</td>' +
      '<td class="ver">' + esc(r.version || '—') + '</td>' +
      '<td>' + esc(r.release_date || '—') + '</td>' +
      '<td class="num age ' + (BUCKET_CLASS[r.bucket] || '') + '">' + (r.days == null ? '—' : r.days) + '</td>' +
      '<td><span class="tag">' + esc(r.source) + '</span></td>';
    if (HAS_GA) {
      const state = r.ga_state === 'current' ? '<span class="tag ok">current</span>'
                  : r.ga_state === 'behind' ? '<span class="tag bad">behind</span>'
                  : '<span class="tag">—</span>';
      const ver = r.ga_source_url
        ? '<a href="' + esc(r.ga_source_url) + '" target="_blank" rel="noreferrer noopener">' + esc(r.ga_version || '—') + '</a>'
        : esc(r.ga_version || '—');
      cells += '<td class="ver">' + ver + '</td>' +
               '<td>' + esc(r.ga_release_date || '—') + '</td>' +
               '<td class="num">' + (r.ga_lag_days == null ? '—' : r.ga_lag_days) + '</td>' +
               '<td>' + state + '</td>';
    }
    cells += '<td>' + statusTag(r) + '</td>';
    return '<tr>' + cells + '</tr>';
  }).join('');
  countEl.textContent = shown.length + ' of ' + ROWS.length + ' signatures';
}

document.querySelectorAll('#t th').forEach(th => {
  th.addEventListener('click', () => {
    const key = th.dataset.key;
    if (!key) return;
    if (sortKey === key) { sortDir = -sortDir; } else { sortKey = key; sortDir = 1; }
    document.querySelectorAll('#t th').forEach(o => o.classList.remove('asc', 'desc'));
    th.classList.add(sortDir === 1 ? 'asc' : 'desc');
    render();
  });
});
[q, fsource, fbucket, fstatus].forEach(el => el.addEventListener('input', render));
render();
</script>
</body>
</html>
"""


def render_buckets(summary):
    order = [("0-7", "0–7 days", "ok"), ("8-30", "8–30 days", "ok"),
             ("31-90", "31–90 days", "warn"), ("90+", "over 90 days", "bad"),
             ("unknown", "no date", "")]
    total = max(sum(summary["buckets"].values()), 1)
    out = []
    for key, label, cls in order:
        n = summary["buckets"].get(key, 0)
        pct = round(n * 100.0 / total, 1)
        out.append(
            f'<div class="bar"><span>{label}</span>'
            f'<span class="track"><span class="fill {cls}" style="width:{pct}%"></span></span>'
            f'<span class="v">{n}</span></div>'
        )
    return "\n".join(out)


def render_vendor_panel(vendor, summary, rows):
    if not vendor:
        return (
            '<div class="panel"><h2>Vendor GA comparison</h2>'
            '<p class="note">No vendor GA dataset was supplied, so the vendor-side columns are '
            'omitted. Pass <code>--vendor-ga vendor_ga.json</code> to add each vendor&rsquo;s current '
            'broadly-available GA version and official release date (stable channels only, stored '
            'with its source URL). This page fabricates nothing on the vendor side.</p></div>'
        )
    compared = summary["behind"] + summary["current"]
    lags = sorted(r["ga_lag_days"] for r in rows
                  if r.get("ga_state") == "behind" and r.get("ga_lag_days") is not None)
    median_lag = lags[len(lags) // 2] if lags else None
    generated = html.escape(str(vendor.get("generated") or "date not recorded"))
    return (
        '<div class="panel"><h2>Vendor GA comparison</h2>'
        f'<div class="cards" style="margin:0">'
        f'<div class="card"><div class="n">{summary["current"]}</div><div class="l">At vendor GA</div></div>'
        f'<div class="card"><div class="n">{summary["behind"]}</div><div class="l">Behind vendor GA</div></div>'
        f'<div class="card"><div class="n">{compared}</div><div class="l">Compared</div></div>'
        f'<div class="card"><div class="n">{"—" if median_lag is None else str(median_lag) + "d"}</div>'
        f'<div class="l">Median days behind</div></div></div>'
        f'<p class="note" style="margin-top:14px">Vendor dataset generated {generated}. '
        'Stable channels only — no beta, dev, canary, insider or staged early-stable rollouts.</p></div>'
    )


def render_page(rows, summary, vendor, snapshot, header_time, today):
    columns = [
        ("product", "Product"), ("signature", "Sig"), ("patch_id", "Patch"),
        ("version", "Catalog version"), ("release_date", "Released"), ("days", "Age (d)"),
        ("source", "Source"),
    ]
    if vendor:
        columns += [("ga_version", "Vendor GA"), ("ga_release_date", "GA released"),
                    ("ga_lag_days", "Behind (d)"), ("ga_state", "vs GA")]
    columns += [("status", "Status")]
    head = "".join(f'<th data-key="{key}">{html.escape(label)}</th>' for key, label in columns)

    def fmt_days(value):
        return "—" if value is None else f"{value}d"

    page = PAGE
    replacements = {
        "__SNAPSHOT__":    html.escape(snapshot or "unknown"),
        "__GENERATED__":   today.isoformat(),
        "__TODAY__":       today.isoformat(),
        "__HEADERTIME__":  html.escape(str(header_time or "unknown")),
        "__TRACKED__":     str(summary["tracked"]),
        "__COVERED__":     str(summary["covered"]),
        "__NOTPATCHED__":  str(summary["not_patched"]),
        "__GAPS__":        str(summary["gaps"]),
        "__MEDIAN__":      fmt_days(summary["median_age"]),
        "__OLDEST__":      fmt_days(summary["oldest"]),
        "__BUCKETS__":     render_buckets(summary),
        "__VENDORPANEL__": render_vendor_panel(vendor, summary, rows),
        "__HEAD__":        head,
        "__NOTREAD__":     "".join(f"<span>{html.escape(n)}</span>" for n in NOT_READ),
        "__DATA__":        json.dumps(rows, ensure_ascii=False),
        "__HASGA__":       "true" if vendor else "false",
    }
    for token, value in replacements.items():
        page = page.replace(token, value)
    return page


def read_signature_list(path):
    """Tracked signature ids, one per line or comma separated; '#' starts a comment."""
    text = open(path, "r", encoding="utf-8").read()
    text = re.sub(r"#.*", "", text)
    ids = [int(tok) for tok in re.findall(r"\d+", text)]
    if not ids:
        print(f"ERROR: no signature ids found in {path}.")
        sys.exit(2)
    return sorted(set(ids))


def main():
    parser = argparse.ArgumentParser(
        description="Build a third-party patch freshness page from the Analog catalog snapshot.")
    parser.add_argument("--signatures", help="file of tracked OESIS v4 signature ids "
                                             "(default: every signature with a latest patch association)")
    parser.add_argument("--vendor-ga", help="optional JSON of vendor GA versions/dates")
    parser.add_argument("--out", default="patch-freshness.html", help="output HTML path")
    args = parser.parse_args()

    server = cat.require_server_dir()
    snapshot, header_time = snapshot_stamp(server)
    today = datetime.now(tz=timezone.utc).date()

    tracked = read_signature_list(args.signatures) if args.signatures else None
    vendor = load_vendor_ga(args.vendor_ga)

    rows = build_rows(server, tracked, vendor, today)
    summary = summarize(rows)
    page = render_page(rows, summary, vendor, snapshot, header_time, today)

    out_path = os.path.abspath(args.out)
    with open(out_path, "w", encoding="utf-8") as f:
        f.write(page)

    print(f"Catalog snapshot : {snapshot} ({header_time})")
    print(f"Tracked signatures : {summary['tracked']}")
    print(f"  with patch record   : {summary['covered']}")
    print(f"  not patched by OPSWAT: {summary['not_patched']}")
    print(f"  gaps (no record)     : {summary['gaps']}")
    if summary["median_age"] is not None:
        print(f"  median patch age     : {summary['median_age']} days "
              f"(newest {summary['newest']}d, oldest {summary['oldest']}d)")
    for src, n in sorted(summary["sources"].items()):
        print(f"  source {src:<8}      : {n}")
    if vendor:
        print(f"  at vendor GA         : {summary['current']}")
        print(f"  behind vendor GA     : {summary['behind']}")
    print(f"\nWrote {out_path}")


if __name__ == "__main__":
    main()
