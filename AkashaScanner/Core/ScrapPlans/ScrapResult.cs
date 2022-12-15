namespace AkashaScanner.Core.ScrapPlans
{
    [Flags]
    public enum ScrapResult
    {
        None = 0,       // Indicates the scanning should continue.
        ShouldStop = 1, // Indicates the scanning should stop.
        IsInvalid = 2,  // Indicates the data is invalid.
        ShouldDrop = 4, // Indicates the data should not be stored.
    }

    public static class ScrapResultExtension
    {
        public static bool ShouldStop(this ScrapResult result)
        {
            return result.HasFlag(ScrapResult.ShouldStop);
        }
        public static bool ShouldContinue(this ScrapResult result)
        {
            return !result.HasFlag(ScrapResult.ShouldStop);
        }
        public static bool IsInvalid(this ScrapResult result)
        {
            return result.HasFlag(ScrapResult.IsInvalid);
        }
        public static bool IsValid(this ScrapResult result)
        {
            return !result.HasFlag(ScrapResult.IsInvalid);
        }
        public static bool ShouldDrop(this ScrapResult result)
        {
            return result.HasFlag(ScrapResult.ShouldDrop);
        }
        public static bool ShouldKeep(this ScrapResult result)
        {
            return !result.HasFlag(ScrapResult.ShouldDrop);
        }
    }
}
