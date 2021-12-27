using System.ComponentModel.DataAnnotations;

namespace DbEntities
{
    public class PictureDetails
    {
        [Key]
        public int PictureDetailsId { get; set; }

        public int PictureInfoId { get; set; }

        public byte[] Content { get; set; }
    }
}
