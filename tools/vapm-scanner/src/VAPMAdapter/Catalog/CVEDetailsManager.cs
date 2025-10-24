using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAPMAdapter.Catalog
{
    public class CVEDetailsManager
    {
        private ConcurrentDictionary<string, JObject> cveJsonDictionary = new ConcurrentDictionary<string, JObject>();

        public void AddCveDetail(string cveId, JObject cveJson)
        {
            cveJsonDictionary[cveId] = cveJson;
        }

        public string GetCveJsonContentById(string cveId)
        {
            if (cveJsonDictionary.TryGetValue(cveId, out JObject cveJson))
            {
                JObject cvss2 = cveJson["cvss_2_0"] as JObject;
                JObject cvss3 = cveJson["cvss_3_0"] as JObject;
                string description = cveJson["description"]?.ToString() ?? "No description available";

                return $"CVSS 2.0: {cvss2?.ToString() ?? "N/A"}\n\nCVSS 3.0: {cvss3?.ToString() ?? "N/A"}\n\nDescription: {description}";
            }
            else
            {
                return "CVE not found.";
            }
        }
    }
}
