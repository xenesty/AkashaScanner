using AkashaScanner.Core.Navigation.Inventory;
using AkashaScanner.Core.ScrapPlans;

namespace AkashaScanner.Core.Artifacts
{
    public class ArtifactScrapPlan : BaseScrapPlanManager<IArtifactConfig, Artifact>
    {
        private readonly IInventoryNavigation Navigation;
        public ArtifactScrapPlan(IInventoryNavigation navigation)
        {
            Navigation = navigation;
        }

        protected override BasicScrapPlan GetScrapPlan(IArtifactConfig config)
        {
            if (config.ArtifactMinLevel > 0)
                return new ByLevelScrapPlan(Navigation) { MinRarity = config.ArtifactMinRarity, MinLevel = config.ArtifactMinLevel };
            else if (config.ArtifactMinRarity > 1)
                return new ByRarityScrapPlan(Navigation) { MinRarity = config.ArtifactMinRarity };
            else
                return new BasicScrapPlan();
        }

        protected class ByRarityScrapPlan : BasicScrapPlan
        {
            public int MinRarity { get; init; }

            private readonly IInventoryNavigation Navigation;
            public ByRarityScrapPlan(IInventoryNavigation navigation)
            {
                Navigation = navigation;
            }

            public override void Activate()
            {
                Navigation.SelectSortOrder(0);
            }

            public override ScrapResult OnReceive(Artifact item, int _)
            {
                if (item.Rarity < MinRarity)
                    return ScrapResult.ShouldDrop | ScrapResult.ShouldStop;

                return ScrapResult.None;
            }
        }

        protected class ByLevelScrapPlan : BasicScrapPlan
        {
            public int MinRarity { get; init; }
            public int MinLevel { get; init; }

            private readonly IInventoryNavigation Navigation;
            public ByLevelScrapPlan(IInventoryNavigation navigation)
            {
                Navigation = navigation;
            }

            public override void Activate()
            {
                Navigation.SelectSortOrder(1);
            }

            public override ScrapResult OnReceive(Artifact item, int _)
            {
                if (item.Level < MinLevel || (item.Level == MinLevel && item.Rarity < MinRarity))
                    return ScrapResult.ShouldDrop | ScrapResult.ShouldStop;

                if (item.Rarity < MinRarity)
                    return ScrapResult.ShouldDrop;

                return ScrapResult.None;
            }
        }

    }
}
