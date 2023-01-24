using Microsoft.Extensions.Logging;
using System.Drawing.Imaging;
using Tesseract;

namespace AkashaScanner.Core.TextRecognition.Tesseract
{
    public class TesseractService : ITextRecognitionService
    {
        private static readonly string TrainedDataPath = Path.Combine(Utils.ExecutableDirectory, "Resources", "tessdata");
        private const string TrainedDataLanguage = "genshin_eng";
        private const int MaxColorVariation = 10;

        static TesseractService()
        {
            Environment.SetEnvironmentVariable("OMP_THREAD_LIMIT", "1");
            TesseractEnviornment.CustomSearchPath = Utils.ExecutableDirectory;
        }

        private static byte NormalFn(byte red, byte green, byte blue)
        {
            return (byte)Math.Round(red * .3f + green * .59f + blue * .11f);
        }

        private static byte InvertFn(byte red, byte green, byte blue)
        {
            return (byte)(255 - NormalFn(red, green, blue));
        }

        private static Func<byte, byte, byte, byte> CreateColorFn(Color color)
        {
            return (byte red, byte green, byte blue) =>
            {
                if (Math.Abs(red - color.R) > MaxColorVariation) return 255;
                if (Math.Abs(green - color.G) > MaxColorVariation) return 255;
                if (Math.Abs(blue - color.B) > MaxColorVariation) return 255;
                return 0;
            };
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


        public string FindChar(Bitmap bitmap, Rectangle? region = null, bool inverted = false, Color? color = null)
        {
            using var instance = new TesseractInstance(this);
            return instance.FindChar(bitmap, region, inverted, color);
        }

        public string FindText(Bitmap bitmap, Rectangle? region = null, bool inverted = false, Color? color = null)
        {
            using var instance = new TesseractInstance(this);
            return instance.FindText(bitmap, region, inverted, color);
        }

        public List<string> FindLines(Bitmap bitmap, Rectangle? region = null, bool inverted = false, Color? color = null)
        {
            using var instance = new TesseractInstance(this);
            return instance.FindLines(bitmap, region, inverted, color);
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

            public string FindChar(Bitmap bitmap, Rectangle? region = null, bool inverted = false, Color? color = null)
            {
                using var page = Engine.Data.Process(Service.ToPix(bitmap, region, inverted, color), PageSegMode.SingleChar);
                using var iter = page.GetIterator();
                iter.Begin();
                var result = iter.GetText(PageIteratorLevel.Word);

                Service.Logger.LogDebug("Found `{result}`", result);

                return result ?? "";
            }

            public string FindText(Bitmap bitmap, Rectangle? region = null, bool inverted = false, Color? color = null)
            {
                using var page = Engine.Data.Process(Service.ToPix(bitmap, region, inverted, color), PageSegMode.SingleLine);
                using var iter = page.GetIterator();
                iter.Begin();
                var result = iter.GetText(PageIteratorLevel.Block)?.Trim() ?? "";

                Service.Logger.LogDebug("Found `{result}`", result);

                return result ?? "";
            }

            public List<string> FindLines(Bitmap bitmap, Rectangle? region = null, bool inverted = false, Color? color = null)
            {
                List<string> texts = new();
                using var page = Engine.Data.Process(Service.ToPix(bitmap, region, inverted, color), PageSegMode.SingleBlock);
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

        private unsafe Pix ToPix(Bitmap bitmap, Rectangle? region, bool inverted, Color? color)
        {
            BitmapData? imgData = null;
            var area = region ?? new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var fn = color != null ? CreateColorFn(color.Value) : inverted ? InvertFn : NormalFn;

            lock (bitmap)
            {
                try
                {
                    imgData = bitmap.LockBits(area, ImageLockMode.ReadOnly, bitmap.PixelFormat);

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
                            PixData.SetDataByte(pixLine, x, fn(red, green, blue));
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
