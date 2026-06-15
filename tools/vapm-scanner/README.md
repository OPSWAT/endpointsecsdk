# VAPM Scanner (AcmeScanner)

A Windows desktop reference application that demonstrates Vulnerability And Patch Management (VAPM) using the OESIS Framework. It scans the local machine for installed third-party and operating-system software, reports known vulnerabilities (CVEs) and patch status, browses the OESIS product/patch catalog, and can download and install patches.

This is sample/reference code intended to show how to integrate the OESIS Framework's vulnerability and patch modules from a .NET application. It is not a command-line tool.

## What It Is

The solution builds a single WinForms GUI executable (`AcmeScanner.exe`). There is no Python entrypoint and no command-line scanning interface — the tool is driven entirely through its graphical interface.

The application is organized into two projects:

| Project | Description |
|---------|-------------|
| `AcmeScanner` | The WinForms desktop application (`net9.0-windows`, `WinExe`). Contains the UI, scan workflows, catalog browser, and export functions. |
| `VAPMAdapter` | A .NET class library (`net8.0`) that wraps the OESIS Framework. It P/Invokes the native `libwaapi` library (`wa_api_setup` / `wa_api_invoke` / `wa_api_teardown`), parses the OESIS and catalog data, and exposes the scan, lookup, install, and update tasks used by the UI. |

Note: the `VAPMAdapter` folder contains two project files — `VAPMAdapter.csproj` (the one referenced by the solution and by `AcmeScanner`) and a misspelled `VAPMAdapater.csproj`. The solution uses `VAPMAdapter.csproj`.

## Capabilities

The UI exposes the following tabs and actions (driven by the OESIS Framework through `VAPMAdapter`):

- **Offline scan** — Detects installed products on the local machine, reports their version, latest available version, patch status ("Missing"/patched), CVE count, and OPSWAT severity. Optionally includes OS-level CVEs.
- **Online / orchestration scan (Windows)** — Detects missing Windows (KB) patches with severity and description, and can download and install them.
- **Catalog browser** — Loads the OESIS product/signature catalog, shows install/patch/fresh-install support, CVE counts, and supports text search. CVEs for a selected signature can be listed, and a specific CVE can be looked up by ID.
- **Patch install** — For a scanned product or a catalog signature that supports it, downloads and installs the latest version (auto-patch or fresh install), with options for background install, install validation, and force-close.
- **Exports** — Writes CSV files to the working directory: `ProductSupport.csv` (catalog product support), `urls.csv` and `domains.csv` (patch download URLs and their hosts), and `vapm-list.csv` (Moby product/signature list).
- **Developer tabs** — Additional Status, Moby, and Vulnerabilities tabs are shown when the app is launched with the `--dev` argument (or by pressing Ctrl+D in the app).

## Prerequisites

- Windows (the application targets `net9.0-windows` and uses Windows Forms).
- Visual Studio 2022 (or the corresponding .NET SDK) with the .NET desktop development workload.
- The OESIS Framework client binaries and data files (see "Obtaining the SDK" below).
- License and authentication files in the run directory:
  - `license.cfg` and `pass_key.txt` — required; the application will not run without them.
  - `download_token.txt` — required for the in-app SDK / database / catalog downloaders to authenticate. If you need a download token or evaluation license, contact `oem@opswat.com`.

## Obtaining the SDK

The application depends on the OESIS Framework native libraries (notably `libwaapi`, `libwavmodapi.dll`, and supporting `libwa*.dll` files) plus database files (e.g. `patch.dat`, vulnerability/patch `.dat` files).

There are two ways these can be put in place:

1. **Repository SDK downloader (recommended, run first).** Run the repo-root `sdk-downloader` utility, which authenticates with `eval-license/download_token.txt` and populates the `OPSWAT-SDK/` directory with the OESIS Framework client binaries. See [`../../sdk-downloader/README.md`](../../sdk-downloader/README.md). Run this before using the tool so the required SDK libraries are available.

2. **In-app download/update.** When `download_token.txt` is present in the run directory, the application can download and update the SDK and database files itself:
   - The **Download SDK / Update SDK** button calls `UpdateSDK.DownloadAndInstall_OPSWAT_SDK()`, which downloads the OESIS package (via the `OesisPackageLinks.xml` descriptor), extracts it, and copies the required `libwa*` binaries (including `libwavmodapi.dll`) into the working directory.
   - The **Download DB / Update DB** button (`UpdateDBFiles.DownloadFiles()`) downloads the vulnerability and patch database files (`patch.dat`, `v2mod.dat`, `wuo.dat`, `wiv-lite.dat`, `ap_checksum.dat`).
   - A separate **Update Moby** action downloads the Moby data file.

   The app considers the SDK/DB "current" if `libwavmodapi.dll` / `patch.dat` were updated within the last 7 days; otherwise it highlights the update buttons. Scanning and install actions stay disabled until both the SDK and the database files are present.

## Build and Run

1. Run the repository SDK downloader first (see above) so the OESIS Framework libraries are available.
2. Open the solution in Visual Studio 2022:
   ```
   tools/vapm-scanner/src/AcmeScanner/AcmeScanner.sln
   ```
3. Build the solution (Build > Build Solution). This builds both `AcmeScanner` and the `VAPMAdapter` library.
4. Ensure `license.cfg`, `pass_key.txt`, and `download_token.txt` are present in the output (run) directory.
5. Run `AcmeScanner` (F5 / Start). On first launch, use the Download SDK and Download DB buttons if the SDK and database files are not already in the run directory.

To launch the developer tabs (Status, Moby, Vulnerabilities), start the app with the `--dev` argument, or press Ctrl+D once it is running.

## Output

The application is interactive; results are displayed in the GUI (scan result lists, catalog views, and CVE detail dialogs). The export actions additionally write the following files to the working directory:

- `ProductSupport.csv` — catalog product/signature support summary.
- `urls.csv` / `domains.csv` — patch download URLs and their distinct hosts.
- `vapm-list.csv` — Moby product/signature listing.

## Related Tools

- [SDK Downloader](../../sdk-downloader/README.md) — downloads the OESIS Framework client binaries into `OPSWAT-SDK/`.
- [posture](../posture/README.md) — endpoint security posture assessment.
- [python-scanner](../python-scanner/README.md) — multi-platform vulnerability and patch scanner.

## Support

For evaluation licenses, download tokens, or SDK assistance, contact `oem@opswat.com`.
