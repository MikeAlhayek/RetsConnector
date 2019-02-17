using System;

namespace RetsSdk.Exceptions
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
