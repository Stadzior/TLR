using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Org.Tensorflow.Contrib.Android;
using Plugin.CurrentActivity;
using TLR.Enums;
using TLR.Services;

namespace TLR.Droid
{
    public class PhotoDetector : IPhotoDetector
    {
        private static readonly string ModelFile = "model.pb";
        private static readonly string LabelFile = "labels.txt";
        private static readonly string InputName = "Placeholder";
        private static readonly string OutputName = "loss";
        private static readonly int InputSize = 227;
        private readonly TensorFlowInferenceInterface _inferenceInterface;
        private readonly string[] _labels;

        public PhotoDetector()
        {
            _inferenceInterface = new TensorFlowInferenceInterface(CrossCurrentActivity.Current.Activity.Assets, ModelFile);
            using var sr = new StreamReader(CrossCurrentActivity.Current.Activity.Assets.Open(LabelFile));
            _labels = sr.ReadToEnd().Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
        }

        public async Task<Tag> DetectAsync(Stream photo)
        {
            var bitmap = await BitmapFactory.DecodeStreamAsync(photo);
            var floatValues = GetBitmapPixels(bitmap);
            var outputs = new float[_labels.Length];
            _inferenceInterface.Feed(InputName, floatValues, 1, InputSize, InputSize, 3);
            _inferenceInterface.Run(new[] { OutputName });
            _inferenceInterface.Fetch(OutputName, outputs);
            var index = Array.IndexOf(outputs, outputs.Max());
            return (Tag)Enum.Parse(typeof(Tag), _labels[index]);
        }

        private async Task LoadByteArrayFromAssetsAsync(string name)
        {
            await using var s = CrossCurrentActivity.Current.Activity.Assets.Open(name);
            await using var ms = new MemoryStream();
            await s.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);
        }

        private static float[] GetBitmapPixels(Bitmap bitmap)
        {
            var floatValues = new float[InputSize * InputSize * 3];
            using var scaledBitmap = Bitmap.CreateScaledBitmap(bitmap, InputSize, InputSize, false);
            using var resizedBitmap = scaledBitmap.Copy(Bitmap.Config.Argb8888, false);
            var intValues = new int[InputSize * InputSize];
            resizedBitmap.GetPixels(intValues, 0, resizedBitmap.Width, 0, 0, resizedBitmap.Width, resizedBitmap.Height);
            resizedBitmap.Recycle();
            scaledBitmap.Recycle();

            return floatValues;
        }
    }
} 