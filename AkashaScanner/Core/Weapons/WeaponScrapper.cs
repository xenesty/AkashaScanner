using AkashaScanner.Core.DataCollections;
using AkashaScanner.Core.Navigation.Inventory;
using AkashaScanner.Core.ProcessControl;
using AkashaScanner.Core.ResultHandler;
using AkashaScanner.Core.Scappers;
using AkashaScanner.Core.ScrapPlans;
using AkashaScanner.Core.Screenshot;
using AkashaScanner.Core.Suspender;
using AkashaScanner.Core.TextRecognition;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace AkashaScanner.Core.Weapons
{
    public class WeaponScrapper : EquipableScrapper<Weapon, WeaponEntry, IWeaponConfig>
    {
        private static readonly List<string> WeaponUnrelatedItems = new() { "Mystic Enhancement Ore", "Fine Enhancement Ore", "Enhancement Ore" };

        private static readonly Dictionary<string, (int, int)> AllLevels = new();
        private static readonly List<string> AllLevelsText = new();

        static WeaponScrapper()
        {

            var ascensions = new List<int>() { 1, 20, 40, 50, 60, 70, 80, 90 };
            for (int i = 1; i < ascensions.Count; ++i)
            {
                var asc = i - 1;
                var minLevel = ascensions[asc];
                var maxLevel = ascensions[i];
                for (int level = minLevel; level <= maxLevel; ++level)
                {
                    var text = $"Lv. {level}/{maxLevel}";
                    AllLevels.Add(text, (level, asc));
                    AllLevelsText.Add(text);
                }
            }
        }

        protected override List<string> UnrelatedItems => WeaponUnrelatedItems;

        private Rectangle RefinementRect;
        private Rectangle LevelRect;

        public WeaponScrapper(
            ILogger<WeaponScrapper> logger,
            ISuspender suspender,
            GameWindow win,
            IProcessControl control,
            ITextRecognitionService ocr,
            IInventoryNavigation navigation,
            IScreenshotProvider screenshots,
            IResultHandler<Weapon> resultHandler,
            IScrapPlanManager<IWeaponConfig, Weapon> scrapPlan,
            IWeaponCollection inventoryCollection,
            ICharacterCollection characterCollection)
        {
            Logger = logger;
            Suspender = suspender;
            Win = win;
            Control = control;
            ResultHandler = resultHandler;
            ScrapPlan = scrapPlan;
            Ocr = ocr;
            Navigation = navigation;
            Screenshots = screenshots;
            InventoryCollection = inventoryCollection;
            CharacterCollection = characterCollection;
        }

        protected override void Init()
        {
            base.Init();
            RefinementRect = Win.ScaleRectangle(20, 235, 20, 19); // The Refinement number box
            //RefinementRect = Win.ScaleRectangle(45, 235, 166, 19); // The `Refinement Rank X` text
            LevelRect = Win.ScaleRectangle(20, 206, 84, 18);
        }

        protected override Weapon ProcessImage(Bitmap image, IWeaponConfig config)
        {
            using var ocr = Ocr.GetInstance();
            Weapon weapon = new();
            LoadEntry(ocr, image, weapon);
            LoadLevel(ocr, image, weapon);
            LoadEquipped(ocr, image, weapon, config.CharacterNameOverrides);
            Logger.LogInformation("Weapon: {weapon}", weapon);
            return weapon;
        }

        private void LoadEntry(ITextRecognitionService ocr, Bitmap image, Weapon weapon)
        {
            var entry = GetItemEntry(ocr, image);
            if (entry != null)
            {
                weapon.Name = entry.Name;
                weapon.Rarity = entry.Rarity;
                weapon.Type = entry.Type;
                if (entry.Rarity >= 3)
                    LoadRefinement(ocr, image, weapon);
            }
        }

        private void LoadLevel(ITextRecognitionService ocr, Bitmap image, Weapon weapon)
        {
            var text = ocr.FindText(image, region: LevelRect, inverted: true);
            (var score, var output) = text.FuzzySearch(AllLevelsText);
            if (output != null)
            {
                (var level, var ascension) = AllLevels[output];
                Logger.LogDebug("Identify {text} as (Level: {level}, Ascension: {ascension}) with confidence {score}/100", text, level, ascension, score);
                weapon.Level = level;
                weapon.Ascension = ascension;
            }
            else
            {
                Logger.LogWarning("Fail to identify the level of weapon: {text}", text);
            }
        }

        private void LoadRefinement(ITextRecognitionService ocr, Bitmap image, Weapon weapon)
        {
            var text = ocr.FindChar(image, region: RefinementRect, inverted: true);
            text = Regex.Replace(text, @"[^\d]", string.Empty);
            if (int.TryParse(text, out int refinement))
            {
                weapon.Refinement = refinement;
            }
            else
            {
                Logger.LogWarning("Fail to identify the refinement of weapon: {text}", text);
            }
        }

        private void LoadEquipped(ITextRecognitionService ocr, Bitmap image, Weapon weapon, Dictionary<string, string> CharacterNameOverrides)
        {
            var character = GetEquipped(ocr, image, CharacterNameOverrides);
            if (character != null)
            {
                weapon.EquippedCharacter = character.Name;
            }
        }
    }
}
