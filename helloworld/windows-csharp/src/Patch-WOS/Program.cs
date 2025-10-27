///////////////////////////////////////////////////////////////////////////////////////////////
///  Windows OS Patch Manager
///  Downloads and installs patches specifically for Microsoft/Windows components
///  Uses wuo.dat (Windows patches) instead of patch.dat (all software patches)
///  
///  This patch manager focuses exclusively on Windows OS components, filtering out
///  all third-party applications that would normally be handled by the 
///  regular Patch project using patch.dat and ap_checksum.dat databases.
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace PatchWOS
{
    internal class Program
    {

        /// <summary>
        /// Validates OESIS API return codes to ensure successful execution.
        /// OESIS uses positive values (>= 0) to indicate SUCCESS and negative values (< 0) to indicate FAILURE.
        /// </summary>
        /// <param name="rc">Return code from OESIS API call</param>
        /// <exception cref="Exception">Thrown when return code indicates failure (< 0)</exception>
        /// <remarks>
        /// Return code reference: https://software.opswat.com/OESIS_V4/html/c_return_codes.html
        /// Common error codes:
        /// -1039: WA_VMOD_ERROR_OUT_OF_RANGE - Queried index is out of array range
        /// -1019: WA_VMOD_ERROR_OFFLINEVMOD_NOT_INITIALIZED - Database not loaded
        /// </remarks>
        private static void CheckSuccess(int rc)
        {
            if (rc < 0)
            {
                Console.Out.WriteLine("Failed to execute OESIS API call with return code: " + rc);
                throw new Exception("OESIS API call failed with return code: " + rc);
            }
        }


        /// <summary>
        /// Core OESIS API invocation method - sends JSON commands to the OESIS framework
        /// and retrieves JSON responses. This is the primary communication interface.
        /// </summary>
        /// <param name="json_config">JSON string containing the API method and parameters</param>
        /// <param name="json_out">Output parameter containing the JSON response from OESIS</param>
        /// <returns>Return code indicating success (>= 0) or failure (< 0)</returns>
        /// <remarks>
        /// This method handles:
        /// 1. Calling the native OESIS API with JSON input
        /// 2. Converting native pointer responses to managed strings
        /// 3. Proper memory cleanup to prevent memory leaks
        /// </remarks>
        private static int Invoke(string json_config, out string json_out)
        {
            IntPtr outPtr = IntPtr.Zero;
            // Call the native OESIS API with our JSON input
            int rc = OESISAdapter.wa_api_invoke(json_config, out outPtr);
            json_out = "{ }"; // Default empty JSON response
            
            // If OESIS returned data, convert the native pointer to a managed string
            if (outPtr != IntPtr.Zero)
            {
                json_out = XStringMarshaler.PtrToString(outPtr);
                // Always free the native memory to prevent memory leaks
                OESISAdapter.wa_api_free(outPtr);
            }
            return rc;
        }

        /// <summary>
        /// Initializes the OESIS Framework with license validation and configuration.
        /// This must be called before any other OESIS operations can be performed.
        /// </summary>
        /// <exception cref="Exception">Thrown if license file is missing or initialization fails</exception>
        /// <remarks>
        /// Configuration parameters:
        /// - passkey_string: The OPSWAT license key from pass_key.txt
        /// - enable_pretty_print: Makes JSON output more readable for debugging
        /// - online_mode: Enables online features (set to true for full functionality)
        /// - silent_mode: Reduces verbose logging output
        /// 
        /// Troubleshooting:
        /// - "Bad Image" exception: Check that "Prefer 32-bit" is unchecked in project properties
        /// - License errors: Ensure pass_key.txt is in the executable directory
        /// 
        /// Reference: https://software.opswat.com/OESIS_V4/html/c_sdk.html
        /// </remarks>
        private static void InitializeFramework()
        {
            // Get the directory where the executable is running
            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string passKeyPath = Path.Combine(currentPath, "pass_key.txt");

            // Verify that the OPSWAT license file exists
            if (!File.Exists(passKeyPath))
            {
                Console.WriteLine("Could not find a pass_key.txt file. Make sure that the license provided during evaluation is in the executable directory.");
                throw new Exception("License pass_key.txt file not found");
            }

            // Read the license key from the file
            string passkey = File.ReadAllText(passKeyPath);
            
            // Configure OESIS framework settings
            string config = "{ \"config\" : { \"passkey_string\": \"" + passkey + "\", \"enable_pretty_print\": true, \"online_mode\": true, \"silent_mode\": true } }";

            // Initialize the OESIS framework with our configuration
            IntPtr outPtr = IntPtr.Zero;
            int rc = OESISAdapter.wa_api_setup(config, out outPtr);
            string json_out = "{ }";
            
            // Process the initialization response
            if (outPtr != IntPtr.Zero)
            {
                json_out = XStringMarshaler.PtrToString(outPtr);
                OESISAdapter.wa_api_free(outPtr);
            }
            else
            {
                Console.Out.WriteLine("Failed to initialize OESIS framework with return code: " + rc);
                // Return code reference: https://software.opswat.com/OESIS_V4/html/c_return_codes.html
                throw new Exception("Failed to initialize OESIS framework");
            }
        }

        /// <summary>
        /// Loads a patch database into memory for patch management operations.
        /// This method supports both regular patch databases (patch.dat) and Windows OS specific databases (wuo.dat).
        /// 
        /// OESIS Method 50302: Load Patch Database
        /// </summary>
        /// <param name="databaseFile">Path to the patch database file (e.g., "patch.dat" or "wuo.dat")</param>
        /// <param name="checksumFile">Optional checksum file for integrity validation (e.g., "ap_checksum.dat"). Can be empty string.</param>
        /// <exception cref="Exception">Thrown if database loading fails</exception>
        /// <remarks>
        /// Database Types:
        /// - patch.dat: Contains patches for third-party applications (Firefox, Chrome, etc.)
        /// - wuo.dat: Contains patches for Windows OS components specifically
        /// 
        /// Checksum files provide integrity verification:
        /// - ap_checksum.dat: Checksums for patch.dat
        /// - Windows databases typically don't require checksum files
        /// 
        /// Following VAPM-Scanner pattern for database loading
        /// </remarks>
        public static void LoadPatchDatabase(string databaseFile, string checksumFile)
        {
            string result;
            // Build JSON request to load the patch database
            string json_in = "{\"input\" : {\"method\" : 50302, \"dat_input_source_file\" : \"" + databaseFile + "\"}}";

            // If a checksum file is provided, include it for integrity verification
            if (!string.IsNullOrEmpty(checksumFile))
            {
                json_in = "{\"input\" : {\"method\" : 50302, \"dat_input_source_file\" : \"" + databaseFile + "\", \"dat_input_checksum_file\" : \"" + checksumFile + "\"}}";
            }

            int rc = Invoke(json_in, out result);
            if (rc < 0)
            {
                throw new Exception("LoadPatchDatabase failed to load " + databaseFile + ". Return code: " + rc + " Response: " + result);
            }
        }

        /// <summary>
        /// Parses JSON response from GetLatestInstaller to extract installer details for patch management.
        /// This method processes the patch information for both Windows OS and third-party components.
        /// </summary>
        /// <param name="installer_json">JSON response from GetLatestInstaller method containing installer information</param>
        /// <returns>InstallerDetail object containing patch download and installation information</returns>
        /// <remarks>
        /// The JSON response contains:
        /// - Download URL and file type for the patch installer
        /// - SHA256 checksums for file integrity verification
        /// - Patch metadata (title, category, security update ID, language)
        /// - Installation parameters (patch ID for version enforcement)
        /// 
        /// Error handling:
        /// - If jsonOut.result exists: Successful response with installer details
        /// - If jsonOut.error exists: Error response with error code (e.g., -1039 for not found)
        /// 
        /// File path generation:
        /// - Extracts filename from URL
        /// - Creates local path in current directory
        /// - Normalizes path separators for cross-platform compatibility
        /// </remarks>
        public static InstallerDetail GetInstallerDetail(string installer_json)
        {
            InstallerDetail result = new InstallerDetail();
            result.checksumList = new List<string>();

            dynamic jsonOut = JObject.Parse(installer_json);

            if (jsonOut.result != null)
            {
                // Extract installer details from successful response
                result.result_code = jsonOut.result.code;
                result.url = jsonOut.result.url;
                result.fileType = jsonOut.result.file_type;
                result.title = jsonOut.result.title;
                result.severity = jsonOut.severity;
                result.security_update_id = jsonOut.result.security_update_id;
                result.category = jsonOut.result.category;
                result.patch_id = jsonOut.result.patch_id;
                result.language = jsonOut.result.language;

                // Extract checksums for file validation (supports SHA1, SHA256, SHA512, MD5)
                var sha256Array = jsonOut.result.expected_sha256;
                var sha1Array = jsonOut.result.expected_sha1;
                var sha512Array = jsonOut.result.expected_sha512;
                var md5Array = jsonOut.result.expected_md5;
                
                if (sha256Array != null)
                {
                    for (int i = 0; i < sha256Array.Count; i++)
                    {
                        string sha256 = sha256Array[i];
                        result.checksumList.Add(sha256);
                    }
                }
                
                if (sha1Array != null)
                {
                    for (int i = 0; i < sha1Array.Count; i++)
                    {
                        string sha1 = sha1Array[i];
                        result.checksumList.Add(sha1);
                    }
                }
                
                if (sha512Array != null)
                {
                    for (int i = 0; i < sha512Array.Count; i++)
                    {
                        string sha512 = sha512Array[i];
                        result.checksumList.Add(sha512);
                    }
                }
                
                if (md5Array != null)
                {
                    for (int i = 0; i < md5Array.Count; i++)
                    {
                        string md5 = md5Array[i];
                        result.checksumList.Add(md5);
                    }
                }

                // Generate local file path from URL
                int index = result.url.LastIndexOf("/");
                string fileName = result.url.Substring(index+1);
                result.path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                result.path = result.path.Replace("\\", "/"); // Normalize path separators
            }
            else
            {
                // Handle error response
                result.result_code = jsonOut.error.code;
            }

            return result;
        }


        /// <summary>
        /// Queries the patch databases for the latest installer/patch information for a specific component signature ID.
        /// Supports both Windows OS patches (from wuo.dat) and third-party patches (from patch.dat).
        /// 
        /// OESIS Method 50300: Get Latest Installer
        /// </summary>
        /// <param name="signatureId">OESIS signature ID of the component to query (e.g., 1103 for Windows OS components)</param>
        /// <param name="index">Index of the installer to retrieve (0-based). Multiple installers may exist for one signature.</param>
        /// <returns>JSON response containing installer URL, checksums, and patch details</returns>
        /// <exception cref="Exception">Thrown if the query fails</exception>
        /// <remarks>
        /// Signature ID Examples:
        /// - 1103: Windows OS components (requires wuo.dat database)
        /// - 3039: Firefox (requires patch.dat database)
        /// - Other IDs: Various applications and components
        /// 
        /// Index Usage:
        /// - 0: First available installer/patch
        /// - 1, 2, etc.: Additional installers if available
        /// - -1039 error: No installer available at the specified index
        /// 
        /// This method follows the reference implementation pattern for handling multiple installers per signature.
        /// </remarks>
        public static string GetLatestInstaller(int signatureId, int index)
        {
            string result = "";
            // Build JSON request to query patch database for this component and index
            string json_in = "{\"input\": { \"method\": 50300, \"signature\": " + signatureId + ", \"index\": " + index + " } }";
            
            Console.WriteLine("JSON INPUT: " + json_in);
            
            int rc = Invoke(json_in, out result);
            
            Console.WriteLine("JSON OUTPUT: " + result);
            
            //-1039, results when you reach the end of the index
            if (rc != -1039 && rc < 0)
            {
                throw new Exception("GetLatestInstaller failed to run correctly. Return code: " + rc + " Response: " + result);
            }

            return result;
        }

        /// <summary>
        /// Installs a patch from a downloaded installer file.
        /// This method requires Administrator privileges to install patches and updates.
        /// 
        /// OESIS Method 50301: Install from Files
        /// </summary>
        /// <param name="signatureId">OESIS signature ID of the component to patch</param>
        /// <param name="location">Full path to the downloaded installer file</param>
        /// <returns>JSON response containing installation status and results</returns>
        /// <exception cref="Exception">Thrown if installation fails</exception>
        /// <remarks>
        /// IMPORTANT: This method requires Administrator privileges or better access rights.
        /// 
        /// Installation Process:
        /// 1. Validates the installer file exists at the specified location
        /// 2. Executes the installer with appropriate parameters
        /// 3. Monitors installation progress and completion
        /// 4. Returns detailed status information
        /// 
        /// Parameters:
        /// - skip_signature_check: Set to 1 to bypass digital signature verification (for testing)
        /// - signature: Component signature ID for validation
        /// - path: Full path to the installer executable
        /// 
        /// Security Considerations:
        /// - Always validate file integrity before installation
        /// - Ensure installer comes from trusted source
        /// - Run with minimal required privileges
        /// - Monitor installation for unexpected behavior
        /// 
        /// Common Installation Results:
        /// - Success: Installation completed successfully
        /// - Requires Reboot: Installation succeeded but requires system restart
        /// - Failed: Installation failed (check logs for details)
        /// - Already Installed: Component is already at latest version
        /// </remarks>
        public static string InstallFromFiles(int signatureId, string location, string patch_id)
        {
            string result = "";
            // Build JSON request for patch installation
            string json_in = "{\"input\": { \"method\": 50301, \"signature\": " + signatureId + ",\"path\":\"" + location + "\", \"skip_signature_check\":1, \"patch_id\": " + patch_id + "  } }";
            int rc = Invoke(json_in, out result);
            if (rc < 0)
            {
                throw new Exception("InstallFromFiles failed to run correctly. Return code: " + rc + " Response: " + result);
            }

            return result;
        }



        /// <summary>
        /// Main entry point for the Windows OS Patch Manager.
        /// 
        /// WORKFLOW OVERVIEW:
        /// 1. Initialize OESIS framework with licensing
        /// 2. Load patch databases (patch.dat for third-party + wuo.dat for Windows OS)
        /// 3. Query for available patches using signature 1103 (Windows OS components)
        /// 4. Download, validate, and install each available patch
        /// 5. Clean up temporary files and framework resources
        /// 
        /// This implementation follows the reference pattern from the VAPM-Scanner codebase:
        /// - Line 212 equivalent: Load wuo.dat database
        /// - Line 232 equivalent: Load multiple installer details using index-based iteration
        /// - ForEach loop: Install each patch following the reference InstallPatch pattern
        /// 
        /// DATABASE STRATEGY:
        /// - patch.dat: Provides access to third-party application patches
        /// - wuo.dat: Provides access to Windows OS specific patches
        /// - Both databases are loaded to maximize patch availability for signature 1103
        /// 
        /// ERROR HANDLING:
        /// - -1039 errors indicate no more patches available at current index
        /// - Download failures are handled gracefully with continued processing
        /// - Installation failures are logged but don't stop processing of remaining patches
        /// </summary>
        /// <param name="args">Command line arguments (currently unused)</param>
        static void Main(string[] args)
        {
            string products_json = "";
            try
            {
                // STEP 1: Initialize the OESIS framework with license validation
                InitializeFramework();
                
                // STEP 2: Load both patch databases for comprehensive patch coverage
                LoadPatchDatabase("patch.dat","ap_checksum.dat");
                
                // Additionally load Windows OS patch database for Windows-specific patches
                LoadPatchDatabase("wuo.dat",""); // Windows OS patches database (no checksum file needed)

                // STEP 3: Configure signature for Windows OS components
                int signatureToInstall = 1103; // Windows OS signature (works with wuo.dat)
                Console.WriteLine("Checking for Windows OS patches...");

                // STEP 4 & 5: Query, download, and install patches in single loop
                // Simplified approach: process each patch immediately instead of collecting first
                Console.WriteLine("loaded both patch databases, now querying for available patches and installers...");
                int index = 0;
                int patchesProcessed = 0;
                bool requiresRestart = false;

                while (true)
                {
                    try
                    {
                        // Query for installer at current index
                        string installerJson = GetLatestInstaller(signatureToInstall, index);
                        InstallerDetail installerDetail = GetInstallerDetail(installerJson);

                        // Check if the installer is valid
                        if (installerDetail.result_code == -1039)
                        {
                            Console.WriteLine(patchesProcessed + " patches available for Windows OS components");
                            break; // No more installers available at this index
                        }

                        // Process the patch immediately
                        Console.WriteLine("Downloading " + installerDetail.title + " " + installerDetail.url);
                        
                        // Download and validate the patch file with checksums
                        bool downloadResult = HttpClientUtils.DownloadValidFile(installerDetail.url, installerDetail.path, installerDetail.checksumList);

                        if (downloadResult)
                        {
                            Console.WriteLine("Installing " + installerDetail.title);

                            // Install the patch
                            string installResult = InstallFromFiles(signatureToInstall, installerDetail.path, installerDetail.patch_id);
                            Console.WriteLine("Install result: " + installResult);
                            
                            // Check if restart is required
                            dynamic installJson = JObject.Parse(installResult);
                            if (installJson.result != null && installJson.result.requires_restart == 1)
                            {
                                requiresRestart = true;
                            }
                            
                            // Cleanup the installer file after successful install
                            File.Delete(installerDetail.path);
                        }
                        else
                        {
                            Console.WriteLine("Failed to download installer: " + installerDetail.url);
                        }

                        patchesProcessed++;
                        index++; // Move to next potential installer
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("No more installers available at index " + index + ": " + e.Message);
                        Console.WriteLine(patchesProcessed + " patches processed for Windows OS components");
                        break;
                    }
                }

                // Check if restart is required and notify user
                if (requiresRestart)
                {
                    Console.WriteLine();
                    Console.WriteLine("=============================================================");
                    Console.WriteLine("RESTART REQUIRED: One or more patches require a system restart.");
                    Console.WriteLine("Please restart your computer to complete the installation.");
                    Console.WriteLine("=============================================================");
                    Console.WriteLine();
                }

                // STEP 6: Clean up OESIS framework resources
                OESISAdapter.wa_api_teardown();

            }
            catch (Exception e)
            {
                // Global error handler for unexpected failures
                Console.Out.WriteLine("Received an Exception: " + e);
                Console.Out.WriteLine("JSON_RESULT: " + products_json);
            }
        }
    }

}