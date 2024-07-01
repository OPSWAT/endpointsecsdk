///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using VAPMAdapter.Updates;

namespace VAPMAdapater.Updates
{
    /// <summary>
    /// Provides methods to check, download, and update the OPSWAT SDK.
    /// </summary>
    public class UpdateSDK
    {
        /// <summary>
        /// Checks if the SDK is downloaded
        /// </summary>
        /// <returns>True if the SDK is downloaded, otherwise false.</returns>
        public static bool doesSDKExist()
        {
            bool result = false;
            if (File.Exists("libwavmodapi.dll"))
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Checks if the SDK has been updated in the last 7 days.
        /// </summary>
        /// <returns>True if the SDK has been updated in the last 7 days, otherwise false.</returns>
        public static bool isSDKUpdated()
        {
            bool result = false;

            if (File.Exists("libwavmodapi.dll"))
            {
                FileInfo vmodInfo = new FileInfo("libwavmodapi.dll");
                // Update the SDK every 7 days
                if (vmodInfo.LastWriteTime > DateTime.Now.AddDays(-7))
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Copies a specific SDK file from the source directory to the destination directory.
        /// Retries up to 3 times if there is an IOException.
        /// </summary>
        /// <param name="sdkDir">The root SDK directory.</param>
        /// <param name="folder">The subfolder where the file is located.</param>
        /// <param name="filename">The name of the file to copy.</param>
        private static void CopySdkFile(string sdkDir, string folder, string filename)
        {
            const int numberOfRetries = 3;
            const int delayOnRetry = 1000; // 1 second delay between retries

            for (int i = 1; i <= numberOfRetries; ++i)
            {
                try
                {
                    string rootFile = Path.Combine(sdkDir, folder);
                    rootFile = Path.Combine(rootFile, "x64/release");
                    rootFile = Path.Combine(rootFile, filename);

                    // Use FileStream to ensure file is properly closed after use
                    using (var sourceStream = new FileStream(rootFile, FileMode.Open, FileAccess.Read))
                    {
                        using (var destStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                        {
                            sourceStream.CopyTo(destStream);
                        }
                    }

                    break; // If the copy succeeds, exit the retry loop
                }
                catch (IOException ex) when (i <= numberOfRetries)
                {
                    // Retry if there's an IOException
                    Thread.Sleep(delayOnRetry);
                }
            }
        }

        /// <summary>
        /// Deletes the specified SDK directory and its contents.
        /// </summary>
        /// <param name="sdkDir">The SDK directory to delete.</param>
        private static void CleanSDKFiles(string sdkDir)
        {
            Directory.Delete(sdkDir, true);
        }

        /// <summary>
        /// Copies all necessary SDK files from the source directory to the current directory.
        /// </summary>
        /// <param name="sdkDir">The source SDK directory.</param>
        private static void CopyAllFiles(string sdkDir)
        {
            CopySdkFile(sdkDir, "bin/detection", "libwaaddon.dll");
            CopySdkFile(sdkDir, "bin/detection", "libwaapi.dll");
            CopySdkFile(sdkDir, "bin/detection", "libwaheap.dll");
            CopySdkFile(sdkDir, "bin/detection", "libwautils.dll");
            CopySdkFile(sdkDir, "bin/manageability", "libwalocal.dll");
            CopySdkFile(sdkDir, "bin/manageability", "wa_3rd_party_host_32.exe");
            CopySdkFile(sdkDir, "bin/manageability", "wa_3rd_party_host_64.exe");
            CopySdkFile(sdkDir, "bin/vulnerability", "libwavmodapi.dll");
            File.Copy(Path.Combine(sdkDir, "bin/libwaresource.dll"), "libwaresource.dll", true);
        }

        /// <summary>
        /// Gets the local SDK directory, creating it if it doesn't exist.
        /// </summary>
        /// <returns>The path to the local SDK directory.</returns>
        public static string getLocalSDKDir()
        {
            // First delete the SDK directory if it exists
            string sdkDir = Path.Combine(Directory.GetCurrentDirectory(), "sdktemp");

            if (Directory.Exists(sdkDir))
            {
                Directory.Delete(sdkDir, true);
            }

            Directory.CreateDirectory(sdkDir);

            return sdkDir;
        }

        /// <summary>
        /// Downloads and installs the OPSWAT SDK.
        /// </summary>
        public static void DownloadAndInstall_OPSWAT_SDK()
        {
            // Get the local SDK directory
            string sdkDir = getLocalSDKDir();

            // Download all SDK files
            DownloadSDK.DownloadAllSDKFiles(sdkDir);

            // Extract all zip files in the SDK directory
            ExtractUtils.ExtractZipFiles(sdkDir);

            // Copy all necessary files from the SDK directory to the current directory
            CopyAllFiles(sdkDir);

            // Clean up the SDK directory
            CleanSDKFiles(sdkDir);
        }
    }
}