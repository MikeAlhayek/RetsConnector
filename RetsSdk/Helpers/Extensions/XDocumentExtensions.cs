using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CrestApps.RetsSdk.Helpers.Extensions
{
    public static class XDocumentExtensions
    {
        public static IEnumerable<XElement> DescendantsCaseInsensitive(this XContainer source, XName name)
        {
            return source.Elements().Where(e => e.Name.Namespace == name.Namespace && e.Name.LocalName.Equals(name.LocalName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
