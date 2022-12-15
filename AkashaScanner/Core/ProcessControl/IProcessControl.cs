namespace AkashaScanner.Core.ProcessControl
{
    public interface IProcessControl
    {
        ProcessStatus Status { get; }
        Rectangle GetWindowDimension();
        bool IsActive();
        void SetActive();

        public delegate void OnStatusChangeEvent(ProcessStatus status);
        public event OnStatusChangeEvent? OnStatusChange;
    }
}
