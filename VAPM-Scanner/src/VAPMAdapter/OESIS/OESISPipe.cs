///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Reflection;

namespace VAPMAdapter.OESIS
{
    internal class OESISPipe
    {
        public static void InitializeFramework(bool onlineMode)
        {
            // This code is used to initialize the OESIS Framework
            // The folling linkk describes the setup
            // https://software.opswat.com/OESIS_V4/html/c_sdk.html
            //

            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string passkey = File.ReadAllText(path + "/pass_key.txt");
            string config = "{ \"config\" : { \"passkey_string\": \"" + passkey + "\", \"enable_pretty_print\": true, \"online_mode\": false, \"silent_mode\": true } }";

            if (onlineMode)
            {
                config = "{ \"config\" : { \"passkey_string\": \"" + passkey + "\", \"enable_pretty_print\": true, \"online_mode\": true, \"silent_mode\": true } }";
            }

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

        public static void Teardown()
        {
            OESISAdapter.wa_api_teardown();
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


        // 
        // This will return JSON for all of the products found in the system
        // https://software.opswat.com/OESIS_V4/html/c_method.html
        // on the left select OESIS Core/Discover Products
        public static string DetectProducts()
        {
            string result = "";
            string json_in = "{\"input\": { \"method\": 0 } }";
            int rc = Invoke(json_in, out result);

            if (rc < 0)
            {
                throw new Exception("DetectProducts failed to run correctly.  " + result);
            }

            return result;
        }

        //
        // This loads a database of CVE information
        // 
        public static void ConsumeOfflineVmodDatabase(string databaseFile)
        {
            string json_in = "{\"input\" : {\"method\" : 50520, \"dat_input_source_file\" : \"" + databaseFile + "\"}}";
            string result;

            int rc = Invoke(json_in, out result);
            if (rc < 0)
            {
                throw new Exception("ConsumeOfflineVmodDatabase failed to run correctly.  " + result);
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

        public static string GetProductVersion(string signatureId)
        {
            string result;

            string json_in = "{\"input\": { \"method\": 100, \"signature\": " + signatureId + " } }";
            int rc = Invoke(json_in, out result);

            if (rc < 0)
            {
                throw new Exception("GetProductPatchLevel failed to run correctly.  " + result);
            }

            return result;
        }


        public static string GetProductPatchLevel(string signatureId)
        {
            string result;

            string json_in = "{\"input\": { \"method\": 50500, \"signature\": " + signatureId + " } }";
            int rc = Invoke(json_in, out result);

            if (rc < 0)
            {
                throw new Exception("GetProductPatchLevel failed to run correctly.  " + result);
            }

            return result;
        }

        public static string GetProductVulnerability(string signatureId)
        {
            string result = "";
            string json_in = "{\"input\": { \"method\": 50505, \"signature\": " + signatureId + " } }";
            int rc = Invoke(json_in, out result);
            if (rc < 0)
            {
                throw new Exception("GetProductVulnerability failed to run correctly.  " + result);
            }

            return result;
        }

        public static string GetMissingPatches(string signatureId)
        {
            string result = "";
            string json_in = "{\"input\": { \"method\": 1013, \"signature\": " + signatureId + " } }";
            int rc = Invoke(json_in, out result);
            if (rc < 0)
            {
                throw new Exception("GetMissingPatches failed to run correctly.  " + result);
            }

            return result;
        }


        public static string GetInstalledPatches(string signatureId)
        {
            string result = "";
            string json_in = "{\"input\": { \"method\": 1023, \"signature\": " + signatureId + " \"query_history\":true } }";
            int rc = Invoke(json_in, out result);
            if (rc < 0)
            {
                throw new Exception("GetInstalledPatches failed to run correctly.  " + result);
            }

            return result;
        }

        public static string DownloadMissingPatches(string signatureId, string title, string product, string vendor )
        {
            string result = "";
            string json_in = "{\"input\": { \"method\": 1016, \"signature\": " + signatureId + " \"patches\": [{ \"title\":\"" + title + "\", \"product\":\"" + product + "\",\"vendor\":\"" + vendor + "\"}] } }";
            int rc = Invoke(json_in, out result);
            if (rc < 0)
            {
                throw new Exception("GetInstalledPatches failed to run correctly.  " + result);
            }

            return result;
        }

        public static string InstallMissingPatches(string signatureId, string title, string product, string vendor)
        {
            string result = "";
            string json_in = "{\"input\": { \"method\": 1014, \"signature\": " + signatureId + " \"patches\": [{ \"title\":\"" + title + "\", \"product\":\"" + product + "\",\"vendor\":\"" + vendor + "\"}] } }";
            int rc = Invoke(json_in, out result);
            if (rc < 0)
            {
                throw new Exception("GetInstalledPatches failed to run correctly.  " + result);
            }

            return result;
        }


        public static string GetLatestInstaller(string signatureId, int download, int index)
        {
            string result = "";
            // This is used to demonstrate languages
            //string json_in = "{\"input\" : {\"method\" : 50300, \"signature\" :" + signatureId + ", \"download\": " + download + ", \"index\" : " + index + ", \"language\" : \"fr-lu\" }}";

            string json_in = "{\"input\" : {\"method\" : 50300, \"signature\" :" + signatureId + ", \"download\": " + download + ", \"index\" : " + index + "}}";
            if (index == -1)
            {
                json_in = "{\"input\" : {\"method\" : 50300, \"signature\" :" + signatureId + ", \"download\": " + download + " }}";
            }

            int rc = Invoke(json_in, out result);


            if (rc < 0 && rc != -1039)//Ignore -1039 since that is end of index
            {
                throw new Exception("GetLatestInstaller failed to run correctly.  " + result);
            }

            return result;
        }

        public static string GetLatestInstaller(string signatureId)
        {
            return GetLatestInstaller(signatureId, 0, -1);
        }


        // !!!! This requires Administrator or better access
        public static string InstallFromFiles(string signatureId, int force_close, string location)
        {
            string result;

            if (force_close != 1)
            {
                force_close = 0;
            }

            string json_in = "{\"input\" : { \"method\" : 50301, \"signature\" :" + signatureId + ", \"force_close\" : " + force_close + ", \"path\":" + "\"" + location + "\" }}";
            int rc = Invoke(json_in, out result);
            if (rc < 0)
            {
                throw new Exception("InstallFromFiles failed to run correctly.  " + result);
            }

            return result;
        }
    }

}
