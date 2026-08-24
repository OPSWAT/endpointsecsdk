///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using VAPMAdapater;
using VAPMAdapater.Log;
using VAPMAdapter.OESIS;
using VAPMAdapter.OESIS.POCO;

namespace VAPMAdapter.Tasks
{
    /// <summary>
    /// Scans the local endpoint for applicable driver/firmware patches (including BIOS) by loading
    /// the driver/firmware database (OESIS method 50900) and detecting applicable patches against
    /// the collected device inventory (method 50902). Windows only, and requires a license that
    /// entitles the driver/firmware feature.
    /// </summary>
    public class TaskScanDriverFirmware
    {
        // WA_VMOD_ERROR_MODEL_NOT_SUPPORTED: the detected device model has no entries in the
        // loaded driver/firmware catalog. A coverage gap, not a failure.
        public const int MODEL_NOT_SUPPORTED = -1067;

        /// <summary>
        /// Collects the installed BIOS/driver inventory and overlays the applicable patches, so
        /// the caller gets every device found with the ones needing an update flagged Missing.
        /// </summary>
        public static List<DriverFirmwareStatus> ScanWithInventory()
        {
            // Inventory first: it is local, fast, and still useful if detection finds nothing
            // (an uncovered model) or the catalog is unavailable.
            List<DriverFirmwareStatus> inventory = DeviceInventory.Collect();

            List<DriverFirmwarePatch> patches = Scan();

            List<DriverFirmwareStatus> merged = DriverFirmwareMerge.Merge(inventory, patches);
            Logger.Log("Driver/firmware view: " + merged.Count + " device(s), " +
                       patches.Count + " applicable patch(es).");
            return merged;
        }


        /// <summary>
        /// Runs a driver/firmware/BIOS scan and returns the applicable patches (empty if the device
        /// is fully up to date or its model isn't covered by the catalog).
        /// </summary>
        public static List<DriverFirmwarePatch> Scan()
        {
            List<DriverFirmwarePatch> result = new List<DriverFirmwarePatch>();

            // The driver/firmware database must be present in the working directory.
            if (!File.Exists(VAPMSettings.DRIVER_FIRMWARE_DB))
            {
                throw new Exception(
                    "Driver/firmware database '" + VAPMSettings.DRIVER_FIRMWARE_DB +
                    "' was not found in " + Directory.GetCurrentDirectory() +
                    ". Download the latest DB files and try again.");
            }

            OESISPipe.InitializeFramework(false);
            try
            {
                // Initialize the driver/firmware vmod component (throws on rc<0, e.g. rc=-5 when the
                // active license does not entitle the driver/firmware feature).
                OESISPipe.LoadDriverFirmwareDatabase(VAPMSettings.DRIVER_FIRMWARE_DB);

                // Detect applicable patches; the engine collects the device inventory internally.
                string detectJson;
                int rc = OESISPipe.DetectDriverFirmwarePatches(out detectJson);

                if (rc == MODEL_NOT_SUPPORTED)
                {
                    Logger.Log("Driver/firmware scan: device model not covered by the catalog (rc=-1067).");
                    return result; // empty
                }

                result = OESISUtil.GetDriverFirmwarePatchList(detectJson);
                Logger.Log("Driver/firmware scan: " + result.Count + " applicable patch(es).");
            }
            finally
            {
                OESISPipe.Teardown();
            }

            return result;
        }
    }
}
