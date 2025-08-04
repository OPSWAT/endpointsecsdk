///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OPSWAT_Adapter.POCO;
using System;

namespace ComplianceAdapater.OESIS
{
    public class OESISCompliance
    {

        private static void UpdateResult(MethodResult resultObject, int code, string rawJson)
        {
            resultObject.success = code >= 0 ? true : false;
            resultObject.code = code;
            resultObject.rawJson = rawJson;
        }


        public static FirewallState GetFirewallState(int signature)
        {
            FirewallState result = new FirewallState();

            string json_in = "{\"input\": { \"method\": 1007 \"signature\":" + signature + " } }";

            string json_out = "";
            int callResult = OESISFramework.Invoke(json_in, out json_out);
            UpdateResult(result, callResult, json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
                result.enabled = parsedObject["result"]["enabled"];
                result.profileDetailsJson = parsedObject["result"]["profile_details"];
                result.managedBy3rdPartyProductsJson = parsedObject["result"]["managed_by_3rd_party_products"];
            }

            return result;
        }


        public static EncryptionState GetEncryptionState(int signature)
        {
            EncryptionState result = new EncryptionState();
            string json_in = "{\"input\": { \"method\": 1009 \"signature\":" + signature + " } }";
            string json_out = "";
            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
                result.encryption_active = parsedObject["result"]["encryption_active"];
                result.fully_encrypted = parsedObject["result"]["fully_encrypted"];
                result.support_wde = parsedObject["result"]["support_wde"];
                //result.locationsJson = parsedObject["result"]["locations"];
            }

            return result;
        }


        public static RealTimeProtectionState GetRealTimeProtectionState(int signature)
        {
            RealTimeProtectionState result = new RealTimeProtectionState();
            string json_in = "{\"input\": { \"method\": 1000 \"signature\":" + signature + " } }";
            string json_out = "";
            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
                result.enabled = parsedObject["result"]["enabled"];

                JObject resultObject = parsedObject["result"]["details"];
                result.detailsJson = JsonConvert.SerializeObject(resultObject, Formatting.Indented);
            }

            return result;
        }

        public static Threats GetThreats(int signature)
        {
            Threats result = new Threats();
            string json_in = "{\"input\": { \"method\": 1002 \"signature\":" + signature + " } }";
            string json_out = "";
            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
                result.no_threats = parsedObject["result"]["no_threats"];
                result.lastThreatTime = Util.UnixTimeStampToDateTime(parsedObject["result"]["last_threat_time"]);
                result.threatsJson = parsedObject["result"]["threats"];
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

        public static BackupState GetBackupState(int signature)
        {
            BackupState result = new BackupState();
            string json_in = "{\"input\": { \"method\": 1008 \"signature\":" + signature + " } }";
            string json_out = "";
            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
                JObject resultObject = parsedObject["result"];

                if (resultObject.ContainsKey("backup_active"))
                {
                    result.last_backup_activity = Util.UnixTimeStampToDateTime((string)parsedObject["result"]["last_backup_activity"]);
                    result.backup_active = parsedObject["result"]["backup_active"];
                    //Parse this later. result.backupJson = parsedObject["result"]["backups"];
                }
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
