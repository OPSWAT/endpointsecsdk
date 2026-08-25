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
    /// <summary>
    /// Thin wrappers around the OESIS Compliance module methods. Each method builds the OESIS
    /// JSON request ({"input": { "method": &lt;id&gt;, "signature": &lt;sig&gt; }}), invokes the
    /// framework, and parses the "result" node. Method ids and their input/output are documented
    /// at https://software.opswat.com/OESIS_V4/html/c_method.html (see the Compliance sections).
    /// A "signature" identifies a specific detected product, obtained from DetectProducts.
    /// </summary>
    public class OESISCompliance
    {
        /// <summary>
        /// Method 1007 (GetFirewallState). Returns true if the firewall product identified by
        /// <paramref name="signature"/> is enabled/running. Returns false if the call fails.
        /// </summary>
        public static bool IsFirewallRunning(int signature)
        {
            bool result = false;
            string json_in = "{\"input\": { \"method\": 1007, \"signature\":" + signature + " } }";
            string json_out = "";
            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
                result = parsedObject["result"]["enabled"];
            }

            return result;
        }


        /// <summary>
        /// Method 1009 (GetEncryptionState). Returns true only if the disk-encryption product is
        /// both fully encrypted and actively encrypting (result.fully_encrypted &amp;&amp;
        /// result.encryption_active). Returns false if the call fails.
        /// </summary>
        public static bool IsDiskFullyEncrypted(int signature)
        {
            bool result = false;
            string json_in = "{\"input\": { \"method\": 1009, \"signature\":" + signature + " } }";
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


        /// <summary>
        /// Antimalware protection check. First calls method 1000 (GetRealTimeProtectionState) and,
        /// if real-time protection is enabled, calls method 1002 (GetThreats) and returns true only
        /// when no active threats are present (result.no_threats). Returns false if either call
        /// fails or RTP is off.
        /// </summary>
        public static bool IsAntimalwareProtected(int signature)
        {
            bool result = false;
            string json_in = "{\"input\": { \"method\": 1000, \"signature\":" + signature + " } }";
            string json_out = "";
            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
                bool enabled = parsedObject["result"]["enabled"];

                if (enabled)
                {
                    json_in = "{\"input\": { \"method\": 1002, \"signature\":" + signature + " } }";
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


        /// <summary>
        /// Method 1001 (GetDefinitionState). Returns the last antimalware definition update time
        /// (result.definitions[0].last_update, a Unix timestamp), or DateTime.MinValue on failure.
        /// </summary>
        public static DateTime GetLastUpdateTime(int signature)
        {
            DateTime result = DateTime.MinValue;
            string json_in = "{\"input\": { \"method\": 1001, \"signature\":" + signature + " } }";
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

        /// <summary>
        /// Method 1004 (GetLastScanTime). Returns the last antimalware scan time
        /// (result.scan_time, a Unix timestamp), or DateTime.MinValue on failure.
        /// </summary>
        public static DateTime GetLastScanTime(int signature)
        {
            DateTime result = DateTime.MinValue;
            string json_in = "{\"input\": { \"method\": 1004, \"signature\":" + signature + " } }";
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


        /// <summary>
        /// Method 30012. Turns the OS location service on/off (operation is "enable"/"disable")
        /// so geolocation (method 30011) can be queried. Returns the raw call result code.
        /// </summary>
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


        /// <summary>
        /// Method 30011. Returns the device geolocation (latitude/longitude and country name/code)
        /// from result.coordinates and result.country. Requires the location service to be enabled
        /// (see SetLocationServiceState). Fields are left empty if the call fails.
        /// </summary>
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

        /// <summary>
        /// Method 2000. Enumerates the plugins/extensions installed in the browser identified by
        /// <paramref name="signatureId"/>, returning each plugin's id/name/version/type and any
        /// detail JSON. Returns an empty BrowserPlugins if the call fails.
        /// </summary>
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
