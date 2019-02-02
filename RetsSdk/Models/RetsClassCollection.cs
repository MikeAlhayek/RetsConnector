using System.ComponentModel;
using System.Xml.Linq;

namespace RetsSdk.Models
{
    [Description("METADATA-CLASS")]
    public class RetsClassCollection : RetsCollection<RetsClass>
    {
        public string Resource { get; set; }

        public override void Load(XElement xElement)
        {
            Load(GetType(), xElement);
        }
    }
}
