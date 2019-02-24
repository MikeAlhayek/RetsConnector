using CrestApps.RetsSdk.Exceptions;
using System;

namespace CrestApps.RetsSdk.Models
{
    public class Version
    {
        public int? Major { get; set; }
        public int? Minor { get; set; }
        public int? Patch { get; set; }
        public char WildCard { get; set; } = '*';

        public Version()
        {

        }
        public Version(string version, char wildCard = '*')
        {
            WildCard = wildCard;
            Load(version);
        }

        public void Load(string version)
        {
            string[] parts = GetVersionParts(version);
            int totalParts = parts.Length;

            Major = ParseValue(parts[0]);
            Minor = ParseValue(parts[1]);

            if (totalParts == 3)
            {
                Patch = ParseValue(parts[2]);
            }
        }

        public Version(int? major)
        {
            Major = major;
        }

        public Version(int? major, int? minor)
            :this(major)
        {
            Minor = minor;
        }

        public Version(int? major, int? minor, int? patch)
            : this(major, minor)
        {
            Patch = patch;
        }

        public override string ToString()
        {
            return $"{Major ?? WildCard}.{Minor ?? WildCard}.{Patch ?? WildCard}";
        }

        protected virtual string[] GetVersionParts(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentNullException($"{nameof(version)} cannot be null.");
            }

            string[] parts = version.Replace('_','.').Split('.');
            int totalParts = parts.Length;
            if (totalParts == 0 || totalParts > 3)
            {
                throw new VersionParsingException();
            }

            return parts;
        }

        protected virtual int? ParseValue(string part)
        {

            if (string.IsNullOrWhiteSpace(part) || part.Equals(WildCard.ToString()))
            {
                return default(int?);
            }

            if (!int.TryParse(part, out int value))
            {
                throw new VersionParsingException();
            }

            return value;
        }
    }
}
