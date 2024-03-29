﻿///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using VAPMAdapater.Log;
using VAPMAdapter.OESIS;
using System.Collections.Generic;
using System;
using VAPMAdapter.OESIS.POCO;
using System.Reflection.Metadata;
using VAPMAdapter.Catalog;
using VAPMAdapater;

namespace VAPMAdapter.Tasks
{
    public class TaskScanAll
    {

        private static List<CVEDetail> GetVulnerabilities(Product product, out string cveJSON)
        {
            string detectString = OESISPipe.GetProductVulnerability(product.signatureId);
            List<CVEDetail> result = OESISUtil.GetCVEDetailList(product, detectString);
            Logger.Log("{0, 5} {1,-50} {2,-10}", product.signatureId, product.name, result.Count);

            // Assign the detectstring as an outputparameter
            cveJSON = detectString;

            return result;
        }

        private static PatchLevelDetail GetPatchLevel(Product product)
        {
            string patchString = OESISPipe.GetProductPatchLevel(product.signatureId);
            PatchLevelDetail result = OESISUtil.GetPatchLevelDetail(patchString);
            Logger.Log("{0, 5} {1,-50} {2,-10}", "PatchLevel", product.name, result.isLatest);
            return result;
        }

        private static VersionDetail GetVersion(Product product)
        {
            string versionString = OESISPipe.GetProductVersion(product.signatureId);
            VersionDetail result = OESISUtil.GetVersionDetail(versionString);
            return result;
        }

        private static List<InstallerDetail> GetInstallDetailListForScan(Product product)
        {
            List<InstallerDetail> result = new List<InstallerDetail>();
            bool installerStillExists = true;
            int  index = 0;

            while(installerStillExists)
            {
                try
                {

                    // Note a 2 is used for checking applicability for a signature
                    // This will get the currently installed product.
                    string installDetailString = OESISPipe.GetLatestInstallerScan(product.signatureId,index);
                    InstallerDetail currentDetail = OESISUtil.GetInstallerDetail(installDetailString);
                    index++;
                    if (currentDetail.result_code != -1039)
                    {
                        result.Add(currentDetail);
                    }
                    else
                    {
                        break;
                    }
                }
                catch(Exception e)
                {
                    Logger.Log("Installer not available. " + e);
                    break;
                }
            }

            return result;
        }



        public static Dictionary<string, ProductScanResult> Scan(bool scanWindows)
        {
            Dictionary<string, ProductScanResult> result = new Dictionary<string, ProductScanResult>();

            // 
            // Check to make sure that vmod.dat is available in the working directory
            //
            OESISUtil.ValidateDatabaseFiles();

            //
            // First initialize the OESIS Framework
            //
            OESISPipe.InitializeFramework(false);

            //
            // Now Load all the Patch databases
            //
            OESISPipe.LoadPatchDatabase(VAPMSettings.THIRD_PARTY_PATCH_DB, ""); // Note for a scan only you can skip loading this.  Checksums are not needed for scan
            OESISPipe.ConsumeOfflineVmodDatabase(VAPMSettings.THIRD_PARTY_VULNERABILITY_DB); // This loads Third-Party Patches. You can also use vmod-oft.dat if you do not need a CVE Description or text

            //
            // Load Windows Detection libraries
            //
            if (scanWindows)
            {
                OESISPipe.LoadPatchDatabase(VAPMSettings.WINDOWS_PATCH_DB, ""); // This is needed for scanning for CVE's in Microsoft Products. 
                OESISPipe.ConsumeOfflineVmodDatabase(VAPMSettings.WINDOWS_VULNERABILITY_DB); // This is needed for Microsoft Product CVE Detection
            }

            //
            // Detect all the installed Applications
            //
            string productsJson = OESISPipe.DetectProducts();
            List<Product> productList = OESISUtil.GetProductList(productsJson);




            Logger.Log("{0, 5} {1,-50} {2,-10}", "SigId", "Product", "CVE Count");
            Logger.Log("------------------------------------------------------------------------------------------------------------------------");


            //
            // Iterate through all the different products and check for vulnerabilities
            //
            foreach (Product current in productList)
            {
                try
                {
                    ProductScanResult productScanResult = new ProductScanResult();
                    current.versionDetail = GetVersion(current);

                    productScanResult.product = current;
                    productScanResult.patchLevelDetail = GetPatchLevel(current);
                    productScanResult.cveDetailList = GetVulnerabilities(current,out productScanResult.cveJson);
                    productScanResult.installDetail = GetInstallDetailListForScan(current);
                    result.Add(current.signatureId.ToString(), productScanResult);
                }
                catch (Exception e)
                {
                    Logger.Log("Unable to retrieve Vulnerability for: " + current.signatureId + ":" + e);
                }
            }

            OESISPipe.Teardown();

            return result;
        }

    }
}
