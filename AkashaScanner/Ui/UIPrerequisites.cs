using AkashaScanner.Core;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;

namespace AkashaScanner.Ui
{
    public static class UIPrerequisites
    {
        private static readonly string WebView2Installer = Path.Combine(Utils.ExecutableDirectory, "Resources", "Dependencies", "MicrosoftEdgeWebview2Setup.exe");
        private static bool IsWebView2Installed()
        {
            try
            {
                string? availableVersion = CoreWebView2Environment.GetAvailableBrowserVersionString();
                if (availableVersion != null &&
                    CoreWebView2Environment.CompareBrowserVersions(availableVersion, "100.0.0.0") >= 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (WebView2RuntimeNotFoundException)
            {
                return false;
            }
        }

        public static bool Install()
        {
            if (!IsWebView2Installed())
            {
                using var proc = Process.Start(WebView2Installer);
                proc.WaitForExit();
                return IsWebView2Installed();
            }
            return true;
        }
    }
}
