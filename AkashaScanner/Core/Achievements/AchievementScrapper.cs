using AkashaScanner.Core.DataCollections;
using AkashaScanner.Core.Navigation.Achievement;
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

namespace AkashaScanner.Core.Achievements
{
    public class AchievementScrapper : BaseScrapper<Achievement, IAchievementConfig>
    {
        // BGR: 212, 242, 254
        private static readonly Scalar StarColorLower = new(202, 232, 244);
        private static readonly Scalar StarColorUpper = new(222, 252, 255);

        private readonly IAchievementNavigation Navigation;
        private readonly IAchievementCollection Achievements;

        private Rectangle StarsRect;
        private int StarMinWidth;
        private int StarMaxWidth;
        private int StarMinHeight;
        private int StarMaxHeight;

        public AchievementScrapper(
            ILogger<AchievementScrapper> logger,
            ISuspender suspender,
            GameWindow win,
            IProcessControl control,
            ITextRecognitionService ocr,
            IScreenshotProvider screenshots,
            IResultHandler<Achievement> resultHandler,
            IAchievementNavigation navigation,
            IScrapPlanManager<IAchievementConfig, Achievement> scrapPlan,
            IAchievementCollection achievements)
        {
            Logger = logger;
            Suspender = suspender;
            Win = win;
            Control = control;
            ResultHandler = resultHandler;
            ScrapPlan = scrapPlan;
            Ocr = ocr;
            Screenshots = screenshots;
            Navigation = navigation;
            Achievements = achievements;
        }

        protected override void Init()
        {
            base.Init();
            Navigation.Init();
            StarsRect = Win.GetRectangle(520, 180, 560, 225);
            StarMinWidth = Win.Scale(9);
            StarMaxWidth = Win.Scale(14);
            StarMinHeight = Win.Scale(9);
            StarMaxHeight = Win.Scale(14);
        }

        protected override void Execute(IAchievementConfig config)
        {
            StartMonitoringProcess();
            ScrapPlan.Activate();

            Navigation.ClearSearch();

            var order = 0;
            foreach (var category in Achievements)
            {
                foreach (var entry in category.Achievements)
                {
                    if (ShouldStop()) break;

                    int overrode = 0;
                    foreach (var id in entry.Ids)
                    {
                        if (config.AchievementOverrides.TryGetValue(id, out var active))
                        {
                            ++overrode;
                            if (active)
                            {
                                var achievement = new Achievement() { Id = id, CategoryId = category.Id };
                                var k = ++order;
                                var result = ScrapPlan.Add(achievement, k);
                                if (result.ShouldKeep())
                                    ResultHandler.Add(achievement, k);
                            }
                        }
                    }

                    if (overrode == entry.Ids.Count || Achievements.IsOverlapping(entry)) continue;

                    Navigation.Search(entry.Name);
                    if (ShouldStop()) break;
                    using var screenshot = Screenshots.Capture(StarsRect);
                    var stars = GetStars(screenshot);
                    stars = Math.Min(stars, entry.Ids.Count);
                    Logger.LogInformation("Achievement {name} has {stars} star(s)", entry.Name, stars);
                    Navigation.ClearSearch();
                    for (int i = 0; i < stars; ++i)
                    {
                        var achievement = new Achievement() { Id = entry.Ids[i], CategoryId = category.Id };
                        var k = ++order;
                        var result = ScrapPlan.Add(achievement, k);
                        if (result.ShouldKeep())
                            ResultHandler.Add(achievement, k);
                    }
                }
            }

            StopMonitoringProcess();
            if (!Interrupted)
                ResultHandler.Save();
        }

        private int GetStars(Bitmap image)
        {
            using var src = image.ToMat();
            using var filtered = new Mat();
            Cv2.InRange(src, StarColorLower, StarColorUpper, filtered);
            using var ret = new Mat();
            Cv2.Threshold(filtered, ret, 128, 255, ThresholdTypes.Binary);
            using var labels = new Mat();
            using var stats = new Mat();
            using var centroids = new Mat();
            var nLabels = Cv2.ConnectedComponentsWithStats(ret, labels, stats, centroids);
            int stars = 0;
            for (int i = 0; i < nLabels; ++i)
            {
                var width = stats.Get<int>(i, 2);
                var height = stats.Get<int>(i, 3);
                if (StarMinWidth <= width && width <= StarMaxWidth && StarMinHeight <= height && height <= StarMaxHeight)
                    ++stars;
            }
            return stars;
        }
    }
}
