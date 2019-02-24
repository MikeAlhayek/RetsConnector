using System;

namespace CrestApps.RetsSdk.Models
{
    public class RetsSystem
    {
        public string Version { get; set; }
        public DateTime? Date { get; set; }
        public string SystemId { get; set; }
        public string SystemDescription { get; set; }
        public string TimeZoneOffset { get; set; } // Not sure what is the proper type but it could be Time.

        public RetsResourceCollection Resources { get; set; }
    }
}
