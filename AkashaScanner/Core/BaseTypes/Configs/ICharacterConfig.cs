namespace AkashaScanner.Core
{
    public interface ICharacterConfig : IBaseScrapConfig, ICharacterNamesConfig
    {
        int CharacterMinLevel { get; set; }
    }
}
