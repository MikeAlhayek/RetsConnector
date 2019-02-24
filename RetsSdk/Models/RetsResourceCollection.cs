using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace CrestApps.RetsSdk.Models
{
    [Description("METADATA-RESOURCE")]
    public class RetsResourceCollection : RetsCollection<RetsResource>
    {
        public override void Load(XElement xElement)
        {
            Load(GetType(), xElement);
        }

        public override RetsResource Get(object value)
        {
            RetsResource item = Get().FirstOrDefault(x => x.ResourceId.Equals(value.ToString(), StringComparison.CurrentCultureIgnoreCase));

            return item;
        }
    }
}
