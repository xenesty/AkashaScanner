namespace AkashaScanner.Core
{
    public record WeaponEntry : IEntry
    {
        public string Name { get; init; } = default!;
        public string Icon { get; init; } = default!;
        public int Rarity { get; init; }
        public WeaponType Type { get; init; }
    }
}
