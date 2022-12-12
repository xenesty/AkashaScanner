using AkashaScanner.Core.DataFiles;

namespace AkashaScanner.Core.Artifacts
{
    public class ArtifactDataFileRepository : DataFileRepository<ArtifactOutput>
    {
        public override int CurrentVersion => 1;

        public ArtifactDataFileRepository() : base("artifacts") { }
    }
}
