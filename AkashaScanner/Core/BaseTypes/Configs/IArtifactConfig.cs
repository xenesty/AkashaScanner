namespace AkashaScanner.Core
{
    public interface IArtifactConfig : IBaseScrapConfig, ITravelerNameConfig, ICharacterNamesConfig
    {
        int ArtifactMinLevel { get; set; }
        int ArtifactMinRarity { get; set; }
    }
}
