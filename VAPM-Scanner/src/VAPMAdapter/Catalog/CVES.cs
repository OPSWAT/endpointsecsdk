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
using VAPMAdapter.Catalog.POCO;

namespace VAPMAdapter.Catalog
{
    internal class CVES
    {
        string jsonLocation;

        public bool Load(string fileLocation)
        {
            jsonLocation = fileLocation;
            return true;
        }

        // Note this will be slow until caching can be implemented
        private JObject getCVEJsonObject()
        {
            JObject result;
            string jsonString = File.ReadAllText(jsonLocation);

            if (!string.IsNullOrEmpty(jsonString))
            {
                result = JObject.Parse(jsonString);
            }
            else
            {
                Console.WriteLine("Failed to load Product Json");
                throw (new Exception("Unable to load Json"));
            }

            return result;
        }

        private JObject getCvesListJson()
        {
            JObject result = null;

            JObject productJsonObject = getCVEJsonObject();
            JArray oesisJson = (JArray)productJsonObject["oesis"];

            foreach (JObject current in oesisJson.Children<JObject>())
            {
                if ("cves" == JsonUtil.GetJObjectName(current))
                {
                    return ((JObject)current["cves"]);
                }
            }

            return result;
        }




        public string GetCVEDetails(string CVE)
        {
            string result = "";
            JObject cveJsonObject = getCvesListJson();

            JObject cveData = (JObject)cveJsonObject[CVE];
            if (cveData != null)
            {
                result = cveData.ToString();
            }
            return result;
        }


        public List<CatalogCVEDate> GetCVEsFromDate(DateTime date)
        {
            List<CatalogCVEDate> result = new List<CatalogCVEDate>();
            JObject cveJsonObject = getCvesListJson();

            double unixTimestamp = JsonUtil.DateTimeToUnixTimestamp(date);

            foreach (JProperty cve in cveJsonObject.Properties())
            {
                string lastModified = (string)cveJsonObject[cve.Name]["last_modified_epoch"];

                long modifiedTime = long.Parse(lastModified);

                if (unixTimestamp < modifiedTime)
                {
                    CatalogCVEDate cveDate = new CatalogCVEDate();
                    cveDate.cveID = cve.Name;
                    //string releaseDate = (string)cveJsonObject[cve.Name]["last_modified_epoch"];
                    cveDate.modifiedDate = JsonUtil.UnixTimeStampToDateTime(modifiedTime);
                    result.Add(cveDate);
                }
            }

            return result;
        }

    }
}
