using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AkashaScanner.Ui.StateManagerment
{
    public sealed partial class Flux
    {
        private readonly ILogger Logger;
        private readonly IDictionary<Type, MethodInfo> Methods;

        public Flux(ILogger<Flux> logger)
        {
            Logger = logger;
            Methods = GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<Dispatcher>() != null)
                .ToDictionary(m => m.GetParameters()[0].ParameterType);
        }
    }
}
