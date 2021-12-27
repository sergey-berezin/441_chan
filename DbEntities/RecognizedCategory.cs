using System.ComponentModel.DataAnnotations;

namespace DbEntities
{
    public class RecognizedCategory
    {
        [Key]
        public int ObjectId { get; set; }

        public int PictureInfoId { get; set; }

        public string Name { get; set; }

        public double Confidence { get; set; }
    }
}
