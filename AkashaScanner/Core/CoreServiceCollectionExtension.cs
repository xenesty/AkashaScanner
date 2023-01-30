using AkashaScanner.Core.Achievements;
using AkashaScanner.Core.Artifacts;
using AkashaScanner.Core.Characters;
using AkashaScanner.Core.DataCollections;
using AkashaScanner.Core.DataCollections.Repositories;
using AkashaScanner.Core.DataFiles;
using AkashaScanner.Core.Navigation.Achievement;
using AkashaScanner.Core.Navigation.Character;
using AkashaScanner.Core.Navigation.Inventory;
using AkashaScanner.Core.Navigation.Keyboard;
using AkashaScanner.Core.Navigation.Mouse;
using AkashaScanner.Core.ProcessControl;
using AkashaScanner.Core.ResultHandler;
using AkashaScanner.Core.Scappers;
using AkashaScanner.Core.ScrapPlans;
using AkashaScanner.Core.Screenshot;
using AkashaScanner.Core.Suspender;
using AkashaScanner.Core.TextRecognition;
using AkashaScanner.Core.TextRecognition.Tesseract;
using AkashaScanner.Core.Weapons;
using Microsoft.Extensions.DependencyInjection;

namespace AkashaScanner.Core
{
    public static class CoreServiceCollectionExtension
    {
        public static void AddCoreServices(this IServiceCollection services)
        {
            services
                .AddScoped<GameWindow>()
                .AddSingleton<AppUpdate>()
                .AddSingleton<ISuspender, RandomSuspender>()
                .AddSingleton<IProcessControl, GenshinProcessControl>()
                .AddScoped<ITextRecognitionService, TesseractService>()
                .AddScoped<TemplateMatchingService>()
                .AddScoped<IScreenshotProvider, ScreenshotProvider>()

                .AddSingleton<IMouseService, WindowsMouseService>()
                .AddSingleton<IKeyboardService, WindowsKeyboardService>()
                .AddScoped<IInventoryNavigation, GenshinInventoryNavigation>()
                .AddScoped<ICharacterNavigation, GenshinCharacterNavigation>()
                .AddScoped<IAchievementNavigation, GenshinAchievementNavigation>()

                .AddSingleton<IRepository<List<WeaponEntry>>, WeaponsHoYoWikiRepository>()
                .AddSingleton<IWeaponCollection, WeaponCollection>()

                .AddSingleton<IRepository<List<ArtifactEntry>>, ArtifactsHoYoWikiRepository>()
                .AddSingleton<IArtifactCollection, ArtifactCollection>()

                .AddSingleton<IRepository<List<CharacterEntry>>, CharactersHoYoWikiRepository>()
                .AddSingleton<ICharacterCollection, CharacterCollection>()

                .AddSingleton<IRepository<List<AchievementCategoryEntry>>, AchievementRepository>()
                .AddSingleton<IAchievementCollection, AchievementCollection>()

                .AddSingleton<IConfig, Config>()
                .AddSingleton<IGlobalConfig>(p => p.GetService<IConfig>()!)
                .AddSingleton<IAchievementConfig>(p => p.GetService<IConfig>()!)
                .AddSingleton<IArtifactConfig>(p => p.GetService<IConfig>()!)
                .AddSingleton<ICharacterConfig>(p => p.GetService<IConfig>()!)
                .AddSingleton<IWeaponConfig>(p => p.GetService<IConfig>()!)

                .AddScoped<IDataFileRepository<WeaponOutput>, WeaponDataFileRepository>()
                .AddScoped<IResultHandler<Weapon>, WeaponResultHandler>()
                .AddScoped<IScrapPlanManager<IWeaponConfig, Weapon>, WeaponScrapPlan>()
                .AddScoped<IScrapper<IWeaponConfig>, WeaponScrapper>()

                .AddScoped<IDataFileRepository<ArtifactOutput>, ArtifactDataFileRepository>()
                .AddScoped<IResultHandler<Artifact>, ArtifactResultHandler>()
                .AddScoped<IScrapPlanManager<IArtifactConfig, Artifact>, ArtifactScrapPlan>()
                .AddScoped<IScrapper<IArtifactConfig>, ArtifactScrapper>()

                .AddScoped<IDataFileRepository<CharacterOutput>, CharacterDataFileRepository>()
                .AddScoped<IResultHandler<Character>, CharacterResultHandler>()
                .AddScoped<IScrapPlanManager<ICharacterConfig, Character>, CharacterScrapPlan>()
                .AddScoped<IScrapper<ICharacterConfig>, CharacterScrapper>()

                .AddScoped<IDataFileRepository<AchievementOutput>, AchievementDataFileRepository>()
                .AddScoped<IResultHandler<Achievement>, AchievementResultHandler>()
                .AddScoped<IScrapPlanManager<IAchievementConfig, Achievement>, AchievementScrapPlan>()
                .AddScoped<IScrapper<IAchievementConfig>, AchievementScrapper>();
        }
    }
}
