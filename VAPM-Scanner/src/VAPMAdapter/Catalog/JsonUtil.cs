﻿///////////////////////////////////////////////////////////////////////////////////////////////
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
        public static string GetJObjectName(JObject json)
        {
            string result = null;
            foreach (JProperty prop in json.Properties())
            {
                result = prop.Name;
            }

            return result;
        }

        public static List<string> GetStringArrayFromJson(JArray json)
        {
            List<string> result = new List<string>();

            foreach (JValue jsonObject in json)
            {
                result.Add(jsonObject.ToString());
            }

            return result;
        }


        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        public static string EscapeJSONString(string json)
        {
            string result = json.Replace("\\","/");
            return result;
        }
    }

}
