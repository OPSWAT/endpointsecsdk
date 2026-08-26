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

namespace VAPMAdapter.OESIS
{
    internal class OESISPipe
    {
        public static void InitializeFramework(bool enableLogging)
        {
            // This code is used to initialize the OESIS Framework
            // The following link describes the setup
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
            // Close the framework, then unload libwaapi.dll so the engine files are no longer
            // locked on disk. This is what allows "Update SDK" to replace them in-process
            // (without it, libwaapi/libwaheap/libwautils stay pinned until the app exits).
            OESISAdapter.wa_api_teardown();
            OESISAdapter.Unload();
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
                if (rc == -1005)
                {
                    // Return a specific message for the -1005 error code
                    return "-1005";
                }
                else
                {
                    throw new Exception("GetProductVulnerability failed to run correctly.  " + result);
                }
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

        //
        // Loads the driver/firmware metadata database (patch_driver_firmware.dat). This initializes
        // the driver/firmware vmod component; without it DetectDriverFirmwarePatches fails with
        // rc=-5 (WAAPI_ERROR_NOT_INITIALIZED).
        // https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50900
        //
        public static void LoadDriverFirmwareDatabase(string databaseFile)
        {
            string result;
            string json_in = "{\"input\" : {\"method\" : 50900, \"input_path\" : \"" + databaseFile.Replace("\\", "\\\\") + "\"}}";

            int rc = Invoke(json_in, out result);
            if (rc < 0)
            {
                throw new Exception("LoadDriverFirmwareDatabase failed to run correctly (rc=" + rc + ").  " + result);
            }
        }

        //
        // Collects the endpoint inventory the driver/firmware matcher works from: system identity,
        // OS, BIOS and the full hardware device list with driver and firmware versions. This is the
        // SDK's own answer to "what is on this machine", so the view never needs Windows APIs.
        // Returns result.inventory_collection with system / os / bios / devices.
        // Windows only.
        // https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50901
        //
        public static string CollectDeviceInventory(string outputFile)
        {
            string result;
            // output_file is optional; when given the engine also writes the inventory to disk.
            string json_in = string.IsNullOrEmpty(outputFile)
                ? "{\"input\" : { \"method\" : 50901 }}"
                : "{\"input\" : { \"method\" : 50901, \"output_file\" : \"" +
                  outputFile.Replace("\\", "\\\\") + "\" }}";

            int rc = Invoke(json_in, out result);
            if (rc < 0)
            {
                throw new Exception("CollectDeviceInventory failed to run correctly (rc=" + rc + ").  " + result);
            }

            return result;
        }

        //
        // Detects applicable driver/firmware patches (incl. BIOS) by matching the collected system
        // inventory against the loaded driver/firmware database. With no inventory the engine
        // collects it internally. Returns the raw JSON and the return code so the caller can treat
        // the "not covered" codes as a clean no-coverage outcome rather than a failure.
        // Windows only.
        // https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50902
        //
        public static int DetectDriverFirmwarePatches(out string result)
        {
            string json_in = "{\"input\" : { \"method\" : 50902 }}";
            int rc = Invoke(json_in, out result);
            // Coverage gaps, not call failures - let them through for the caller to handle:
            //   -1066 WA_VMOD_ERROR_VENDOR_NOT_SUPPORTED: this hardware vendor has no
            //         driver/firmware coverage at all (common on VMs and white-box hardware).
            //   -1067 WA_VMOD_ERROR_MODEL_NOT_SUPPORTED: the vendor is covered, this model is not.
            // Both mean "we cannot patch this box", which is a normal answer on a demo machine and
            // must not discard the device inventory we can still show.
            if (rc < 0 && rc != VENDOR_NOT_SUPPORTED && rc != MODEL_NOT_SUPPORTED)
            {
                throw new Exception("DetectDriverFirmwarePatches failed to run correctly (rc=" + rc + ").  " + result);
            }

            return rc;
        }

        public const int VENDOR_NOT_SUPPORTED = -1066;
        public const int MODEL_NOT_SUPPORTED = -1067;

        //
        // (Windows only) Installs a driver/firmware/BIOS update package that was detected by
        // DetectDriverFirmwarePatches (method 50902) and downloaded locally. The engine may extract
        // the package to a temp directory and writes installer logs alongside it.
        //
        // Unlike the other pipe calls this does NOT throw on rc<0: a failed install (bad package,
        // insufficient privileges, or a blocked BIOS preflight check) is a normal result the caller
        // needs to report with its exact code, not an exception. The raw JSON carries
        // install_return_code, require_restart and log_paths.
        // https://software.opswat.com/OESIS_V4/html/c_method.html -> method 50903
        //
        public static int InstallDriverFirmwareUpdate(string patchId, string installerPath, out string result)
        {
            JsonObject inputObject = new JsonObject();
            inputObject.Add("method", 50903);
            inputObject.Add("patch_id", patchId);
            inputObject.Add("installer_path", installerPath);

            JsonObject json = new JsonObject();
            json.Add("input", inputObject);

            return Invoke(json.ToJsonString(), out result);
        }

        //
        // Device Info family. These are how the scanner learns what hardware it is running on -
        // the SDK is the source of truth for that, not Windows APIs, because an OEM embedding
        // OESIS gets exactly these answers and nothing more.
        //

        //
        // Computer manufacturer and model, e.g. { "manufacturer": "Dell Inc.", "model":
        // "Latitude 5450" }. This is the identity the driver/firmware catalog matches on, so it
        // is what to show when a device turns out not to be covered.
        // https://software.opswat.com/OESIS_V4/html/c_method.html -> method 30001
        //
        public static string GetPCModel()
        {
            string result;
            int rc = Invoke("{\"input\" : { \"method\" : 30001 }}", out result);
            if (rc < 0)
            {
                throw new Exception("GetPCModel failed to run correctly (rc=" + rc + ").  " + result);
            }

            return result;
        }

        //
        // Installed hardware: video adapters and network adapters.
        // https://software.opswat.com/OESIS_V4/html/c_method.html -> method 30003
        //
        public static string GetPCComponents()
        {
            string result;
            int rc = Invoke("{\"input\" : { \"method\" : 30003 }}", out result);
            if (rc < 0)
            {
                throw new Exception("GetPCComponents failed to run correctly (rc=" + rc + ").  " + result);
            }

            return result;
        }

        //
        // Whether this machine is a virtual machine. Worth surfacing next to an unsupported-device
        // notice: a VM has no real vendor firmware, which is usually the reason it is uncovered.
        // https://software.opswat.com/OESIS_V4/html/c_method.html -> method 30006
        //
        public static string IsCurrentDeviceVirtual()
        {
            string result;
            int rc = Invoke("{\"input\" : { \"method\" : 30006 }}", out result);
            if (rc < 0)
            {
                throw new Exception("IsCurrentDeviceVirtual failed to run correctly (rc=" + rc + ").  " + result);
            }

            return result;
        }
    }

}
