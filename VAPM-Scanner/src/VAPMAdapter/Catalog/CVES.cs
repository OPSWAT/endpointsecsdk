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
    /// <summary>
    /// Represents a class that houses CVE objects.
    /// </summary>
    internal class CVES
    {
        string jsonLocation;
        JObject cveJsonObject;


        /// <summary>
        /// Takes a specified json file location and loads it into the class.
        /// </summary>
        /// <param name="fileLocation">The json file location</param>
        /// <returns>Returns a boolean value to state that the json location has been loaded</returns>
        public bool Load(string fileLocation)
        {
            jsonLocation = fileLocation;
            return true;
        }

        /// <summary>
        /// Using the object-stored json file location we parse said file and return it.
        /// </summary>
        /// <returns>Parsed json found in the jsonLocation file</returns>
        private JObject getCVEJsonObject() // Note this will be slow until caching can be implemented
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

        /// <summary>
        /// This function parses the json object, if its not present already, and then stores the list of cve's derived from it.
        /// </summary>
        /// <returns>Returns the list of CVE's derived from the json object.</returns>
        private JObject getCvesListJson()
        {
            JObject result = cveJsonObject;

            if (result == null)
            {
                JObject productJsonObject = getCVEJsonObject();
                JArray oesisJson = (JArray)productJsonObject["oesis"];

                foreach (JObject current in oesisJson.Children<JObject>())
                {
                    if ("cves" == JsonUtil.GetJObjectName(current))
                    {
                        result = ((JObject)current["cves"]);
                        cveJsonObject = result;
                        return result;
                    }
                }
            }

            return result;
        }



        /// <summary>
        /// Using the cveJsonObject we will take the CVE argument and conduct a lookup to provide details on that CVE
        /// </summary>
        /// <param name="CVE">The specific CVE we are looking for</param>
        /// <returns>Details for the specified CVE</returns>
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

        /// <summary>
        /// Gets a list a CVE's from our cveJsonObject that are present after the date argument provided
        /// </summary>
        /// <param name="date">The date you want to search from</param>
        /// <returns>List of cves from the given date onwards</returns>
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
