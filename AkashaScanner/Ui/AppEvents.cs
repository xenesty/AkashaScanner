namespace AkashaScanner.Ui
{
    public static class AppEvents
    {
        public delegate void OnLoadEvent();
        public static event OnLoadEvent? OnLoad;

        public delegate void OnCloseEvent();
        public static event OnCloseEvent? OnClose;

        public static void Load()
        {
            OnLoad?.Invoke();
        }

        public static void Close()
        {
            OnClose?.Invoke();
        }
    }
}
