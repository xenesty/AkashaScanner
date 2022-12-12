namespace AkashaScanner.Core.Navigation.Mouse
{
    public interface IMouseService
    {
        void MoveCursorTo(int x, int y);
        void Click();
        void ScrollVerticalBy(int dy);
    }
}
