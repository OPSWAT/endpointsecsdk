///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using ComplianceAdapater.Log;
using ComplianceAdapater.OESIS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace OPSWAT_Adapter.Tasks
{
    public class TaskSecurityScore
    {
        private Logger checkLog = new Logger();
        private JArray productList = null;


        public Logger GetLogger()
        {
            return checkLog;
        }

        private JArray GetProductList()
        {
            if (productList == null)
            {
                string json_product;
                OESISCore.DetectAllProducts(out json_product);
                productList = OESISCore.GetProductArrayFromString(json_product);
            }

            return productList;
        }

        public bool IsFirewallRunning(List<int> firewallProductList)
        {
            bool result = true;

            foreach (int sigId in firewallProductList)
            {
                string productName = Util.GetProductSignatureName(sigId, GetProductList());
                bool running = OESISCompliance.IsFirewallRunning(sigId);
                GetLogger().Log(running, "Firewall: " + productName + ":Is Running  = " + running);

                if (!running)
                {
                    result = false;
                }
            }

            return result;
        }

        private bool IsDiskEncrypted(List<int> encryptionProductList)
        {
            bool result = true;

            foreach (int sigId in encryptionProductList)
            {
                string productName = Util.GetProductSignatureName(sigId, GetProductList());
                bool encrypted = OESISCompliance.IsDiskFullyEncrypted(sigId);
                GetLogger().Log(encrypted, "Encryption: " + productName + ":Is DiskEncrypted  = " + encrypted);

                if (!encrypted)
                {
                    result = false;
                }
            }

            return result;
        }

        private bool IsAntimalwareProtected(List<int> antimalwareProductList)
        {
            bool result = false;

            foreach (int sigId in antimalwareProductList)
            {
                string productName = Util.GetProductSignatureName(sigId, GetProductList());
                bool antimalwareProtected = OESISCompliance.IsAntimalwareProtected(sigId);
                GetLogger().Log(antimalwareProtected, "Antimalware: " + productName + ":Is Antimalware Protected  = " + antimalwareProtected);

                if (antimalwareProtected)
                {
                    result = true;
                }
            }

            return result;
        }


        private bool IsUpdateDefinitionRecent(List<int> antimalwareProductList, DateTime updateWindow)
        {
            bool result = false;

            foreach (int sigId in antimalwareProductList)
            {
                string productName = Util.GetProductSignatureName(sigId, GetProductList());
                DateTime lastUpdateTime = OESISCompliance.GetLastUpdateTime(sigId);
                GetLogger().Log(true, "Antimalware: " + productName + ":Last defintion update : " + lastUpdateTime);

                if (lastUpdateTime > updateWindow)
                {
                    result = true;
                }

                GetLogger().Log(result, "Antimalware: " + productName + ":Update Defintion is recent : " + result);
            }

            return result;
        }

        private bool IsScanRecent(List<int> antimalwareProductList, DateTime scanWindow)
        {
            bool result = false;

            foreach (int sigId in antimalwareProductList)
            {
                string productName = Util.GetProductSignatureName(sigId, GetProductList());
                DateTime lastScanTime = OESISCompliance.GetLastScanTime(sigId);
                GetLogger().Log(true, "Antimalware: " + productName + ":Last scan update : " + lastScanTime);

                if (lastScanTime > scanWindow)
                {
                    result = true;
                }

                GetLogger().Log(result, "Antimalware: " + productName + ":Last scan is recent : " + result);
            }

            return result;
        }


        public int GetSecurityScore()
        {
            int totalScore = 0;

            OESISFramework.InitializeFramework();
            try
            {
                // Ask the SDK to calculate the OPSWAT Security Score directly
                // (WAAPI_MID_GET_SECURITY_SCORE, method 111) instead of summing individual checks.
                // force_refresh makes the engine refresh the underlying security status rather
                // than using cached values.
                string json_in = "{\"input\": { \"method\": 111, \"force_refresh\": true } }";
                string json_out = "";
                int callResult = OESISFramework.Invoke(json_in, out json_out);

                if (callResult < 0)
                {
                    GetLogger().Log(false, "Security Score: SDK call failed (rc=" + callResult + ")");
                    return 0;
                }

                JObject parsed = JObject.Parse(json_out);
                JToken result = parsed["result"];

                // total_score is on a 0-100 scale.
                totalScore = (int)result["total_score"];
                string scoreStatus = (string)result["score_status"];
                GetLogger().Log(scoreStatus == "good",
                    "OPSWAT Security Score: " + totalScore + " / 100 (" + scoreStatus + ")");

                // Log the per-category breakdown so the tab shows where the score came from.
                JToken categories = result["categories"];
                if (categories != null)
                {
                    foreach (JToken category in categories)
                    {
                        string name = (string)category["name"];
                        int score = (int)category["score"];
                        int maxScore = (int)category["max_score"];
                        string status = (string)category["status"];
                        GetLogger().Log(status == "good",
                            name + ": " + score + " / " + maxScore + " (" + status + ")");
                    }
                }
            }
            finally
            {
                OESISFramework.TearDown();
            }

            return totalScore;
        }

    }
}
