namespace AkashaScanner.Core.Navigation.Inventory
{
    public interface IInventoryNavigation
    {
        int CurrentRow { get; }

        int RowPerPage { get; }

        int ItemsPerPage { get; }

        int Columns { get; }

        void Init();

        void SelectTab(int tab);

        void Focus();

        void GoToLast();
        void GoToLast(int numberOfRows);

        void GoToRow(int row);

        void SelectItem(int rowOnScreen, int col);

        void SelectItemOnLastPage(int rowFromBelow, int col);

        void SelectSortOrder(int index);
    }
}
