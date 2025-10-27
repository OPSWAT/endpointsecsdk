///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json.Linq;
using ComplianceAdapater.POCO;
using System;
using System.Collections.Generic;

namespace ComplianceAdapater.OESIS
{
    public class Util
    {

        public static List<SetupErrorDetail> GetInitializationErrors(string json)
        {
            List<SetupErrorDetail> result = new List<SetupErrorDetail>();

            dynamic parsedJSON = JObject.Parse(json);
            var errorList = parsedJSON.errors;

            for (int i = 0; i < errorList.Count; i++)
            {
                SetupErrorDetail errorDetail = new SetupErrorDetail();

                errorDetail.timing = errorList[i].timing;
                errorDetail.module = errorList[i].module;
                errorDetail.version = errorList[i].version;
                errorDetail.code = errorList[i].code;
                errorDetail.path = errorList[i].path;

                result.Add(errorDetail);
            }

            return result;
        }


        // Note this is just passing in the json_result to be able to print it out on a failure
        private static void CheckSuccess(int rc)
        {
            // Return code link: https://software.opswat.com/OESIS_V4/html/c_return_codes.html
            // Note any code above 0 is a SUCCESS
            if (rc < 0)
            {
                Console.Out.WriteLine("Failed to execute call: " + rc);
                throw new Exception();
            }
        }


        //
        // This will return the first signature found with a category
        // More info can be found here: https://software.opswat.com/OESIS_V4/html/c_sdk.html  
        // Click on the left Product categories
        //
        public static List<int> GetProductSignaturesByCategory(OESISCategory category, JArray productList)
        {
            List<int> result = new List<int>();

            foreach (JObject product in productList)
            {
                System.Console.WriteLine(product);
                JArray categoryArray = (JArray)product["categories"];
                foreach (int currentCategory in categoryArray)
                {
                    if (currentCategory == (int)category)
                    {
                        int sigId = (int)product["signature"];
                        result.Add(sigId);
                        break;
                    }
                }
            }

            return result;
        }


        //
        // This will return the first signature found with a category
        // More info can be found here: https://software.opswat.com/OESIS_V4/html/c_sdk.html  
        // Click on the left Product categories
        //
        public static string GetProductSignatureName(int sigId, JArray productList)
        {
            string result = null;

            foreach (JObject product in productList)
            {
                int currentSig = (int)product["signature"];
                if(currentSig == sigId)
                {
                    result = (string)product["sig_name"];
                    break;
                }
            }

            return result;
        }


        public static DateTime UnixTimeStampToDateTime(string unixTimeStamp)
        {
            double unixTimeDouble = double.Parse(unixTimeStamp);

            return UnixTimeStampToDateTime(unixTimeDouble);
        }


        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

    }
}
