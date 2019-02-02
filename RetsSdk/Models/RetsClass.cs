using System;
using System.Collections.Generic;

namespace RetsSdk.Models
{
    public class RetsClass
    {
        public string ClassName { get; set; }
        public string StandardName { get; set; }
        public string VisibleName { get; set; }
        public string Description { get; set; }
        public string TableVersion { get; set; }
        public DateTime? TableDate { get; set; }
        public string UpdateVersion { get; set; }
        public DateTime? UpdateDate { get; set; }

        public RetsTableCollection Fields { get; set; }
    }
}