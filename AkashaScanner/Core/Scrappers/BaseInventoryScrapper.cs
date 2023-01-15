using AkashaScanner.Core.DataCollections;
using AkashaScanner.Core.Navigation.Inventory;
using AkashaScanner.Core.TextRecognition;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace AkashaScanner.Core.Scappers
{
    public abstract class BaseInventoryScrapper<R, E, C> : BaseScrapper<R, C> where E : class, IEntry where C : IBaseScrapConfig
    {
        protected IInventoryNavigation Navigation { get; init; } = default!;
        protected IInventoryCollection<E> InventoryCollection { get; init; } = default!;
        private Rectangle NameRect;
        private Rectangle NameScreenRect;
        private Rectangle ItemInfoRect;

        protected override void Init()
        {
            base.Init();
            Navigation.Init();
            NameScreenRect = Win.GetRectangle(-408, 80, -81, 118);
            ItemInfoRect = Win.GetRectangle(-408, 80, -81, -80);
            NameRect = Win.ScaleRectangle(15, 1, 267, 35);
        }

        protected Bitmap GetNameImg()
        {
            var img = Screenshots.Capture(NameScreenRect);
            return img;
        }

        protected Bitmap GetInfoImg()
        {
            var img = Screenshots.Capture(ItemInfoRect);
            return img;
        }

        protected E? GetItemEntry(ITextRecognitionService ocr, Bitmap image)
        {
            var name = string.Join(' ', ocr.FindLines(image, region: NameRect, inverted: true));
            var item = InventoryCollection.SearchByName(name);
            return item;
        }
    }
}
