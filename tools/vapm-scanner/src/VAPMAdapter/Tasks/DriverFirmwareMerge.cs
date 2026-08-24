///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using VAPMAdapter.OESIS.POCO;

namespace VAPMAdapter.Tasks
{
    /// <summary>
    /// Overlays applicable driver/firmware patches onto the installed inventory so a single view
    /// can show everything found and flag what needs updating.
    ///
    /// The catalog and Windows describe the same hardware differently, so the join is by version,
    /// not by name. On the test machine the catalog reported
    /// "Intel Rapid Storage Technology Driver and Application" at current_version 20.2.6.1025,
    /// while Windows reported "Intel RST VMD Controller 7D0B" at DriverVersion 20.2.6.1025 - no
    /// textual overlap at all, but an exact version match. BIOS is simpler: a machine has one, so
    /// a BIOS patch attaches to the BIOS row directly.
    /// </summary>
    public class DriverFirmwareMerge
    {
        /// <summary>
        /// Returns one row per installed device, with applicable patches applied, plus a row for
        /// any patch that could not be tied to an installed device.
        ///
        /// Patches are never dropped. If the version join fails, the patch still appears on its
        /// own row - under-reporting a missing update would be the worse failure.
        /// </summary>
        public static List<DriverFirmwareStatus> Merge(
            List<DriverFirmwareStatus> inventory, List<DriverFirmwarePatch> patches)
        {
            List<DriverFirmwareStatus> result = new List<DriverFirmwareStatus>(inventory);

            foreach (DriverFirmwarePatch patch in patches)
            {
                bool attached = false;

                // Match only against the original installed inventory, never against synthetic rows
                // added below for earlier unmatched patches - otherwise a later patch sharing a
                // version could overwrite an earlier patch's row and be lost. (result starts as a
                // shallow copy of inventory, so applying to an inventory item updates result too.)
                foreach (DriverFirmwareStatus item in inventory)
                {
                    if (!Matches(item, patch))
                    {
                        continue;
                    }

                    Apply(item, patch);
                    attached = true;
                    // Keep going: one driver package can cover several device instances, and all
                    // of them are genuinely out of date.
                }

                if (!attached)
                {
                    // The catalog knows about an update for hardware the inventory did not
                    // surface (or a component type WMI does not report, such as firmware).
                    result.Add(FromPatch(patch));
                }
            }

            return result;
        }


        private static bool Matches(DriverFirmwareStatus item, DriverFirmwarePatch patch)
        {
            bool patchIsBios = Equals(patch.component, "BIOS");
            bool itemIsBios = Equals(item.component, "BIOS");

            // A machine has exactly one BIOS, so component agreement is enough - and is more
            // robust than a version compare, which would miss when the catalog and SMBIOS
            // disagree on formatting.
            if (patchIsBios || itemIsBios)
            {
                return patchIsBios && itemIsBios;
            }

            // Otherwise require an exact version match, and only trust versions specific enough
            // to be meaningful. A bare "1.0" collides across unrelated drivers.
            string patchVersion = Normalize(patch.currentVersion);
            string itemVersion = Normalize(item.currentVersion);
            if (patchVersion.Length == 0 || patchVersion != itemVersion)
            {
                return false;
            }
            return CountDots(patchVersion) >= 2;
        }


        private static void Apply(DriverFirmwareStatus item, DriverFirmwarePatch patch)
        {
            item.status = DriverFirmwareStatus.STATUS_MISSING;
            item.severity = patch.severity;
            item.targetVersion = patch.targetVersion;
            item.rebootLabel = patch.rebootLabel;
            item.downloadUrl = patch.downloadUrl;
            item.patchId = patch.patchId;

            // Show the catalog's package name *and* the device it applies to. The package name
            // is what the vendor's download page uses, so it is what an operator searches for;
            // the device name is what distinguishes the rows, because one package can cover
            // several devices (the Intel RST package covers two on the test machine, and
            // collapsing both to the package name made the list look duplicated).
            if (!string.IsNullOrEmpty(patch.title))
            {
                bool sameName = string.Equals(patch.title, item.title,
                                              StringComparison.OrdinalIgnoreCase);
                // Not for BIOS: there is only one, so there is nothing to disambiguate, and
                // vendors put unhelpful values in Win32_BIOS.Name (Dell reports the version
                // string, which would render as "... System BIOS (1.23.1)").
                bool disambiguate = !Equals(item.component, "BIOS")
                                    && !sameName
                                    && !string.IsNullOrEmpty(item.title);
                item.title = disambiguate
                    ? patch.title + " (" + item.title + ")"
                    : patch.title;
            }
            if (!string.IsNullOrEmpty(patch.category))
            {
                item.category = patch.category;
            }
        }


        private static DriverFirmwareStatus FromPatch(DriverFirmwarePatch patch)
        {
            DriverFirmwareStatus item = new DriverFirmwareStatus();
            item.status = DriverFirmwareStatus.STATUS_MISSING;
            item.component = patch.component;
            item.title = patch.title;
            item.category = patch.category;
            item.severity = patch.severity;
            item.currentVersion = patch.currentVersion;
            item.targetVersion = patch.targetVersion;
            item.rebootLabel = patch.rebootLabel;
            item.vendor = patch.detectedVendor;
            item.downloadUrl = patch.downloadUrl;
            item.patchId = patch.patchId;
            return item;
        }


        private static bool Equals(string left, string right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }


        private static string Normalize(string version)
        {
            return version == null ? "" : version.Trim();
        }


        private static int CountDots(string value)
        {
            int count = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '.')
                {
                    count++;
                }
            }
            return count;
        }
    }
}
