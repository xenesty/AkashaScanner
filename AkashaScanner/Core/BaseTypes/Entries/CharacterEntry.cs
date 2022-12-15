namespace AkashaScanner.Core
{
    public record CharacterEntry : IEntry
    {
        public string Name { get; init; } = default!;
        public string Icon { get; init; } = default!;
        public int Rarity { get; init; }
        public Element Element { get; init; }
        public WeaponType WeaponType { get; init; }
        public bool IsTraveler { get; init; }
        public List<Constellation> Constellations { get; init; } = default!;
        public List<Talent> Talents { get; init; } = default!;

        public record Constellation
        {
            public string Name { get; init; } = default!;
            public int SkillLevel { get; init; }
            public int BurstLevel { get; init; }
        }

        public record Talent
        {
            public string Name { get; init; } = default!;
            public TalentType Type { get; init; }
        }
    }
}
