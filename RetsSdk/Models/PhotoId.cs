namespace RetsSdk.Models
{
    public class PhotoId
    {
        public int Id { get; set; }
        public int? ObjectId { get; set; }

        public PhotoId()
        {

        }

        public PhotoId(int id, int? objectId = null)
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
