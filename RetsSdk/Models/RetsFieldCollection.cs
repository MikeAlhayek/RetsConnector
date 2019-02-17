using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace RetsSdk.Models
{
    [Description("METADATA-TABLE")]
    public class RetsFieldCollection : RetsCollection<RetsField>
    {
        public override void Load(XElement xElement)
        {
            Load(GetType(), xElement);
        }

        public override RetsField Get(object value)
        {
            RetsField item = Get().FirstOrDefault(x => x.SystemName.Equals(value.ToString(), StringComparison.CurrentCultureIgnoreCase));

            return item;
        }
    }
}
