using Microsoft.Extensions.Logging;

namespace AkashaScanner.Ui.StateManagerment
{
    public partial class Flux
    {
        public void Dispatch(IAction action)
        {
            Logger.LogDebug("Dispatch {action}", action);
            var method = Methods[action.GetType()];
            if (method == null)
            {
                Logger.LogWarning("Action not handled: {action}", action);
            }
            else
            {
                method.Invoke(this, new object[] { action });
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        private class Dispatcher : Attribute
        {
        }
    }
}
