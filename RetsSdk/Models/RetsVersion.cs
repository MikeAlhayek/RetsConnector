using CrestApps.RetsSdk.Helpers;
using CrestApps.RetsSdk.Models.Enums;
using System;

namespace CrestApps.RetsSdk.Models
{
    public class RetsVersion : Version
    {
        private SupportedRetsVersion RetVersion;

        public RetsVersion(SupportedRetsVersion retsVersion)
        {
            RetVersion = retsVersion;
            string version = ExtractVersionNumber(retsVersion);

            Load(version);
        }

        public string AsHeader()
        {
            return $"RETS/{ToString()}";
        }

        public override string ToString()
        {
            string version = string.Empty;

            if (!Major.HasValue)
            {
                throw new NullReferenceException($"The {Major} value cannot be null.");
            }

            if (!Minor.HasValue)
            {
                throw new NullReferenceException($"The {Major} value cannot be null.");
            }

            if (!Patch.HasValue)
            {
                return $"{Major}.{Minor}";
            }

            return base.ToString();
        }

        private static string ExtractVersionNumber(SupportedRetsVersion retsVersion)
        {
            return Str.TrimStart(retsVersion.ToString(), "Version_").Replace('_', '.');
        }

        public static SupportedRetsVersion Make(string version)
        {
            var v = Str.TrimStart(version, "RETS/").Replace('.', '_');

            var castable = Str.PrependOnce(v, "Version_");

            return Enum.Parse<SupportedRetsVersion>(castable);
        }

    }
}
