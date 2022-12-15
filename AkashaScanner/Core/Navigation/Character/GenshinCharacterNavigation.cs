using AkashaScanner.Core.Navigation.Mouse;
using AkashaScanner.Core.Suspender;
using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core.Navigation.Character
{
    public class GenshinCharacterNavigation : ICharacterNavigation
    {
        private readonly ILogger Logger;
        private readonly ISuspender Suspender;
        private readonly IMouseService MouseService;
        private readonly GameWindow Win;

        private int NextX;
        private int NextY;
        private int MenuX;
        private int AttributesY;
        private int ConstellationY;
        private int TalentsY;
        private int TalentX;
        private int TalentStartY;
        private int TalentDeltaY;
        private int MiddleX;
        private int MiddleY;

        public GenshinCharacterNavigation(
            ILogger<GenshinCharacterNavigation> logger,
            ISuspender suspender,
            IMouseService mouseService,
            GameWindow win)
        {
            Logger = logger;
            Suspender = suspender;
            MouseService = mouseService;
            Win = win;
        }

        public void Init()
        {
            NextX = Win.GetX(-50);
            NextY = Win.GetY(Win.RefHeight / 2);
            MenuX = Win.GetX(130);
            AttributesY = Win.GetY(104);
            ConstellationY = Win.GetY(242);
            TalentsY = Win.GetY(288);
            TalentX = Win.GetX(-220);
            TalentStartY = Win.GetY(100);
            TalentDeltaY = Win.Scale(60);
            MiddleX = Win.WindowX + Win.WindowWidth / 2;
            MiddleY = Win.WindowY + Win.WindowHeight / 2;
        }

        public void GoNext()
        {
            MouseService.MoveCursorTo(NextX, NextY);
            Suspender.Sleep(40);
            MouseService.Click();
            Suspender.Sleep(400);
        }

        public void SelectAttributes()
        {
            MouseService.MoveCursorTo(MenuX, AttributesY);
            Suspender.Sleep(40);
            MouseService.Click();
        }

        public void SelectConstellation()
        {
            MouseService.MoveCursorTo(MenuX, ConstellationY);
            Suspender.Sleep(40);
            MouseService.Click();
        }

        public void SelectTalents()
        {
            MouseService.MoveCursorTo(MenuX, TalentsY);
            Suspender.Sleep(40);
            MouseService.Click();
        }

        public void SelectTalent(int index)
        {
            MouseService.MoveCursorTo(TalentX, TalentStartY + TalentDeltaY * index);
            Suspender.Sleep(40);
            MouseService.Click();
        }

        public void DeselectTalent()
        {
            MouseService.MoveCursorTo(MiddleX, MiddleY);
            Suspender.Sleep(40);
            MouseService.Click();
        }
    }
}
