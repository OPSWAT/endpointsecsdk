///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////
using System.IO;

namespace VAPMAdapter.Updates
{
    internal class ExtractUtils
    {
        //
        // Extract all the zip files in the SDK Directory
        //
        public static void ExtractZipFiles(string sdkDir)
        {
            string[] sdkFiles = Directory.GetFiles(sdkDir);

            if (sdkFiles != null)
            {
                foreach (string current in sdkFiles)
                {
                    FileInfo fileInfo = new FileInfo(current);
                    if (fileInfo.Extension == ".zip")
                    {
                        //
                        // New Directory
                        //
                        System.IO.Compression.ZipFile.ExtractToDirectory(fileInfo.FullName, sdkDir, true);
                        File.Delete(fileInfo.FullName);
                    }
                }
            }
        }
    }
}
