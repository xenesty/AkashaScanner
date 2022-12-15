namespace AkashaScanner.Core.DataCollections
{
    public interface IAchievementCollection : IDataCollection, IEnumerable<AchievementCategoryEntry>
    {
        List<AchievementEntry> ListOverlappingAchievements();

        bool IsOverlapping(AchievementEntry entry);

        public static List<AchievementCategoryEntry> GroupByCategory(List<AchievementEntry> entries)
        {
            List<AchievementCategoryEntry> output = new();
            int catId = -1;
            foreach (var entry in entries)
            {
                if (catId != entry.Category.Id)
                {
                    catId = entry.Category.Id;
                    output.Add(entry.Category with { Achievements = new() });
                }

                output.Last().Achievements.Add(entry);
            }
            return output;
        }
    }
}
