namespace AkashaScanner.Core.Navigation.Achievement
{
    public interface IAchievementNavigation
    {
        void Init();

        void Search(string text);

        void ClearSearch();
    }
}
