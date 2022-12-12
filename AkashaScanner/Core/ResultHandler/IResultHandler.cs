namespace AkashaScanner.Core.ResultHandler
{
    public interface IResultHandler<R>
    {
        void Add(R item, int order);
        void Save();
    }
}
