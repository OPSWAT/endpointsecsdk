"""Compute CVE-coverage and 3rd-party-patch counts from moby.json and write them as HTML.

Usage: python moby_counts.py [path/to/moby.json] [--out moby-counts.html]
"""
import argparse
import html
import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))
DEFAULT = os.path.normpath(os.path.join(
    HERE, "..", "..", "OPSWAT-SDK", "extract", "compliance", "windows", "docs", "moby", "moby.json"))
PLATFORMS = ["windows", "mac", "linux"]

# (metric, unit, rule HTML, matching key in moby.json "counts" block or None)
ROWS = [
    ("CVE coverage", "signature",
     "sig has non-empty <code>vulnerability_versions</code>, in a row with "
     "<code>cve_detection = true</code>", None),
    ("CVE coverage", "product id",
     "row with <code>cve_detection = true</code> and &ge;1 sig with non-empty "
     "<code>vulnerability_versions</code>", None),
    ("3rd-party patch", "signature",
     "<code>support_auto_patching = true</code>", "support_auto_patching"),
    ("3rd-party patch", "product id",
     "row with &ge;1 sig <code>support_auto_patching = true</code>", "product_auto_patching"),
]


def compute(data):
    results = [dict.fromkeys(PLATFORMS, 0) for _ in ROWS]
    cve_sig, cve_prod, patch_sig, patch_prod = results

    # products: { name: { "WINDOWS"|"MAC"|"LINUX": {product_id, cve_detection, signatures: [...]}}}
    for product in data["products"].values():
        for plat_key, row in product.items():
            plat = plat_key.lower()
            if plat not in PLATFORMS:
                continue
            sigs = row.get("signatures", [])
            cve_sigs = sum(1 for s in sigs if s.get("vulnerability_versions"))
            patch_sigs = sum(1 for s in sigs if s.get("support_auto_patching"))

            if row.get("cve_detection"):
                cve_sig[plat] += cve_sigs
                if cve_sigs:
                    cve_prod[plat] += 1
            patch_sig[plat] += patch_sigs
            if patch_sigs:
                patch_prod[plat] += 1
    return results


def render_html(path, data, results):
    counts = data.get("counts", {})
    body = []
    for (metric, unit, rule, key), c in zip(ROWS, results):
        total = sum(c.values())
        check = ""
        if key in counts:
            ok = counts[key]["platforms"] == c and counts[key]["total"] == total
            check = ('<span class="ok">matches counts.%s</span>' if ok
                     else '<span class="bad">differs from counts.%s</span>') % key
        cells = "".join(f'<td class="num">{c[p]:,}</td>' for p in PLATFORMS)
        body.append(f"<tr><td>{metric}</td><td>{unit}</td><td>{rule}<div>{check}</div></td>"
                    f'{cells}<td class="num total">{total:,}</td></tr>')

    return f"""<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Moby Coverage Counts</title>
<style>
:root {{
  --bg: #f6f7f9; --panel: #ffffff; --ink: #14181f; --muted: #5c6673; --line: #e2e6ec;
  --code: #b4233c; --code-bg: #f3f4f6; --ok: #1a7f37; --bad: #c2410c;
}}
@media (prefers-color-scheme: dark) {{
  :root:not([data-theme="light"]) {{
    --bg: #10141a; --panel: #171c24; --ink: #e7ecf3; --muted: #9aa6b5; --line: #262e39;
    --code: #f08a9a; --code-bg: #1f2630; --ok: #4ac26b; --bad: #f59e66;
  }}
}}
:root[data-theme="dark"] {{
  --bg: #10141a; --panel: #171c24; --ink: #e7ecf3; --muted: #9aa6b5; --line: #262e39;
  --code: #f08a9a; --code-bg: #1f2630; --ok: #4ac26b; --bad: #f59e66;
}}
body {{ margin: 0; background: var(--bg); color: var(--ink);
  font: 15px/1.5 system-ui, -apple-system, "Segoe UI", sans-serif; }}
main {{ max-width: 1000px; margin: 0 auto; padding: 32px 16px; }}
h1 {{ font-size: 22px; margin: 0 0 4px; }}
.meta {{ color: var(--muted); font-size: 13px; margin-bottom: 20px; word-break: break-all; }}
.wrap {{ overflow-x: auto; background: var(--panel); border: 1px solid var(--line); border-radius: 8px; }}
table {{ border-collapse: collapse; width: 100%; }}
th, td {{ padding: 12px 14px; border-bottom: 1px solid var(--line); text-align: left; vertical-align: top; }}
tr:last-child td {{ border-bottom: 0; }}
th {{ font-size: 13px; color: var(--muted); font-weight: 600; }}
.num {{ text-align: right; font-variant-numeric: tabular-nums; }}
.total {{ font-weight: 700; }}
code {{ color: var(--code); background: var(--code-bg); padding: 1px 5px; border-radius: 4px; font-size: 13px; }}
.ok, .bad {{ font-size: 12px; }}
.ok {{ color: var(--ok); }} .bad {{ color: var(--bad); }}
</style>
</head>
<body>
<main>
<h1>Moby coverage counts</h1>
<div class="meta">Source: {html.escape(path)}<br>moby.json timestamp: {html.escape(str(data.get("timestamp", "?")))}</div>
<div class="wrap">
<table>
<thead><tr><th>Metric</th><th>Unit</th><th>Rule (moby.json)</th>
<th class="num">Windows</th><th class="num">Mac</th><th class="num">Linux</th><th class="num">Total</th></tr></thead>
<tbody>
{chr(10).join(body)}
</tbody>
</table>
</div>
</main>
</body>
</html>
"""


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("moby", nargs="?", default=DEFAULT, help="path to moby.json")
    parser.add_argument("--out", default="moby-counts.html", help="output HTML path")
    args = parser.parse_args()

    with open(args.moby, encoding="utf-8") as f:
        data = json.load(f)
    results = compute(data)

    print(f"{'Metric':<16}{'Unit':<12}{'Windows':>8}{'Mac':>6}{'Linux':>7}{'Total':>7}")
    for (metric, unit, _, _), c in zip(ROWS, results):
        print(f"{metric:<16}{unit:<12}{c['windows']:>8}{c['mac']:>6}{c['linux']:>7}{sum(c.values()):>7}")

    with open(args.out, "w", encoding="utf-8") as f:
        f.write(render_html(os.path.abspath(args.moby), data, results))
    print(f"\nWrote {os.path.abspath(args.out)}")


if __name__ == "__main__":
    main()
