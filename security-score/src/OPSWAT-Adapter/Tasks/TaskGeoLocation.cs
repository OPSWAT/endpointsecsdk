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
using System;

namespace OPSWAT_Adapter.Tasks
{
    public class TaskGeoLocation
    {
        private Logger checkLog = new Logger();
        private GeoLocationInfo geoLocationInfo;


        public Logger GetLogger()
        {
            return checkLog;
        }

        public GeoLocationInfo GetGeoLocationInfo()
        {
            return geoLocationInfo;
        }


        public double CalculateMiles(double sLatitude, double sLongitude)
        {
            var radiansOverDegrees = (Math.PI / 180.0);

            var sLatitudeRadians = sLatitude * radiansOverDegrees;
            var sLongitudeRadians = sLongitude * radiansOverDegrees;
            var eLatitudeRadians = double.Parse(this.geoLocationInfo.latitude) * radiansOverDegrees;
            var eLongitudeRadians = double.Parse(this.geoLocationInfo.longitude) * radiansOverDegrees;

            var dLongitude = eLongitudeRadians - sLongitudeRadians;
            var dLatitude = eLatitudeRadians - sLatitudeRadians;

            var result1 = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                          Math.Cos(sLatitudeRadians) * Math.Cos(eLatitudeRadians) *
                          Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            // Using 3956 as the number of miles around the earth
            var result2 = 3956.0 * 2.0 *
                          Math.Atan2(Math.Sqrt(result1), Math.Sqrt(1.0 - result1));

            return result2;
        }


        public int GetGeolocation()
        {
            int resultCount = 0;

            OESISFramework.InitializeFramework();

            OESISCompliance.SetLocationServiceState("enable");
            geoLocationInfo = OESISCompliance.GetGeoLocation();

            OESISFramework.TearDown();

            return resultCount;
        }
    }

}

