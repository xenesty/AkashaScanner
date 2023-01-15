using AkashaScanner.Core.DataCollections;
using AkashaScanner.Core.ScrapPlans;
using AkashaScanner.Core.TextRecognition;
using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core.Scappers
{
    public abstract class EquipableScrapper<R, E, C> : BaseInventoryScrapper<R, E, C> where E : class, IEntry where C : IBaseScrapConfig, ITravelerNameConfig, ICharacterNamesConfig
    {
        protected ICharacterCollection CharacterCollection { get; init; } = default!;

        private const int IsUnrelatedMinScore = 90;
        private const int CharacterNameOverridesScore = 70;
        protected abstract List<string> UnrelatedItems { get; }

        private Rectangle ItemCountRect;
        private Rectangle EquippedRect;
        private string TravelerName = string.Empty;

        protected override void Init()
        {
            base.Init();
            ItemCountRect = Win.GetRectangle(-250, 20, -75, 45);
            EquippedRect = Win.ScaleRectangle(136, Win.RefHeight - 190, 190, 25);
        }

        private void Run(TaskQueue tasks, C config)
        {
            int order = 0;
            StartMonitoringProcess();
            ScrapPlan.Activate();
            var itemCount = GetItemCount();
            Logger.LogInformation("Number of items: {itemCount}", itemCount);

            var columns = Navigation.Columns;
            var rowPerPage = Navigation.RowPerPage;
            var itemPerPage = columns * rowPerPage;
            var numberOfRows = (itemCount - 1) / columns + 1;
            var minNumberOfPages = Math.Max(1, (numberOfRows - 1) / rowPerPage);

            var counter = 0;
            Navigation.Focus();

            for (var p = 0; p < minNumberOfPages; ++p)
            {
                Navigation.GoToRow(p * rowPerPage);

                tasks.WaitAll();

                for (var i = 0; i < itemPerPage; ++i)
                {
                    if (ShouldStop()) return;
                    var row = i / columns;
                    var col = i % columns;
                    Navigation.SelectItem(row, col);
                    var img = GetInfoImg();
                    tasks.Add((k) => CreateTask(img, k, config), ++order);
                    if (++counter == itemCount) return;
                }
            }

            var currentRow = (minNumberOfPages - 1) * rowPerPage;
            var targetRow = numberOfRows - rowPerPage - 1;
            if (targetRow > currentRow)
            {
                Navigation.GoToRow(targetRow);

                tasks.WaitAll();

                for (var row = currentRow + rowPerPage - targetRow; row < rowPerPage; ++row)
                {
                    for (var col = 0; col < columns; ++col)
                    {
                        if (ShouldStop()) return;
                        Navigation.SelectItem(row, col);
                        var img = GetInfoImg();
                        tasks.Add((k) => CreateTask(img, k, config), ++order);
                        if (++counter == itemCount) return;
                    }
                }
            }

            Navigation.GoToLast(itemCount / columns + 2);
            Suspender.Sleep(200);
            var unrelatedRows = GetUnrelatedRows();
            var remaining = itemCount - counter;

            for (var col = 0; col < remaining; ++col)
            {
                if (ShouldStop()) return;
                Navigation.SelectItemOnLastPage(unrelatedRows, col);
                var img = GetInfoImg();
                tasks.Add((k) => CreateTask(img, k, config), ++order);
            }
        }

        protected override void Execute(C config)
        {
            TaskQueue tasks = new();
            TravelerName = config.TravelerName;
            Run(tasks, config);
            StopMonitoringProcess();
            tasks.WaitAll();
            if (!Interrupted)
            {
                ResultHandler.Save();
            }
        }

        protected abstract R ProcessImage(Bitmap image, C config);

        private void CreateTask(Bitmap image, int order, C config)
        {
            if (ScrapPlan.ShouldStopProcessing(order)) return;
            var data = ProcessImage(image, config);
            var result = ScrapPlan.Add(data, order);
            if (result.ShouldKeep())
                ResultHandler.Add(data, order);
        }

        private int GetItemCount()
        {
            using var img = Screenshots.Capture(ItemCountRect);
            var text = Ocr.FindText(img, inverted: true);
            if (!text.TryParseFraction(out (int, int) output))
                throw new IOException("Cannot get item count");
            return output.Item1;
        }

        private int GetUnrelatedRows()
        {
            var i = 0;
            while (true)
            {
                Navigation.SelectItemOnLastPage(i, 0);
                using var img = GetNameImg();
                var isDummy = IsItemUnrelated(img);
                if (!isDummy)
                    return i;
                ++i;
            }
        }

        protected CharacterEntry? GetEquipped(ITextRecognitionService ocr, Bitmap image, Dictionary<string, string> CharacterNameOverrides)
        {
            var text = ocr.FindText(image, region: EquippedRect).Trim();
            foreach (var (actualName, givenName) in CharacterNameOverrides)
            {
                var score = givenName.FuzzySearch(text);
                if (score > CharacterNameOverridesScore)
                {
                    text = actualName;
                    break;
                }
            }
            var entry = CharacterCollection.SearchByName(text, TravelerName);
            return entry;
        }

        private bool IsItemUnrelated(Bitmap img)
        {
            if (UnrelatedItems.Count == 0) return false;
            var text = Ocr.FindText(img, inverted: true);
            (int score, string _) = text.FuzzySearch(UnrelatedItems);
            return score >= IsUnrelatedMinScore;
        }
    }
}
