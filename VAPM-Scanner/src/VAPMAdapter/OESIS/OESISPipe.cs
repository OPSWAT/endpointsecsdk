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
using System.Text;
using System.Text.Json.Nodes;
using static System.Net.WebRequestMethods;

namespace VAPMAdapter.OESIS
{
    internal class OESISPipe
    {
        public static void InitializeFramework(bool enableLogging)
        {
            // This code is used to initialize the OESIS Framework
            // The folling linkk describes the setup
            // https://software.opswat.com/OESIS_V4/html/c_sdk.html
            //

            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string passkey = System.IO.File.ReadAllText(path + "/pass_key.txt");
            StringBuilder configString = new StringBuilder();

            configString.Append("{");
            configString.Append("\"config\":{\"passkey_string\":\"" + passkey + "\", \"enable_pretty_print\":true, \"online_mode\":false, \"silent_mode\":true}");

            if(enableLogging)
            {
                configString.Append(",");
                configString.Append("\"config_debug\":{\"debug_log_level\":\"ALL\"}");
            }

            configString.Append("}");


            // Note if you get a Bad Image exception, that may be because Prefer 32-bit is checked
            IntPtr outPtr = IntPtr.Zero;
            int rc = OESISAdapter.wa_api_setup(configString.ToString(), out outPtr);
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


        public static string GetLatestInstaller(string signatureId, int download, int index, string language, bool backgroundInstall, bool validateInstaller, string downloadPath)
        {
            string result = "";

            JsonObject inputObject = new JsonObject();
            inputObject.Add("method", 50300);
            inputObject.Add("signature", int.Parse(signatureId));
            inputObject.Add("download", download);
            inputObject.Add("index", index);

            if (language != null)
            {
                inputObject.Add("language", language.ToString());
            }

            if(validateInstaller)
            {
                inputObject.Add("validate_installer", 1);
            }

            if (backgroundInstall)
            {
                inputObject.Add("background", 1);
            }

            if (downloadPath != null)
            {
                inputObject.Add("path", downloadPath.ToString());
            }

            JsonObject json = new JsonObject();
            json.Add("input", inputObject);

            int rc = Invoke(json.ToJsonString(), out result);

            if (rc < 0 && rc != -1039)//Ignore -1039 since that is end of index
            {
                throw new Exception("GetLatestInstaller failed to run correctly.  " + result);
            }

            return result;
        }

        public static string GetLatestInstaller(string signatureId, int download, string downloadPath, string language, bool isBackgroundInstall)
        {
            string result = GetLatestInstaller(signatureId, download, 0, language, isBackgroundInstall, false, downloadPath);
            return result;
        }

        public static string GetLatestInstaller(string signatureId, string language)
        {
            return GetLatestInstaller(signatureId, 0, 0,language,false,false,null);
        }

        public static string GetLatestInstallerScan(string signatureId,int index)
        {
            return GetLatestInstaller(signatureId, 2, index, null, false, false, null);
        }


        // !!!! This requires Administrator or better access
        public static string InstallFromFiles(string signatureId, string location, string patchId, string language, bool force_close, bool isBackground)
        {
            string result;

            JsonObject inputObject = new JsonObject();
            inputObject.Add("method", 50301);
            inputObject.Add("signature", int.Parse(signatureId));
            inputObject.Add("path", location);

            if (force_close)
            {
                inputObject.Add("force_close", 1);
            }

            if (language != null)
            {
                inputObject.Add("language", language.ToString());
            }

            if (patchId != null)
            {
                inputObject.Add("patch_id", int.Parse(patchId));
            }

            if (isBackground)
            {
                inputObject.Add("background", 1);
            }

 
            JsonObject json = new JsonObject();
            json.Add("input", inputObject);

            int rc = Invoke(json.ToJsonString(), out result);
            if (rc < 0)
            {
                throw new OESISException("InstallFromFiles failed to run correctly.", result);
            }

            return result;
        }


        public static string GetOSInfo()
        {
            string result;

            string json_in = "{\"input\" : { \"method\" : 1}}";
            int rc = Invoke(json_in, out result);
            if (rc < 0)
            {
                throw new Exception("GetOSInfo failed to run correctly.  " + result);
            }

            return result;
        }
    }

}
