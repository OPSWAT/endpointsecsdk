///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PatchWOS
{
    internal class XStringMarshaler : ICustomMarshaler
    {
        private static ICustomMarshaler _instance = null;
        private Dictionary<IntPtr, object> managedObjects = new Dictionary<IntPtr, object>();

        public static ICustomMarshaler GetInstance(string cookie)
        {
            if (_instance == null)
                _instance = new XStringMarshaler();
            return _instance;
        }
        public static string PtrToString(IntPtr ptr)
        {
            return (string)GetInstance("").MarshalNativeToManaged(ptr);
        }
        public static IntPtr StringToPtr(string str)
        {
            return GetInstance("").MarshalManagedToNative(str);
        }

        public void CleanUpManagedData(object ManagedObj)
        {
            ManagedObj = null;
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            lock (managedObjects)
            {
                if (managedObjects.ContainsKey(pNativeData))
                {
                    managedObjects.Remove(pNativeData);
                }
            }
            Marshal.FreeCoTaskMem(pNativeData);
        }

        public int GetNativeDataSize()
        {
            return -1;
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            if (ManagedObj == null || ManagedObj as string == null)
                return IntPtr.Zero;

            if (!(ManagedObj is string))
                throw new MarshalDirectiveException("XPlatformStringMarshaler can only be used on String.");

            string utf16string = ManagedObj as string;
            IntPtr buffer = IntPtr.Zero;
            buffer = Marshal.StringToCoTaskMemUni(utf16string);

            lock (managedObjects)
            {
                managedObjects.Add(buffer, ManagedObj);
            }
            return buffer;
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero)
                return null;

            string sOut = string.Empty;
            lock (managedObjects)
            {
                if (managedObjects.ContainsKey(pNativeData))
                {
                    sOut = (string)managedObjects[pNativeData];
                    if (sOut == null)
                        sOut = string.Empty;
                    managedObjects.Remove(pNativeData);
                }
            }

            sOut = Marshal.PtrToStringUni(pNativeData);
            return sOut;
        }
    }

}