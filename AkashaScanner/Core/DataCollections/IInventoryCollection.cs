namespace AkashaScanner.Core.DataCollections
{
    public interface IInventoryCollection<E> : IDataCollection where E : class, IEntry
    {
        E? SearchByName(string text);
    }
}
