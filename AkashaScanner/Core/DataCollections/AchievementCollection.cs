using AkashaScanner.Core.DataCollections.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections;

namespace AkashaScanner.Core.DataCollections
{
    public sealed class AchievementCollection : BaseCollection<List<AchievementCategoryEntry>>, IAchievementCollection
    {
        protected override int CurrentVersion => 1;
        protected override string LocalFile => "achievements.json";

        public AchievementCollection(ILogger<AchievementCollection> logger, IRepository<List<AchievementCategoryEntry>> repository)
        {
            Logger = logger;
            Repository = repository;
        }

        public IEnumerator<AchievementCategoryEntry> GetEnumerator() => Data.Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private List<AchievementEntry> Overlapping = new();
        private ISet<int> OverlappingIds = new HashSet<int>();

        public override async Task LoadLocal()
        {
            await base.LoadLocal();
            if (!IsLoaded()) return;
            foreach (var category in Data.Data)
            {
                foreach (var entry in category.Achievements)
                {
                    entry.Category = category;
                }
            }
            LoadAllOverlappingAchievements();
        }

        public override async Task LoadRemote()
        {
            await base.LoadRemote();
            LoadAllOverlappingAchievements();
        }

        private void LoadOverlappingAchievements(AchievementEntry entry, ISet<int> overlapped)
        {
            var e1Name = entry.Name.ToLowerInvariant();
            foreach (var category in Data.Data)
            {
                foreach (var entry2 in category.Achievements)
                {
                    if (ReferenceEquals(entry, entry2)) return;

                    var e2Name = entry2.Name.ToLowerInvariant();

                    if (e2Name.Contains(e1Name))
                        overlapped.Add(entry.Ids[0]);

                    if (e1Name.Contains(e2Name))
                        overlapped.Add(entry2.Ids[0]);
                }
            }
        }

        private void LoadAllOverlappingAchievements()
        {
            HashSet<int> overlapped = new();
            foreach (var category in Data.Data)
            {
                foreach (var entry in category.Achievements)
                {
                    LoadOverlappingAchievements(entry, overlapped);
                }
            }
            Overlapping = new();
            OverlappingIds = new HashSet<int>();
            foreach (var category in Data.Data)
            {
                foreach (var entry in category.Achievements)
                {
                    var id = entry.Ids[0];
                    if (overlapped.Contains(id))
                    {
                        Overlapping.Add(entry);
                        OverlappingIds.Add(id);
                    }
                }
            }
        }

        public List<AchievementEntry> ListOverlappingAchievements() => Overlapping;

        public bool IsOverlapping(AchievementEntry entry) => OverlappingIds.Contains(entry.Ids[0]);
    }
}
