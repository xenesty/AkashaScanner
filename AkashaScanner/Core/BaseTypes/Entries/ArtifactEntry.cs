namespace AkashaScanner.Core
{
    public record ArtifactEntry : IEntry
    {
        public string Name { get; init; } = default!;
        public string Icon { get; init; } = default!;
        public ArtifactSlot Slot { get; init; }
        public string SetName { get; init; } = default!;
    }
}
