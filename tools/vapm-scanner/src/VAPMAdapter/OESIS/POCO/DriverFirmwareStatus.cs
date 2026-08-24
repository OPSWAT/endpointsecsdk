///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

namespace VAPMAdapter.OESIS.POCO
{
    // One row for the BIOS & Drivers view: a device that is installed on this machine, an
    // applicable patch for it, or both.
    //
    // The driver/firmware detection call (OESIS method 50902) only reports what is *missing* -
    // it runs in "model_based" mode, matching the machine model against the catalog rather than
    // enumerating hardware. The installed inventory therefore comes from Windows (Win32_BIOS and
    // Win32_PnPSignedDriver), and the two sets are merged so the view can show everything that
    // was found and flag the entries that need updating.
    public class DriverFirmwareStatus
    {
        public const string STATUS_MISSING = "Missing";
        public const string STATUS_UP_TO_DATE = "Up to date";

        public string status;           // Missing / Up to date
        public string component;        // BIOS / Driver / Firmware / Application / Other
        public string title;            // catalog title when known, else the device name
        public string category;         // catalog category, else the PnP device class
        public string severity;         // only set when a patch applies
        public string currentVersion;   // installed version
        public string targetVersion;    // only set when a patch applies
        public string rebootLabel;      // only set when a patch applies
        public string vendor;           // driver provider, or the vendor the catalog detected
        public string downloadUrl;      // only set when a patch applies
        public string patchId;          // only set when a patch applies

        public bool IsMissing
        {
            get { return status == STATUS_MISSING; }
        }
    }
}
