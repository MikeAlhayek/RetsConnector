using CrestApps.RetsSdk.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CrestApps.RetsSdk.Models
{
    [Description("METADATA-TABLE")]
    public class RetsFieldCollection : RetsCollection<RetsField>
    {
        public string Resource { get; set; }
        public IEnumerable<RetsLookupTypeCollection> LookupTypes { get; set; }


        public override void Load(XElement xElement)
        {
            Load(GetType(), xElement);
        }

        public override RetsField Get(object value)
        {
            RetsField item = Get().FirstOrDefault(x => x.SystemName.Equals(value.ToString(), StringComparison.CurrentCultureIgnoreCase));

            return item;
        }


        public async Task<IEnumerable<RetsLookupTypeCollection>> GetLookupTypes(IRetsClient session)
        {
            if (LookupTypes == null)
            {
                LookupTypes = await session.GetLookupValues(Resource);
            }

            return LookupTypes;
        }

        public async Task<RetsLookupTypeCollection> GetLookupType(IRetsClient session, string lookupName)
        {
            var lookupTypes = await GetLookupTypes(session);

            RetsLookupTypeCollection lookupType = lookupTypes.FirstOrDefault(x => x.Lookup.Equals(lookupName, StringComparison.CurrentCultureIgnoreCase));

            return lookupType;
        }
    }
}
