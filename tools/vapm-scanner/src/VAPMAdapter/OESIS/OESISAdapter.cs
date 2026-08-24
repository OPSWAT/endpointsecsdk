///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace VAPMAdapter.OESIS
{
    internal class OESISAdapter
    {
        //
        // This class is used to create an Adapter between C# and C++ code.
        // This exports the functions that will need to used to integrate using the OESIS SDK
        // Note the use of the custom XStringMarshaler
        //
        // The engine is loaded DYNAMICALLY (NativeLibrary) rather than with a static
        // [DllImport]. A [DllImport] makes the CLR pin libwaapi.dll - and its statically
        // linked libwaheap.dll / libwautils.dll dependencies - for the life of the process,
        // which locks those files on disk and blocks an in-place SDK update. Loading
        // explicitly lets Teardown() free the module so the files can be replaced without
        // restarting the application.
        //
        const string LIB_WAAPI = "libwaapi";

        // The engine's JSON strings are UTF-16 (the old [DllImport] used XStringMarshaler,
        // which is StringToCoTaskMemUni / PtrToStringUni). CharSet.Unicode marshals the input
        // string identically (LPWStr), so no custom marshaler is needed on these delegates.
        // Out strings are still read/freed via XStringMarshaler.PtrToString + wa_api_free in
        // OESISPipe, exactly as before.
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate int SetupDelegate(string json_config, out IntPtr json_out);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate int InvokeDelegate(string json_config, out IntPtr json_out);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate int TeardownDelegate();
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate int FreeDelegate(IntPtr json_data);

        private static IntPtr _handle = IntPtr.Zero;
        private static SetupDelegate _setup;
        private static InvokeDelegate _invoke;
        private static TeardownDelegate _teardown;
        private static FreeDelegate _free;

        /// <summary>
        /// Loads libwaapi.dll (resolved next to the adapter, exactly as a [DllImport] would)
        /// and binds the exported entry points. Idempotent - a no-op if already loaded.
        /// </summary>
        public static void Load()
        {
            if (_handle != IntPtr.Zero)
            {
                return;
            }

            _handle = NativeLibrary.Load(LIB_WAAPI, typeof(OESISAdapter).Assembly, null);
            _setup = Marshal.GetDelegateForFunctionPointer<SetupDelegate>(NativeLibrary.GetExport(_handle, "wa_api_setup"));
            _invoke = Marshal.GetDelegateForFunctionPointer<InvokeDelegate>(NativeLibrary.GetExport(_handle, "wa_api_invoke"));
            _teardown = Marshal.GetDelegateForFunctionPointer<TeardownDelegate>(NativeLibrary.GetExport(_handle, "wa_api_teardown"));
            _free = Marshal.GetDelegateForFunctionPointer<FreeDelegate>(NativeLibrary.GetExport(_handle, "wa_api_free"));
        }

        /// <summary>
        /// Frees the native engine so libwaapi.dll (and the component DLLs it loaded) are no
        /// longer held open on disk. Must be called AFTER wa_api_teardown(); see OESISPipe.Teardown.
        /// </summary>
        public static void Unload()
        {
            _setup = null;
            _invoke = null;
            _teardown = null;
            _free = null;

            if (_handle != IntPtr.Zero)
            {
                NativeLibrary.Free(_handle);
                _handle = IntPtr.Zero;
            }
        }

        public static int wa_api_setup(string json_config, out IntPtr json_out)
        {
            Load();
            return _setup(json_config, out json_out);
        }

        public static int wa_api_invoke(string json_config, out IntPtr json_out)
        {
            return _invoke(json_config, out json_out);
        }

        public static int wa_api_teardown()
        {
            // Guard so a double teardown (teardown already unloaded the module) is harmless.
            if (_teardown == null)
            {
                return 0;
            }
            return _teardown();
        }

        public static int wa_api_free(IntPtr json_data)
        {
            return _free(json_data);
        }
    }

}
