#if HEADLESS
using AkashaScanner.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AkashaScanner.Core.Scappers;
using AkashaScanner.Core.DataCollections;
#else
using AkashaScanner.Ui;
#endif

namespace AkashaScanner
{
    internal static class Program
    {

        [STAThread]
        static void Main()
        {
#if HEADLESS
            var host = Host.CreateDefaultBuilder().ConfigureServices(services => services.AddCoreServices()).Build();
            var Weapons = host.Services.GetService<IWeaponCollection>()!;
            var Artifacts = host.Services.GetService<IArtifactCollection>()!;
            var Characters = host.Services.GetService<ICharacterCollection>()!;
            var Achievements = host.Services.GetService<IAchievementCollection>()!;

            var Config = host.Services.GetService<IConfig>()!;

            var WeaponScrapper = host.Services.GetService<IScrapper<IWeaponConfig>>()!;
            var ArtifactScrapper = host.Services.GetService<IScrapper<IArtifactConfig>>()!;
            var CharacterScrapper = host.Services.GetService<IScrapper<ICharacterConfig>>()!;
            var AchievementScrapper = host.Services.GetService<IScrapper<IAchievementConfig>>()!;

            Config.Load();

            Config.ArtifactMinLevel = 20;

            //Weapons.LoadRemote().Wait();
            //Artifacts.LoadRemote().Wait();
            //Characters.LoadRemote().Wait();
            //Achievements.LoadRemote().Wait();

            //Weapons.LoadLocal().Wait();
            Artifacts.LoadLocal().Wait();
            Characters.LoadLocal().Wait();
            //Achievements.LoadLocal().Wait();

            //WeaponScrapper.Start(Config);
            ArtifactScrapper.Start(Config);
            //CharacterScrapper.Start(Config);
            //AchievementScrapper.Start(Config);
#else
            var success = UIPrerequisites.Install();
            if (!success) return;
            AppEvents.OnClose += () => Application.Exit();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.PerMonitor);
            ApplicationConfiguration.Initialize();
            Application.Run(new MainWindow());
#endif
        }
    }
}
