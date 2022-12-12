using AkashaScanner.Core.DataFiles;

namespace AkashaScanner.Core.Characters
{
    public class CharacterDataFileRepository : DataFileRepository<CharacterOutput>
    {
        public override int CurrentVersion => 1;

        public CharacterDataFileRepository() : base("characters") { }
    }
}
