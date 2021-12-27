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
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace YOLOv4
{
    public class Processing
    {
        const string ModelPath = @"C:\Users\chenr\Desktop\models-master\vision\object_detection_segmentation\yolov4\model\yolov4.onnx";

        const string imageOutputFolder = @"Assets\Output";

        static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet", "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };

        public TransformerChain<OnnxTransformer> model = null;
        public CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public IEnumerable<Results> ProcessImagesAsync(string imageFolder)
        {
            List<string> folderPath = new(Directory.GetFiles(imageFolder));
            List<Task<IReadOnlyList<YoloV4Result>>> tasks = new();
            var model = createModel(ModelPath);

            foreach (string imagePath in folderPath)
            {
                if (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Task<IReadOnlyList<YoloV4Result>> iamge = imagePredict(imagePath, model);
                    tasks.Add(iamge);
                }
                else
                    break;
            }

            while (tasks.Count > 0 && !cancellationTokenSource.IsCancellationRequested)
            {
                for (int i = 0; i < tasks.Count; i++)
                {
                    Task.WhenAny(tasks.ToArray());
                    yield return new Results(tasks[i].Result, folderPath[i]);
                    tasks.RemoveAt(i);
                    folderPath.RemoveAt(i);
                }
            }
        }

        public async Task<IReadOnlyList<YoloV4Result>> imagePredict(string imagePath, TransformerChain<OnnxTransformer> model)
        {
            return await Task.Factory.StartNew(() =>
            {
                string imageFolder = imagePath.Substring(0, imagePath.LastIndexOf(Path.DirectorySeparatorChar));
                MLContext mlContext = new MLContext();
                PredictionEngine<YoloV4BitmapData, YoloV4Prediction> predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);

                using (var bitmap = new Bitmap(Image.FromFile(imagePath)))
                {
                    // predict
                    YoloV4Prediction predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                    var results = predict.GetResults(classesNames, 0.3f, 0.7f);
                    return results;
                }
            });
        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
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
    }

    public class Results
    {
        public Results(IReadOnlyList<YoloV4Result> res, string fileName)
        {
            objectList = new List<YoloV4Result>(res);
            ImageName = fileName;
        }

        public List<YoloV4Result> objectList { get; }
        public string ImageName { get; }
    }
}
