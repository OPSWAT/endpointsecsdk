///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VAPMAdapter.OESIS
{
    internal class OESISAdapter
    {
        //
        // This class is used to create an Adapter between C# and C++ code.
        // This exports the functions that will need to used to integrate using the OESIS SDK
        // Note the use of the custom XStringMarshaler
        //
        const string LIB_WAAPI = "libwaapi";
        [DllImport(LIB_WAAPI, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int wa_api_setup([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(XStringMarshaler))] string json_config,
                                                out IntPtr json_out);

        [DllImport(LIB_WAAPI, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int wa_api_invoke([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(XStringMarshaler))] string json_config,
                                                out IntPtr json_out);

        [DllImport(LIB_WAAPI, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int wa_api_teardown();

        [DllImport(LIB_WAAPI, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int wa_api_free(IntPtr json_data);
    }

}
