using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace CrestApps.RetsSdk.Models
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

        public override RetsLookupType Get(object value)
        {
            RetsLookupType item = Get().FirstOrDefault(x => x.ShortValue.Equals(value.ToString(), StringComparison.CurrentCultureIgnoreCase));

            return item;
        }
    }
}
