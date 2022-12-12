using AkashaScanner.Core.ScrapPlans;

namespace AkashaScanner.Core.Achievements
{
    public class AchievementScrapPlan : BaseScrapPlanManager<IAchievementConfig, Achievement>
    {
        protected override BasicScrapPlan GetScrapPlan(IAchievementConfig config)
        {
            return new BasicScrapPlan();
        }
    }
}
