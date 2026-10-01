#!/usr/bin/env python3
###############################################################################################
##  Catalog Lookup — find-os-vulns
##
##  What is a specific Windows build vulnerable to? Given an OS (id or name) and the installed
##  build (e.g. 17763.4974 from "10.0.17763.4974"), reports every catalog build NEWER than the
##  installed one, the KB that ships it, and the CVEs those missing KBs remediate.
##
##  Why a build delta: no dataset is keyed "OS version -> CVE list". The catalog's own
##  get_system_vuln query (sample_code/get_system_vuln.rb) is keyed by KB ARTICLE + os_id and
##  returns the CVEs that one KB fixes — OPSWAT labels that path "Not recommended" for Windows.
##  kb_info.json, however, maps build -> KB articles and KB -> CVEs, so the CVEs a build is still
##  exposed to are those fixed by KBs in builds above it. That is a deterministic set read from
##  the catalog, not an inference.
##
##  Usage:
##      python3 find-os-vulns.py 73 17763.4974              # Windows Server 2019 Standard
##      python3 find-os-vulns.py "server 2019" 17763.4974
##      python3 find-os-vulns.py 73 10.0.17763.4974         # leading 10.0. is ignored
##      python3 find-os-vulns.py 73 17763.4974 --list-cves
##      python3 find-os-vulns.py 73 17763.4974 --details    # severity/CVSS from cves.json (slow)
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO and Brent Beachem - Director of Products
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


def build_key(build):
    """'17763.4974' -> (17763, 4974) so builds sort numerically, not lexically."""
    return tuple(int(part) for part in re.findall(r"\d+", str(build)))


def normalize_build(value):
    """Accept '10.0.17763.4974', '17763.4974' or '17763' -> the catalog's build key form.

    kb_info.json keys builds as '<build>.<revision>', so a leading Windows major.minor
    ('10.0.') is dropped when present.
    """
    parts = [int(p) for p in re.findall(r"\d+", str(value))]
    if not parts:
        return None
    if len(parts) >= 3 and parts[0] == 10 and parts[1] == 0:
        parts = parts[2:]
    return tuple(parts)


def load_kb_info(server):
    """Returns (id_os_map, sections, snapshot_date) from kb_info.json."""
    path = os.path.join(server, "kb_info.json")
    if not os.path.isfile(path):
        print(f"ERROR: kb_info.json not found in {server}.")
        print("       It carries the build -> KB -> CVE data this lookup is built on.")
        print("       Run the SDK downloader again so the catalog is extracted in full.")
        sys.exit(2)
    data = cat.load_json(path)
    id_os_map, sections, snapshot = {}, {}, None
    for element in data.get("oesis", []):
        for key, value in element.items():
            if key == "header":
                if isinstance(value, dict) and value.get("timestamp"):
                    snapshot = cat.fmt_epoch(value["timestamp"])
                continue
            if key == "id_os_map" and isinstance(value, dict):
                for k, v in value.items():
                    try:
                        id_os_map[int(k)] = str(v)
                    except (TypeError, ValueError):
                        pass
            elif isinstance(value, dict):
                sections[str(key)] = value
    return id_os_map, sections, snapshot


def resolve_os(target, id_os_map, os_names):
    """Accept an os_id or a name substring -> (os_id, os_name, section_label)."""
    if str(target).isdigit():
        os_id = int(target)
        if os_id not in id_os_map:
            return None, None, None
        return os_id, os_names.get(os_id, ""), id_os_map[os_id]

    needle = str(target).lower()
    matches = [(oid, os_names.get(oid, "")) for oid in id_os_map
               if needle in (os_names.get(oid, "") or "").lower()]
    if not matches:
        return None, None, None
    if len({id_os_map[oid] for oid, _ in matches}) > 1:
        print(f"'{target}' is ambiguous — matches more than one OS family:")
        for oid, name in sorted(matches):
            print(f"  os_id {oid:<4} {name}   [{id_os_map[oid]}]")
        print("Re-run with a specific os_id.")
        sys.exit(1)
    os_id, name = sorted(matches)[0]
    return os_id, name, id_os_map[os_id]


