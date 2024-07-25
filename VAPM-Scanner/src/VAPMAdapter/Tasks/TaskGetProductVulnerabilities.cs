using VAPMAdapater.Log;
using VAPMAdapter.OESIS;
using System.Collections.Generic;
using System.IO;
using System;
using VAPMAdapter.OESIS.POCO;
using VAPMAdapater;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace VAPMAdapter.Tasks
{
    public class TaskGetProductVulnerabilities
    {
        //could speed up this process by saving a hask map of sigID, and iterating through it and only initializing the framework once. Not initializing it over and over.
        public static Dictionary<string, string> MapPatchData(Dictionary<string, string> productDictionary)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string Database = "v2mod.dat";
            string DetectedProducts = "";

            // Check to make sure that vmod.dat is available in the working directory
            OESISUtil.ValidateDatabaseFiles();

            // First initialize the OESIS Framework
            // Always enable debugging on an install. Clean this up on a success, but save this on a failure
            OESISPipe.InitializeFramework(true);

            OESISPipe.ConsumeOfflineVmodDatabase(Database);

            DetectedProducts = OESISPipe.DetectProducts();

            OESISUtil.GetProductList(DetectedProducts);

            foreach (var kvp in productDictionary)
            {
                string productID = kvp.Key;
                string ProductVulJson = "";

                // Get the vulnerability string for the product ID
                string vulnerabilityResult = OESISPipe.GetProductVulnerability(productID);

                if (vulnerabilityResult == "-1005")
                {
                    ProductVulJson = "Install app to see data";
                }
                else
                {
                    ProductVulJson = vulnerabilityResult;
                }

                result[productID] = ProductVulJson;
            }

            // Teardown the framework
            OESISPipe.Teardown();

            return result;
        }

    }
}
