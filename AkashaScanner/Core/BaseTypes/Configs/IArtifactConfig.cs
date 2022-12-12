namespace AkashaScanner.Core
{
    public interface IArtifactConfig : IBaseScrapConfig, ITravelerNameConfig
    {
        int ArtifactMinLevel { get; set; }
        int ArtifactMinRarity { get; set; }
    }
}
