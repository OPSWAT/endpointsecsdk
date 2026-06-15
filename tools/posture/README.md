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

The UI is organized into tabs that map to the adapter tasks in `src/OPSWAT-Adapter/Tasks/`:

- **Validate Policy** (`TaskValidatePolicy`) — checks the endpoint against an antimalware/firewall/encryption policy you configure in the form (expected product, protection enabled, last definition/scan dates). Results are listed with green/red status indicators.
- **Security Score** (`TaskSecurityScore`) — runs live compliance checks (firewall running, disk encrypted, antimalware protected, definitions recent, scan recent) and produces a small numeric score compared against a configured threshold.
- **GeoLocation** (`TaskGeoLocation`) — retrieves device geolocation and evaluates it against a geo-fence (distance in miles from a point, or an allowed-country list).
- **Plugins** (`TaskGetPlugins`) — enumerates installed browser plugins and flags any browser or plugin marked as blocked.

Output is presented in the GUI (list views, status lights, map links). The tool does not write JSON/CSV/HTML report files.

## Prerequisites

- Windows
- Visual Studio 2022 (with the .NET desktop development workload) and .NET Framework 4.8
- The OESIS Framework SDK binaries and a valid license (see below)
- NuGet package restore enabled (the app references `Microsoft.Web.WebView2` and `Newtonsoft.Json`)

## Obtaining the SDK

This tool depends on the native OESIS Framework SDK binaries (for example `libwalocal.dll`, `libwaapi.dll`, `libwadeviceinfo.dll`, and the XML support charts).

There are two ways these binaries are made available:

1. **Repository SDK downloader (recommended first step).** The repo-root `sdk-downloader` populates the `OPSWAT-SDK/` directory with the OESIS Framework client binaries. Run it before building so the SDK is present locally. See `sdk-downloader/README.md` for details. The downloader authenticates using the token in `eval-license/download_token.txt`.

2. **Automatic download on startup.** When the app launches, `MainForm` calls `UpdateSDK.isSDKUpdated()` and, if the SDK is missing or older than 7 days, `UpdateSDK.DownloadAndInstall_OPSWAT_SDK()` downloads the OESIS packages, extracts them, and copies the required DLLs and support charts into the application's running directory. This path requires a `download_token.txt` file present in the running (output) directory; without it the download will fail.

> If you need an evaluation license or a download token, contact OPSWAT at oem@opswat.com.

### License files required at runtime

The OESIS Framework is initialized in `OESISFramework.InitializeFramework()`, which requires the following files to be present in the application's running directory (the build output folder, e.g. `bin\Debug\`):

- `pass_key.txt` — your OESIS passkey
- `license.cfg` — your OESIS license configuration

If these are missing, the tool throws a "License Files Missing" error. An expired or invalid license surfaces as a licensing error on the first compliance call.

## Building and running

1. Run the repo-root `sdk-downloader` (or ensure the SDK binaries are otherwise available). See `sdk-downloader/README.md`.
2. Open `tools/posture/src/opswat-posture.sln` in Visual Studio 2022.
3. Restore NuGet packages if prompted.
4. Build the solution (the `opswat-posture` project is the startup project).
5. Place `pass_key.txt`, `license.cfg`, and (if relying on automatic download) `download_token.txt` in the build output directory.
6. Run the `opswat-posture` project. On first launch it will stage the SDK if needed, then present the posture UI.

Because the tool inspects local security products and may require access to system security information, run it with the privileges appropriate for the checks you want to exercise.

## Related Tools

- **sdk-downloader** — downloads and organizes the OESIS Framework client binaries (`OPSWAT-SDK/`)
- **vapm-scanner** — vulnerability and patch management scanner
- **python-scanner** — multi-platform vulnerability scanner

## Support

For evaluation keys, download tokens, license files, or SDK assistance, contact OPSWAT at oem@opswat.com.
