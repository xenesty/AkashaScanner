using AkashaScanner.Core.Navigation.Mouse;
using AkashaScanner.Core.Suspender;
using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core.Navigation.Inventory
{
    public class GenshinInventoryNavigation : IInventoryNavigation
    {
        private const int Columns = 8;
        private const int RawStartX = 120;
        private const int RawStartY = 131;
        private const int RawDeltaX = 98;
        private const int RawDeltaY = 117;
        private const int RawPaddingVerticalTotal = 160; // Top padding + Bottom padding
        private const int RawPaddingBottomLast = -183;
        private const double ScrollPerRow = 9.734;
        private const int RawTabStartX = 384;
        private const int RawTabDeltaX = 64;
        private const int RawTabY = 38;
        private const int RawSortOrderX = 210;
        private const int RawSortOrderStartY = -40;
        private const int RawSortOrderDeltaY = -33;

        private const int MaxRow = 250;
        private const int AtBottom = -1;
        private const int UnknownLastRow = -1;

        private readonly ILogger Logger;
        private readonly ISuspender Suspender;
        private readonly IMouseService MouseService;
        private readonly GameWindow Win;

        private int StartX;
        private int StartY;
        private int DeltaX;
        private int DeltaY;
        private int RowPerPage;
        private int ItemsPerPage;
        private int EndY;
        private int TabStartX;
        private int TabDeltaX;
        private int TabY;
        private int SortOrderX;
        private int SortOrderStartY;
        private int SortOrderDeltaY;

        private int Scrolled = 0;
        private int Row = 0;
        private int LastRow = UnknownLastRow;

        public int CurrentRow => Row;

        int IInventoryNavigation.RowPerPage => RowPerPage;

        int IInventoryNavigation.Columns => Columns;

        int IInventoryNavigation.ItemsPerPage => ItemsPerPage;

        public GenshinInventoryNavigation(
            ILogger<GenshinInventoryNavigation> logger,
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
            StartX = Win.GetX(RawStartX);
            StartY = Win.GetY(RawStartY);
            DeltaX = Win.Scale(RawDeltaX);
            DeltaY = Win.Scale(RawDeltaY);
            EndY = Win.GetY(RawPaddingBottomLast);
            RowPerPage = (Win.RefHeight - RawPaddingVerticalTotal) / RawDeltaY;
            ItemsPerPage = RowPerPage * Columns;
            TabStartX = Win.GetX(RawTabStartX);
            TabDeltaX = Win.Scale(RawTabDeltaX);
            TabY = Win.GetY(RawTabY);
            SortOrderX = Win.GetX(RawSortOrderX);
            SortOrderStartY = Win.GetY(RawSortOrderStartY);
            SortOrderDeltaY = Win.Scale(RawSortOrderDeltaY);
        }

        public void SelectTab(int tab)
        {
            MouseService.MoveCursorTo(TabStartX + tab * TabDeltaX, TabY);
            Suspender.Sleep(40);
            MouseService.Click();
            Suspender.Sleep(200);
        }

        public void Focus()
        {
            MouseService.MoveCursorTo(StartX, StartY);
            Suspender.Sleep(80);
        }

        public void GoToLast()
        {
            if (Row != AtBottom)
            {
                GoToRow(MaxRow);
                Row = AtBottom;
            }
        }

        public void GoToLast(int numberOfRows)
        {
            LastRow = numberOfRows;
            if (Row != AtBottom)
            {
                GoToRow(numberOfRows);
                Row = AtBottom;
            }
        }

        public void GoToRow(int row)
        {
            if (Row == AtBottom)
            {
                MouseService.ScrollVerticalBy((int)Math.Round((LastRow == UnknownLastRow ? MaxRow : LastRow) * ScrollPerRow));
                Scrolled = 0;
            }
            var scroll = (int)Math.Round(row * ScrollPerRow);
            MouseService.ScrollVerticalBy(Scrolled - scroll);
            Row = row;
            Scrolled = scroll;
            Suspender.Sleep(100);
        }

        public void SelectItem(int rowOnScreen, int col)
        {
            MouseService.MoveCursorTo(StartX + DeltaX * col, StartY + DeltaY * rowOnScreen);
            Suspender.Sleep(40);
            MouseService.Click();
            Suspender.Sleep(100);
        }

        public void SelectItemOnLastPage(int rowFromBelow, int col)
        {
            MouseService.MoveCursorTo(StartX + DeltaX * col, EndY - DeltaY * rowFromBelow);
            Suspender.Sleep(40);
            MouseService.Click();
            Suspender.Sleep(100);
        }

        public void SelectSortOrder(int index)
        {
            MouseService.MoveCursorTo(SortOrderX, SortOrderStartY);
            Suspender.Sleep(40);
            MouseService.Click();
            Suspender.Sleep(400);
            MouseService.MoveCursorTo(SortOrderX, SortOrderStartY + SortOrderDeltaY * (index + 1));
            Suspender.Sleep(40);
            MouseService.Click();
            Suspender.Sleep(400);
        }
    }
}
