using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using YOLOv4MLNet.DataStructures;
using System.Drawing;

namespace YOLOv4
{
    public class Result : IDisposable
    {
        public struct imageResult
        {
            public string ObjName { get; set; }
            public double Confidence { get; set; }
            public override string ToString() => $"{ObjName} with confidence = {Confidence}";
        }

        private Dictionary<string, int> _categoriesCount = null;

        public Result(IReadOnlyList<YoloV4Result> results, string imgName, Bitmap bitmap)
        {
            ImageName = imgName;
            _categoriesCount = new Dictionary<string, int>();
            var categories = new List<imageResult>();
            foreach (var res in results)
            {
                if (!_categoriesCount.ContainsKey(res.Label))
                    _categoriesCount[res.Label] = 0;
                _categoriesCount[res.Label]++;

                imageResult imageResult = new imageResult();
                imageResult.ObjName = res.Label;
                imageResult.Confidence = res.Confidence;
                categories.Add(imageResult);
            }

            Categories = categories;
            Bitmap = bitmap;
        }

        public string ImageName { get; }
        public IReadOnlyList<imageResult> Categories { get; }
        public Bitmap Bitmap { get; }

        public bool IsEmpty() => Categories.Count == 0;

        public override string ToString() =>
           $"In {ImageName} Found {string.Join(", ", _categoriesCount.ToList().Select(pair => $"{pair.Key} x{pair.Value}"))}";

        public void Dispose()
        {
            Bitmap.Dispose();
        }
    }
}
