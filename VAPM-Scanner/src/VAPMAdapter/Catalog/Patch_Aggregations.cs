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
using VAPMAdapater.Log;
using VAPMAdapter.Catalog.POCO;

namespace VAPMAdapter.Catalog
{
    internal class Patch_Aggregations
    {
        private string jsonLocation;
        public Dictionary<string, CatalogPatchAggregation> patchIdToPatchAggregation = null;
        List<CatalogPatchAggregation> patchAggregationList = null;

        public bool Load(string fileLocation)
        {
            jsonLocation = fileLocation;
            return true;
        }

        private JObject getPatchAggregationJsonObject()
        {
            JObject result;
            string jsonString = File.ReadAllText(jsonLocation);

            if (!string.IsNullOrEmpty(jsonString))
            {
                result = JObject.Parse(jsonString);
            }
            else
            {
                Console.WriteLine("Failed to load Patch Aggregation Json");
                throw (new Exception("Unable to load Json"));
            }

            return result;
        }


        private List<JObject> getPatchAgregationListJson()
        {
            List<JObject> result = new List<JObject>();

            JObject patchAssociationsJsonObject = getPatchAggregationJsonObject();
            JArray oesisJson = (JArray)patchAssociationsJsonObject["oesis"];



            foreach (JObject current in oesisJson.Children<JObject>())
            {
                if (JsonUtil.GetJObjectName(current).Contains("patch_aggregations"))
                {
                    foreach (JProperty prop in current.Properties())
                    {
                        result.Add((JObject)current[prop.Name]);
                    }
                }
            }

            return result;
        }


        private List<CatalogDownloadDetails> parseDownloadDetails(JArray downloadLinkArrayJson)
        {
            List<CatalogDownloadDetails> result = new List<CatalogDownloadDetails>();

            foreach (JObject current in downloadLinkArrayJson)
            {
                CatalogDownloadDetails newDownloadDetails = new CatalogDownloadDetails();
                newDownloadDetails.Link = (string)current["link"];
                newDownloadDetails.Architecture = (string)current["architecture"];
                newDownloadDetails.Language = (string)current["language"];

                result.Add(newDownloadDetails);
            }

            return result;
        }

        // Note this is not written for multi-thread protection
        public List<CatalogPatchAggregation> GetList()
        {
            if (patchAggregationList != null)
            {
                return (patchAggregationList);
            }

            List<CatalogPatchAggregation> result = null;

            if (result == null)
            {
                List<JObject> aggregations = getPatchAgregationListJson();
                result = new List<CatalogPatchAggregation>();

                foreach (JObject jsonPatchAggregationList in aggregations)
                {
                    foreach (JProperty current in jsonPatchAggregationList.Properties())
                    {
                        CatalogPatchAggregation newPatchAggregation = new CatalogPatchAggregation();

                        newPatchAggregation.PatchId = (string)jsonPatchAggregationList[current.Name]["_id"];
                        newPatchAggregation.DownloadDetailsList = parseDownloadDetails((JArray)jsonPatchAggregationList[current.Name]["download_links"]);
                        newPatchAggregation.Fresh_Installable = (string)jsonPatchAggregationList[current.Name]["fresh_installable"];
                        newPatchAggregation.ReleaseNoteLink = (string)jsonPatchAggregationList[current.Name]["release_note_link"];
                        newPatchAggregation.Release_Date = (string)jsonPatchAggregationList[current.Name]["release_date"];
                        newPatchAggregation.Architectures = JsonUtil.GetStringArrayFromJson((JArray)jsonPatchAggregationList[current.Name]["architectures"]);
                        newPatchAggregation.Requires_Reboot = (string)jsonPatchAggregationList[current.Name]["requires_reboot"];

                        result.Add(newPatchAggregation);
                    }
                }

                patchAggregationList = result;
            }

            return result;
        }


        // Note this is not thread safe
        public Dictionary<string, CatalogPatchAggregation> GetPatchIdAggregationsDictionary()
        {
            if (patchIdToPatchAggregation != null)
            {
                return (patchIdToPatchAggregation);
            }

            Dictionary<string, CatalogPatchAggregation> newPatchIdAggregation = new Dictionary<string, CatalogPatchAggregation>();
            List<CatalogPatchAggregation> patchList = GetList();
            if (patchList != null)
            {
                foreach (CatalogPatchAggregation aggregation in patchList)
                {
                    if (aggregation != null)
                    {
                        string patchId = aggregation.PatchId;

                        if (!newPatchIdAggregation.ContainsKey(patchId))
                        {
                            newPatchIdAggregation.Add(patchId, aggregation);
                        }
                        else
                        {
                            Logger.Log("This should not get hit - PatchAggregationDictionary");
                        }
                    }
                }
                patchIdToPatchAggregation = newPatchIdAggregation;
            }

            return patchIdToPatchAggregation;
        }
    }
}