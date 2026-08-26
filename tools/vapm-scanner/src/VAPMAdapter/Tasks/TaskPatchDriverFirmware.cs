///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using VAPMAdapater;
using VAPMAdapater.Log;
using VAPMAdapater.Updates;
using VAPMAdapter.OESIS;
using VAPMAdapter.OESIS.POCO;

namespace VAPMAdapter.Tasks
{
    /// <summary>
    /// Installs a single applicable driver/firmware/BIOS patch for the selected device using OESIS
    /// method 50903 (InstallDriverFirmwareUpdate). The patch must first have been surfaced by
    /// <see cref="TaskScanDriverFirmware"/>; this task downloads its package and hands the local
    /// path to the engine. Windows only, and requires a license that entitles the driver/firmware
    /// feature and (for BIOS/driver installs) administrator privileges.
    /// </summary>
    public class TaskPatchDriverFirmware
    {
        // Application-level guard codes, returned in DriverFirmwarePatchResult.rc. They are chosen
        // well away from the SDK's own return-code range so they can never be confused with an OESIS
        // error code (e.g. -5, -1066, -1067).
        public const int ERR_NO_ITEM_SELECTED = -900001;      // caller passed no device
        public const int ERR_NOT_AVAILABLE_TO_PATCH = -900002; // device has no applicable update
        public const int ERR_DOWNLOAD_FAILED = -900003;        // package could not be downloaded

        /// <summary>
        /// Installs the update for <paramref name="device"/> and returns the outcome.
        ///
        /// Guard cases return a populated result with a negative <c>rc</c> (one of the ERR_* codes
        /// above) instead of throwing, so the UI can report them uniformly:
        ///  - <c>device</c> is null (nothing selected)            -> ERR_NO_ITEM_SELECTED
        ///  - the device is up to date / not covered / has no
        ///    patch id or download package (nothing to patch)     -> ERR_NOT_AVAILABLE_TO_PATCH
        ///  - the package failed to download                      -> ERR_DOWNLOAD_FAILED
        /// Otherwise the engine is called and its exact return code is passed straight back.
        /// </summary>
        public static DriverFirmwarePatchResult Install(DriverFirmwareStatus device)
        {
            DriverFirmwarePatchResult result = new DriverFirmwarePatchResult();

            // Guard: nothing selected.
            if (device == null)
            {
                result.rc = ERR_NO_ITEM_SELECTED;
                result.message = "No device is selected. Select a BIOS/driver row that needs an " +
                                 "update, then try again.";
                return result;
            }

            // Guard: the row is not something we can patch. An "Up to date" row, or a row from a
            // machine the catalog does not cover, carries no patch id or download package.
            if (!device.IsMissing || string.IsNullOrEmpty(device.patchId))
            {
                result.rc = ERR_NOT_AVAILABLE_TO_PATCH;
                result.message = "'" + device.title + "' has no update available to patch - it is " +
                                 "up to date or not covered by the driver/firmware catalog.";
                return result;
            }

            if (string.IsNullOrEmpty(device.downloadUrl))
            {
                result.rc = ERR_NOT_AVAILABLE_TO_PATCH;
                result.message = "'" + device.title + "' has an update but no download package is " +
                                 "available for it.";
                return result;
            }

            RequireDriverFirmwareDatabase();

            // Download the package next to a temp working directory. The status POCO does not carry
            // the expected hash, so this passes null (no checksum validation) - a production
            // integration should validate against download_urls[].expected_sha256.
            string installerPath = DownloadInstaller(device);
            if (installerPath == null || !File.Exists(installerPath))
            {
                result.rc = ERR_DOWNLOAD_FAILED;
                result.message = "Failed to download the patch package for '" + device.title + "'.";
                return result;
            }

            OESISPipe.InitializeFramework(false);
            try
            {
                // The driver/firmware vmod component must be loaded before the install call, exactly
                // as it is for detection.
                OESISPipe.LoadDriverFirmwareDatabase(VAPMSettings.DRIVER_FIRMWARE_DB);

                string installJson;
                int rc = OESISPipe.InstallDriverFirmwareUpdate(device.patchId, installerPath, out installJson);
                result.rc = rc;
                OESISUtil.FillDriverFirmwareInstallResult(installJson, result);

                result.success = rc >= 0;
                if (result.success)
                {
                    result.message = "'" + device.title + "' was patched to " + device.targetVersion +
                                     "." + (string.IsNullOrEmpty(result.rebootLabel)
                                                ? ""
                                                : " Reboot: " + result.rebootLabel + ".");
                }
                else
                {
                    result.message = "Failed to install the update for '" + device.title +
                                     "' (rc=" + rc + ").  " + installJson;
                }

                Logger.Log("Driver/firmware install: patchId=" + device.patchId + ", rc=" + rc +
                           ", reboot=" + result.rebootLabel);
            }
            finally
            {
                OESISPipe.Teardown();
            }

            return result;
        }


        // Downloads the patch package to a temp working directory and returns the local path, or
        // null on failure. The file name is taken from the URL where possible.
        private static string DownloadInstaller(DriverFirmwareStatus device)
        {
            string dir = Path.Combine(Path.GetTempPath(), "acme-driver-firmware");
            Directory.CreateDirectory(dir);

            string fileName;
            try
            {
                fileName = Path.GetFileName(new Uri(device.downloadUrl).LocalPath);
            }
            catch
            {
                fileName = null;
            }
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = device.patchId + ".exe";
            }

            string installerPath = Path.Combine(dir, fileName);

            // No expected hash is carried on the row, so validation is skipped (pass null).
            bool ok = HttpClientUtils.DownloadValidFile(device.downloadUrl, installerPath, null);
            return ok ? installerPath : null;
        }


        // The driver/firmware database must be present in the working directory (same requirement
        // as the scan).
        private static void RequireDriverFirmwareDatabase()
        {
            if (!File.Exists(VAPMSettings.DRIVER_FIRMWARE_DB))
            {
                throw new Exception(
                    "Driver/firmware database '" + VAPMSettings.DRIVER_FIRMWARE_DB +
                    "' was not found in " + Directory.GetCurrentDirectory() +
                    ". Download the latest DB files and try again.");
            }
        }
    }
}
