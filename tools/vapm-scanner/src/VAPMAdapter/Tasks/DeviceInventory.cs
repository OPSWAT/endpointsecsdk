///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using VAPMAdapater.Log;
using VAPMAdapter.OESIS;
using VAPMAdapter.OESIS.POCO;

namespace VAPMAdapter.Tasks
{
    /// <summary>
    /// Collects the BIOS and hardware inventory for the BIOS & Drivers view using
    /// CollectDeviceInventory (OESIS method 50901).
    ///
    /// 50901 is the same inventory the driver/firmware matcher works from, which is what makes it
    /// the right source here: the devices shown are exactly the devices
    /// DetectDriverFirmwarePatches (50902) matches against, so a patch always has a row to attach
    /// to. Everything goes through the SDK by design - this is reference code for OEMs embedding
    /// OESIS, and reaching around it to Windows APIs would show an integration path no customer
    /// can follow.
    /// </summary>
    public class DeviceInventory
    {
        /// <summary>
        /// Returns the BIOS entry followed by every distinct device OESIS reports, each marked up
        /// to date. Callers overlay applicable patches on top of this and flag those rows Missing.
        /// </summary>
        public static List<DriverFirmwareStatus> Collect()
        {
            List<DriverFirmwareStatus> result = new List<DriverFirmwareStatus>();

            try
            {
                dynamic json = JObject.Parse(OESISPipe.CollectDeviceInventory(null));
                var inv = json.result != null ? json.result.inventory_collection : null;
                if (inv == null)
                {
                    Logger.Log("CollectDeviceInventory returned no inventory_collection.");
                    return result;
                }

                AddBios(result, inv);
                AddDevices(result, inv);
            }
            catch (Exception ex)
            {
                // An inventory failure must not sink the scan - the patch list is still useful.
                Logger.Log("CollectDeviceInventory failed: " + ex.Message);
            }

            return result;
        }


        /// <summary>
        /// "Manufacturer Model" for this machine, e.g. "Dell Inc. Latitude 5450", from
        /// GetPCModel (30001). Empty when the SDK cannot determine it.
        ///
        /// This is the identity the driver/firmware catalog matches on, so when detection reports
        /// the vendor or model as unsupported, this is the string that explains why.
        /// </summary>
        public static string GetSystemModel()
        {
            try
            {
                dynamic json = JObject.Parse(OESISPipe.GetPCModel());
                var res = json.result;
                if (res != null)
                {
                    string maker = res.manufacturer != null ? (string)res.manufacturer : "";
                    string model = res.model != null ? (string)res.model : "";
                    return (maker + " " + model).Trim();
                }
            }
            catch (Exception ex)
            {
                // Never let inventory metadata sink the scan.
                Logger.Log("GetPCModel failed: " + ex.Message);
            }

            return "";
        }


        /// <summary>
        /// True when OESIS reports this machine as a virtual machine. A VM has no vendor firmware
        /// to patch, which is normally why it comes back as an unsupported device.
        /// </summary>
        public static bool IsVirtualMachine()
        {
            try
            {
                dynamic json = JObject.Parse(OESISPipe.IsCurrentDeviceVirtual());
                var res = json.result;
                if (res != null && res.is_virtual != null)
                {
                    return (bool)res.is_virtual;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("IsCurrentDeviceVirtual failed: " + ex.Message);
            }

            return false;
        }


        // inventory_collection.bios -> the single BIOS row. A BIOS patch attaches to it directly,
        // so the version here is what the view shows as Current.
        private static void AddBios(List<DriverFirmwareStatus> result, dynamic inv)
        {
            var bios = inv.bios;
            if (bios == null)
            {
                return;
            }

            string vendor = bios.bios_vendor != null ? (string)bios.bios_vendor : "";
            string version = bios.bios_version != null ? (string)bios.bios_version : "";

            string product = "";
            var system = inv.system;
            if (system != null && system.system_product_name != null)
            {
                product = (string)system.system_product_name;
            }

            DriverFirmwareStatus item = new DriverFirmwareStatus();
            item.status = DriverFirmwareStatus.STATUS_UP_TO_DATE;
            item.component = "BIOS";
            item.title = (string.IsNullOrEmpty(product) ? vendor : product).Trim() + " System BIOS";
            item.category = "BIOS";
            item.currentVersion = version;
            item.vendor = vendor;
            result.Add(item);
        }


        // inventory_collection.devices -> one row per distinct device.
        private static void AddDevices(List<DriverFirmwareStatus> result, dynamic inv)
        {
            var devices = inv.devices;
            if (devices == null)
            {
                return;
            }

            // The same driver package backs many device instances, so collapse on name plus
            // version - otherwise the view is mostly duplicates.
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < devices.Count; i++)
            {
                var d = devices[i];

                string name = d.name != null ? (string)d.name : "";
                if (string.IsNullOrEmpty(name))
                {
                    continue;   // nothing an operator could act on
                }

                string driverVersion = "";
                string provider = "";
                var drv = d.driver;
                if (drv != null)
                {
                    driverVersion = drv.version != null ? (string)drv.version : "";
                    provider = drv.provider != null ? (string)drv.provider : "";
                }

                // Firmware version is reported as "n/a" on devices that expose none.
                string firmwareVersion = "";
                var fw = d.firmware;
                if (fw != null && fw.version != null)
                {
                    string v = (string)fw.version;
                    if (!string.IsNullOrEmpty(v) && v != "n/a")
                    {
                        firmwareVersion = v;
                    }
                }

                // Prefer the driver version: that is the value the catalog's current_version is
                // compared against when a patch is matched to this device.
                string current = !string.IsNullOrEmpty(driverVersion)
                                 ? driverVersion : firmwareVersion;

                if (!seen.Add(name + "|" + current))
                {
                    continue;
                }

                DriverFirmwareStatus item = new DriverFirmwareStatus();
                item.status = DriverFirmwareStatus.STATUS_UP_TO_DATE;
                item.component = string.IsNullOrEmpty(driverVersion) && !string.IsNullOrEmpty(firmwareVersion)
                                 ? "Firmware" : "Driver";
                item.title = name;
                item.category = d["class"] != null ? (string)d["class"] : "";
                item.currentVersion = current;
                item.vendor = provider;
                result.Add(item);
            }
        }
    }
}
