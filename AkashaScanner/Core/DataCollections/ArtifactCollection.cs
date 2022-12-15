using AkashaScanner.Core.DataCollections.Repositories;
using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core.DataCollections
{
    public sealed class ArtifactCollection : BaseCollection<List<ArtifactEntry>>, IArtifactCollection
    {
        private const int FuzzySearchMinScore = 80;
        protected override int CurrentVersion => 1;
        protected override string LocalFile => "artifacts.json";

        public ArtifactCollection(ILogger<ArtifactCollection> logger, IRepository<List<ArtifactEntry>> repository)
        {
            Logger = logger;
            Repository = repository;
        }

        public ArtifactEntry? SearchByName(string text)
        {
            (int score, var item) = Data.Data.Search(item => item.Name.FuzzySearch(text));

            if (item == null || score < FuzzySearchMinScore)
            {
                NotifyFail(text);
                return null;
            }

            NotifySuccess(text, item.Name, score);
            return item;
        }
    }
}
