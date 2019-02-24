using System;

namespace CrestApps.RetsSdk.Exceptions
{
    public class VersionParsingException : Exception
    {
        public VersionParsingException()
            : base("The given version is not valid. valid version should in the following format 'Number.Number.Number' ")
        {

        }

        public VersionParsingException(string message)
            : base(message)
        {

        }
    }
}
