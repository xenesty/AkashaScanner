using System;
using System.Collections.Generic;
using System.Drawing;

namespace AkashaScanner.Core.TextRecognition
{
    public interface ITextRecognitionService : IDisposable
    {
        string FindChar(Bitmap bitmap, bool inverted = false);
        string FindChar(Bitmap bitmap, Func<byte, byte, byte, float> fn);
        string FindChar(Bitmap bitmap, Rectangle region, bool inverted = false);
        string FindChar(Bitmap bitmap, Rectangle region, Func<byte, byte, byte, float> fn);

        string FindText(Bitmap bitmap, bool inverted = false);
        string FindText(Bitmap bitmap, Func<byte, byte, byte, float> fn);
        string FindText(Bitmap bitmap, Rectangle region, bool inverted = false);
        string FindText(Bitmap bitmap, Rectangle region, Func<byte, byte, byte, float> fn);

        List<string> FindLines(Bitmap bitmap, bool inverted = false);
        List<string> FindLines(Bitmap bitmap, Func<byte, byte, byte, float> fn);
        List<string> FindLines(Bitmap bitmap, Rectangle region, bool inverted = false);
        List<string> FindLines(Bitmap bitmap, Rectangle region, Func<byte, byte, byte, float> fn);

        List<string> Find(Bitmap bitmap, bool inverted = false);
        List<string> Find(Bitmap bitmap, Func<byte, byte, byte, float> fn);
        List<string> Find(Bitmap bitmap, Rectangle region, bool inverted = false);
        List<string> Find(Bitmap bitmap, Rectangle region, Func<byte, byte, byte, float> fn);

        ITextRecognitionService GetInstance();
    }
}
