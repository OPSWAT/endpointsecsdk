using System;
using VAPMAdapter.OESIS.POCO;

namespace VAPMAdapter.OESIS
{
    public class OESISException : Exception
    {
        private string SDKResult;
        public OESISException(string message, string SDKResult) : base(message)
        {
            this.SDKResult = SDKResult;
        }

        public ErrorResult GetErrorResult()
        {
            return OESISUtil.GetErrorResult(SDKResult);
        }
    }
}
