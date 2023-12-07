///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using VAPMAdapater;
using VAPMAdapater.Log;
using VAPMAdapter.OESIS.POCO;

namespace VAPMAdapter.OESIS
{
    internal class OESISUtil
    {

        // Expects JSON from DETECT Products
        public static List<Product> GetProductList(string detect_product_json)
        {
            List<Product> result = new List<Product>();

            dynamic jsonOut = JObject.Parse(detect_product_json);
            var products = jsonOut.result.detected_products;

            for (int i = 0; i < products.Count; i++)
            {
                Product newProduct = new Product();
                newProduct.signatureId = products[i].signature;
                newProduct.name = (string)products[i].product.name;
                newProduct.vendor = (string)products[i].vendor.name;
                result.Add(newProduct);
            }

            return result;
        }


        // Expects JSON from GetProductVulnerability Products
        public static List<CVEDetail> GetCVEDetailList(Product product, string product_vulnerability_json)
        {
            List<CVEDetail> result = new List<CVEDetail>();

            dynamic jsonOut = JObject.Parse(product_vulnerability_json);
            var cveList = jsonOut.result.cves;

            for (int i = 0; i < cveList.Count; i++)
            {
                CVEDetail cveDetail = new CVEDetail();
                cveDetail.product = product;
                cveDetail.cveId = (string)cveList[i].cve;
                cveDetail.description = (string)cveList[i].description;
                cveDetail.opswatSeverity = (int)cveList[i].severity_index;
                cveDetail.rawData = product_vulnerability_json;
                result.Add(cveDetail);
            }

            return result;
        }


        // Expects JSON from GetProductVulnerability Products
        public static PatchLevelDetail GetPatchLevelDetail(string patch_level_json)
        {
            PatchLevelDetail result;

            dynamic jsonOut = JObject.Parse(patch_level_json);
            var patchLevel = jsonOut.result;

            result = new PatchLevelDetail();
            result.latestVersion = patchLevel.details.version;
            result.isLatest = (bool)patchLevel.is_current;

            return result;
        }



        // Expects JSON from GetProductVulnerability Products
        public static VersionDetail GetVersionDetail(string version_json)
        {
            VersionDetail result;

            dynamic jsonOut = JObject.Parse(version_json);
            var versionJson = jsonOut.result;

            result = new VersionDetail();
            result.version = versionJson.version.ToString();
            result.architecture = versionJson.architecture.name.ToString();
            result.language = versionJson.language.name.ToString();

            return result;
        }

        // Expects JSON from GetProductVulnerability Products
        public static List<OnlinePatchDetail> GetOnlinePatchDetail(string version_json, bool installed)
        {
            List<OnlinePatchDetail> result = new List<OnlinePatchDetail>();

            dynamic jsonOut = JObject.Parse(version_json);
            var onlinePatchJson = jsonOut.result;

            var patchList = jsonOut.result.patches;

            for (int i = 0; i < patchList.Count; i++)
            {
                OnlinePatchDetail patchDetail = new OnlinePatchDetail();
                patchDetail.title = patchList[i].title;
                patchDetail.kb = patchList[i].kb_id;
                patchDetail.severity = patchList[i].severity;
                patchDetail.vendor = patchList[i].vendor;
                patchDetail.product = patchList[i].product;
                patchDetail.description = patchList[i].description;
                patchDetail.installed = installed;

                result.Add(patchDetail);
            }

            return result;
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
                result.path = jsonOut.result.path;
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
            }
            else
            {
                result.result_code = jsonOut.error.code;
            }

            return result;
        }


        // Expects JSON from GetLatestInstaller Products
        public static OSInfoDetail GetOSInfo(string osInfo_json)
        {
            OSInfoDetail result = new OSInfoDetail();

            dynamic jsonOut = JObject.Parse(osInfo_json);
            if (jsonOut.result != null)
            {
                result.code = jsonOut.result.code;
                result.version = jsonOut.result.version;
                result.architecture = jsonOut.result.architecture;
                result.service_pack = jsonOut.result.service_pack;
                result.os_type = jsonOut.result.os_type;
                result.os_id = jsonOut.result.os_id;
                result.compatibility_mode_detected = jsonOut.result.details.compatibility_mode_detected;
                result.computer_type = jsonOut.result.details.computer_type;
                result.os_language = jsonOut.result.details.os_language;
                result.domain = jsonOut.result.details.domain;
                result.netbios_name = jsonOut.result.details.netbios_name;
                result.host_name = jsonOut.result.details.host_name;
            }
            else
            {
                result.code = jsonOut.error.code;
            }

            return result;
        }



