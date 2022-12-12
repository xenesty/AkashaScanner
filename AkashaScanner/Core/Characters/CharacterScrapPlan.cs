using AkashaScanner.Core.ScrapPlans;

namespace AkashaScanner.Core.Characters
{
    public class CharacterScrapPlan : BaseScrapPlanManager<ICharacterConfig, Character>
    {

        protected override BasicScrapPlan GetScrapPlan(ICharacterConfig config)
        {
            if (config.CharacterMinLevel > 1)
                return new ByLevelScrapPlan() { MinLevel = config.CharacterMinLevel };
            return new BasicScrapPlan();
        }

        protected class ByLevelScrapPlan : BasicScrapPlan
        {
            public int MinLevel { get; init; }

            public override ScrapResult OnReceive(Character item)
            {
                if (item.Level < MinLevel)
                    return ScrapResult.ShouldDrop | ScrapResult.ShouldStop;

                return ScrapResult.None;
            }
        }
    }
}
