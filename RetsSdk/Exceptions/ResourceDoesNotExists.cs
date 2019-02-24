using System;

namespace CrestApps.RetsSdk.Exceptions
{
    public class ResourceDoesNotExists : Exception
    {
        public ResourceDoesNotExists()
            : base("The given resource does not exists.")
        {
        }
    }
}