        // Expects JSON from GetLatestInstaller Products
        public static InstallResults GetInstallResults(string install_results_json)
        {
            InstallResults result = new InstallResults();

            dynamic jsonOut = JObject.Parse(install_results_json);
            result.result_code = jsonOut.result.code;
            result.require_restart = jsonOut.result.require_restart;
            result.version = jsonOut.result.version;

            if (result.result_code >= 0)
            {
                result.success = true;
            }
            else
            {
                result.success = false;
            }

            return result;
        }


        private static string GetFile256Checksum(string filePath)
        {
            string result = null;
            using (SHA256 mySHA256 = SHA256.Create())
            {
                FileInfo fInfo = new FileInfo(filePath);

                // Compute and print the hash values for each file in directory.
                using (FileStream fileStream = fInfo.Open(FileMode.Open))
                {
                    try
                    {
                        // Create a fileStream for the file.
                        // Be sure it's positioned to the beginning of the stream.
                        fileStream.Position = 0;
                        // Compute the hash of the fileStream.
                        byte[] hashValue = mySHA256.ComputeHash(fileStream);

                        StringBuilder resultSb = new StringBuilder();
                        foreach (byte b in hashValue)
                            resultSb.Append(b.ToString("x2"));

                        // The Expected Checksum is all upper case
                        result = resultSb.ToString().ToUpper();
                    }
                    catch (IOException e)
                    {
                        Logger.Log($"I/O Exception: {e.Message}");
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Logger.Log($"Access Exception: {e.Message}");
                    }
                }
            }

            return result;
        }


        public static string ParseMacrosInUrl(InstallerDetail installerDetail)
        {
            string result = installerDetail.url;
            return result;
        }

        public static string GetFilenameFromUrl(InstallerDetail installerDetail)
        {
            string result = installerDetail.url;

            int lastIndex = result.LastIndexOf("/") + 1;
            result = result.Substring(lastIndex);

            if (string.IsNullOrEmpty(result))
            {
                result = installerDetail.url.GetHashCode() + "." + installerDetail.fileType;
            }

            return result;
        }

        public static bool ValidateChecksum(InstallerDetail installerDetail, string localFile)
        {
            bool result = false;

            string localChecksum = GetFile256Checksum(localFile);

            // Depending on the resource for the checksum there can be multiple checksums
            // this usually happens when downloading a checksum from different regions
            foreach (string current in installerDetail.checksumList)
            {
                if (localChecksum == current)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }


        public static string GetCSVString(string inputString)
        {
            string result = inputString;

            result = inputString.Replace(",", "_");

            return result;
        }

        public static bool ValidateDatabaseFiles()
        {
            bool result = true;

            if (!File.Exists(VAPMSettings.THIRD_PARTY_VULNERABILITY_DB))
            {
                result = false;
            }

            if (!File.Exists(VAPMSettings.THIRD_PARTY_PATCH_DB))
            {
                result = false;
            }

            if (!File.Exists(VAPMSettings.WINDOWS_PATCH_DB))
            {
                result = false;
            }

            if (!File.Exists(VAPMSettings.WINDOWS_VULNERABILITY_DB))
            {
                result = false;
            }

            if (!result)
            {
                Logger.Log("One of the VAPM databases is missing.  Run 'patchutil downloaddb' to download the latest patch files and then try scanning again.");
                throw new Exception("Database file missing");
            }

            return result;
        }

        public static void CleanupDebugFiles()
        {
            foreach(string f in Directory.EnumerateFiles(Directory.GetCurrentDirectory(),"v4DebugInfo*"))
            {
                File.Delete(f);
            }
        }

        public static void MoveDebugFiles(string destPath)
        {
            if(!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            foreach (string f in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "v4DebugInfo*"))
            {
                FileInfo fileInfo = new FileInfo(f);
                string newFile = Path.Combine(destPath, fileInfo.Name);
                File.Move(f, newFile, true);
            }
        }

    }

}
