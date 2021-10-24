using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Onnx;
using YOLOv4MLNet.DataStructures;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;
using System.Threading;
using System.Threading.Tasks;

namespace YOLOv4
{
    public class Processing : IDisposable
    {
        const string ModelPath = @"C:\Users\chenr\Desktop\models-master\vision\object_detection_segmentation\yolov4\model\yolov4.onnx";

        const string imageOutputFolder = @"Assets\Output";

        static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet", "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };

        public TransformerChain<OnnxTransformer> model = null;
        public CancellationTokenSource cancellationTokenSource = null;

        public Processing()
        {
            model = createModel(ModelPath);
            cancellationTokenSource = new CancellationTokenSource();
        }

        public async IAsyncEnumerable<string> ProcessImagesAsync(string imageFolder)
        {
            Directory.CreateDirectory(imageOutputFolder);

            var images = Directory.GetFiles(imageFolder);
            List<Task<IReadOnlyList<YoloV4Result>>> tasks = new List<Task<IReadOnlyList<YoloV4Result>>>();
            foreach (var imageName in images)
            {
                if (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Task<IReadOnlyList<YoloV4Result>> iamge = imagePredict(imageName);
                    tasks.Add(iamge);
                }
                else
                    break;
            }

            while (tasks.Count > 0)
            {
                for (int i = 0; i < tasks.Count; i++)
                {
                    await Task.WhenAny(tasks);
                    yield return Result(tasks[i].Result, images[i]);
                    tasks.Remove(tasks[i]);
                }
            }
        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }

        public void Dispose() { }

        public async Task<IReadOnlyList<YoloV4Result>> imagePredict(string fileName)
        {
            Directory.CreateDirectory(imageOutputFolder);

            string imageFolder = fileName.Substring(0, fileName.LastIndexOf(Path.DirectorySeparatorChar));

            MLContext mlContext = new MLContext();

            // Create prediction engine
            var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);
            IReadOnlyList<YoloV4Result> results = null;
            return await Task.Factory.StartNew(() =>
            {
                //Check status of the cancellationToken
                if (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    using (var bitmap = new Bitmap(Image.FromFile(Path.Combine(imageFolder, fileName))))
                    {
                        // predict
                        var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                        var results = predict.GetResults(classesNames, 0.3f, 0.7f);

                        using (var g = Graphics.FromImage(bitmap))
                        {
                            foreach (var res in results)
                            {
                                // draw predictions
                                var x1 = res.BBox[0];
                                var y1 = res.BBox[1];
                                var x2 = res.BBox[2];
                                var y2 = res.BBox[3];
                                g.DrawRectangle(Pens.Red, x1, y1, x2 - x1, y2 - y1);
                                using (var brushes = new SolidBrush(Color.FromArgb(50, Color.Red)))
                                {
                                    g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                                }

                                g.DrawString(res.Label + " " + res.Confidence.ToString("0.00"),
                                             new Font("Arial", 12), Brushes.Blue, new PointF(x1, y1));
                            }
                            bitmap.Save(Path.Combine(imageOutputFolder, Path.ChangeExtension(fileName.Substring(fileName.LastIndexOf(Path.DirectorySeparatorChar) + 1), "_processed" + Path.GetExtension(fileName))));
                        }
                        return results;
                    }
                }
                return results;
            });
        }

        public TransformerChain<OnnxTransformer> createModel(string modelPath)
        {
            MLContext mlContext = new MLContext();

            // Define scoring pipeline
            var pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                .Append(mlContext.Transforms.ApplyOnnxModel(
                    shapeDictionary: new Dictionary<string, int[]>()
                    {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                    },
                    inputColumnNames: new[]
                    {
                        "input_1:0"
                    },
                    outputColumnNames: new[]
                    {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                    },
                    modelFile: modelPath, recursionLimit: 50));

            // Fit on empty list to obtain input data schema
            var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));
            return model;
        }
        public string Result(IReadOnlyList<YoloV4Result> res, string fileName)
        {
            List<string> objectList = new List<string>();
            foreach (var obj in res)
                objectList.Add(obj.Label);

            string obj_List = string.Join(", ", objectList);

            return $"Found {obj_List} in {fileName}\n";
        }
    }
}