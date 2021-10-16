using System;
using System.Threading.Tasks;
using YOLOv4;

namespace Image_Identification
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Please enter the folder path : ");
            string imageFolder = Console.ReadLine();

            using (var picture = new Processing())
            {
                await foreach (var processResult in picture.ProcessImagesAsync(imageFolder))
                    Console.WriteLine(processResult);
            }
        }
    }
}