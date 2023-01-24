using Microsoft.Extensions.Logging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace AkashaScanner.Core.TextRecognition
{
    public class TemplateMatchingService : IDisposable
    {
        private static readonly string InGameFontFile = Path.Combine(Utils.ExecutableDirectory, "Resources", "InGameFont.ttf");
        private readonly FontFamily InGameFontFamily;
        private readonly Dictionary<string, Dictionary<float, Mat>> Mats = new();
        private readonly object Lock = new();

        private readonly ILogger Logger;

        public TemplateMatchingService(ILogger<TemplateMatchingService> logger)
        {
            Logger = logger;
            var myFonts = new PrivateFontCollection();
            myFonts.AddFontFile(InGameFontFile);
            InGameFontFamily = myFonts.Families[0];
        }

        private Mat InternalGetTextImg(string text, float fontSize)
        {
            using var font = new Font(InGameFontFamily, fontSize);
            using var measureBitmap = new Bitmap(1, 1);
            using var measureGraphics = Graphics.FromImage(measureBitmap);
            var textSize = measureGraphics.MeasureString(text, font);
            using var img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            using var drawing = Graphics.FromImage(img);
            drawing.Clear(Color.Black);
            drawing.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

            using var textBrush = new SolidBrush(Color.White);
            drawing.DrawString(text, font, textBrush, 0, 0);
            drawing.Save();
            using var cloned = img.Clone(new Rectangle(0, 0, img.Width, img.Height), PixelFormat.Format8bppIndexed);
            using var textMat = cloned.ToMat();
            var rect = Cv2.BoundingRect(textMat);
            return new Mat(textMat, rect);
        }

        public Mat GetTextImg(string text, float fontSize)
        {
            lock (Lock)
            {
                if (!Mats.TryGetValue(text, out var dict))
                {
                    dict = new();
                    Mats[text] = dict;
                }
                if (!dict.TryGetValue(fontSize, out var img))
                {
                    img = InternalGetTextImg(text, fontSize);
                    dict[fontSize] = img;
                }
                return img;
            }
        }

        public Mat GetResult(Mat img, Mat textImg)
        {
            var result = new Mat();
            Cv2.MatchTemplate(img, textImg, result, TemplateMatchModes.CCorrNormed);
            return result;
        }

        public (int, OpenCvSharp.Point) GetPoint(Mat result)
        {
            var dummy = new int[2];
            var point = new int[2];
            Cv2.MinMaxIdx(result, out double _, out double max, dummy, point);
            return ((int)Math.Round(max * 100), new OpenCvSharp.Point(point[1], point[0]));
        }

        public (int, OpenCvSharp.Point) MatchRect(Mat img, Mat textImg)
        {
            using var result = GetResult(img, textImg);
            return GetPoint(result);
        }

        public int Match(Mat img, Mat textImg)
        {
            using var result = GetResult(img, textImg);
            Cv2.MinMaxLoc(result, out double _, out double max);
            return (int)Math.Round(max * 100);
        }

        public void Dispose()
        {
            foreach (var (_, dict) in Mats)
            {
                foreach (var (_, mat) in dict)
                {
                    mat.Dispose();
                }
            }
            Mats.Clear();
        }
    }
}
