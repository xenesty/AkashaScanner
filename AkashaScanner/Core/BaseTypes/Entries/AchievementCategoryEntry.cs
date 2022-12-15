namespace AkashaScanner.Core
{
    public record AchievementCategoryEntry : IEntry, IComparable<AchievementCategoryEntry>
    {
        public int Id { get; init; }
        public string Name { get; init; } = default!;
        public int Order { get; init; }
        public List<AchievementEntry> Achievements { get; init; } = default!;

        public int CompareTo(AchievementCategoryEntry? other)
        {
            if (other == null) return 1;
            if (ReferenceEquals(this, other)) return 0;
            return Order - other.Order;
        }
    }
}
