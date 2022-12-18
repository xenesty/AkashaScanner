namespace AkashaScanner.Core.ScrapPlans
{
    public abstract class BaseScrapPlanManager<C, R> : IScrapPlanManager<C, R> where C : IBaseScrapConfig
    {
        private BasicScrapPlan ScrapPlan = new();

        protected abstract BasicScrapPlan GetScrapPlan(C config);

        public void Configure(C config)
        {
            ScrapPlan = GetScrapPlan(config);
        }

        public void Activate() => ScrapPlan.Activate();
        public bool ShouldStopScrapping() => ScrapPlan.ShouldStopScrapping();
        public bool ShouldStopProcessing(int order) => ScrapPlan.ShouldStopProcessing(order);

        public ScrapResult Add(R item, int order) => ScrapPlan.Add(item, order);

        protected class BasicScrapPlan : IScrapPlan
        {
            private const int Ongoing = -1;

            private volatile int StoppedAt = Ongoing;
            public virtual void Activate() { }

            public bool ShouldStopScrapping() => StoppedAt != Ongoing;
            public bool ShouldStopProcessing(int order) => StoppedAt != Ongoing && StoppedAt < order;

            public virtual ScrapResult OnReceive(R item, int order) => ScrapResult.None;

            public ScrapResult Add(R item, int order)
            {
                var result = OnReceive(item, order);

                if (StoppedAt == Ongoing && result.ShouldStop())
                    StoppedAt = order;

                return result;
            }
        }
    }
}
