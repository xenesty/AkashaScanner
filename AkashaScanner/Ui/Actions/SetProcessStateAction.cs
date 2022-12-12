using AkashaScanner.Core;

namespace AkashaScanner.Ui.StateManagerment
{
    public record SetProcessStatusAction : IAction
    {
        public ProcessStatus Status;
    }
}
