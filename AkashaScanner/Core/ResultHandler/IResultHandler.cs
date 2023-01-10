namespace AkashaScanner.Core.ResultHandler
{
    public interface IResultHandler<R>
    {
        void Init();
        void Add(R item, int order);
        void Save();
    }
}
