using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Tesseract;

namespace AkashaScanner.Core.TextRecognition.Tesseract
{
    public class TesseractService : ITextRecognitionService
    {
        private static readonly string TrainedDataPath = Path.Combine(Utils.ExecutableDirectory, "Resources", "tessdata");
        private const string TrainedDataLanguage = "genshin_eng";

        private static float NormalFn(byte red, byte green, byte blue)
        {
            return red * .3f + green * .59f + blue * .11f;
        }

        private static float InvertFn(byte red, byte green, byte blue)
        {
            return 1 - NormalFn(red, green, blue);
        }

        private readonly ObjectPool<TesseractEngine> Engines;

        private readonly ObjectPool<Pix, (int, int)> PixPool;

        private readonly ILogger Logger;

        public TesseractService(ILogger<TesseractService> logger)
        {
            Logger = logger;
            Engines = new(() => new TesseractEngine(TrainedDataPath, TrainedDataLanguage, EngineMode.LstmOnly));
            PixPool = new((size) => Pix.Create(size.Item1, size.Item2, 8));
        }

        public string FindChar(Bitmap bitmap, bool inverted = false)
        {
            using var instance = new TesseractInstance(this);
            return instance.FindChar(bitmap, inverted);
        }

        public string FindChar(Bitmap bitmap, Func<byte, byte, byte, float> fn)
        {
            using var instance = new TesseractInstance(this);
            return instance.FindChar(bitmap, fn);
        }

        public string FindChar(Bitmap bitmap, Rectangle region, bool inverted = false)
        {
            using var instance = new TesseractInstance(this);
            return instance.FindChar(bitmap, region, inverted);
        }

        public string FindChar(Bitmap bitmap, Rectangle region, Func<byte, byte, byte, float> fn)
        {
            using var instance = new TesseractInstance(this);
            return instance.FindChar(bitmap, region, fn);
        }

        public string FindText(Bitmap bitmap, bool inverted = false)
        {
            using var instance = new TesseractInstance(this);
            return instance.FindText(bitmap, inverted);
        }

        public string FindText(Bitmap bitmap, Func<byte, byte, byte, float> fn)
        {
            using var instance = new TesseractInstance(this);
            return instance.FindText(bitmap, fn);
        }

        public string FindText(Bitmap bitmap, Rectangle region, bool inverted = false)
        {
            using var instance = new TesseractInstance(this);
            return instance.FindText(bitmap, region, inverted);
        }

        public string FindText(Bitmap bitmap, Rectangle region, Func<byte, byte, byte, float> fn)
        {
            using var instance = new TesseractInstance(this);
            return instance.FindText(bitmap, region, fn);
        }

        public List<string> FindLines(Bitmap bitmap, bool inverted = false)
        {
            using var instance = new TesseractInstance(this);
            return instance.FindLines(bitmap, inverted);
        }

        public List<string> FindLines(Bitmap bitmap, Func<byte, byte, byte, float> fn)
        {
            using var instance = new TesseractInstance(this);
            return instance.FindLines(bitmap, fn);
        }

        public List<string> FindLines(Bitmap bitmap, Rectangle region, bool inverted = false)
        {
            using var instance = new TesseractInstance(this);
            return instance.FindLines(bitmap, region, inverted);
        }

        public List<string> FindLines(Bitmap bitmap, Rectangle region, Func<byte, byte, byte, float> fn)
        {
            using var instance = new TesseractInstance(this);
            return instance.FindLines(bitmap, region, fn);
        }

        public List<string> Find(Bitmap bitmap, bool inverted = false)
        {
            using var instance = new TesseractInstance(this);
            return instance.Find(bitmap, inverted);
        }

        public List<string> Find(Bitmap bitmap, Func<byte, byte, byte, float> fn)
        {
            using var instance = new TesseractInstance(this);
            return instance.Find(bitmap, fn);
        }

        public List<string> Find(Bitmap bitmap, Rectangle region, bool inverted = false)
        {
            using var instance = new TesseractInstance(this);
            return instance.Find(bitmap, region, inverted);
        }

        public List<string> Find(Bitmap bitmap, Rectangle region, Func<byte, byte, byte, float> fn)
        {
            using var instance = new TesseractInstance(this);
            return instance.Find(bitmap, region, fn);
        }

        public ITextRecognitionService GetInstance()
        {
            return new TesseractInstance(this);
        }

        public void Dispose()
        {
            Engines.Dispose();
            PixPool.Dispose();
            GC.SuppressFinalize(this);
        }

        private class TesseractInstance : ITextRecognitionService
        {
            private readonly ObjectPool<TesseractEngine>.IObject Engine;
            private readonly TesseractService Service;

            public TesseractInstance(TesseractService service)
            {
                Service = service;
                Engine = service.Engines.Get();
            }

            public void Dispose()
            {
                Engine.Dispose();
            }

            public ITextRecognitionService GetInstance()
            {
                return new TesseractInstance(Service);
            }

            public string FindChar(Bitmap bitmap, bool inverted = false)
            {
                return FindChar(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), inverted);
            }

            public string FindChar(Bitmap bitmap, Func<byte, byte, byte, float> fn)
            {
                return FindChar(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), fn);
            }

