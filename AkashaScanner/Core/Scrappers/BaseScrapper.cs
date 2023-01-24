using AkashaScanner.Core.ProcessControl;
using AkashaScanner.Core.ResultHandler;
using AkashaScanner.Core.ScrapPlans;
using AkashaScanner.Core.Screenshot;
using AkashaScanner.Core.Suspender;
using AkashaScanner.Core.TextRecognition;
using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core.Scappers
{
    public abstract class BaseScrapper<R, C> : IScrapper<C> where C : IBaseScrapConfig
    {
        protected ILogger Logger { get; init; } = default!;
        protected ISuspender Suspender { get; init; } = default!;
        protected IProcessControl Control { get; init; } = default!;
        protected GameWindow Win { get; init; } = default!;
        protected IResultHandler<R> ResultHandler { get; init; } = default!;
        protected ITextRecognitionService Ocr { get; init; } = default!;
        protected IScreenshotProvider Screenshots { get; init; } = default!;
        protected IScrapPlanManager<C, R> ScrapPlan { get; init; } = default!;

        private readonly object Mutex = new();
        protected volatile bool Interrupted = false;

        protected virtual void Init()
        {
            ResultHandler.Init();
        }

        public virtual void Dispose()
        {
            Ocr.Dispose();
            GC.SuppressFinalize(this);
        }

        protected abstract void Execute(C config);

        protected virtual bool ShouldStop()
        {
            return Interrupted || ScrapPlan.ShouldStopScrapping();
        }

        private void WaitProcessActive(ProcessStatus status)
        {
            lock (Mutex)
            {
                Monitor.PulseAll(Mutex);
            }
        }

        private void InterruptOnInactive(ProcessStatus status)
        {
            if (status != ProcessStatus.Active)
                Interrupted = true;
        }

        protected void StartMonitoringProcess()
        {
            if (Control.Status == ProcessStatus.Active)
            {
                Control.OnStatusChange += InterruptOnInactive;
            }
            else
            {
                Interrupted = true;
            }
        }

        protected void StopMonitoringProcess()
        {
            Control.OnStatusChange -= InterruptOnInactive;
        }

        protected bool SetProcessActive()
        {
            Control.OnStatusChange += WaitProcessActive;
            Control.SetActive();
            lock (Mutex)
            {
                while (Control.Status != ProcessStatus.Active)
                {
                    Monitor.Wait(Mutex);
                }
            }
            Control.OnStatusChange -= WaitProcessActive;

            var rect = Control.GetWindowDimension();
            return Win.SetRect(rect);
        }

        public bool Start(C config)
        {
            ScrapPlan.Configure(config);
            if (!SetProcessActive())
            {
                return false;
            }
            Interrupted = false;
            Init();
            Execute(config);
            return !Interrupted;
        }
    }
}
