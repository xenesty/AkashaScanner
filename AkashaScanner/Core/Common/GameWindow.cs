using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core
{
    public class GameWindow
    {
        private const int ReferenceWidth = 1280;

        public int WindowX { get; private set; }
        public int WindowY { get; private set; }
        public int WindowWidth { get; private set; }
        public int WindowHeight { get; private set; }
        public readonly int RefWidth = ReferenceWidth;
        public int RefHeight { get; private set; }
        public double ScaleMultiplier { get; private set; }

        private readonly ILogger Logger;
        public GameWindow(ILogger<GameWindow> logger)
        {
            Logger = logger;
        }

        public bool SetRect(Rectangle winRect)
        {
            // Will not support aspect ratio wider than 16:9
            if (winRect.Height * 1.0d / winRect.Width < 0.56)
            {
                Logger.LogCritical("Does not support wide window. Current window size: {Width}x{Height}", winRect.Width, winRect.Height);
                return false;
            }

            WindowX = winRect.Left;
            WindowY = winRect.Top;
            WindowWidth = winRect.Width;
            WindowHeight = winRect.Height;

            ScaleMultiplier = winRect.Width * 1.0 / ReferenceWidth;
            RefHeight = (int)Math.Round(winRect.Height / ScaleMultiplier, MidpointRounding.AwayFromZero);
            return true;
        }

        public int Scale(int value)
        {
            return (int)Math.Round(value * ScaleMultiplier, MidpointRounding.AwayFromZero);
        }

        public int GetX(int x)
        {
            return (x < 0 ? WindowWidth : 0) + Scale(x) + WindowX;
        }

        public int GetY(int y)
        {
            return (y < 0 ? WindowHeight : 0) + Scale(y) + WindowY;
        }

        public Size ScaleSize(int width, int height)
        {
            return new Size(Scale(width), Scale(height));
        }

        public Point GetPoint(int x, int y)
        {
            return new Point(GetX(x), GetY(y));
        }

        public Point ScalePoint(Point p)
        {
            return ScalePoint(p.X, p.Y);
        }

        public Point ScalePoint(int x, int y)
        {
            return new Point(Scale(x), Scale(y));
        }

        public Rectangle GetRectangle(int left, int top, int right, int bottom)
        {
            var x = GetX(left);
            var y = GetY(top);
            var w = right == 0 ? WindowWidth : GetX(right) - x;
            var h = bottom == 0 ? WindowHeight : GetY(bottom) - y;
            return new Rectangle(x, y, w, h);
        }

        public Rectangle ScaleRectangle(Rectangle rect)
        {
            return ScaleRectangle(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        public Rectangle ScaleRectangle(int left, int top, int width, int height)
        {
            return new Rectangle(Scale(left), Scale(top), Scale(width), Scale(height));
        }
    }
}
