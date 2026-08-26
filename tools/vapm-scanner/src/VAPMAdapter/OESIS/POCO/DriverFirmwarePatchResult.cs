///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace VAPMAdapter.OESIS.POCO
{
    // Outcome of installing a single driver/firmware/BIOS patch (OESIS method 50903) via
    // TaskPatchDriverFirmware.Install. Carries both the SDK/app return code and the human-readable
    // detail so the UI can show exactly what happened.
    public class DriverFirmwarePatchResult
    {
        public bool success;             // true only when the install call returned rc >= 0
        public int rc;                   // SDK return code, or one of the ERR_* app codes on a guard failure
        public string message;           // human-readable summary suitable for a message box
        public string installReturnCode; // vendor installer's own return code (from the engine)
        public int requireRestart = -1;  // -1 unknown, 0 none, 1 reboot, 2 force reboot, 3 shutdown, 4 delayed
        public string rebootLabel = "";  // human-readable form of requireRestart
        public List<string> logPaths = new List<string>();
    }
}
