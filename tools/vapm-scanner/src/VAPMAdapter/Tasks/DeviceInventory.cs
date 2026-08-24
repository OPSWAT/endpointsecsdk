///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Management;
using VAPMAdapater.Log;
using VAPMAdapter.OESIS.POCO;

namespace VAPMAdapter.Tasks
{
    /// <summary>
    /// Collects the installed BIOS and driver inventory from Windows.
    ///
    /// The driver/firmware detection call reports only applicable patches, so on its own it cannot
    /// answer "what is installed on this machine". WMI can: Win32_BIOS for the system firmware and
    /// Win32_PnPSignedDriver for the signed driver set. Windows only.
    /// </summary>
    public class DeviceInventory
    {
        /// <summary>
        /// Returns the BIOS entry followed by every distinct installed driver, each marked
        /// up to date. Callers overlay applicable patches on top of this.
        /// </summary>
        public static List<DriverFirmwareStatus> Collect()
        {
            List<DriverFirmwareStatus> result = new List<DriverFirmwareStatus>();
            result.AddRange(CollectBios());
            result.AddRange(CollectDrivers());
            return result;
        }


        private static List<DriverFirmwareStatus> CollectBios()
        {
            List<DriverFirmwareStatus> result = new List<DriverFirmwareStatus>();

            try
            {
                // SMBIOSBIOSVersion is the version the vendor's catalog uses (it matched the
                // catalog's current_version exactly in testing); Version is the raw SMBIOS
                // identifier, which does not.
                using (ManagementObjectSearcher searcher =
                       new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
                {
                    foreach (ManagementObject bios in searcher.Get())
                    {
                        DriverFirmwareStatus item = new DriverFirmwareStatus();
                        item.status = DriverFirmwareStatus.STATUS_UP_TO_DATE;
                        item.component = "BIOS";
                        // Win32_BIOS.Name is not a name on every vendor - Dell puts the
                        // version string in it - so build a stable label instead and leave the
                        // version to the Current column.
                        string maker = AsString(bios["Manufacturer"]);
                        item.title = string.IsNullOrEmpty(maker)
                            ? "System BIOS"
                            : maker + " System BIOS";
                        item.category = "BIOS";
                        item.currentVersion = AsString(bios["SMBIOSBIOSVersion"]);
                        item.vendor = AsString(bios["Manufacturer"]);
                        result.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                // An inventory failure must not sink the scan - the patch list is still useful.
                Logger.Log("BIOS inventory failed: " + ex.Message);
            }

            return result;
        }


        private static List<DriverFirmwareStatus> CollectDrivers()
        {
            List<DriverFirmwareStatus> result = new List<DriverFirmwareStatus>();

            try
            {
                // Win32_PnPSignedDriver returns one row per device instance, so the same driver
                // package appears repeatedly (310 rows / 180 distinct on the test machine).
                // De-duplicate on name plus version.
                HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT DeviceName, DriverVersion, DriverProviderName, DeviceClass " +
                    "FROM Win32_PnPSignedDriver"))
                {
                    foreach (ManagementObject driver in searcher.Get())
                    {
                        string name = AsString(driver["DeviceName"]);
                        string version = AsString(driver["DriverVersion"]);

                        // Unnamed entries carry nothing an operator can act on.
                        if (string.IsNullOrEmpty(name))
                        {
                            continue;
                        }
                        if (!seen.Add(name + "|" + version))
                        {
                            continue;
                        }

                        DriverFirmwareStatus item = new DriverFirmwareStatus();
                        item.status = DriverFirmwareStatus.STATUS_UP_TO_DATE;
                        item.component = "Driver";
                        item.title = name;
                        item.category = AsString(driver["DeviceClass"]);
                        item.currentVersion = version;
                        item.vendor = AsString(driver["DriverProviderName"]);
                        result.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Driver inventory failed: " + ex.Message);
            }

            return result;
        }


        private static string AsString(object value)
        {
            return value == null ? "" : value.ToString().Trim();
        }
    }
}
