using AkashaScanner.Ui.StateManagerment;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AkashaScanner.Ui
{
    public abstract class RazorComponent : ComponentBase, IDisposable, Flux.ISubject
    {
        [Inject]
        private ILoggerFactory LoggerFactory { get; set; } = default!;

        private bool _disposed;
        public bool Disposed => _disposed;

        protected ILogger Logger = default!;

        [CascadingParameter]
        public Flux Flux { get; set; } = default!;

        public virtual void OnFluxStateChanged(PropertyInfo property, object? value)
        {
            Logger.LogDebug("Changed {prop} {value}", property, value);
            InvokeAsync(StateHasChanged);
        }

        protected void Initialize()
        {
            Logger = LoggerFactory.CreateLogger(GetType());
            var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var proj = prop.GetCustomAttribute<Flux.Projection>();
                proj?.Subscribe(Flux, this, prop);
            }
        }

        protected override Task OnInitializedAsync()
        {
            Initialize();
            return base.OnInitializedAsync();
        }

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Flux.UnsubscribeAll(this);
            }
            GC.SuppressFinalize(this);
        }
    }
}
