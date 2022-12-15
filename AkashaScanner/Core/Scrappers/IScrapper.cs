namespace AkashaScanner.Core.Scappers
{
    public interface IScrapper<C> : IDisposable where C : IBaseScrapConfig
    {
        bool Start(C config);
    }
}
