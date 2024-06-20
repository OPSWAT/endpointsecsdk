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
    /// <summary>
    /// Provides utility methods for extracting files.
    /// </summary>
    internal class ExtractUtils
    {
        /// <summary>
        /// Extracts all zip files in the specified SDK directory and deletes the original zip files after extraction.
        /// </summary>
        /// <param name="sdkDir">The directory containing the zip files to be extracted.</param>
        public static void ExtractZipFiles(string sdkDir)
        {
            // Get all files in the specified SDK directory
            string[] sdkFiles = Directory.GetFiles(sdkDir);

            if (sdkFiles != null)
            {
                foreach (string current in sdkFiles)
                {
                    FileInfo fileInfo = new FileInfo(current);
                    if (fileInfo.Extension == ".zip")
                    {
                        // Extract the zip file to the SDK directory, overwriting any existing files
                        System.IO.Compression.ZipFile.ExtractToDirectory(fileInfo.FullName, sdkDir, true);

                        // Delete the original zip file after extraction
                        File.Delete(fileInfo.FullName);
                    }
                }
            }
        }
    }
}
