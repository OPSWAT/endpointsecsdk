///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the
///  Compliance capability
///
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using ComplianceAdapater.OESIS;

namespace OPSWAT_Adapter.Tasks
{
    /// <summary>
    /// Produces a device compliance report as raw JSON, straight from the SDK.
    /// </summary>
    public class TaskComplianceReport
    {
        /// <summary>
        /// Initializes the framework, asks the SDK for the compliance/posture report
        /// (WAAPI_MID_GET_SECURITY_SCORE, method 111) and returns the raw JSON response. The
        /// framework is set up with enable_pretty_print, so the JSON is already formatted for
        /// display. Tears down in a finally so the engine is released even on error.
        /// </summary>
        /// <returns>The raw JSON compliance report (or a small JSON error object on failure).</returns>
        public static string GetReportJson()
        {
            OESISFramework.InitializeFramework();
            try
            {
                // force_refresh makes the engine refresh the underlying security status rather than
                // returning cached values.
                string json_in = "{\"input\": { \"method\": 111, \"force_refresh\": true } }";
                string json_out;
                int rc = OESISFramework.Invoke(json_in, out json_out);

                if (rc < 0)
                {
                    return "{ \"error\": \"Compliance report call failed\", \"rc\": " + rc + " }";
                }

                return json_out;
            }
            finally
            {
                OESISFramework.TearDown();
            }
        }
    }
}
