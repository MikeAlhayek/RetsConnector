using System;

namespace RetsSdk.Exceptions
{
    public class TooManyOutstandingRequests : Exception
    {
        public TooManyOutstandingRequests()
            : base("Too many outstanding requests")
        {
        }

        public TooManyOutstandingRequests(string message)
            : base(message)
        {
        }
    }
}
