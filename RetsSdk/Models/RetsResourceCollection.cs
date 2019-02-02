using System.ComponentModel;
using System.Xml.Linq;

namespace RetsSdk.Models
{
    [Description("METADATA-RESOURCE")]
    public class RetsResourceCollection : RetsCollection<RetsResource>
    {
        public override void Load(XElement xElement)
        {
            Load(GetType(), xElement);
        }
    }
}
