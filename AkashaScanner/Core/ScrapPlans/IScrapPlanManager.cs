namespace AkashaScanner.Core.ScrapPlans
{
    public interface IScrapPlanManager<C> : IScrapPlan where C : IBaseScrapConfig
    {
        void Configure(C config);
    }

    public interface IScrapPlanManager<C, R> : IScrapPlanManager<C> where C : IBaseScrapConfig
    {
        ScrapResult Add(R item, int order);
    }
}
