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

        private JObject getVulnAssociationJsonObject()
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



        private JObject getVulnAssociationsListJson()
        {
            JObject result = null;

            JObject vulnAssociationsJsonObject = getVulnAssociationJsonObject();
            JArray oesisJson = (JArray)vulnAssociationsJsonObject["oesis"];

            foreach (JObject current in oesisJson.Children<JObject>())
            {
                if (current.TryGetValue("vuln_associations", out JToken vulnAssociationsToken))
                {
                    return (JObject)vulnAssociationsToken;
                }

            }

            return result;
        }


        private HashSet<string> getProductIds(JArray processList)
        {
            HashSet<string> result = new HashSet<string>();

            foreach (JValue current in processList)
            {
                result.Add(current.Value<string>());
            }

            return result;
        }

        private List<CatalogRange> getRanges(JArray rangeList)
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
        public List<CatalogVulnerabilityAssociation> GetList()
        {
            if (vulnAssociationList != null)
            {
                return vulnAssociationList;
            }

            List<CatalogVulnerabilityAssociation> result = new List<CatalogVulnerabilityAssociation>();
            JObject jsonVulnAssociationsList = (JObject)getVulnAssociationsListJson();

            if (jsonVulnAssociationsList != null)
            {
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
                    newVulnAssociation.ProductIdSet = v4_pids != null ? (HashSet<string>)getProductIds(v4_pids) : new HashSet<string>();

                    JArray ranges = (JArray)jsonVulnAssociationsList[current.Name]["ranges"];
                    newVulnAssociation.RangeList = ranges != null ? (List<CatalogRange>)getRanges(ranges) : new List<CatalogRange>();

                    result.Add(newVulnAssociation);
                }

                vulnAssociationList = result;
            }

            return result;
        }


        // Note this is not thread safe
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
                        }
                    }
                }
            }
            prodIdToVulnAssociation = newProductVulnDictionary;

            return prodIdToVulnAssociation;
        }
    }
}
