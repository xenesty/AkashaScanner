using Newtonsoft.Json;

namespace AkashaScanner.Core
{
    public record AchievementEntry : IEntry
    {
        public string Name { get; init; } = default!;
        public List<int> Ids { get; init; } = default!;

        [JsonIgnore]
        public AchievementCategoryEntry Category { get; set; } = default!;
    }
}
