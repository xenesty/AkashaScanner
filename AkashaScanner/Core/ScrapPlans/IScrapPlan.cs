namespace AkashaScanner.Core.ScrapPlans
{
    public interface IScrapPlan
    {
        void Activate();
        bool ShouldStopScrapping();
        bool ShouldStopProcessing(int order);
    }
}
