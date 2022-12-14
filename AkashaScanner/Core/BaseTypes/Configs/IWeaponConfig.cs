namespace AkashaScanner.Core
{
    public interface IWeaponConfig : IBaseScrapConfig, ITravelerNameConfig, ICharacterNamesConfig
    {
        int WeaponMinLevel { get; set; }
        int WeaponMinRarity { get; set; }
    }
}
