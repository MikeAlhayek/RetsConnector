using System;
using System.Collections.Generic;

namespace RetsSdk.Models
{
    public class RetsSystem
    {
        public string Version { get; set; }
        public DateTime? Date { get; set; }
        public string SystemId { get; set; }
        public string SystemDescription { get; set; }

        public RetsResourceCollection Resources { get; set; }
    }
}
