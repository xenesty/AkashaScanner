using Microsoft.Extensions.Logging;
using System.Drawing.Imaging;

namespace AkashaScanner.Core.Screenshot
{
    public class ScreenshotProvider : IScreenshotProvider
    {
        private readonly ILogger Logger;
        public ScreenshotProvider(ILogger<ScreenshotProvider> logger)
        {
            Logger = logger;
        }

        public Bitmap Capture(Rectangle rect)
        {
            Logger.LogDebug("Capturing {rect}", rect);
            Bitmap bmp = new(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
            using Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size);
            return bmp;
        }
    }
}
