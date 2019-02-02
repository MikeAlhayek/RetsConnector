using System.ComponentModel;
using System.Xml.Linq;

namespace RetsSdk.Models
{
    [Description("METADATA-LOOKUP_TYPE")]
    public class RetsLookupTypeCollection : RetsCollection<RetsLookupType>
    {
        public string Resource { get; set; }
        public string Lookup { get; set; }

        public override void Load(XElement xElement)
        {
            Load(GetType(), xElement);
        }
    }
}
