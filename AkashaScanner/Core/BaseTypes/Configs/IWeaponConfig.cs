namespace AkashaScanner.Core
{
    public interface IWeaponConfig : IBaseScrapConfig, ITravelerNameConfig
    {
        int WeaponMinLevel { get; set; }
        int WeaponMinRarity { get; set; }
    }
}
