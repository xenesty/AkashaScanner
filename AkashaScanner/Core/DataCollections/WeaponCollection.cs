using AkashaScanner.Core.DataCollections.Repositories;
using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core.DataCollections
{
    public sealed class WeaponCollection : BaseCollection<List<WeaponEntry>>, IWeaponCollection
    {
        private const int FuzzySearchMinScore = 80;
        protected override int CurrentVersion => 1;

        protected override string LocalFile => "weapons.json";

        public WeaponCollection(ILogger<WeaponCollection> logger, IRepository<List<WeaponEntry>> repository)
        {
            Logger = logger;
            Repository = repository;
        }

        public WeaponEntry? SearchByName(string text)
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
