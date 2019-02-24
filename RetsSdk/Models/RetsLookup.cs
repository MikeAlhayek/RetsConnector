namespace CrestApps.RetsSdk.Models
{
    public class RetsLookup
    {
        public string MetadataEntryId { get; set; }
        public string LookupName { get; set; }
        public string VisibleName { get; set; }
        public string LookupTypeVersion { get; set; }
        public string LookupTypeDate { get; set; }

        public RetsLookupTypeCollection LookupTypes { get; set; }
    }
}
