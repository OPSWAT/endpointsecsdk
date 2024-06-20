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
using System.Linq;
using System.Text;

using System.Threading.Tasks;

namespace VAPMAdapter.Catalog
{
    internal class JsonUtil
    {
        /// <summary>
        /// Retrieves the last property of the provided JObject.
        /// </summary>
        /// <param name="json">The JObject from which to retrieve the last property.</param>
        /// <returns>The last property of the JObject or null if JObject is empty.</returns>
        public static string GetJObjectName(JObject json)
        {
            string result = null;
            foreach (JProperty prop in json.Properties())
            {
                result = prop.Name;
            }

            return result;
        }

        /// <summary>
        /// Converts each JObject to a string and returns them as a list of strings.
        /// </summary>
        /// <param name="json">The JArray whose JObjects are to be converted to strings and added to the list.</param>
        /// <returns>A list of strings where each entry is a string representation of a JObject.</returns>
        public static List<string> GetStringArrayFromJson(JArray json)
        {
            List<string> result = new List<string>();

            foreach (JValue jsonObject in json)
            {
                result.Add(jsonObject.ToString());
            }

            return result;
        }

        /// <summary>
        /// Converts a Unix timestamp to a DateTime object.
        /// </summary>
        /// <param name="unixTimeStamp">The Unix timestamp to convert.</param>
        /// <returns>A DateTime object representing the Unix timestamp in local time.</returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        /// <summary>
        /// Converts a DateTime object to a Unix timestamp.
        /// </summary>
        /// <param name="dateTime">The DateTime object to convert.</param>
        /// <returns>The Unix timestamp representing the DateTime object.</returns>
        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// Escapes the special characters in a JSON string.
        /// </summary>
        /// <param name="json">The JSON string to escape.</param>
        /// <returns>The JSON string with special characters escaped.</returns>
        public static string EscapeJSONString(string json)
        {
            string result = json.Replace("\\","/");
            return result;
        }
    }

}
