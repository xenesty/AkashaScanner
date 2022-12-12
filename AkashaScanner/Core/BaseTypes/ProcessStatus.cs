namespace AkashaScanner.Core
{
    public enum ProcessStatus
    {
        Unknown,
        Stopped,
        Inactive,
        Active,
    }
    public static class ProcessStatusExtension
    {
        public static bool IsRunning(this ProcessStatus status)
        {
            return status == ProcessStatus.Active || status == ProcessStatus.Inactive;
        }
    }
}
