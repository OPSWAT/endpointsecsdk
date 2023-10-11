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
                if ("vuln_associations" == JsonUtil.GetJObjectName(current))
                {
                    return ((JObject)current["vuln_associations"]);
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
                return (vulnAssociationList);
            }

            List<CatalogVulnerabilityAssociation> result = null;

            if (result == null)
            {
                JObject jsonVulnAssociationsList = (JObject)getVulnAssociationsListJson();
                result = new List<CatalogVulnerabilityAssociation>();


                foreach (JProperty current in jsonVulnAssociationsList.Properties())
                {
                    CatalogVulnerabilityAssociation newVulnAssociation = new CatalogVulnerabilityAssociation();

                    newVulnAssociation.ID = current.Name;
                    newVulnAssociation.Cve = (string)jsonVulnAssociationsList[current.Name]["cve"];
                    newVulnAssociation.Type_id = (string)jsonVulnAssociationsList[current.Name]["type_id"];
                    newVulnAssociation.Os_type = (string)jsonVulnAssociationsList[current.Name]["os_type"];
                    newVulnAssociation.ProductIdSet = (HashSet<string>)getProductIds((JArray)jsonVulnAssociationsList[current.Name]["v4_pids"]);
                    newVulnAssociation.RangeList = (List<CatalogRange>)getRanges((JArray)jsonVulnAssociationsList[current.Name]["ranges"]);

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
