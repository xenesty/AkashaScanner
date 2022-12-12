using AkashaScanner.Core;

namespace AkashaScanner.Ui.StateManagerment
{
    public partial class Flux
    {
        private ProcessStatus ProcessStatus;

        [Dispatcher]
        private void OnSetProcessStatus(SetProcessStatusAction action)
        {
            ProcessStatus = action.Status;
            Notify<GetProcessStatus>();
        }

        public class GetProcessStatus : Projection
        {
            public override object? GetValue(Flux flux) => flux.ProcessStatus;
        }
    }
}
