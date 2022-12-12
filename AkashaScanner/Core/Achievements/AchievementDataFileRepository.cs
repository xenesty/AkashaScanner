using AkashaScanner.Core.DataFiles;

namespace AkashaScanner.Core.Achievements
{
    public class AchievementDataFileRepository : DataFileRepository<AchievementOutput>
    {
        public override int CurrentVersion => 1;

        public AchievementDataFileRepository() : base("achievements") { }
    }
}
