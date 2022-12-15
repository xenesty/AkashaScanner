namespace AkashaScanner.Core.Screenshot
{
    public interface IScreenshotProvider
    {
        public Bitmap Capture(Rectangle rect);
    }
}
