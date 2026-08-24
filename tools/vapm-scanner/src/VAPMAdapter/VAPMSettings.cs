///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////


using System;
using System.IO;
using Newtonsoft.Json;
using VAPMAdapater.Log;

namespace VAPMAdapater
{
    internal class VAPMSettings
    {
        //
        // Please email me for the values for %download_token% and %SDK-URL%.   You need these values for the auto download to work
        // Email: christopher.seiler@opswat.com
        //
        private static string VCR_URL = "https://vcr.opswat.com/gw/file/download/%file%?type=1&token=%token%";

        private const string DEFAULT_TOKEN_FILE = "download_token.txt";

        // The catalog ("analog") ships in two flavors on the same VCR path: the production file
        // (the current running catalog) and the staging file. Same host and token - only the file
        // name differs.
        private const string PRODUCTION_CATALOG_FILE = "analog.zip";
        private const string STAGING_CATALOG_FILE    = "dev-analog.zip";

        // Optional, runtime-only config placed next to the executable. It selects which catalog
        // flavor "Update Catalog" downloads. Absent or channel="production" => production (default):
        //
        //   { "channel": "staging" }                  // downloads dev-analog.zip
        //   { "channel": "production" }               // downloads analog.zip (same as no file)
        //   { "analogFile": "some-other-analog.zip" } // explicit file name, overrides channel
        //
        private const string CATALOG_CONFIG_FILE = "catalog_config.json";

        //public static string THIRD_PARTY_VULNERABILITY_DB   = "vmod.dat";
        public static string THIRD_PARTY_VULNERABILITY_DB = "v2mod.dat";
        public static string THIRD_PARTY_PATCH_DB           = "patch.dat";
        public static string WINDOWS_VULNERABILITY_DB       = "wiv-lite.dat";
        public static string WINDOWS_PATCH_DB               = "wuo.dat";
        public static string PATCH_CHECKSUMS_DB             = "ap_checksum.dat";
        // Driver/firmware (incl. BIOS) metadata database, loaded via OESIS method 50900.
        public static string DRIVER_FIRMWARE_DB             = "patch_driver_firmware.dat";

        private static string GetDownloadToken()
        {
            if (!File.Exists(DEFAULT_TOKEN_FILE))
            {
                throw new Exception("Make sure there is a download token file available in the running directory: " + Directory.GetCurrentDirectory());
            }

            return File.ReadAllText(DEFAULT_TOKEN_FILE);
        }

        public static string GetTokenDownloadURL(string fileName)
        {
            string downloadToken = GetDownloadToken();
            string result = VCR_URL.Replace("%token%", downloadToken);
            result = result.Replace("%file%", fileName);
            return result;
        }

        public static string GetSDKURL()
        {
            return GetTokenDownloadURL("OesisPackageLinks.xml");
        }

        public static string GetCatalogURL()
        {
            // Same VCR path and token for both flavors - only the file name differs.
            return GetTokenDownloadURL(GetCatalogFileName());
        }

        // Human-readable description of the configured catalog channel, e.g.
        // "staging (dev-analog.zip)" or "production (analog.zip)". For display in the UI.
        public static string GetCatalogChannelDescription()
        {
            string file = GetCatalogFileName();
            if (string.Equals(file, STAGING_CATALOG_FILE, StringComparison.OrdinalIgnoreCase))
            {
                return "staging (" + file + ")";
            }
            if (string.Equals(file, PRODUCTION_CATALOG_FILE, StringComparison.OrdinalIgnoreCase))
            {
                return "production (" + file + ")";
            }
            return "custom (" + file + ")";
        }

        // Shape of the optional catalog_config.json.
        private class CatalogConfigFile
        {
            public string channel { get; set; }     // "production" (default) or "staging"
            public string analogFile { get; set; }  // optional explicit file name; overrides channel
        }

        // Chooses which catalog file to download. No config file (or channel != "staging") means
        // production - identical to the previous behavior. A malformed file falls back to production
        // with a log so a bad config never blocks the (default) production download.
        private static string GetCatalogFileName()
        {
            if (!File.Exists(CATALOG_CONFIG_FILE))
            {
                return PRODUCTION_CATALOG_FILE;
            }

            CatalogConfigFile cfg;
            try
            {
                cfg = JsonConvert.DeserializeObject<CatalogConfigFile>(File.ReadAllText(CATALOG_CONFIG_FILE));
            }
            catch (Exception ex)
            {
                Logger.Log(CATALOG_CONFIG_FILE + " could not be parsed (" + ex.Message + "); using production " + PRODUCTION_CATALOG_FILE + ".");
                return PRODUCTION_CATALOG_FILE;
            }

            if (cfg == null)
            {
                return PRODUCTION_CATALOG_FILE;
            }

            // An explicit file name wins over the channel shorthand.
            if (!string.IsNullOrWhiteSpace(cfg.analogFile))
            {
                Logger.Log("Catalog file: " + cfg.analogFile + " (per " + CATALOG_CONFIG_FILE + ")");
                return cfg.analogFile;
            }

            if (string.Equals(cfg.channel, "staging", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Log("Catalog source: STAGING (" + STAGING_CATALOG_FILE + ")");
                return STAGING_CATALOG_FILE;
            }

            return PRODUCTION_CATALOG_FILE;
        }

        public static string GetStatusURL()
        {
            return GetTokenDownloadURL("patch_status.json");
        }


        public static string GetLocalCatalogDir()
        {
            //
            // First delete the SDK directory if it exists
            //
            string sdkDir = Path.Combine(Directory.GetCurrentDirectory(), "catalog");
            return sdkDir;
        }




    }
}
