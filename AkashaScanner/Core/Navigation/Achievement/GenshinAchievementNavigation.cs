using AkashaScanner.Core.Navigation.Keyboard;
using AkashaScanner.Core.Navigation.Mouse;
using AkashaScanner.Core.Suspender;
using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core.Navigation.Achievement
{
    public class GenshinAchievementNavigation : IAchievementNavigation
    {
        private readonly ILogger Logger;
        private readonly ISuspender Suspender;
        private readonly IMouseService MouseService;
        private readonly IKeyboardService KeyboardService;
        private readonly GameWindow Win;

        private int ClearX;
        private int ClearY;

        public GenshinAchievementNavigation(
            ILogger<GenshinAchievementNavigation> logger,
            ISuspender suspender,
            IMouseService mouseService,
            IKeyboardService keyboardService,
            GameWindow win)
        {
            Logger = logger;
            Suspender = suspender;
            MouseService = mouseService;
            KeyboardService = keyboardService;
            Win = win;
        }

        public void Init()
        {
            ClearX = Win.GetX(300);
            ClearY = Win.GetY(90);
        }

        public void ClearSearch()
        {
            MouseService.MoveCursorTo(ClearX, ClearY);
            Suspender.Sleep(40);
            MouseService.Click();
            Suspender.Sleep(100);
            MouseService.MoveCursorTo(ClearX, ClearY);
            Suspender.Sleep(40);
            MouseService.Click();
            Suspender.Sleep(250);
        }

        public void Search(string text)
        {
            KeyboardService.Type(text);
            Suspender.Sleep(100);
            KeyboardService.SendEnter();
            Suspender.Sleep(300);
        }
    }
}
