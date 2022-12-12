namespace AkashaScanner.Core.Navigation.Keyboard
{
    public interface IKeyboardService
    {
        void Type(string text);
        void SendEnter();
    }
}
