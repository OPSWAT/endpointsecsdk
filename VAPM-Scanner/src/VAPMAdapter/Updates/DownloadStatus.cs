///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using VAPMAdapater.Updates;
using VAPMAdapater;

namespace VAPMAdapter.Updates
{
    /// <summary>
    /// Represents a class responsible for downloading the status file.
    /// </summary>
    internal class DownloadStatus
    {
        /// <summary>
        /// Downloads the status file from the specified URL and saves it to the given path.
        /// </summary>
        /// <param name="statusFile">The path where the downloaded status file will be saved.</param>
        public static void Download(string statusFile)
        {
            // Downloads the file from the URL obtained from VAPMSettings.getStatusURL()
            // and saves it to the specified statusFile path.
            HttpClientUtils.DownloadFileSynchronous(VAPMSettings.getStatusURL(), statusFile);
        }

    }
}
