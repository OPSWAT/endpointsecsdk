///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

namespace VAPMAdapter.OESIS.POCO
{
    // One applicable driver/firmware patch (includes BIOS) returned by
    // DetectDriverFirmwarePatches (OESIS method 50902).
    public class DriverFirmwarePatch
    {
        public string patchId;
        public string title;
        public string component;        // BIOS / Driver / Firmware / Application / Other
        public string category;
        public string severity;
        public string currentVersion;
        public string targetVersion;
        public string rebootLabel;      // human-readable reboot_required
        public string downloadUrl;
        public string detectedVendor;   // dell / lenovo / ...
    }
}
