using AkashaScanner.Core.DataCollections;
using AkashaScanner.Core.Navigation.Inventory;
using AkashaScanner.Core.ProcessControl;
using AkashaScanner.Core.ResultHandler;
using AkashaScanner.Core.ScrapPlans;
using AkashaScanner.Core.Screenshot;
using AkashaScanner.Core.Suspender;
using AkashaScanner.Core.TextRecognition;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace AkashaScanner.Core.Artifacts
{
    public class ExperimentalArtifactScrapper : ArtifactScrapper
    {

        private static readonly Dictionary<string, List<ArtifactStatType>> SubStatsTemplateMapping = new()
        {
            ["HP"] = new() { ArtifactStatType.HpPercent, ArtifactStatType.HpFlat },
            ["ATK"] = new() { ArtifactStatType.AtkPercent, ArtifactStatType.AtkFlat },
            ["DEF"] = new() { ArtifactStatType.DefPercent, ArtifactStatType.DefFlat },
            ["Energy Recharge"] = new() { ArtifactStatType.EnergyRecharge },
            ["Elemental Mastery"] = new() { ArtifactStatType.ElementalMastery },
            ["CRIT Rate"] = new() { ArtifactStatType.CritRate },
            ["CRIT DMG"] = new() { ArtifactStatType.CritDmg },
        };

        private const int IsValidSubStatScore = 80;

        private static readonly Dictionary<int, float> MainstatFontSize = new()
        {
            [1600] = 22f,
            [1920] = 25f,
        };
        private static readonly Dictionary<int, float> SubstatFontSize = new()
        {
            [1600] = 24.5f,
            [1920] = 29.5f,
        };

        private readonly TemplateMatchingService TemplateMatching;
        private int SubstatValueWidth;
        private int SubstatMaxLeft;
        private readonly List<Rect> SubstatBounds = new();

        public ExperimentalArtifactScrapper(
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
            ICharacterCollection characterCollection,
            TemplateMatchingService templateMatching)
            : base(logger, suspender, win, control, ocr, navigation, screenshots, resultHandler, scrapPlan, inventoryCollection, characterCollection)
        {
            TemplateMatching = templateMatching;
        }

        protected override void Init()
        {
            base.Init();
            SubstatValueWidth = Win.Scale(160);
            SubstatMaxLeft = Win.Scale(10);
            var substatHeight = SubStatsRect.Height / 4.0;
            for (int i = 0; i < 4; ++i)
            {
                int top = (int)Math.Round(substatHeight * i);
                int bottom = (int)Math.Round(substatHeight * (i + 1));
                SubstatBounds.Add(new Rect(0, top, SubStatsRect.Width, bottom - top));
            }
        }

        protected override void LoadMainStat(Bitmap image, Artifact artifact)
        {
            if (!MainstatFontSize.TryGetValue(Win.WindowWidth, out float fontSize))
            {
                base.LoadMainStat(image, artifact);
                return;
            }
            using var mainstat = image.Clone(MainStatRect, image.PixelFormat);
            using var mat = mainstat.ToMat();
            using var ret = new Mat();
            Cv2.InRange(mat, new Scalar(130, 130, 130), new Scalar(255, 255, 255), ret);
            int maxScore = 0;
            foreach (var (text, stat) in MainStatsMapping)
            {
                var textImg = TemplateMatching.GetTextImg(text, fontSize);
                var score = TemplateMatching.Match(ret, textImg);
                if (score > maxScore)
                {
                    maxScore = score;
                    artifact.MainStat = stat;
                }
            }
        }

        protected override void LoadSubStats(Bitmap image, Artifact artifact)
        {
            if (!SubstatFontSize.TryGetValue(Win.WindowWidth, out float fontSize))
            {
                base.LoadSubStats(image, artifact);
                return;
            }
            var mappings = ArtifactSubstatValues.GetValues(artifact.Rarity);
            if (mappings == null)
            {
                base.LoadSubStats(image, artifact);
                return;
            }
            using var mainstat = image.Clone(SubStatsRect, image.PixelFormat);
            using var mat = mainstat.ToMat();
            using var ret = new Mat();
            Cv2.InRange(mat, new Scalar(0, 0, 0), new Scalar(180, 180, 180), ret);
            List<(int, Artifact.SubStat)> substats = new();
            foreach (var (str, stats) in SubStatsTemplateMapping)
            {
                var textImg = TemplateMatching.GetTextImg(str, fontSize);
                var result = TemplateMatching.GetResult(ret, textImg);
                foreach (var rowRect in SubstatBounds)
                {
                    var resultRect = new Rect(rowRect.X, rowRect.Y, Math.Min(rowRect.Width, result.Width - rowRect.X), Math.Min(rowRect.Height, result.Height - rowRect.Y));
                    using var rowResult = new Mat(result, resultRect);
                    var (score, point) = TemplateMatching.GetPoint(rowResult);
                    if (score < IsValidSubStatScore || point.X > SubstatMaxLeft) continue;

                    Logger.LogDebug("Identify {type} with confidence {score}/100", str, score);
                    var top = Math.Max(resultRect.Top + point.Y - 2, rowRect.Top);
                    var height = Math.Min(textImg.Height + 4, rowRect.Height);
                    var left = point.X + textImg.Width;
                    var width = Math.Min(SubStatsRect.Width - left, SubstatValueWidth);
                    using var rowMat = new Mat(ret, new Rect(left, top, width, height));
                    ArtifactStatType stat = stats[0];
                    if (stats.Count == 2)
                    {
                        var percentScore = TemplateMatching.Match(rowMat, TemplateMatching.GetTextImg("%", fontSize));
                        if (percentScore < IsValidSubStatScore)
                        {
                            stat = stats[1];
                            Logger.LogDebug("Identify {type} as flat with confidence {score}/100", str, 100 - percentScore);
                        }
                        else
                        {
                            Logger.LogDebug("Identify {type} as percent with confidence {score}/100", str, percentScore);
                        }
                    }
                    var (rowScore, value) = MatchSubstat(rowMat, fontSize, mappings[stat], new());
                    if (rowScore >= IsValidSubStatScore)
                    {
                        Logger.LogDebug("Identify {type}, {value} with confidence {score}/100", stat, value, rowScore);
                        substats.Add((rowRect.Top, new() { Type = stat, Value = value }));
                    }
                }
            }
            substats.Sort();
            foreach (var (_, stat) in substats)
            {
                artifact.Substats.Add(stat);
            }
        }

        private (int, decimal) MatchSubstat(
            Mat mat,
            float fontSize,
            List<ArtifactSubstatValues.SubstatValue> entries,
            HashSet<string> matched,
            int minScore = IsValidSubStatScore)
        {
            var rect = Cv2.BoundingRect(mat);
            var maxWidth = (int)Math.Round(rect.Width * 1.1);
            var minWidth = (int)Math.Round(rect.Width * 0.9);
            foreach (var entry in entries)
            {
                if (!matched.Add(entry.Text)) continue;
                var textImg = TemplateMatching.GetTextImg(entry.Text, fontSize);
                var textWidth = textImg.Width;
                if (textWidth > maxWidth || textWidth < minWidth) continue;
                int score = TemplateMatching.Match(mat, textImg);
                if (score > minScore)
                {
                    if (entry.SimilarValues.Count > 0)
                    {
                        var (similarScore, similarValue) = MatchSubstat(mat, fontSize, entry.SimilarValues, matched, score);
                        if (similarScore > score) return (similarScore, similarValue);
                    }
                    return (score, entry.Value);
                }
            }
            return (0, 0);
        }
    }
}
