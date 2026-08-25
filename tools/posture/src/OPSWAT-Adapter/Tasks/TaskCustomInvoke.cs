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
    /// Passes an arbitrary OESIS request JSON straight through to the engine (wa_api_invoke) and
    /// returns the raw response JSON. Used by the Custom tab to let a developer experiment with any
    /// OESIS method. The caller is responsible for validating the JSON before calling.
    /// </summary>
    public class TaskCustomInvoke
    {
        /// <summary>
        /// Initializes the framework, invokes the given request JSON, and returns the raw response
        /// (or the engine's error response). Tears down in a finally so the engine is released even
        /// on error.
        /// </summary>
        public static string Invoke(string jsonIn)
        {
            OESISFramework.InitializeFramework();
            try
            {
                string jsonOut;
                OESISFramework.Invoke(jsonIn, out jsonOut);
                return jsonOut;
            }
            finally
            {
                OESISFramework.TearDown();
            }
        }
    }
}
