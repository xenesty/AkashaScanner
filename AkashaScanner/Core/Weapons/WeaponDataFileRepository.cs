using AkashaScanner.Core.DataFiles;

namespace AkashaScanner.Core.Weapons
{
    public class WeaponDataFileRepository : DataFileRepository<WeaponOutput>
    {
        public override int CurrentVersion => 1;

        public WeaponDataFileRepository() : base("weapons") { }
    }
}
