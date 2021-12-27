using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using EntityStorage;
using System.Drawing;
using DbEntities;

namespace StorageImageIdentificationUI
{
    public static class DbSession
    {
        public static PictureInfo WriteTo(PictureInfo pictureInfo)
        {
            byte[] byteArray = (byte[])new ImageConverter().ConvertTo(pictureInfo.Bitmap, typeof(byte[]));
            string hashCode = 
                string.Join("", new MD5CryptoServiceProvider().ComputeHash(byteArray).Select(byteEl => byteEl.ToString("X2")));

            using (var db = new entityDataContext())
            {
                if (!IsDuplicateExist(db, byteArray, hashCode)) // check if db contains a similar image
                {
                    List<RecognizedCategory> recognizedObjects = new List<RecognizedCategory>();
                    foreach (var category in pictureInfo.RecognizedObjects)
                    {
                        var recObj = new RecognizedCategory()
                        {
                            Name = category.Key,
                            Confidence = category.Value
                        };
                        recognizedObjects.Add(recObj);
                        db.Add(recObj);
                    }
                    
                    PictureInformation imageInfo = new PictureInformation()
                    {
                        Name = pictureInfo.Name,
                        Hash = hashCode,
                        PictureDetails = new PictureDetails() { Content = byteArray },
                        RecognizedCategories = recognizedObjects
                    };
                    db.Add(imageInfo);

                    db.SaveChanges();
                }
            }

            return pictureInfo;
        }

        public static IEnumerable<PictureInfo> ReadFrom()
        {
            using (var db = new entityDataContext())
            {
                foreach (var pictureInfo in db.PicturesInfo.AsEnumerable())
                {
                    db.Entry(pictureInfo).Collection(picInfo => picInfo.RecognizedCategories).Load();
                    db.Entry(pictureInfo).Reference(picInfo => picInfo.PictureDetails).Load();
                    var pictureName = pictureInfo.Name;
                    var recognizedCategories = new List<KeyValuePair<string, double>>(
                        pictureInfo.RecognizedCategories.Select(obj => new KeyValuePair<string, double>(obj.Name, obj.Confidence)));

                    yield return
                        new PictureInfo(pictureInfo.Name, recognizedCategories, pictureInfo.PictureDetails.Content, pictureInfo.Id);
                }
            }
        }

        public static PictureInfo RemoveFrom(PictureInfo pictureInfo)
        {
            using (var db = new entityDataContext())
            {
                var imageInfo = db.PicturesInfo.Include(picInfo => picInfo.RecognizedCategories)
                                               .Include(picInfo => picInfo.PictureDetails)
                                               .Where(picInfo => picInfo.Id == pictureInfo.Id)
                                               .FirstOrDefault();
                if (imageInfo != null)
                {
                    db.Remove(imageInfo);
                    db.SaveChanges();
                }
            }
            return pictureInfo;
        }

        private static bool IsDuplicateExist(entityDataContext db, byte[] imgByteArray, string imgHashCodeStr)
        {
            bool repeated = false;
            var similarImgs = db.PicturesInfo.Where(info => info.Hash.Equals(imgHashCodeStr));
            if (similarImgs != null)
            {
                foreach (var img in similarImgs)
                {
                    db.Entry(img).Reference(imgInfo => imgInfo.PictureDetails).Load();
                    repeated = Enumerable.SequenceEqual(img.PictureDetails.Content, imgByteArray);
                    if (repeated)
                        break;
                }
            }
            return repeated;
        }
    }
}
