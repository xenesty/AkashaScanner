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

            public override ScrapResult OnReceive(Character item, int order)
            {
                if (item.Level < MinLevel)
                {
                    if (order > 4)
                        return ScrapResult.ShouldDrop | ScrapResult.ShouldStop;
                    else
                        return ScrapResult.ShouldDrop;
                }

                return ScrapResult.None;
            }
        }
    }
}
