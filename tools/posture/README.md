# Posture

A Windows desktop sample application that demonstrates the OESIS Framework Compliance module by evaluating the security posture of the local endpoint and checking it against a configurable policy.

## Purpose

The Posture tool is a reference implementation that shows how to use the OESIS Framework to:

- Detect installed security products (antimalware, firewall, disk encryption) on the local machine
- Validate the endpoint against a configurable security policy
- Calculate a simple security score from live compliance checks
- Determine the device location and evaluate it against a geo-fencing policy
- Enumerate installed browser plugins and flag blocked browsers/plugins

It is intended as sample/demo code for OEM developers integrating the OESIS Framework, not as a production scanning utility.

## What it actually is

This is a **Windows Forms (GUI) application** built on **.NET Framework 4.8**. It is not a command-line tool and has no Python entrypoint.

The solution contains two projects:

| Project | Type | Description |
|---------|------|-------------|
| `src/opswat-posture/opswat-posture.csproj` | WinExe (WinForms app) | The desktop UI (`MainForm`). This is the startup project. |
| `src/OPSWAT-Adapter/OPSWAT-Adapter.csproj` | Library | The adapter that wraps the native OESIS Framework SDK (P/Invoke into `libwalocal.dll`, etc.) and provides the compliance/detection tasks. |

At the top of the window an **Update SDK** button downloads/refreshes the OESIS SDK on demand, and a label shows the **installed SDK version and date**. (The SDK is no longer updated automatically on startup — see "Obtaining the SDK".)

The UI is organized into tabs that map to the adapter tasks in `src/OPSWAT-Adapter/Tasks/`:

- **Validate Policy** (`TaskValidatePolicy`) — checks the endpoint against an antimalware/firewall/encryption policy you configure in the form (expected product, protection enabled, last definition/scan dates). Results are listed with green/red status indicators.
- **Security Score** (`TaskSecurityScore`) — asks the SDK to compute the OPSWAT Security Score directly (`WAAPI_MID_GET_SECURITY_SCORE`, method 111). It shows the overall score (0–100) compared against a configured threshold, and logs the per-category breakdown (anti-malware, firewall, encryption, patch management, vulnerabilities, etc.).
- **Compliance Report** (`TaskComplianceReport`) — a **Get Report** button fetches the SDK's compliance/posture report (method 111) and displays the raw JSON in a read-only text box.
- **Categories** (`TaskCategories`) — a **Get Categories** button detects installed products (including winget-sourced apps) and lists them as sortable **Application / Signature ID / Category** rows (one row per category; products in multiple categories are duplicated).
- **GeoLocation** (`TaskGeoLocation`) — retrieves device geolocation and evaluates it against a geo-fence (distance in miles from a point, or an allowed-country list).
- **Plugins** (`TaskGetPlugins`) — enumerates installed browser plugins and flags any browser or plugin marked as blocked.
- **Custom** (`TaskCustomInvoke`) — a developer scratchpad: type any OESIS request JSON into the input box and click **Invoke** to pass it straight through to the engine (`wa_api_invoke`); the raw response JSON is shown in the read-only output box. The JSON is validated before sending (a parse error is reported instead of invoking). Prefill buttons seed common examples — **GetVersion** (method 100), **GetProductInfo** (method 109), and **DetectProducts** (method 0) — using the Firefox signature (3039) as the default where a signature is required.

Output is presented in the GUI (list views, status lights, map links, and the Compliance Report JSON view). The tool does not write JSON/CSV/HTML report files.

## Prerequisites

- Windows
- Visual Studio 2022 (with the .NET desktop development workload) and .NET Framework 4.8
- The OESIS Framework SDK binaries and a valid license (see below)
- NuGet package restore enabled (the app references `Microsoft.Web.WebView2` and `Newtonsoft.Json`)

## Obtaining the SDK

This tool depends on the native OESIS Framework SDK binaries (for example `libwalocal.dll`, `libwaapi.dll`, `libwadeviceinfo.dll`, and the XML support charts).

There are two ways these binaries are made available:

1. **Repository SDK downloader (recommended first step).** The repo-root `sdk-downloader` populates the `OPSWAT-SDK/` directory with the OESIS Framework client binaries. Run it before building so the SDK is present locally. See `sdk-downloader/README.md` for details. The downloader authenticates using the token in `eval-license/download_token.txt`.

2. **The in-app "Update SDK" button.** Click **Update SDK** at the top of the window to run `UpdateSDK.DownloadAndInstall_OPSWAT_SDK()`, which downloads the OESIS packages, extracts them, and copies the required DLLs and support charts into the application's running directory. Progress shows as "Downloading SDK..." with a spinner, and the version/date label refreshes when it completes. This path requires a `download_token.txt` in the running (output) directory (auto-provisioned from `eval-license/` — see below); without it the download fails. There is **no** automatic update on startup — the app uses whatever SDK is present until you click the button. If the SDK is not present at all, the tabs stay disabled and the label reads "SDK: not installed - click Update SDK".

> If you need an evaluation license or a download token, contact OPSWAT at oem@opswat.com.

### License files required at runtime

The OESIS Framework requires the following files in the application's running directory (the build output folder, e.g. `bin\Debug\`):

- `pass_key.txt` — your OESIS passkey
- `license.cfg` — your OESIS license configuration

**Automatic provisioning from `eval-license/`.** On startup the app calls `EnsureLicenseFiles`, which — if `license.cfg`/`pass_key.txt` aren't already in the running directory — locates the repo root (the `sdkroot` marker file) and copies them (plus `download_token.txt`) from `<repo-root>/eval-license/`. So if you've placed your eval files in `eval-license/`, you don't have to copy them into the output folder manually.

If the files can't be found in the running directory **or** in `eval-license/`, the app shows a clear "License not found. Please include license.cfg and pass_key.txt in the running directory" message and exits. An expired or invalid license surfaces as a licensing error on the first compliance call.

## Building and running

1. Run the repo-root `sdk-downloader` (or ensure the SDK binaries are otherwise available). See `sdk-downloader/README.md`.
2. Open `tools/posture/src/opswat-posture.sln` in Visual Studio 2022.
3. Restore NuGet packages if prompted.
4. Build the solution (the `opswat-posture` project is the startup project).
5. Ensure `pass_key.txt`, `license.cfg`, and `download_token.txt` are available — either in the build output directory, or in `<repo-root>/eval-license/` (the app auto-copies them from there on startup).
6. Run the `opswat-posture` project. If the SDK isn't present yet, click **Update SDK** to download it; then use the tabs.

Because the tool inspects local security products and may require access to system security information, run it with the privileges appropriate for the checks you want to exercise.

## Related Tools

- **sdk-downloader** — downloads and organizes the OESIS Framework client binaries (`OPSWAT-SDK/`)
- **vapm-scanner** — vulnerability and patch management scanner
- **python-scanner** — multi-platform vulnerability scanner

## Support

For evaluation keys, download tokens, license files, or SDK assistance, contact OPSWAT at oem@opswat.com.
