# OPSWAT Patch Management Catalog Simplifier

Generate a single, consolidated JSON that’s **app-centric**: each product (by signature) with its associated CVEs and the patch that brings it to the latest installer. The tool downloads `analog.zip`, extracts the catalog, loads metadata, and writes a compact JSON you can feed into dashboards or downstream pipelines.

---

## 🔑 Important – Download Token

To run this tool, you **must have the `download_token.txt` file that OPSWAT provides during the evaluation or purchase of the Endpoint Security SDK**.  

- Place the provided token file in the **project root** (or use `--token-file` to point to its location).
- The file must contain **only** the token string, with no extra spaces or line breaks.

Example of the file content:

```
abc123xyz987yourtokenhere
```

If you don’t have the token file, please contact your OPSWAT account manager or support to obtain it.

---

## 📦 Requirements

- **Python:** 3.9+ recommended  
- **Dependencies:** standard library + modules included in this repo:
  - `util`, `system_patch`, `download_catalog`, `third_party`, `patch_classes`
- **Network access** to download the OPSWAT Patch catalog

---

## 🚀 Quick Start

```bash
# 1) Create a virtual environment (optional but recommended)
python -m venv .venv
source .venv/bin/activate       # Windows: .venv\Scripts\activate

# 2) Copy the OPSWAT-provided token file (download_token.txt) to this folder
#    or specify its path with --token-file.

# 3) Run the generator
python gen_app_centric_file.py --dir ./CatalogExtract --out app_centric.json
```

This will:
1. Read your token from `download_token.txt`
2. Download `analog.zip` (the OPSWAT Patch catalog)
3. Extract it to `./CatalogExtract`
4. Build the app-centric JSON at `app_centric.json`

---

## ⚙️ Command-Line Options

```text
--dir           Path to extracted folder to read from (default: ./CatalogExtract)
--out           Output JSON path (default: app_centric.json)

--token-file    Path to token file (default: download_token.txt)
--url-template  Download URL template containing "{token}"
                (default: https://vcr.opswat.com/gw/file/download/analog.zip?type=1&token={token})
--zip-path      Where to save the downloaded analog.zip (default: analog.zip)
--extract-dir   Where to extract files (default: ./CatalogExtract)
```

### Examples

**Use defaults with token in `download_token.txt`:**
```bash
python gen_app_centric_file.py
```

**Custom paths:**
```bash
python gen_app_centric_file.py   --token-file /secure/path/my_token.txt   --zip-path /tmp/analog.zip   --extract-dir /data/catalog   --dir /data/catalog   --out /data/app_centric.json
```

---

## 📄 Output

Default file: `app_centric.json`

Structure:
```json
{
  "generated_at": "2025-01-01T00:00:00Z",
  "products": [
    {
      "...": "3rd-party product record (signature-centric) with CVEs and latest patch"
    }
  ]
}
```

> Currently the build focuses on **third-party products**. System product aggregation exists in the codebase but is disabled by default.

---

## 🔍 How It Works (High Level)

1. **Token & Download:** Reads your token and downloads `analog.zip`.  
2. **Extract & Load:** Unzips the catalog and loads Moby compliance and product metadata.  
3. **Assemble Products:** Builds signature-centric entries with CVEs and “latest” patch details.  
4. **Write JSON:** Saves a consolidated `app_centric.json`.

---

## 🛠️ Common Issues & Fixes

- **“Token file not found”** → Ensure the token file exists and is specified with `--token-file`.  
- **Permission errors when extracting** → Use a directory you own or run with appropriate permissions.  
- **No output / empty products** → Check that `server/products.json` exists under the extracted catalog path.  
- **Import errors** → Run from the repo root or add it to `PYTHONPATH`.

---

## 👨‍💻 Development Tips

- Use `python -m pdb gen_app_centric_file.py` for debugging.
- To include system products, uncomment the relevant `get_system_products(...)` call in the generator.

---

## 📜 License

(Insert your project’s license here.)

---

## 🙌 Acknowledgements

- Script author: **Chris Seiler**  
- OPSWAT Patch Management catalog and related assets belong to their respective owners.
