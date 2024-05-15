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

namespace VAPMAdapter.Updates
{
    public class UpdateStatus
    {

        public static void Update()
        {
            string directory = Directory.GetCurrentDirectory();
            string localFilePath = Path.Combine(directory, "patch_status.json");
            DownloadStatus.Download(localFilePath);
        }

    }
}
