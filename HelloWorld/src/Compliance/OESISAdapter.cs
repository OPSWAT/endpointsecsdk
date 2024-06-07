using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Compliance
{
    internal class OESISAdapter
    {
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
