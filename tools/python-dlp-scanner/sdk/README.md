# sdk/ — DLP engine runtime

`dlp_scan.py` loads the OESIS Endpoint-DLP engine from this folder. It is **not**
committed (the binaries are large and not ours to redistribute) — stage it here
with the helper from the parent folder:

```bash
python prepare.py
```

That copies the minimal set the scanner needs out of the OESIS DLP package for
your platform. Library extensions are `.dll` on Windows, `.dylib` on macOS, and
`.so` on Linux:

| File | Purpose |
|------|---------|
| `libwadlpscan.*` | The DLP scanner engine (`wa_dlpscan_*`) |
| `libwautils.*`, `libwaheap.*` | Engine dependencies |
| `pdfium.*` (or `libpdfium.*`) | PDF text extraction |
| `dlp_rules.dat` | Detector rulepack (~2,000 detectors) |
| `dlp_rules.manifest.json` | Rulepack integrity hash |
| `tessdata/eng.traineddata` | OCR language data (needed to scan images) |

No license file is required for this engine build. If a build you use does need
one, drop `pass_key.txt` (or the offline `license_bytes.txt` + `license_key.txt`
pair) into this folder and `dlp_scan.py` will pick it up.
