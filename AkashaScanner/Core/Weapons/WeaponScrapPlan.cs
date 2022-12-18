using AkashaScanner.Core.Navigation.Inventory;
using AkashaScanner.Core.ScrapPlans;

namespace AkashaScanner.Core.Weapons
{
    public class WeaponScrapPlan : BaseScrapPlanManager<IWeaponConfig, Weapon>
    {
        private readonly IInventoryNavigation Navigation;
        public WeaponScrapPlan(IInventoryNavigation navigation)
        {
            Navigation = navigation;
        }

        protected override BasicScrapPlan GetScrapPlan(IWeaponConfig config)
        {
            if (config.WeaponMinLevel > 1)
                return new ByLevelScrapPlan(Navigation) { MinRarity = config.WeaponMinRarity, MinLevel = config.WeaponMinLevel };
            else if (config.WeaponMinRarity > 1)
                return new ByRarityScrapPlan(Navigation) { MinRarity = config.WeaponMinRarity };
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
                Navigation.SelectSortOrder(1);
            }

            public override ScrapResult OnReceive(Weapon item, int _)
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
                Navigation.SelectSortOrder(2);
            }

            public override ScrapResult OnReceive(Weapon item, int _)
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
