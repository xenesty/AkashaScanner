namespace AkashaScanner.Core
{
    public interface IConfig : IGlobalConfig, IWeaponConfig, IArtifactConfig, ICharacterConfig, IAchievementConfig
    {
        Task Load();
        Task Save();
    }
}
