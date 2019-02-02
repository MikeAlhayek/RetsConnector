using System.ComponentModel;
using System.Xml.Linq;

namespace RetsSdk.Models
{
    [Description("METADATA-OBJECT")]
    public class RetsObjectCollection : RetsCollection<RetsObject>
    {
        public override void Load(XElement xElement)
        {
            Load(GetType(), xElement);
        }
    }
}
