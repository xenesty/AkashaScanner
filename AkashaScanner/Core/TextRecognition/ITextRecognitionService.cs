namespace AkashaScanner.Core.TextRecognition
{
    public interface ITextRecognitionService : IDisposable
    {
        string FindChar(Bitmap bitmap, Rectangle? region = null, bool inverted = false, Color? color = null);
        string FindText(Bitmap bitmap, Rectangle? region = null, bool inverted = false, Color? color = null);
        List<string> FindLines(Bitmap bitmap, Rectangle? region = null, bool inverted = false, Color? color = null);
        ITextRecognitionService GetInstance();
    }
}
