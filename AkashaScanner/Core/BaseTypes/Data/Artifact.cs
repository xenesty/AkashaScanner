using System.Text;

namespace AkashaScanner.Core
{
    public sealed record Artifact
    {
        public string SetName = "";
        public ArtifactSlot Slot;
        public int Rarity;
        public ArtifactStatType MainStat;
        public int Level;
        public List<SubStat> Substats = new();
        public string EquippedCharacter = "";
        public bool Locked;

        public bool IsValid() => true;

        private bool PrintMembers(StringBuilder builder)
        {
            builder.Append("SetName = ");
            builder.Append(SetName);
            builder.Append(", Slot = ");
            builder.Append(Slot);
            builder.Append(", Rarity = ");
            builder.Append(Rarity);
            builder.Append(", MainStat = ");
            builder.Append(MainStat);
            builder.Append(", Level = ");
            builder.Append(Level);
            builder.Append(", Substats = [");
            bool first = true;
            foreach (var stat in Substats)
            {
                if (first) first = false;
                else builder.Append(", ");

                builder.Append("{ Type = ");
                builder.Append(stat.Type);
                builder.Append(", Value = ");
                builder.Append(stat.Value);
                builder.Append(" }");
            }
            builder.Append("], EquippedCharacter = ");
            builder.Append(EquippedCharacter);
            return true;
        }

        public record SubStat
        {
            public ArtifactStatType Type;
            public decimal Value;
        }
    }
}
