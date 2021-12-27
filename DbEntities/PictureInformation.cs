using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DbEntities
{
    public class PictureInformation
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Hash { get; set; }

        virtual public PictureDetails PictureDetails { get; set; }

        virtual public  ICollection<RecognizedCategory> RecognizedCategories { get; set; }
    }
}
