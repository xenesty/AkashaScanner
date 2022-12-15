using AkashaScanner.Core;
using AkashaScanner.Ui.StateManagerment;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#if RELEASE
using Microsoft.Extensions.Logging;
#endif

namespace AkashaScanner
{
    public static class AppHost
    {
        public static IHost Create()
        {
            var builder = Host.CreateDefaultBuilder();

#if RELEASE
            builder.ConfigureLogging(logging =>
            {
                logging.AddFile($"Logs/akashascanner-{DateTime.Now:yyyyMMddHHmmss}.txt", LogLevel.Information);
            });
#endif

            builder.ConfigureServices(services =>
            {
                services.AddCoreServices();

                services.AddWindowsFormsBlazorWebView();
                services.AddTransient<Flux>();
#if DEBUG
                services.AddBlazorWebViewDeveloperTools();
#endif
            });

            return builder.Build();
        }
    }
}
