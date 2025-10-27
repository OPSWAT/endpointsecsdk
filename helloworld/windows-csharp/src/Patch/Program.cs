///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
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

namespace Patch
{
    internal class Program
    {

        // Note this is just passing in the json_result to be able to print it out on a failure
        private static void CheckSuccess(int rc)
        {
            // Return code link: https://software.opswat.com/OESIS_V4/html/c_return_codes.html
            // Note any code above 0 is a SUCCESS
            if (rc < 0)
            {
                Console.Out.WriteLine("Failed to execute call: " + rc);
                throw new Exception();
            }
        }


        // This is the main call used to send JSON in and out of the API
        private static int Invoke(string json_config, out string json_out)
        {
            IntPtr outPtr = IntPtr.Zero;
            int rc = OESISAdapter.wa_api_invoke(json_config, out outPtr);
            json_out = "{ }";
            if (outPtr != IntPtr.Zero)
            {
                json_out = XStringMarshaler.PtrToString(outPtr);
                OESISAdapter.wa_api_free(outPtr);
            }
            return rc;
        }

        private static void InitializeFramework()
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string passKeyPath = Path.Combine(currentPath, "pass_key.txt");

            // Check to makes sure the license is in the directory
            if (!File.Exists(passKeyPath))
            {
                Console.WriteLine("Could not find a pass_key.txt file.  Make sure that the license provided during evaluation is in the 'solutionRoot/license' directory.  That will compy the file to each sample code.");
                throw new Exception("License pass_key.txt file not found");
            }


            // This code is used to initialize the OESIS Framework
            // The folling linkk describes the setup
            // https://software.opswat.com/OESIS_V4/html/c_sdk.html
            //
            string passkey = File.ReadAllText(passKeyPath);
            string config = "{ \"config\" : { \"passkey_string\": \"" + passkey + "\", \"enable_pretty_print\": true, \"online_mode\": true, \"silent_mode\": true } }";


            IntPtr outPtr = IntPtr.Zero;
            // Note if you get a Bad Image exception, that may be because Prefer 32-bit is checked
            int rc = OESISAdapter.wa_api_setup(config, out outPtr);
            string json_out = "{ }";
            if (outPtr != IntPtr.Zero)
            {
                json_out = XStringMarshaler.PtrToString(outPtr);
                OESISAdapter.wa_api_free(outPtr);
            }
            else
            {
                Console.Out.WriteLine("Failed to initialize OESIS: " + rc);
                // Refer to the following doc for errors:  https://software.opswat.com/OESIS_V4/html/c_return_codes.html

                throw new Exception("Failed to initialize");
            }
        }

        //
        // Need more details on what this file is
        // 
        public static void LoadPatchDatabase(string databaseFile, string checksumFile)
        {
            string result;
            string json_in = "{\"input\" : {\"method\" : 50302, \"dat_input_source_file\" : \"" + databaseFile + "\"}}";

            if (!string.IsNullOrEmpty(checksumFile))
            {
                json_in = "{\"input\" : {\"method\" : 50302, \"dat_input_source_file\" : \"" + databaseFile + "\", \"dat_input_checksum_file\" : \"" + checksumFile + "\"}}";
            }

            int rc = Invoke(json_in, out result);
            if (rc < 0)
            {
                throw new Exception("LoadPatchDatabase failed to run correctly.  " + result);
            }
        }

        // Expects JSON from GetLatestInstaller Products
        public static InstallerDetail GetInstallerDetail(string installer_json)
        {
            InstallerDetail result = new InstallerDetail();
            result.checksumList = new List<string>();

            dynamic jsonOut = JObject.Parse(installer_json);

            if (jsonOut.result != null)
            {
                result.result_code = jsonOut.result.code;
                result.url = jsonOut.result.url;
                result.fileType = jsonOut.result.file_type;
                result.title = jsonOut.result.title;
                result.severity = jsonOut.severity;
                result.security_update_id = jsonOut.result.security_update_id;
                result.category = jsonOut.result.category;
                result.patch_id = jsonOut.result.patch_id;
                result.language = jsonOut.result.language;

                var md5Array = jsonOut.result.expected_sha256;
                if (md5Array != null)
                {
                    for (int i = 0; i < md5Array.Count; i++)
                    {
                        string md5 = md5Array[i];
                        result.checksumList.Add(md5);
                    }
                }

                // This can be the name of the installer from the URL or the name on the end of the URL
                int index = result.url.LastIndexOf("/");
                string fileName = result.url.Substring(index+1);
                result.path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                result.path = result.path.Replace("\\", "/");
            }
            else
            {
                result.result_code = jsonOut.error.code;
            }

            return result;
        }


        public static string GetLatestInstaller(int signatureId)
        {
            string result = "";
            string json_in = "{\"input\": { \"method\": 50300, \"signature\": " + signatureId + " } }";
            int rc = Invoke(json_in, out result);
            if (rc < 0)
            {
                throw new Exception("GetProductVulnerability failed to run correctly.  " + result);
            }

            return result;
        }

        // !!!! This requires Administrator or better access
        public static string InstallFromFiles(int signatureId, string location)
        {

            string result = "";
            string json_in = "{\"input\": { \"method\": 50301, \"signature\": " + signatureId + ",\"path\":\"" + location + "\", \"skip_signature_check\":1 } }";
            int rc = Invoke(json_in, out result);
            if (rc < 0)
            {
                throw new Exception("GetProductVulnerability failed to run correctly.  " + result);
            }

            return result;
        }



        static void Main(string[] args)
        {
            string products_json = "";
            try
            {
                // Clean the output file
                StringBuilder resultString = new StringBuilder();
                resultString.Append("[");

                // Setup the default initialization
                InitializeFramework();
                LoadPatchDatabase("patch.dat","ap_checksum.dat"); // This file is downloaded normally from the OPSWAT CDN Server


                // 
                // For this sample it will download and install Firefox.  It will patch firefox to the latest or install a fresh version
                // signatureId = 3039
                //
                int signatureToInstall = 3039; //Firefox
                string installerJson = GetLatestInstaller(signatureToInstall); // Notice the hard coded id
                InstallerDetail installerDetail = GetInstallerDetail(installerJson);

                //
                // Now download and validate the file
                //
                bool downloadResult = HttpClientUtils.DownloadValidFile(installerDetail.url, installerDetail.path, installerDetail.checksumList[0]);

                //
                // Install the patch - This will require Administrator Access
                //
                if (downloadResult)
                {
                    string result = InstallFromFiles(signatureToInstall, installerDetail.path);
                    Console.WriteLine(result);
                }
                else
                {
                    Console.WriteLine("Failed to download installer: " + installerDetail.url);
                }

                OESISAdapter.wa_api_teardown();

            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Received an Exception: " + e);
                Console.Out.WriteLine("JSON_RESULT: " + products_json);
            }
        }
    }

}
