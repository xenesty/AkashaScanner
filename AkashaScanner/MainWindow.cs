using AkashaScanner.Ui;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;

namespace AkashaScanner
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
            AppEvents.OnLoad += () =>
            {
                loading.Hide();
                Cursor = Cursors.Default;
            };
            Cursor = Cursors.WaitCursor;
            var host = AppHost.Create();
            blazorWebView.HostPage = "wwwroot\\index.html";
            blazorWebView.Services = host.Services;
            blazorWebView.RootComponents.Add<App>("#app");
        }
    }
}