def cve_details(server, wanted):
    """CVE id -> {severity, score} for the wanted set, read from the large cves.json.

    Returns None when cves.json is absent, so callers can say "no severity source" rather than
    reporting every CVE as unrated — which would read as a rating instead of a missing file.
    """
    path = os.path.join(server, "cves.json")
    if not os.path.isfile(path):
        return None
    out = {}
    data = cat.load_json(path)
    for element in data.get("oesis", []):
        cves = element.get("cves")
        if not isinstance(cves, dict):
            continue
        for cve_id, rec in cves.items():
            if cve_id not in wanted:
                continue
            cvss3 = rec.get("cvss_3_0") or {}
            out[cve_id] = {
                "severity": rec.get("severity") or "",
                "score":    cvss3.get("base_score") or cvss3.get("impact_score"),
            }
    return out


PAGE = """<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>OS Vulnerability Exposure — __OSNAME__ __BUILD__</title>
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
body { margin: 0; background: var(--bg); color: var(--ink);
  font: 15px/1.55 -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif; }
.wrap { max-width: 1180px; margin: 0 auto; padding: 32px 20px 64px; }
header h1 { font-size: 26px; margin: 0 0 6px; letter-spacing: -0.01em; }
header p.sub { margin: 0; color: var(--muted); font-size: 14px; }
.cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 12px; margin: 24px 0; }
.card { background: var(--panel); border: 1px solid var(--line); border-radius: 10px; padding: 14px 16px; }
.card .n { font-size: 24px; font-weight: 650; letter-spacing: -0.02em; }
.card .n.bad { color: var(--bad); }
.card .l { font-size: 12px; color: var(--muted); text-transform: uppercase; letter-spacing: .04em; margin-top: 2px; }
.panel { background: var(--panel); border: 1px solid var(--line); border-radius: 10px; padding: 18px 20px; margin: 20px 0; }
.panel h2 { font-size: 16px; margin: 0 0 12px; }
.grid2 { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 20px; }
.bars { display: grid; gap: 8px; }
.bar { display: grid; grid-template-columns: 96px 1fr 58px; align-items: center; gap: 10px; font-size: 13px; }
.bar .track { display: block; background: var(--chip); border-radius: 5px; height: 10px; overflow: hidden; }
.bar .fill { display: block; height: 100%; min-width: 2px; background: var(--accent); }
.bar .fill.ok { background: var(--ok); } .bar .fill.warn { background: var(--warn); }
.bar .fill.bad { background: var(--bad); }
.bar .v { text-align: right; color: var(--muted); font-variant-numeric: tabular-nums; }
.controls { display: flex; flex-wrap: wrap; gap: 10px; align-items: center; margin: 20px 0 10px; }
input[type=search], select { font: inherit; font-size: 14px; padding: 8px 10px; border-radius: 8px;
  border: 1px solid var(--line); background: var(--panel); color: var(--ink); }
input[type=search] { flex: 1 1 260px; min-width: 200px; }
.count { color: var(--muted); font-size: 13px; margin-left: auto; }
.tablewrap { overflow-x: auto; border: 1px solid var(--line); border-radius: 10px; background: var(--panel);
  max-height: 560px; overflow-y: auto; }
table { border-collapse: collapse; width: 100%; font-size: 13.5px; }
th, td { text-align: left; padding: 8px 12px; border-bottom: 1px solid var(--line); white-space: nowrap; }
th { position: sticky; top: 0; background: var(--panel); font-size: 12px; text-transform: uppercase;
  letter-spacing: .04em; color: var(--muted); cursor: pointer; user-select: none; z-index: 1; }
th.asc::after { content: " ▲"; } th.desc::after { content: " ▼"; }
tbody tr:hover { background: var(--chip); }
td.num { text-align: right; font-variant-numeric: tabular-nums; }
td.mono { font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace; font-size: 12.5px; }
a { color: var(--accent); }
.tag { display: inline-block; font-size: 11.5px; padding: 2px 7px; border-radius: 999px; background: var(--chip); color: var(--muted); }
.tag.ok { color: var(--ok); } .tag.warn { color: var(--warn); } .tag.bad { color: var(--bad); }
.note { border-left: 3px solid var(--accent); padding: 2px 0 2px 12px; color: var(--muted); font-size: 14px; }
.note.bad { border-color: var(--bad); }
.method dt { font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace; font-size: 13px;
  color: var(--accent); margin-top: 12px; }
.method dd { margin: 4px 0 0; color: var(--muted); font-size: 14px; }
code { font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace; background: var(--chip);
  padding: 1px 5px; border-radius: 4px; font-size: 12.5px; color: var(--ink); }
footer { margin-top: 28px; color: var(--muted); font-size: 12.5px; }
</style>
</head>
<body>
<div class="wrap">

<header>
  <h1>OS Vulnerability Exposure</h1>
  <p class="sub"><strong>__OSNAME__</strong> (os_id __OSID__) &middot; build <strong>__BUILD__</strong>
  __SHIPPEDBY__ &middot; catalog snapshot __SNAPSHOT__</p>
</header>

<div class="cards">
  <div class="card"><div class="n bad">__NKB__</div><div class="l">Missing KBs</div></div>
  <div class="card"><div class="n bad">__NCVE__</div><div class="l">Distinct CVEs</div></div>
  <div class="card"><div class="n">__BEHIND__</div><div class="l">Behind by</div></div>
  <div class="card"><div class="n">__NEWEST__</div><div class="l">Newest build available</div></div>
  __SEVCARD__
</div>

__SEVNOTE__

<div class="panel grid2">
  <div>
    <h2>CVEs by year disclosed</h2>
    <div class="bars">__YEARBARS__</div>
  </div>
  <div>
    <h2>__SEVTITLE__</h2>
    <div class="bars">__SEVBARS__</div>
  </div>
</div>

<div class="panel">
  <h2>Missing updates &mdash; __NKB__ KB(s)</h2>
  <p class="note">Every catalog build above __BUILD__, oldest first. A KB fixing 0 CVEs is a
  servicing/quality update rather than a security rollup.</p>
  <div class="tablewrap" style="margin-top:12px">
    <table id="kbt">
      <thead><tr>
        <th data-key="build">Build</th><th data-key="kb">KB</th>
        <th data-key="available">Available</th><th data-key="cves">CVEs fixed</th>
      </tr></thead>
      <tbody></tbody>
    </table>
  </div>
</div>

<div class="panel">
  <h2>Exposed CVEs</h2>
  <div class="controls">
    <input type="search" id="q" placeholder="Filter by CVE id, KB, build or year…">
    <select id="fyear"><option value="">All years</option>__YEAROPTS__</select>
    __SEVSELECT__
    <span class="count" id="count"></span>
  </div>
  <div class="tablewrap">
    <table id="cvet">
      <thead><tr>__CVEHEAD__</tr></thead>
      <tbody></tbody>
    </table>
  </div>
</div>

<div class="panel method">
  <h2>How this is calculated</h2>
  <p class="note">Nothing in the catalog is keyed &ldquo;OS version &rarr; CVE list&rdquo;. This is a
  missing-patch delta read verbatim from <code>kb_info.json</code> &mdash; no inference.</p>
  <dl>
    <dt>kb_info.json &rarr; kb_base</dt>
    <dd>Builds are compared numerically. Every build above the installed one is missing, and each
        carries the <code>kb_articles</code> that ship it, with an <code>availability_date</code>.</dd>
    <dt>kb_info.json &rarr; kb_cves</dt>
    <dd>Each missing KB maps to the CVEs it remediates. The union across all missing KBs is the
        exposure set. A CVE is attributed to the <em>earliest</em> KB that fixes it.</dd>
    __SEVMETHOD__
  </dl>
  <p class="note bad" style="margin-top:16px">This is a catalog-side patch delta, not a live
  assessment. It cannot see mitigations, configuration, or anything outside the OS. For production
  Windows assessment OPSWAT recommends <strong>WIV.dat + WUO.dat</strong> at runtime rather than the
  KB-article query in <code>sample_code/get_system_vuln.rb</code>, which that doc marks
  &ldquo;Not recommended&rdquo; for Windows.</p>
</div>

<footer>Generated __GENERATED__ by find-os-vulns.py from the __SNAPSHOT__ Analog catalog snapshot.
__OSNOTE__</footer>
</div>

<script>
const KBS = __KBDATA__;
const CVES = __CVEDATA__;
const HAS_SEV = __HASSEV__;
const SEV_CLASS = {critical: 'bad', important: 'warn', moderate: '', low: 'ok', unrated: ''};

function esc(s) {
  return String(s == null ? '' : s).replace(/[&<>"]/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[c]));
}

// --- missing KB table ---
let kbSort = 'build', kbDir = 1;
const kbBody = document.querySelector('#kbt tbody');
function renderKbs() {
  const rows = KBS.slice().sort((a, b) => {
    let x = a[kbSort], y = b[kbSort];
    if (kbSort === 'build') { x = a.sortkey; y = b.sortkey; }
    if (typeof x === 'string' || typeof y === 'string') { x = String(x).toLowerCase(); y = String(y).toLowerCase(); }
    return (x > y ? 1 : x < y ? -1 : 0) * kbDir;
  });
  kbBody.innerHTML = rows.map(r =>
    '<tr><td class="mono">' + esc(r.build) + '</td>' +
    '<td class="mono"><a href="https://support.microsoft.com/help/' + esc(r.kb) +
      '" target="_blank" rel="noreferrer noopener">KB' + esc(r.kb) + '</a></td>' +
    '<td>' + esc(r.available) + '</td>' +
    '<td class="num">' + (r.cves ? r.cves : '<span class="tag">none</span>') + '</td></tr>'
  ).join('');
}
document.querySelectorAll('#kbt th').forEach(th => th.addEventListener('click', () => {
  const k = th.dataset.key;
  if (kbSort === k) kbDir = -kbDir; else { kbSort = k; kbDir = 1; }
  document.querySelectorAll('#kbt th').forEach(o => o.classList.remove('asc','desc'));
  th.classList.add(kbDir === 1 ? 'asc' : 'desc');
  renderKbs();
}));

// --- CVE table ---
let cveSort = HAS_SEV ? 'score' : 'id', cveDir = HAS_SEV ? -1 : 1;
const cveBody = document.querySelector('#cvet tbody');
const q = document.getElementById('q');
const fyear = document.getElementById('fyear');
const fsev = document.getElementById('fsev');
const countEl = document.getElementById('count');

function renderCves() {
  const term = q.value.trim().toLowerCase();
  const year = fyear.value, sev = fsev ? fsev.value : '';
  const shown = CVES.filter(c => {
    if (year && c.year !== year) return false;
    if (sev && (c.severity || 'unrated').toLowerCase() !== sev) return false;
    if (!term) return true;
    // 'KB' prefix included so the string shown in the table is also searchable.
    return (c.id + ' KB' + c.kb + ' ' + c.build + ' ' + c.year).toLowerCase().includes(term);
  });
  shown.sort((a, b) => {
    let x = a[cveSort], y = b[cveSort];
    if (x == null) x = cveSort === 'score' ? -1 : '';
    if (y == null) y = cveSort === 'score' ? -1 : '';
    if (typeof x === 'string' || typeof y === 'string') { x = String(x).toLowerCase(); y = String(y).toLowerCase(); }
    return (x > y ? 1 : x < y ? -1 : 0) * cveDir;
  });
  cveBody.innerHTML = shown.map(c => {
    let cells = '<td class="mono"><a href="https://nvd.nist.gov/vuln/detail/' + esc(c.id) +
        '" target="_blank" rel="noreferrer noopener">' + esc(c.id) + '</a></td>';
    if (HAS_SEV) {
      const s = (c.severity || 'unrated').toLowerCase();
      cells += '<td><span class="tag ' + (SEV_CLASS[s] || '') + '">' + esc(s) + '</span></td>' +
               '<td class="num">' + (c.score == null ? '—' : c.score) + '</td>';
    }
    cells += '<td class="mono"><a href="https://support.microsoft.com/help/' + esc(c.kb) +
        '" target="_blank" rel="noreferrer noopener">KB' + esc(c.kb) + '</a></td>' +
      '<td class="mono">' + esc(c.build) + '</td><td>' + esc(c.available) + '</td>';
    return '<tr>' + cells + '</tr>';
  }).join('');
  countEl.textContent = shown.length.toLocaleString() + ' of ' + CVES.length.toLocaleString() + ' CVEs';
}
document.querySelectorAll('#cvet th').forEach(th => th.addEventListener('click', () => {
  const k = th.dataset.key;
  if (!k) return;
  if (cveSort === k) cveDir = -cveDir; else { cveSort = k; cveDir = 1; }
  document.querySelectorAll('#cvet th').forEach(o => o.classList.remove('asc','desc'));
  th.classList.add(cveDir === 1 ? 'asc' : 'desc');
  renderCves();
}));
[q, fyear, fsev].forEach(el => el && el.addEventListener('input', renderCves));

renderKbs();
renderCves();
</script>
</body>
</html>
"""


