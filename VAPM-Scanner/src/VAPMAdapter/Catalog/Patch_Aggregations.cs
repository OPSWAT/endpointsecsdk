///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace VAPMAdapter.Catalog
{
    internal class Patch_Aggregations
    {
        JObject paJson;

        public bool Load(string fileLocation)
        {
            bool result = false;

            string jsonString = File.ReadAllText(fileLocation);

            if (!string.IsNullOrEmpty(jsonString))
            {
                paJson = JObject.Parse(jsonString);
                result = true;
            }
            else
            {
                Console.WriteLine("Failed to load Patch Aggregations Json");
            }

            return result;
        }
    }
}
