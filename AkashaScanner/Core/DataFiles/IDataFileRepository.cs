namespace AkashaScanner.Core.DataFiles
{
    public interface IDataFileRepository<T>
    {
        List<IDataFile<T>> List();
        IDataFile<T> Create(int count);
    }
}
