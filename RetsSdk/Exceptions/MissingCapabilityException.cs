using System;

namespace CrestApps.RetsSdk.Exceptions
{
    public class MissingCapabilityException : Exception
    {
        public MissingCapabilityException()
            : base("The requested capability does not exists")
        {
        }
    }
}
