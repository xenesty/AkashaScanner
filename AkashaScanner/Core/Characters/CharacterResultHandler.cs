using AkashaScanner.Core.DataFiles;
using AkashaScanner.Core.ResultHandler;
using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core.Characters
{
    public class CharacterResultHandler : ListResultHandler<Character, CharacterOutput>
    {
        public CharacterResultHandler(ILogger<CharacterResultHandler> logger, IDataFileRepository<CharacterOutput> dataFileRepository)
        {
            Logger = logger;
            DataFileRepository = dataFileRepository;
        }
    }
}
