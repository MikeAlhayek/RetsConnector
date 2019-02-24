using System;

namespace CrestApps.RetsSdk.Exceptions
{
    public class RetsException : Exception
    {
        public RetsException()
             : base("Rets server throw an unknow error")
        {
        }

        public RetsException(string message)
             : base(message)
        {
        }
    }
}
