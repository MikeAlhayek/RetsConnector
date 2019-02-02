using System.ComponentModel;
using System.Xml.Linq;

namespace RetsSdk.Models
{
    [Description("METADATA-LOOKUP")]
    public class RetsLookupCollection : RetsCollection<RetsLookup>
    {
        public string Resource { get; set; }
        public RetsLookupTypeCollection LookupTypes { get; set; }

        public override void Load(XElement xElement)
        {
            Load(this.GetType(), xElement);
        }
    }
}