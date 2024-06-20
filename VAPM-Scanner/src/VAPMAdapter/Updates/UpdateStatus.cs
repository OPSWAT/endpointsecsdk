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
    /// <summary>
    /// Provides functionality to update patch status information.
    /// </summary>
    public class UpdateStatus
    {

        /// <summary>
        /// Updates the patch status by downloading the latest information.
        /// </summary>
        /// <remarks>
        /// This method retrieves the current directory path and constructs a file path
        /// for a JSON file named 'patch_status.json'. It then downloads the status 
        /// information using the Download method from the DownloadStatus class.
        /// </remarks>
        public static void Update()
        {
            //retrieve the current directory
            string directory = Directory.GetCurrentDirectory();

            //combine directory path and file name
            string localFilePath = Path.Combine(directory, "patch_status.json");

            //download the patch status information 
            DownloadStatus.Download(localFilePath);
        }

    }
}
