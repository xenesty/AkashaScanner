using AkashaScanner.Core.DataFiles;
using AkashaScanner.Core.ResultHandler;
using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core.Achievements
{
    public class AchievementResultHandler : IResultHandler<Achievement>
    {
        private readonly IDataFileRepository<AchievementOutput> DataFileRepository;
        private readonly ILogger Logger;
        private readonly AchievementOutput Dict = new();

        public AchievementResultHandler(ILogger<AchievementResultHandler> logger, IDataFileRepository<AchievementOutput> dataFileRepository)
        {
            Logger = logger;
            DataFileRepository = dataFileRepository;
        }

        public void Init()
        {
            Dict.Clear();
        }

        public void Add(Achievement item, int order)
        {
            Dict[item.Id] = item.CategoryId;
        }

        public void Save()
        {
            var file = DataFileRepository.Create(Dict.Count);
            file.Write(Dict);
        }
    }
}
