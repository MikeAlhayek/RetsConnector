using System;

namespace CrestApps.RetsSdk.Exceptions
{
    public class TooManyOutstandingQueries : Exception
    {
        public TooManyOutstandingQueries()
            : base("Too many outstanding queries")
        {
        }

        public TooManyOutstandingQueries(string message)
            : base(message)
        {
        }
    }
}
