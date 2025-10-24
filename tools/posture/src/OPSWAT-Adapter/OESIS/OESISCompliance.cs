///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json.Linq;
using OPSWAT_Adapter.POCO;
using System;

namespace ComplianceAdapater.OESIS
{
    public class OESISCompliance
    {
        public static bool IsFirewallRunning(int signature)
        {
            bool result = false;
            string json_in = "{\"input\": { \"method\": 1007 \"signature\":" + signature + " } }";
            string json_out = "";
            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
                result = parsedObject["result"]["enabled"];
            }

            return result;
        }


        public static bool IsDiskFullyEncrypted(int signature)
        {
            bool result = false;
            string json_in = "{\"input\": { \"method\": 1009 \"signature\":" + signature + " } }";
            string json_out = "";
            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
                bool fullyEncrypted = parsedObject["result"]["fully_encrypted"];
                bool encryptionActive = parsedObject["result"]["encryption_active"];

                result = fullyEncrypted && encryptionActive;
            }

            return result;
        }


        public static bool IsAntimalwareProtected(int signature)
        {
            bool result = false;
            string json_in = "{\"input\": { \"method\": 1000 \"signature\":" + signature + " } }";
            string json_out = "";
            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
                bool enabled = parsedObject["result"]["enabled"];

                if (enabled)
                {
                    json_in = "{\"input\": { \"method\": 1002 \"signature\":" + signature + " } }";
                    json_out = "";
                    callResult = OESISFramework.Invoke(json_in, out json_out);

                    if (callResult >= 0)
                    {
                        parsedObject = JObject.Parse(json_out);
                        bool no_threats = parsedObject["result"]["no_threats"];
                        result = no_threats;
                    }
                }
            }

            return result;
        }


        public static DateTime GetLastUpdateTime(int signature)
        {
            DateTime result = DateTime.MinValue;
            string json_in = "{\"input\": { \"method\": 1001 \"signature\":" + signature + " } }";
            string json_out = "";
            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
                string lastUpdateString = parsedObject["result"]["definitions"][0]["last_update"];
                result = Util.UnixTimeStampToDateTime(lastUpdateString);
            }

            return result;
        }

        public static DateTime GetLastScanTime(int signature)                                                                                                                                                                              
        {
            DateTime result = DateTime.MinValue;
            string json_in = "{\"input\": { \"method\": 1004 \"signature\":" + signature + " } }";
            string json_out = "";
            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
                string lastScanString = parsedObject["result"]["scan_time"];
                result = Util.UnixTimeStampToDateTime(lastScanString);
            }

            return result;
        }



        public static int SetLocationServiceState(string state)
        {
            string json_in = "{\"input\": { \"method\": 30012, \"operation\": \"" + state + "\" } }";
            string json_out = "";
            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
            }

            return callResult;
        }


        public static GeoLocationInfo GetGeoLocation()
        {
            GeoLocationInfo result = new GeoLocationInfo();
            string json_out = "";
            string json_in = "{\"input\": { \"method\": 30011 } }";

            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
              
                result.latitude = parsedObject["result"]["coordinates"]["latitude"];
                result.longitude = parsedObject["result"]["coordinates"]["longitude"];
                result.countryName = parsedObject["result"]["country"]["friendly_name"];
                result.countryCode = parsedObject["result"]["country"]["iso2_code"];
            }

            return result;
        }

        public static BrowserPlugins GetBrowserPlugin(string signatureId)
        {
            BrowserPlugins result = new BrowserPlugins();
            
            string json_out = "";
            string json_in = "{\"input\": { \"method\": 2000, \"signature\": " + signatureId + " } }";

            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                result = new BrowserPlugins();
                result.pluginList = new System.Collections.Generic.List<PluginDetail>();
                result.signatureId = signatureId;

                
                dynamic parsedObject = JObject.Parse(json_out);
                result.code = parsedObject["result"]["code"];

                JArray pluginListJSON = parsedObject["result"]["plugins"];
                foreach(dynamic pluginDetailJSON in pluginListJSON)
                {
                    PluginDetail pluginDetail = new PluginDetail();
                    pluginDetail.id = pluginDetailJSON["id"];
                    pluginDetail.name = pluginDetailJSON["name"];
                    pluginDetail.version = pluginDetailJSON["version"];
                    pluginDetail.type = pluginDetailJSON["type"];

                    JObject detailObject = (JObject)pluginDetailJSON["details"];
                    if (detailObject != null)
                    {
                        pluginDetail.details = detailObject.ToString();
                        pluginDetail.description = (string)detailObject["description"];
                    }

                    result.pluginList.Add(pluginDetail);
                }
            }

            return result;
        }



    }
}
