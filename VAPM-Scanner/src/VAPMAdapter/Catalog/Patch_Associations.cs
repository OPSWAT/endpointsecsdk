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

        /// <summary>
        /// Reads the entire JSON content from "patch_associations.json" as a JObject. Throws exception if unsuccessful.
        /// It retrieves the JSON data from the file.
        /// </summary>
        /// <returns>A JObject representing the parsed JSON content from "patch_associations.json".</returns>
        private JObject GetPatchAssociationJsonObject()
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

        /// <summary>
        /// Extracts a list of JObjects representing patch associations.
        /// 
        /// This function assumes the JObject is retrieved using getPatchAssociationJsonObject.
        /// It searches for header named "patch_associations" within the provided JObject.
        /// If found, it returns a list containing all JObjects under the "patch_associations" element.
        /// If the element is not found, the function returns an empty list.
        /// </summary>
        /// <returns>A list of JObjects representing patch associations, or empty list if not found.</returns>
        private List<JObject> GetPatchAssociationsListJson()
        {
            List<JObject> result = new List<JObject>();

            JObject patchAssociationsJsonObject = GetPatchAssociationJsonObject();
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
        /// <summary>
        /// Parses JObjects from getPatchAssociationsListJson() into CatalogPatchAssociation objects filling in their PatchId, SigIdList values.
        /// If the PatchId for the objects are found as a key in the dictionary returned from GetPatchIdAggregationsDictionary(), 
        /// then the PatchAggregation value for the object is set to the value for the patch id key in the dictionary.
        /// Returns a list of these objects.
        /// </summary>
        /// <returns>List of CatalogPatchAssociation objects.</returns>
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
                List<JObject> associations = GetPatchAssociationsListJson();
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
        /// <summary>
        /// Creates a dictionary mapping signature IDs to associated CatalogPatchAssociation objects.
        /// Processes a list of CatalogPatchAssociation objects from GetList() and builds a dictionary using nested loops.
        /// - Key: Signature ID (extracted from SigIdList within CatalogPatchAssociation objects)
        /// - Value: List of CatalogPatchAssociation objects associated with the signature ID
        /// </summary>
        /// <returns>Dictionary mapping signature IDs to lists of CatalogPatchAssociation objects.</returns>
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
