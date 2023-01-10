using AkashaScanner.Core.ProcessControl;
using AkashaScanner.Core.Suspender;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using WindowsInput;

namespace AkashaScanner.Core.Navigation.Mouse
{
    public class WindowsMouseService : IMouseService
    {
        private readonly InputSimulator sim = new();

        private readonly ILogger Logger;
        private readonly ISuspender Suspender;
        private readonly IProcessControl Control;

        public WindowsMouseService(
            ILogger<WindowsMouseService> logger,
            ISuspender suspender,
            IProcessControl control)
        {
            Logger = logger;
            Suspender = suspender;
            Control = control;
        }


        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public void Click()
        {
            if (!Control.IsActive()) return;
            Logger.LogDebug("Click");
            sim.Mouse.LeftButtonClick();
        }

        public void MoveCursorTo(int x, int y)
        {
            if (!Control.IsActive()) return;
            Logger.LogDebug("MoveCursorTo {x}, {y}", x, y);
            SetCursorPos(x, y);
        }

        public void ScrollVerticalBy(int dy)
        {
            if (!Control.IsActive()) return;
            if (dy == 0) return;
            Logger.LogDebug("ScrollVerticalBy {dy}", dy);
            if (dy < 0)
            {
                for (int i = 0; i > dy; --i)
                {
                    sim.Mouse.VerticalScroll(-1);
                    Suspender.Sleep(10);
                }
            }
            else
            {
                for (int i = 0; i < dy; ++i)
                {
                    sim.Mouse.VerticalScroll(1);
                    Suspender.Sleep(10);
                }
            }
        }
    }
}
