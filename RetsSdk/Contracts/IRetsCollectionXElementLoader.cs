using System.Xml.Linq;

namespace RetsSdk.Contracts
{
    public interface IRetsCollectionXElementLoader
    {
        void Load(XElement xElement);
    }
}
