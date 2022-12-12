namespace AkashaScanner.Core.DataCollections
{
    public interface ICharacterCollection : IDataCollection
    {
        CharacterEntry? SearchByName(string text, string travelerName = "");
        CharacterEntry? PartialSearchByName(string text, string travelerName = "");
        CharacterEntry? SearchByConstellation(string text);
        CharacterEntry? GetTravelerByElement(Element element);
    }
}
