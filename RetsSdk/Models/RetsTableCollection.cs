using System.ComponentModel;
using System.Xml.Linq;

namespace RetsSdk.Models
{
    [Description("METADATA-TABLE")]
    public class RetsTableCollection : RetsCollection<RetsField>
    {
        public override void Load(XElement xElement)
        {
            Load(GetType(), xElement);
        }
    }
}
