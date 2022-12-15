using AkashaScanner.Core.DataCollections.Repositories;
using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core.DataCollections
{
    public sealed class CharacterCollection : BaseCollection<List<CharacterEntry>>, ICharacterCollection
    {
        private const int FuzzySearchMinScore = 80;

        private static readonly CharacterEntry Traveler = new()
        {
            Name = "Traveler",
            IsTraveler = true,
            Constellations = new(),
            Talents = new(),
        };

        protected override int CurrentVersion => 1;
        protected override string LocalFile => "characters.json";

        public CharacterCollection(ILogger<CharacterCollection> logger, IRepository<List<CharacterEntry>> repository)
        {
            Logger = logger;
            Repository = repository;
        }

        public CharacterEntry? InternalSearchByName(string text, string travelerName, Func<string, string, int> SearchFn)
        {
            (int score, var item) = Data.Data.Search(item => item.IsTraveler ? 0 : SearchFn(item.Name, text));

            if (!string.IsNullOrEmpty(travelerName))
            {
                var travelerScore = SearchFn(travelerName, text);
                if (travelerScore > score)
                {
                    score = travelerScore;
                    item = Traveler;
                }
            }

            if (item == null || score < FuzzySearchMinScore)
            {
                NotifyFail(text);
                return null;
            }

            NotifySuccess(text, item.Name, score);
            return item;
        }

        public CharacterEntry? SearchByName(string text, string travelerName = "")
        {
            return InternalSearchByName(text, travelerName, Utils.FuzzySearch);
        }


        public CharacterEntry? PartialSearchByName(string text, string travelerName = "")
        {
            return InternalSearchByName(text, travelerName, Utils.PartialSearch);
        }

        public CharacterEntry? SearchByConstellation(string text)
        {
            (int score, var item) = Data.Data.Search(item => item.Constellations.Search(cons => text.FuzzySearch(cons.Name)).Item1);

            if (item == null || score < FuzzySearchMinScore)
            {
                NotifyFail(text);
                return null;
            }

            NotifySuccess(text, item.Name, score);
            return item;
        }

        public CharacterEntry? GetTravelerByElement(Element element)
        {
            return Data.Data.Find(item => item.IsTraveler && item.Element == element);
        }
    }
}
