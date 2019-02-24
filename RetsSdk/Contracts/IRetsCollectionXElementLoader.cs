using System.Xml.Linq;

namespace CrestApps.RetsSdk.Contracts
{
    public interface IRetsCollectionXElementLoader
    {
        void Load(XElement xElement);
    }
}
