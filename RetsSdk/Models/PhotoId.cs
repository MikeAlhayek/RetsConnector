namespace CrestApps.RetsSdk.Models
{
    public class PhotoId
    {
        public long Id { get; set; }
        public int? ObjectId { get; set; }

        public PhotoId()
        {

        }

        public PhotoId(long id, int? objectId = null)
        {
            Id = id;
            ObjectId = objectId;
        }


        public override string ToString()
        {
            if (!ObjectId.HasValue)
            {
                return string.Format("{0}:*", Id);
            }

            return string.Format("{0}:{1}", Id, ObjectId.Value);
        }
    }
}
