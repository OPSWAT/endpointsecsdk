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
    // The outcome of a BIOS & Drivers scan: the devices found, plus whether this machine can be
    // patched at all.
    //
    // Those are two separate questions and the view needs both. The driver/firmware catalog covers
    // named vendors and models; on a VM or white-box machine detection returns
    // VENDOR_NOT_SUPPORTED / MODEL_NOT_SUPPORTED and there are simply no patches to offer. The
    // installed inventory is still perfectly good and still worth showing - so the scan reports
    // "here is the hardware, and here is why nothing can be patched on it" rather than failing.
    public class DriverFirmwareScanResult
    {
        public List<DriverFirmwareStatus> devices = new List<DriverFirmwareStatus>();

        // Return code from the detection call (0 on a normal scan).
        public int detectRc;

        // False when the catalog cannot patch this machine at all.
        public bool patchingSupported = true;

        // Short, user-facing explanation when patchingSupported is false.
        public string unsupportedReason = "";

        // True when OESIS reports this machine as a VM - usually the reason a device is
        // uncovered, since a VM has no vendor firmware to patch.
        public bool isVirtualMachine;

        // "Manufacturer Model" of this machine, e.g. "VMware, Inc. VMware Virtual Platform".
        // Shown with the notice so it is obvious which box is uncovered.
        public string systemModel = "";
    }
}
