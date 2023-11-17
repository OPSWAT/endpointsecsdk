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
    internal class Patch_Associations
    {
        private string jsonLocation;
        Patch_Aggregations patch_Aggregations = null;
        public Dictionary<string, List<CatalogPatchAssociation>> sigIdToPatchAssociation = null;
        List<CatalogPatchAssociation> patchAssociationList = null;


        public bool Load(string fileLocation, Patch_Aggregations patchAgreggations)
        {
            this.patch_Aggregations = patchAgreggations;
            jsonLocation = fileLocation;
            return true;
        }

        private JObject getPatchAssociationJsonObject()
        {
            JObject result;
            string jsonString = File.ReadAllText(jsonLocation);

            if (!string.IsNullOrEmpty(jsonString))
            {
                result = JObject.Parse(jsonString);
            }
            else
            {
                Console.WriteLine("Failed to load Patch Associations Json");
                throw (new Exception("Unable to load Json"));
            }

            return result;
        }


        private List<JObject> getPatchAssociationsListJson()
        {
            List<JObject> result = new List<JObject>();

            JObject patchAssociationsJsonObject = getPatchAssociationJsonObject();
            JArray oesisJson = (JArray)patchAssociationsJsonObject["oesis"];



            foreach (JObject current in oesisJson.Children<JObject>())
            {
                if (JsonUtil.GetJObjectName(current).Contains("patch_associations"))
                {
                    foreach (JProperty prop in current.Properties())
                    {
                        result.Add((JObject)current[prop.Name]);
                    }
                }
            }

            return result;
        }


        // Note this is not written for multi-thread protection
        public List<CatalogPatchAssociation> GetList()
        {
            if (patchAssociationList != null)
            {
                return (patchAssociationList);
            }

            List<CatalogPatchAssociation> result = null;
            Dictionary<string, CatalogPatchAggregation> patchAggregationDictionary = patch_Aggregations.GetPatchIdAggregationsDictionary();

            if (result == null)
            {
                List<JObject> associations = getPatchAssociationsListJson();
                result = new List<CatalogPatchAssociation>();

                foreach (JObject jsonPatchAssociationsList in associations)
                {
                    foreach (JProperty current in jsonPatchAssociationsList.Properties())
                    {
                        CatalogPatchAssociation newPatchAssociation = new CatalogPatchAssociation();

                        newPatchAssociation.PatchId = (string)jsonPatchAssociationsList[current.Name]["patch_id"];
                        newPatchAssociation.SigIdList = JsonUtil.GetStringArrayFromJson((JArray)jsonPatchAssociationsList[current.Name]["v4_signatures"]);
                        
                        if(patchAggregationDictionary.ContainsKey(newPatchAssociation.PatchId))
                        {
                            newPatchAssociation.PatchAggregation = patchAggregationDictionary[newPatchAssociation.PatchId];
                        }

                        result.Add(newPatchAssociation);
                    }
                }

                patchAssociationList = result;
            }

            return result;
        }



        // Note this is not thread safe
        public Dictionary<string, List<CatalogPatchAssociation>> GetProductPatchDictionary()
        {
            if (sigIdToPatchAssociation != null)
            {
                return (sigIdToPatchAssociation);
            }

            Dictionary<string, List<CatalogPatchAssociation>> newSigIdToPatchAssociation = new Dictionary<string, List<CatalogPatchAssociation>>();

            List<CatalogPatchAssociation> patchList = GetList();
            if (patchList != null)
            {
                foreach (CatalogPatchAssociation association in patchList)
                {
                    if (association != null)
                    {
                        foreach (string signatureId in association.SigIdList)
                        {
                            if (!newSigIdToPatchAssociation.ContainsKey(signatureId))
                            {
                                newSigIdToPatchAssociation.Add(signatureId, new List<CatalogPatchAssociation>());
                            }

                            newSigIdToPatchAssociation[signatureId].Add(association);
                        }
                    }
                }
            }
            sigIdToPatchAssociation = newSigIdToPatchAssociation;

            return sigIdToPatchAssociation;
        }
    }
}
