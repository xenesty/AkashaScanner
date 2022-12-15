namespace AkashaScanner.Core
{
    public sealed record Character
    {
        public string Name = "";
        public int Rarity;
        public Element Element;
        public WeaponType WeaponType;
        public int Friendship;
        public int Level;
        public int Ascension;
        public int Constellation;
        public int AttackLevel;
        public int SkillLevel;
        public int BurstLevel;

        public bool IsValid() => true;
    }
}
