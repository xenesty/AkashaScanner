namespace AkashaScanner.Core.Navigation.Character
{
    public interface ICharacterNavigation
    {
        void Init();

        void GoNext();

        void SelectAttributes();

        void SelectConstellation();

        void SelectTalents();

        void SelectTalent(int index);

        void DeselectTalent();
    }
}
