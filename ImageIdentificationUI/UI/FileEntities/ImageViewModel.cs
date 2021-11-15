using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Drawing;
using System.Text;

namespace ImageIdentificationUI.UI.FileEntities
{
    public sealed class ImageViewModel : FileViewModel
    {

        public ImageViewModel(string path) : base(new FileInfo(path)) =>
            Bitmap = new BitmapImage(new Uri(path));

        public ImageViewModel(FileInfo fileInfo) : base(fileInfo) =>
            Bitmap = new BitmapImage(new Uri(fileInfo.FullName));

        public BitmapImage Bitmap { get; set; }


        public void SourceChanged(Bitmap bmp)
        {
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),
                IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            using (var memoryStream = new MemoryStream())
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memoryStream);
                BitmapImage bitmapImage = new BitmapImage();
                memoryStream.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                Bitmap = bitmapImage;
            }

            OnPropertyChanged(nameof(Bitmap));
        }

    }
}
