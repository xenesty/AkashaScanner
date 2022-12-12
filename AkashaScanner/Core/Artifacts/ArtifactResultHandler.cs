using AkashaScanner.Core.DataFiles;
using AkashaScanner.Core.ResultHandler;
using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core.Artifacts
{
    public class ArtifactResultHandler : ListResultHandler<Artifact, ArtifactOutput>
    {
        public ArtifactResultHandler(ILogger<ArtifactResultHandler> logger, IDataFileRepository<ArtifactOutput> dataFileRepository)
        {
            Logger = logger;
            DataFileRepository = dataFileRepository;
        }
    }
}