            public string FindChar(Bitmap bitmap, Rectangle region, bool inverted = false)
            {
                return FindChar(bitmap, region, inverted ? InvertFn : NormalFn);
            }

            public string FindChar(Bitmap bitmap, Rectangle region, Func<byte, byte, byte, float> fn)
            {
                using var page = Engine.Data.Process(Service.ToPix(bitmap, region, fn), PageSegMode.SingleChar);
                using var iter = page.GetIterator();
                iter.Begin();
                var result = iter.GetText(PageIteratorLevel.Word);

                Service.Logger.LogDebug("Found `{result}`", result);

                return result ?? "";
            }

            public string FindText(Bitmap bitmap, bool inverted = false)
            {
                return FindText(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), inverted);
            }

            public string FindText(Bitmap bitmap, Func<byte, byte, byte, float> fn)
            {
                return FindText(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), fn);
            }

            public string FindText(Bitmap bitmap, Rectangle region, bool inverted = false)
            {
                return FindText(bitmap, region, inverted ? InvertFn : NormalFn);
            }

            public string FindText(Bitmap bitmap, Rectangle region, Func<byte, byte, byte, float> fn)
            {
                using var page = Engine.Data.Process(Service.ToPix(bitmap, region, fn), PageSegMode.SingleLine);
                using var iter = page.GetIterator();
                iter.Begin();
                var result = iter.GetText(PageIteratorLevel.Block)?.Trim() ?? "";

                Service.Logger.LogDebug("Found `{result}`", result);

                return result ?? "";
            }

            public List<string> FindLines(Bitmap bitmap, bool inverted = false)
            {
                return FindLines(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), inverted);
            }

            public List<string> FindLines(Bitmap bitmap, Func<byte, byte, byte, float> fn)
            {
                return FindLines(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), fn);
            }

            public List<string> FindLines(Bitmap bitmap, Rectangle region, bool inverted = false)
            {
                return FindLines(bitmap, region, inverted ? InvertFn : NormalFn);
            }

            public List<string> FindLines(Bitmap bitmap, Rectangle region, Func<byte, byte, byte, float> fn)
            {
                List<string> texts = new();
                using var page = Engine.Data.Process(Service.ToPix(bitmap, region, fn), PageSegMode.SingleBlock);
                using var iter = page.GetIterator();
                iter.Begin();
                do
                {
                    var text = iter.GetText(PageIteratorLevel.TextLine);
                    if (!string.IsNullOrWhiteSpace(text)) texts.Add(text.Trim());
                }
                while (iter.Next(PageIteratorLevel.TextLine));

                Service.Logger.LogDebug("Found `{result}`", string.Join(" / ", texts));

                return texts ?? new();
            }

            public List<string> Find(Bitmap bitmap, bool inverted = false)
            {
                return Find(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), inverted);
            }

            public List<string> Find(Bitmap bitmap, Func<byte, byte, byte, float> fn)
            {
                return Find(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), fn);
            }

            public List<string> Find(Bitmap bitmap, Rectangle region, bool inverted = false)
            {
                return Find(bitmap, region, inverted ? InvertFn : NormalFn);
            }

            public List<string> Find(Bitmap bitmap, Rectangle region, Func<byte, byte, byte, float> fn)
            {
                List<string> texts = new();
                using var page = Engine.Data.Process(Service.ToPix(bitmap, region, fn), PageSegMode.Auto);
                using var iter = page.GetIterator();
                iter.Begin();
                do
                {
                    var text = iter.GetText(PageIteratorLevel.TextLine);
                    if (!string.IsNullOrWhiteSpace(text)) texts.Add(text.Trim());
                }
                while (iter.Next(PageIteratorLevel.TextLine));

                Service.Logger.LogDebug("Found `{result}`", string.Join(" / ", texts));

                return texts ?? new();
            }
        }

        private unsafe Pix ToPix(Bitmap bitmap, Rectangle region, Func<byte, byte, byte, float> fn)
        {
            BitmapData? imgData = null;

            lock (bitmap)
            {
                try
                {
                    imgData = bitmap.LockBits(region, ImageLockMode.ReadOnly, bitmap.PixelFormat);

                    var width = imgData.Width;
                    var height = imgData.Height;

                    using var pixObj = PixPool.Get((width, height));

                    var pix = pixObj.Data;

                    pix.XRes = (int)Math.Round(bitmap.HorizontalResolution);
                    pix.YRes = (int)Math.Round(bitmap.VerticalResolution);

                    var pixData = pix.GetData();
                    for (int y = 0; y < height; ++y)
                    {
                        byte* imgLine = (byte*)imgData.Scan0 + (y * imgData.Stride);
                        uint* pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);
                        for (int x = 0; x < width; ++x)
                        {
                            byte* pixelPtr = imgLine + x * 3;
                            byte blue = pixelPtr[0];
                            byte green = pixelPtr[1];
                            byte red = pixelPtr[2];
                            float val = fn(red, green, blue);
                            PixData.SetDataByte(pixLine, x, (byte)Math.Round(val));
                        }
                    }
                    return pix;
                }
                finally
                {
                    if (imgData != null)
                        bitmap.UnlockBits(imgData);
                }
            }
        }
    }
}
