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
        // Coverage gaps in the driver/firmware catalog, not failures.
        // WA_VMOD_ERROR_VENDOR_NOT_SUPPORTED: this hardware vendor is not covered at all.
        public const int VENDOR_NOT_SUPPORTED = -1066;
        // WA_VMOD_ERROR_MODEL_NOT_SUPPORTED: the vendor is covered, this specific model is not.
        public const int MODEL_NOT_SUPPORTED = -1067;

        /// <summary>
        /// Collects the installed BIOS/driver inventory and overlays the applicable patches, so
        /// the caller gets every device found with the ones needing an update flagged Missing.
        ///
        /// When the catalog does not cover this machine the device list is still returned, with
        /// patchingSupported false and a reason - an uncovered machine is a normal outcome (VMs and
        /// white-box hardware hit it routinely) and throwing away the inventory over it leaves the
        /// user staring at an error instead of their hardware.
        /// </summary>
        public static DriverFirmwareScanResult ScanWithInventory()
        {
            DriverFirmwareScanResult result = new DriverFirmwareScanResult();
            List<DriverFirmwareStatus> inventory;
            List<DriverFirmwarePatch> patches;
            int detectRc;

            RequireDriverFirmwareDatabase();

            // One framework session for the whole sequence: the inventory now comes from the
            // OESIS Device Info methods, so it needs the framework up just as detection does.
            OESISPipe.InitializeFramework(false);
            try
            {
                result.systemModel = DeviceInventory.GetSystemModel();
                result.isVirtualMachine = DeviceInventory.IsVirtualMachine();

                // Inventory first: it is still useful if detection finds nothing (an uncovered
                // model) or the catalog is unavailable.
                inventory = DeviceInventory.Collect();

                patches = DetectPatches(out detectRc);
            }
            finally
            {
                OESISPipe.Teardown();
            }

            result.detectRc = detectRc;

            if (detectRc == VENDOR_NOT_SUPPORTED)
            {
                result.patchingSupported = false;
                result.unsupportedReason =
                    "Driver and BIOS patching is not supported on this device - its hardware " +
                    "vendor is not covered by the driver/firmware catalog (rc=-1066). The devices " +
                    "below were found on the system, but no updates can be offered for them.";
            }
            else if (detectRc == MODEL_NOT_SUPPORTED)
            {
                result.patchingSupported = false;
                result.unsupportedReason =
                    "Driver and BIOS patching is not supported on this device - this model is not " +
                    "covered by the driver/firmware catalog (rc=-1067). The devices below were " +
                    "found on the system, but no updates can be offered for them.";
            }

            result.devices = DriverFirmwareMerge.Merge(inventory, patches);
            Logger.Log("Driver/firmware view: " + result.devices.Count + " device(s), " +
                       patches.Count + " applicable patch(es), rc=" + detectRc +
                       ", model='" + result.systemModel + "'.");
            return result;
        }


        /// <summary>
        /// Runs a driver/firmware/BIOS scan and returns the applicable patches (empty if the device
        /// is fully up to date or its model isn't covered by the catalog).
        /// </summary>
        public static List<DriverFirmwarePatch> Scan()
        {
            int ignored;
            return Scan(out ignored);
        }


        /// <summary>
        /// As Scan(), but also reports the detection return code so the caller can tell an
        /// up-to-date machine (rc=0, no patches) from an uncovered one (rc=-1066/-1067).
        /// Both return an empty list, and they mean very different things.
        /// </summary>
        public static List<DriverFirmwarePatch> Scan(out int detectRc)
        {
            RequireDriverFirmwareDatabase();

            OESISPipe.InitializeFramework(false);
            try
            {
                return DetectPatches(out detectRc);
            }
            finally
            {
                OESISPipe.Teardown();
            }
        }


        // The driver/firmware database must be present in the working directory.
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


        // Loads the catalog and detects applicable patches. Assumes the framework is already
        // initialized, so it can share one session with the Device Info inventory calls.
        private static List<DriverFirmwarePatch> DetectPatches(out int detectRc)
        {
            List<DriverFirmwarePatch> result = new List<DriverFirmwarePatch>();
            detectRc = 0;

            {
                // Initialize the driver/firmware vmod component (throws on rc<0, e.g. rc=-5 when the
                // active license does not entitle the driver/firmware feature).
                OESISPipe.LoadDriverFirmwareDatabase(VAPMSettings.DRIVER_FIRMWARE_DB);

                // Detect applicable patches; the engine collects the device inventory internally.
                string detectJson;
                int rc = OESISPipe.DetectDriverFirmwarePatches(out detectJson);
                detectRc = rc;

                if (rc == VENDOR_NOT_SUPPORTED)
                {
                    Logger.Log("Driver/firmware scan: hardware vendor not covered by the catalog (rc=-1066).");
                    return result; // empty
                }

                if (rc == MODEL_NOT_SUPPORTED)
                {
                    Logger.Log("Driver/firmware scan: device model not covered by the catalog (rc=-1067).");
                    return result; // empty
                }

                result = OESISUtil.GetDriverFirmwarePatchList(detectJson);
                Logger.Log("Driver/firmware scan: " + result.Count + " applicable patch(es).");
            }

            return result;
        }
    }
}
