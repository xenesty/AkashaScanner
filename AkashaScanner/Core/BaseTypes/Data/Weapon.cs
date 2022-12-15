namespace AkashaScanner.Core
{
    public sealed record Weapon
    {
        public string Name = "";
        public WeaponType Type;
        public int Rarity;
        public int Level;
        public int Ascension;
        public int Refinement;
        public string EquippedCharacter = "";
        public bool Locked;

        public bool IsValid() => true;
    }
}
