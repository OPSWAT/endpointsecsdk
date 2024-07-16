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
    internal class Vuln_Associations
    {
        private string jsonLocation;
        Dictionary<string, List<CatalogVulnerabilityAssociation>> prodIdToVulnAssociation;
        List<CatalogVulnerabilityAssociation> vulnAssociationList;


        public bool Load(string fileLocation)
        {
            jsonLocation = fileLocation;
            return true;
        }

        /// <summary>
        /// Reads the entire JSON content from "vuln_associations.json" as a JObject.
        /// 
        /// This function attempts to parse the content of "vuln_associations.json" into a JObject.
        /// It throws an exception if the file is empty or cannot be read.
        /// </summary>
        /// <returns>A JObject representing the parsed JSON content, or throws an exception if the file is empty or unreadable.</returns>
        private JObject GetVulnAssociationJsonObject()
        {
            JObject result;
            string jsonString = File.ReadAllText(jsonLocation);

            if (!string.IsNullOrEmpty(jsonString))
            {
                result = JObject.Parse(jsonString);
            }
            else
            {
                Console.WriteLine("Failed to load Vulnerability Associations Json");
                throw (new Exception("Unable to load Json"));
            }

            return result;
        }


        /// <summary>
        /// Gets vulnerability associations as a JObject from the parsed content of "vuln_associations.json".
        /// 
        /// This function assumes the complete JSON content is already retrieved using `getVulnAssociationJsonObject`.
        /// It searches for a header within the JSON data that starts with "vuln_associations".
        /// If the header is found, it extracts the corresponding value as a JObject representing the vulnerability associations.
        /// If the header is not found, the function returns null.
        /// </summary>
        /// <returns>A JObject containing vulnerability associations, or null if the relevant header is not found.</returns>

        private JObject GetVulnAssociationsListJson()
        {
            JObject result = null;

            JObject vulnAssociationsJsonObject = GetVulnAssociationJsonObject();
            JArray oesisJson = (JArray)vulnAssociationsJsonObject["oesis"];

            foreach (JObject current in oesisJson.Children<JObject>())
            {

                //find child json of oesis whose header name starts with vuln_associations
                if (current.TryGetValue("vuln_associations", out JToken vulnAssociationsToken))
                {
                    return (JObject)vulnAssociationsToken;
                }

            }

            return result;
        }

        
        private HashSet<string> GetProductIds(JArray processList)
        {
            HashSet<string> result = new HashSet<string>();

            foreach (JValue current in processList)
            {
                result.Add(current.Value<string>());
            }

            return result;
        }

        private List<CatalogRange> GetRanges(JArray rangeList)
        {
            List<CatalogRange> result = new List<CatalogRange>();

            foreach (JObject current in rangeList)
            {
                CatalogRange newRange = new CatalogRange();

                newRange.Start = (string)current["start"];
                newRange.Limit = (string)current["limit"];

                result.Add(newRange);
            }

            return result;
        }



        // Note this is not written for multi-thread protection
        /// <summary>
        /// Parses JObject from getVulnAssociationsListJson() into CatalogVulnerabilityAssociation objects (ID, type, OS).
        /// Returns a list of these objects.
        /// </summary>
        /// <returns>List of CatalogVulnerabilityAssociation objects.</returns>

        public List<CatalogVulnerabilityAssociation> GetList()
        {
            if (vulnAssociationList != null)
            {
                return vulnAssociationList;
            }

            List<CatalogVulnerabilityAssociation> result = new List<CatalogVulnerabilityAssociation>();
            JObject jsonVulnAssociationsList = (JObject)GetVulnAssociationsListJson();

            if (jsonVulnAssociationsList != null)
            {
                //Go over all objects within vuln_associations json returned by getVulnAssociationsListJson
                foreach (JProperty current in jsonVulnAssociationsList.Properties())
                {
                    CatalogVulnerabilityAssociation newVulnAssociation = new CatalogVulnerabilityAssociation
                    {
                        ID = current.Name,
                        Cve = (string)jsonVulnAssociationsList[current.Name]["cve"],
                        Type_id = (string)jsonVulnAssociationsList[current.Name]["type_id"],
                        Os_type = (string)jsonVulnAssociationsList[current.Name]["os_type"]
                    };

                    JArray v4_pids = (JArray)jsonVulnAssociationsList[current.Name]["v4_pids"];
                    newVulnAssociation.ProductIdSet = v4_pids != null ? (HashSet<string>)GetProductIds(v4_pids) : new HashSet<string>();

                    JArray ranges = (JArray)jsonVulnAssociationsList[current.Name]["ranges"];
                    newVulnAssociation.RangeList = ranges != null ? (List<CatalogRange>)GetRanges(ranges) : new List<CatalogRange>();

                    result.Add(newVulnAssociation);
                }

                vulnAssociationList = result;
            }

            return result;
        }


        // Note this is not thread safe
        /// <summary>
        /// Creates a dictionary mapping product signature IDs (strings) to their corresponding vulnerability associations (represented as a list of CatalogVulnerabilityAssociation objects).
        /// </summary>
        /// <returns>A dictionary of type Dictionary<string, List<CatalogVulnerabilityAssociation>>.</returns>
        public Dictionary<string, List<CatalogVulnerabilityAssociation>> GetProductVulnerablityDictionary()
        {
            if (prodIdToVulnAssociation != null)
            {
                return (prodIdToVulnAssociation);
            }

            Dictionary<string, List<CatalogVulnerabilityAssociation>> newProductVulnDictionary = new Dictionary<string, List<CatalogVulnerabilityAssociation>>();

            List<CatalogVulnerabilityAssociation> vulnList = GetList();
            if (vulnList != null)
            {
                foreach (CatalogVulnerabilityAssociation association in vulnList)
                {
                    HashSet<string> productIdList = association.ProductIdSet;

                    if (productIdList != null)
                    {
                        foreach (string signatureId in productIdList)
                        {
                            if (!newProductVulnDictionary.ContainsKey(signatureId))
                            {
                                newProductVulnDictionary.Add(signatureId, new List<CatalogVulnerabilityAssociation>());
                            }

                            newProductVulnDictionary[signatureId].Add(association);
                            System.Diagnostics.Debug.WriteLine(signatureId+"\n");
                        }
                    }
                }
            }
            
            prodIdToVulnAssociation = newProductVulnDictionary;
            
            return prodIdToVulnAssociation;
        }
    }
}
