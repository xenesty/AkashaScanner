using AkashaScanner.Core.DataFiles;
using AkashaScanner.Core.ResultHandler;
using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core.Weapons
{
    public class WeaponResultHandler : ListResultHandler<Weapon, WeaponOutput>
    {
        public WeaponResultHandler(ILogger<WeaponResultHandler> logger, IDataFileRepository<WeaponOutput> dataFileRepository)
        {
            Logger = logger;
            DataFileRepository = dataFileRepository;
        }
    }
}
