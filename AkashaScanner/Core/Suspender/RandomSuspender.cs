using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core.Suspender
{
    public class RandomSuspender : ISuspender
    {
        private readonly ILogger Logger;
        private readonly IGlobalConfig Config;
        private readonly Random rand = new();

        public RandomSuspender(ILogger<RandomSuspender> logger, IGlobalConfig config)
        {
            Logger = logger;
            Config = config;
        }

        public void Sleep(int ms)
        {
            if (ms > 0)
            {
                var time = GetTime(ms);
                Logger.LogTrace("Sleep {time}", time);
                Thread.Sleep(time);
            }
        }

        private int GetTime(int ms)
        {
            int min = (int)Math.Round(Config.SuspendMultiplierMin * ms);
            int max = (int)Math.Round(Config.SuspendMultiplierMax * ms);
            if (min >= max) return min;
            return rand.Next(min, max);
        }
    }
}
