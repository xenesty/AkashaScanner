namespace AkashaScanner.Core
{
    public enum ArtifactStatType
    {
        Invalid,
        HpFlat,
        HpPercent,
        AtkFlat,
        AtkPercent,
        DefFlat,
        DefPercent,
        EnergyRecharge,
        ElementalMastery,
        HealingBonus,
        CritRate,
        CritDmg,
        PhysicalDmg,
        AnemoDmg,
        PyroDmg,
        CryoDmg,
        ElectroDmg,
        HydroDmg,
        GeoDmg,
        DendroDmg,
    }
    static class ArtifactStatTypeMethods
    {
        public static bool IsFlat(this ArtifactStatType type) => Flat.Contains(type);
        public static bool IsSubstats(this ArtifactStatType type) => Substats.Contains(type);
        public static bool IsValidFor(this ArtifactStatType type, ArtifactSlot slot) => MainStats[slot].Contains(type);

        private static readonly List<ArtifactStatType> Flat = new() {
            ArtifactStatType.HpFlat,
            ArtifactStatType.AtkFlat,
            ArtifactStatType.DefFlat,
            ArtifactStatType.ElementalMastery,
        };

        private static readonly List<ArtifactStatType> Substats = new() {
            ArtifactStatType.HpFlat,
            ArtifactStatType.HpPercent,
            ArtifactStatType.AtkFlat,
            ArtifactStatType.AtkPercent,
            ArtifactStatType.DefFlat,
            ArtifactStatType.DefPercent,
            ArtifactStatType.EnergyRecharge,
            ArtifactStatType.ElementalMastery,
            ArtifactStatType.CritRate,
            ArtifactStatType.CritDmg,
        };

        private static readonly Dictionary<ArtifactSlot, List<ArtifactStatType>> MainStats = new()
        {
            {
                ArtifactSlot.Invalid,
                new List<ArtifactStatType>()
            },
            {
                ArtifactSlot.Flower,
                new List<ArtifactStatType> {
                    ArtifactStatType.HpFlat,
                }
            },
            {
                ArtifactSlot.Plume,
                new List<ArtifactStatType> {
                    ArtifactStatType.AtkFlat,
                }
            },
            {
                ArtifactSlot.Sands,
                new List<ArtifactStatType> {
                    ArtifactStatType.HpPercent,
                    ArtifactStatType.AtkPercent,
                    ArtifactStatType.DefPercent,
                    ArtifactStatType.EnergyRecharge,
                    ArtifactStatType.ElementalMastery,
                }
            },
            {
                ArtifactSlot.Goblet,
                new List<ArtifactStatType> {
                    ArtifactStatType.HpPercent,
                    ArtifactStatType.AtkPercent,
                    ArtifactStatType.DefPercent,
                    ArtifactStatType.ElementalMastery,
                    ArtifactStatType.PyroDmg,
                    ArtifactStatType.CryoDmg,
                    ArtifactStatType.ElectroDmg,
                    ArtifactStatType.HydroDmg,
                    ArtifactStatType.AnemoDmg,
                    ArtifactStatType.GeoDmg,
                    ArtifactStatType.DendroDmg,
                }
            },
            {
                ArtifactSlot.Circlet,
                new List<ArtifactStatType> {
                    ArtifactStatType.HpPercent,
                    ArtifactStatType.AtkPercent,
                    ArtifactStatType.DefPercent,
                    ArtifactStatType.ElementalMastery,
                    ArtifactStatType.HealingBonus,
                    ArtifactStatType.CritRate,
                    ArtifactStatType.CritDmg,
                }
            },
        };
    }

}