def _bars(counter, order=None, classes=None):
    """Render label/track/value bars for a Counter, widths relative to the largest value."""
    items = [(k, counter[k]) for k in (order or sorted(counter))] if (order or counter) else []
    items = [(k, v) for k, v in items if v]
    top = max([v for _, v in items] or [1])
    out = []
    for key, value in items:
        pct = round(value * 100.0 / top, 1)
        cls = (classes or {}).get(key, "")
        out.append(f'<div class="bar"><span>{html.escape(str(key))}</span>'
                   f'<span class="track"><span class="fill {cls}" style="width:{pct}%"></span></span>'
                   f'<span class="v">{value}</span></div>')
    return "\n".join(out) or '<p class="note">No data.</p>'


def render_html(ctx):
    """Build the self-contained summary page. ctx carries everything main() computed."""
    details, all_cves = ctx["details"], ctx["all_cves"]
    has_sev = bool(details)

    kb_data = [{"build": b, "sortkey": list(build_key(b)), "kb": k,
                "available": cat.fmt_epoch(a) or "unknown", "cves": n}
               for b, k, a, n in ctx["rows"]]

    cve_data = []
    for cve in sorted(all_cves):
        kb_id, build, avail = ctx["cve_first"].get(cve, ("", "", None))
        entry = {"id": cve, "year": cve.split("-")[1], "kb": kb_id, "build": build,
                 "available": cat.fmt_epoch(avail) or "unknown"}
        if has_sev:
            info = details.get(cve, {})
            entry["severity"] = (info.get("severity") or "unrated").lower()
            entry["score"] = info.get("score")
        cve_data.append(entry)

    years = Counter(c.split("-")[1] for c in all_cves)
    sev_counter = Counter(e.get("severity", "unrated") for e in cve_data) if has_sev else Counter()
    sev_order = [s for s in ("critical", "important", "moderate", "low", "unrated")
                 if sev_counter.get(s)]

    # Exposure window: installed build's availability date -> newest missing build's.
    behind = "unknown"
    newest_avail = ctx["rows"][-1][2] if ctx["rows"] else None
    if ctx["installed_avail"] and newest_avail:
        days = (newest_avail - ctx["installed_avail"]) // 86400
        behind = f"{days // 365}y {(days % 365) // 30}m" if days >= 365 else f"{days}d"

    if has_sev:
        high = sev_counter.get("critical", 0) + sev_counter.get("important", 0)
        sev_card = (f'<div class="card"><div class="n bad">{high}</div>'
                    f'<div class="l">Critical + important</div></div>')
        sev_note = ""
        sev_title = "CVEs by severity"
        sev_bars = _bars(sev_counter, sev_order,
                         {"critical": "bad", "important": "warn", "low": "ok"})
        sev_select = ('<select id="fsev"><option value="">All severities</option>'
                      + "".join(f'<option value="{s}">{s}</option>' for s in sev_order)
                      + "</select>")
        cve_head = ('<th data-key="id">CVE</th><th data-key="severity">Severity</th>'
                    '<th data-key="score">CVSS</th><th data-key="kb">Fixed by</th>'
                    '<th data-key="build">In build</th><th data-key="available">Available</th>')
        sev_method = ("<dt>cves.json</dt><dd><code>severity</code> and the CVSS 3.x "
                      "<code>base_score</code> for each exposed CVE.</dd>")
    else:
        sev_card = ""
        if ctx.get("sev_missing") == "no-file":
            # Ran with --details but the severity source was absent: say so, rather than
            # letting a blank column read as "these CVEs are unrated".
            sev_note = ('<div class="panel"><p class="note bad">Severity and CVSS are '
                        'unavailable &mdash; <code>cves.json</code> was not found in the catalog '
                        'directory. The CVEs listed below are still accurate; only their ratings '
                        'are missing. Re-run the SDK downloader to fetch it.</p></div>')
            sev_bars = ('<p class="note bad"><code>cves.json</code> not found &mdash; no ratings '
                        'to show.</p>')
        else:
            sev_note = ('<div class="panel"><p class="note">Severity and CVSS are omitted &mdash; '
                        'the page was generated without <code>--details</code>. Re-run with '
                        '<code>--details</code> to read them from <code>cves.json</code>.</p>'
                        '</div>')
            sev_bars = '<p class="note">Run with <code>--details</code> to populate.</p>'
        sev_title = "CVEs by severity"
        sev_select = ""
        cve_head = ('<th data-key="id">CVE</th><th data-key="kb">Fixed by</th>'
                    '<th data-key="build">In build</th><th data-key="available">Available</th>')
        sev_method = ""

    shipped = (f"&middot; shipped by {', '.join('KB' + k for k in ctx['installed_kbs'])}"
               if ctx["installed_kbs"] else "")
    os_note = ("" if ctx["exact_build"] else
               " Build not present verbatim in kb_info.json; compared numerically against the "
               "catalog build list.")

    replacements = {
        "__OSNAME__":   html.escape(ctx["os_name"] or ""),
        "__OSID__":     str(ctx["os_id"]),
        "__BUILD__":    html.escape(ctx["installed_str"]),
        "__SHIPPEDBY__": shipped,
        "__SNAPSHOT__": html.escape(ctx["snapshot"] or "unknown"),
        "__GENERATED__": datetime.now(tz=timezone.utc).date().isoformat(),
        "__NKB__":      str(len(ctx["rows"])),
        "__NCVE__":     f"{len(all_cves):,}",
        "__BEHIND__":   behind,
        "__NEWEST__":   html.escape(ctx["rows"][-1][0] if ctx["rows"] else "—"),
        "__SEVCARD__":  sev_card,
        "__SEVNOTE__":  sev_note,
        "__SEVTITLE__": sev_title,
        "__SEVBARS__":  sev_bars,
        "__SEVSELECT__": sev_select,
        "__SEVMETHOD__": sev_method,
        "__YEARBARS__": _bars(years, sorted(years)),
        "__YEAROPTS__": "".join(f'<option value="{y}">{y}</option>' for y in sorted(years)),
        "__CVEHEAD__":  cve_head,
        "__KBDATA__":   json.dumps(kb_data),
        "__CVEDATA__":  json.dumps(cve_data),
        "__HASSEV__":   "true" if has_sev else "false",
        "__OSNOTE__":   os_note,
    }
    page = PAGE
    for token, value in replacements.items():
        page = page.replace(token, value)
    return page


