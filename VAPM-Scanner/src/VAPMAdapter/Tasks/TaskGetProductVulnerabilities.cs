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
            // 
            // Check to make sure that vmod.dat is available in the working directory
            //
            OESISUtil.ValidateDatabaseFiles();

            //
            // First initialize the OESIS Framework
            // Always enable debugging on an install.  Clean this up on a success, but save this on a failure
            //
            OESISPipe.InitializeFramework(true);

            ProductVulJson = OESISPipe.GetProductVulnerability(SigID);

            //Teardown the framework
            OESISPipe.Teardown();


            return "";
        }

    }
}
