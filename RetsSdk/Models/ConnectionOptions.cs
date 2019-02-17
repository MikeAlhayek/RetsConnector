using RetsSdk.Models.Enums;
using System;

namespace RetsSdk.Models
{
    public class ConnectionOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public AuthenticationType Type { get; set; }
        public string UserAgent { get; set; }
        public string RetsServerVersion { get; set; } = "RETS/1.7.2";
        public string LoginUrl { get; set; }
        public TimeSpan Timeout { get; set; }

        public ConnectionOptions()
        {
            Timeout = TimeSpan.FromHours(4);
        }
    }
}
