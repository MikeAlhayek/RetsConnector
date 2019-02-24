using System;

namespace CrestApps.RetsSdk.Exceptions
{
    public class RetsParsingException : Exception
    {
        public RetsParsingException()
            : base("Unable to parse the respond")
        {
        }

        public RetsParsingException(string message)
            : base(message)
        {
        }
    }
}
