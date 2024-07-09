using CrestApps.RetsSdk.Models.Enums;
using System;

namespace CrestApps.RetsSdk.Models
{
    public class ConnectionOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public AuthenticationType Type { get; set; }
        public string UserAgent { get; set; }
        public string UserAgentPassword { get; set; }
        public SupportedRetsVersion RetsServerVersion { get; set; } = SupportedRetsVersion.Version_1_7_2;
        public string LoginUrl { get; set; }
        public TimeSpan Timeout { get; set; }

        public ConnectionOptions()
        {
            Timeout = TimeSpan.FromHours(1);
        }

        public RetsVersion Version => new RetsVersion(RetsServerVersion);
    }
}
