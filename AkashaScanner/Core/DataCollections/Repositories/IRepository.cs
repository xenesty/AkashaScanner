using System.Threading.Tasks;

namespace AkashaScanner.Core.DataCollections.Repositories
{
    public interface IRepository<E> where E : class
    {
        Task<E?> Load();
    }
}
