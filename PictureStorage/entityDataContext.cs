using DbEntities;
using Microsoft.EntityFrameworkCore;

namespace EntityStorage
{
    public class entityDataContext : DbContext
    {
        public static string dbPath =
            @"C:\Users\chenr\source\repos\StorageImageIdentificationUI\PictureStorage\Library.db";

        public DbSet<PictureInformation> PicturesInfo { get; set; }

        public DbSet<PictureDetails> PicturesDetails { get; set; }

        public DbSet<RecognizedCategory> RecognizedCategories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder o)
        {
            o.UseSqlite($"Data Source={dbPath}");
        }
    }
}