def main():
    parser = argparse.ArgumentParser(
        description="CVEs a specific Windows build is still exposed to, from the Analog catalog.")
    parser.add_argument("os", help="os_id (e.g. 73) or OS name substring (e.g. 'server 2019')")
    parser.add_argument("build", help="installed build, e.g. 17763.4974 or 10.0.17763.4974")
    parser.add_argument("--list-cves", action="store_true", help="print every CVE id")
    parser.add_argument("--details", action="store_true",
                        help="add severity/CVSS from cves.json (loads ~215 MB, slow)")
    parser.add_argument("--html", metavar="PATH",
                        help="also write a self-contained HTML summary to PATH")
    args = parser.parse_args()

    server = cat.require_server_dir()
    os_names = cat.load_os_names(server)
    id_os_map, sections, snapshot = load_kb_info(server)

    os_id, os_name, label = resolve_os(args.os, id_os_map, os_names)
    if not label:
        print(f"ERROR: no OS matched '{args.os}'. Known Windows OS ids in kb_info.json:")
        for oid in sorted(id_os_map):
            print(f"  {oid:<4} {os_names.get(oid, '')}   [{id_os_map[oid]}]")
        sys.exit(2)

    section = sections.get(label) or {}
    kb_base = section.get("kb_base") or {}
    kb_cves = section.get("kb_cves") or {}

    installed = normalize_build(args.build)
    if not installed:
        print(f"ERROR: could not parse build '{args.build}'.")
        sys.exit(2)
    installed_str = ".".join(str(p) for p in installed)

    print(f"OS        : {os_name} (os_id {os_id}, kb_info section '{label}')")
    print(f"Build     : {installed_str}")
    print("=" * 78)

    known = sorted(kb_base, key=build_key)
    if not known:
        print("No build data for this OS section.")
        sys.exit(0)

    newer = [b for b in known if build_key(b) > installed]

    if installed_str not in kb_base:
        print(f"  NOTE: build not in kb_info.json verbatim ({len(known)} builds known for this "
              f"OS).\n        Compared numerically against the catalog build list — the patch "
              f"delta below still holds.")

    installed_arts = (kb_base.get(installed_str) or {}).get("kb_articles", [])
    installed_kbs = [str(a.get("kb_id")) for a in installed_arts]
    installed_avail = next((a.get("availability_date") for a in installed_arts
                            if a.get("availability_date")), None)
    if installed_kbs:
        print(f"  Shipped by: {', '.join('KB' + k for k in installed_kbs)}")

    if not newer:
        print("\n  No newer build in this catalog snapshot — current as of the snapshot date.")
        sys.exit(0)

    # newer[] is build-ascending, so the first KB seen for a CVE is the earliest that fixes it.
    rows, all_cves, cve_first = [], set(), {}
    for build in newer:
        for art in (kb_base.get(build) or {}).get("kb_articles", []):
            kb_id = str(art.get("kb_id"))
            avail = art.get("availability_date")
            cves = {c for c in (cat.normalize_numeric_cve(x)
                                for x in ((kb_cves.get(kb_id) or {}).get("cves") or [])) if c}
            for cve in cves:
                cve_first.setdefault(cve, (kb_id, build, avail))
            all_cves |= cves
            rows.append((build, kb_id, avail, len(cves)))

    print(f"\n  Missing builds  : {len(newer)}   (newest in catalog: {newer[-1]})")
    print(f"  Missing KBs     : {len(rows)}")
    print(f"  Distinct CVEs   : {len(all_cves)}")

    print("\n  BUILD            KB           AVAILABLE     CVES FIXED")
    print("  " + "-" * 60)
    for build, kb_id, avail, count in rows:
        when = cat.fmt_epoch(avail) or "?"
        print(f"  {build:<16} KB{kb_id:<10} {when:<13} {count}")

    years = Counter(c.split("-")[1] for c in all_cves)
    print("\n  CVEs by year    : " + ", ".join(f"{y}: {n}" for y, n in sorted(years.items())))

    details, sev_missing = {}, "no-flag"
    if args.details:
        print("\n  Loading cves.json for severity (this takes a moment)...")
        loaded = cve_details(server, all_cves)
        if loaded is None:
            print(f"  WARNING: cves.json not found in {server} — severity and CVSS are")
            print("           unavailable. Every CVE above is still real; only the rating is")
            print("           missing. Re-run the SDK downloader to fetch it.")
            sev_missing = "no-file"
        else:
            details, sev_missing = loaded, None
            sev = Counter((details.get(c, {}).get("severity") or "unrated").lower()
                          for c in all_cves)
            print("  Severity        : " + ", ".join(f"{s}: {n}" for s, n in sev.most_common()))
            scored = [(details[c]["score"], c) for c in all_cves
                      if details.get(c, {}).get("score") is not None]
            if scored:
                print("  Highest scoring :")
                for score, cve in sorted(scored, reverse=True)[:10]:
                    print(f"     {cve:<18} CVSS {score}  {details[cve]['severity']}")
            undetailed = len(all_cves) - len(details)
            if undetailed:
                print(f"  ({undetailed} CVE(s) referenced by KBs are not detailed in cves.json "
                      f"and show as unrated)")

    if args.list_cves:
        print(f"\n  All {len(all_cves)} CVE(s):")
        for cve in sorted(all_cves):
            print(f"    {cve}")

    print("\n" + "=" * 78)
    print(f"RESULT: build {installed_str} is missing {len(rows)} KB(s) covering "
          f"{len(all_cves)} CVE(s) as of this catalog snapshot.")
    print("NOTE  : this is the missing-patch delta from kb_info.json. For production Windows")
    print("        assessment OPSWAT recommends WIV.dat + WUO.dat at runtime rather than the")
    print("        KB-article query in sample_code/get_system_vuln.rb.")

    if args.html:
        page = render_html({
            "os_name": os_name, "os_id": os_id, "installed_str": installed_str,
            "installed_kbs": installed_kbs, "installed_avail": installed_avail,
            "exact_build": installed_str in kb_base, "snapshot": snapshot,
            "rows": rows, "all_cves": all_cves, "cve_first": cve_first, "details": details,
            "sev_missing": sev_missing,
        })
        out_path = os.path.abspath(args.html)
        with open(out_path, "w", encoding="utf-8") as fh:
            fh.write(page)
        print(f"\nWrote {out_path}")


if __name__ == "__main__":
    main()
