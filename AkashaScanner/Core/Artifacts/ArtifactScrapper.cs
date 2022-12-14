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
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Globalization;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace AkashaScanner.Core.Artifacts
{
    public class ArtifactScrapper : EquipableScrapper<Artifact, ArtifactEntry, IArtifactConfig>
    {
        private static readonly Dictionary<string, ArtifactStatType> MainStatsMapping = new()
        {
            ["HP"] = ArtifactStatType.HpPercent,
            ["ATK"] = ArtifactStatType.AtkPercent,
            ["DEF"] = ArtifactStatType.DefPercent,
            ["Energy Recharge"] = ArtifactStatType.EnergyRecharge,
            ["Elemental Mastery"] = ArtifactStatType.ElementalMastery,
            ["Healing Bonus"] = ArtifactStatType.HealingBonus,
            ["CRIT Rate"] = ArtifactStatType.CritRate,
            ["CRIT DMG"] = ArtifactStatType.CritDmg,
            ["Physical DMG Bonus"] = ArtifactStatType.PhysicalDmg,
            ["Pyro DMG Bonus"] = ArtifactStatType.PyroDmg,
            ["Cryo DMG Bonus"] = ArtifactStatType.CryoDmg,
            ["Electro DMG Bonus"] = ArtifactStatType.ElectroDmg,
            ["Hydro DMG Bonus"] = ArtifactStatType.HydroDmg,
            ["Anemo DMG Bonus"] = ArtifactStatType.AnemoDmg,
            ["Geo DMG Bonus"] = ArtifactStatType.GeoDmg,
            ["Dendro DMG Bonus"] = ArtifactStatType.DendroDmg,
        };

        private static readonly Dictionary<string, ArtifactStatType> SubStatsMapping = new()
        {
            ["HP+.%"] = ArtifactStatType.HpPercent,
            ["HP+"] = ArtifactStatType.HpFlat,
            ["ATK+.%"] = ArtifactStatType.AtkPercent,
            ["ATK+"] = ArtifactStatType.AtkFlat,
            ["DEF+.%"] = ArtifactStatType.DefPercent,
            ["DEF+"] = ArtifactStatType.DefFlat,
            ["Energy Recharge+.%"] = ArtifactStatType.EnergyRecharge,
            ["Elemental Mastery+"] = ArtifactStatType.ElementalMastery,
            ["CRIT Rate+.%"] = ArtifactStatType.CritRate,
            ["CRIT DMG+.%"] = ArtifactStatType.CritDmg,
        };

        private static readonly List<string> MainStatsList = MainStatsMapping.Keys.ToList();
        private static readonly List<string> SubStatsList = SubStatsMapping.Keys.ToList();
        private static readonly CultureInfo cultureInfo = new("en-US");

        // BGR: 50, 204, 255
        private static readonly Scalar StarColorLower = new(40, 194, 245);
        private static readonly Scalar StarColorUpper = new(60, 214, 255);

        private const int IsValidMainStatScore = 80;
        private const int IsValidSubStatScore = 80;

        private static readonly List<string> ArtifactUnrelatedItems = new() { "Sanctifying Essence", "Sanctifying Unction" };
        protected override List<string> UnrelatedItems => ArtifactUnrelatedItems;

        private Rectangle MainStatRect;
        private Rectangle RarityRect;
        private Rectangle LevelRect;
        private Rectangle SubStatsRect;
        private int RarityStarAreaMin;
        private int RarityStarAreaMax;

        public ArtifactScrapper(
            ILogger<ArtifactScrapper> logger,
            ISuspender suspender,
            GameWindow win,
            IProcessControl control,
            ITextRecognitionService ocr,
            IInventoryNavigation navigation,
            IScreenshotProvider screenshots,
            IResultHandler<Artifact> resultHandler,
            IScrapPlanManager<IArtifactConfig, Artifact> scrapPlan,
            IArtifactCollection inventoryCollection,
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
            MainStatRect = Win.ScaleRectangle(17, 100, 170, 20);
            RarityRect = Win.ScaleRectangle(16, 156, 114, 24);
            LevelRect = Win.ScaleRectangle(21, 207, 34, 17);
            SubStatsRect = Win.ScaleRectangle(30, 235, 296, 105);
            RarityStarAreaMin = Win.Scale(Win.Scale(100));
            RarityStarAreaMax = Win.Scale(Win.Scale(155));
        }

        protected override Artifact ProcessImage(Bitmap image, IArtifactConfig config)
        {
            using var ocr = Ocr.GetInstance();
            Artifact artifact = new();
            LoadEntry(ocr, image, artifact);
            LoadRarity(image, artifact);
            LoadLevel(ocr, image, artifact);
            LoadSubStats(ocr, image, artifact);
            LoadEquipped(ocr, image, artifact, config.CharacterNameOverrides);
            Logger.LogInformation("Artifact: {artifact}", artifact);
            return artifact;
        }

        private void LoadEntry(ITextRecognitionService ocr, Bitmap image, Artifact artifact)
        {
            var entry = GetItemEntry(ocr, image);
            if (entry != null)
            {
                var slot = entry.Slot;
                artifact.Slot = slot;
                artifact.SetName = entry.SetName;
                if (slot == ArtifactSlot.Flower)
                    artifact.MainStat = ArtifactStatType.HpFlat;
                else if (slot == ArtifactSlot.Plume)
                    artifact.MainStat = ArtifactStatType.AtkFlat;
                else if (slot != ArtifactSlot.Invalid)
                    LoadMainStat(ocr, image, artifact);
            }
        }

        private void LoadMainStat(ITextRecognitionService ocr, Bitmap image, Artifact artifact)
        {
            var text = ocr.FindText(image, MainStatRect, true);
            (int score, string? value) = text.FuzzySearch(MainStatsList);
            if (score >= IsValidMainStatScore)
            {
                artifact.MainStat = MainStatsMapping[value!];
            }
            else
            {
                Logger.LogWarning("Fail to identify the main stat of artifact: {text}", text);
            }
        }

        private void LoadRarity(Bitmap image, Artifact artifact)
        {
            using var img = image.Clone(RarityRect, image.PixelFormat);
            using var mat = img.ToMat();
            using var filtered = new Mat();
            Cv2.InRange(mat, StarColorLower, StarColorUpper, filtered);
            using var ret = new Mat();
            Cv2.Threshold(filtered, ret, 128, 255, ThresholdTypes.Binary);
            using var labels = new Mat();
            using var stats = new Mat();
            using var centroids = new Mat();
            var nLabels = Cv2.ConnectedComponentsWithStats(ret, labels, stats, centroids);
            int rarity = 0;
            for (int i = 0; i < nLabels; ++i)
            {
                var area = stats.Get<int>(i, 4);
                if (RarityStarAreaMin <= area && area <= RarityStarAreaMax)
                    ++rarity;
            }
            artifact.Rarity = rarity;
            if (rarity == 0)
            {
                Logger.LogWarning("Fail to identify the rarity of artifact");
            }
        }

        private void LoadLevel(ITextRecognitionService ocr, Bitmap image, Artifact artifact)
        {
            var text = ocr.FindText(image, LevelRect, true);
            text = Regex.Replace(text, @"[\D]", string.Empty);
            if (int.TryParse(text, out int level))
            {
                artifact.Level = level;
            }
            else
            {
                Logger.LogWarning("Fail to identify the level of artifact: {text}", text);
            }
        }

        private void LoadSubStats(ITextRecognitionService ocr, Bitmap image, Artifact artifact)
        {
            var lines = ocr.FindLines(image, SubStatsRect);
            foreach (var line in lines)
            {
                if (line.Length < 5) continue;
                var noDigitText = Regex.Replace(line, @"\d", string.Empty).Trim()!;

                (int score, string? statName) = noDigitText.FuzzySearch(SubStatsList)!;
                var type = SubStatsMapping[statName!];
                Logger.LogDebug("Identify {line} as {type} with confidence {score}/100", line, type, score);

                if (score >= IsValidSubStatScore)
                {
                    if (type.IsFlat())
                    {
                        var text = Regex.Replace(line, @"\D", string.Empty);
                        if (int.TryParse(text, out int value))
                        {
                            artifact.Substats.Add(new Artifact.SubStat() { Type = type, Value = value });
                        }
                    }
                    else
                    {
                        var text = Regex.Replace(line, @"[^\d.]", string.Empty);
                        if (decimal.TryParse(text, NumberStyles.Number, cultureInfo, out decimal value))
                        {
                            artifact.Substats.Add(new Artifact.SubStat() { Type = type, Value = value });
                        }
                    }
                }
            }
        }

        private void LoadEquipped(ITextRecognitionService ocr, Bitmap image, Artifact artifact, Dictionary<string, string> CharacterNameOverrides)
        {
            var character = GetEquipped(ocr, image, CharacterNameOverrides);
            if (character != null)
            {
                artifact.EquippedCharacter = character.Name;
            }
        }
    }
}
