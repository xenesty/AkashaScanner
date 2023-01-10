using AkashaScanner.Core.ProcessControl;
using AkashaScanner.Core.Suspender;
using Microsoft.Extensions.Logging;
using WindowsInput;
using WindowsInput.Native;

namespace AkashaScanner.Core.Navigation.Keyboard
{
    public class WindowsKeyboardService : IKeyboardService
    {
        private readonly InputSimulator sim = new();

        private readonly ILogger Logger;
        private readonly ISuspender Suspender;
        private readonly IProcessControl Control;

        public WindowsKeyboardService(
            ILogger<WindowsKeyboardService> logger,
            ISuspender suspender,
            IProcessControl control)
        {
            Logger = logger;
            Suspender = suspender;
            Control = control;
        }

        public void Type(string text)
        {
            if (!Control.IsActive()) return;
            Logger.LogDebug("Typing {text}", text);
            foreach (char c in text.ToCharArray())
            {
                sim.Keyboard.TextEntry(c);
                Suspender.Sleep(10);
            }
        }

        public void SendEnter()
        {
            if (!Control.IsActive()) return;
            Logger.LogDebug("Sending Enter");
            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
        }
    }
}
