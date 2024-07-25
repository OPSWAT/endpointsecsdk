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
        public static string MapPatchData(string SigID)
        {
            string ProductVulJson = "";
            string Database = "v2mod.dat";
            string DetectedProducts = "";

            // 
            // Check to make sure that vmod.dat is available in the working directory
            //
            OESISUtil.ValidateDatabaseFiles();

            //
            // First initialize the OESIS Framework
            // Always enable debugging on an install.  Clean this up on a success, but save this on a failure
            //
            OESISPipe.InitializeFramework(true);

            OESISPipe.ConsumeOfflineVmodDatabase(Database);

            DetectedProducts = OESISPipe.DetectProducts();

            OESISUtil.GetProductList(DetectedProducts);

            try
            {
                ProductVulJson = OESISPipe.GetProductVulnerability(SigID);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("-1005"))
                {
                    ProductVulJson = "Install app to see data";
                }
                else
                {
                    throw;
                }
            }

            //Teardown the framework
            OESISPipe.Teardown();


            return ProductVulJson;
        }

    }
}
