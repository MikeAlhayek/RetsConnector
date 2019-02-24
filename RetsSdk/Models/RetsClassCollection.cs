using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace CrestApps.RetsSdk.Models
{
    [Description("METADATA-CLASS")]
    public class RetsClassCollection : RetsCollection<RetsClass>
    {
        public string Resource { get; set; }

        public override RetsClass Get(object value)
        {
            RetsClass item = Get().FirstOrDefault(x => x.ClassName.Equals(value.ToString(), StringComparison.CurrentCultureIgnoreCase));

            return item;
        }

        public override void Load(XElement xElement)
        {
            Load(GetType(), xElement);
        }

    }
}
