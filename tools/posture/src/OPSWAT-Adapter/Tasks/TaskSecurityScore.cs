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


        public Logger GetLogger()
        {
            return checkLog;
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
