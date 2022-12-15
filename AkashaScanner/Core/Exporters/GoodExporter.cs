using AkashaScanner.Core.DataFiles;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AkashaScanner.Core.Exporters
{
    public static class GoodExporter
    {
        public static bool Export(
            [MaybeNullWhen(false)] out string output,
            IDataFile<WeaponOutput>? weaponFile = null,
            IDataFile<ArtifactOutput>? artifactFile = null,
            IDataFile<CharacterOutput>? characterFile = null)
        {
            JObject obj = new()
            {
                ["format"] = "GOOD",
                ["version"] = 2,
                ["source"] = "AkashaScanner",
            };

            string TravelerName = string.Empty;

            if (characterFile != null)
            {
                if (!characterFile.Read(out var data))
                {
                    output = null;
                    return false;
                }
                JArray arr = new();
                foreach (var character in data)
                {
                    if (!character.IsValid()) continue;
                    JObject dict = new()
                    {
                        ["key"] = ToKey(character.Name),
                        ["level"] = character.Level,
                        ["constellation"] = character.Constellation,
                        ["ascension"] = character.Ascension,
                        ["talent"] = new JObject()
                        {
                            ["auto"] = character.AttackLevel,
                            ["skill"] = character.SkillLevel,
                            ["burst"] = character.BurstLevel,
                        },
                    };
                    if (character.Name.Contains("Traveler"))
                    {
                        TravelerName = character.Name;
                    }
                    arr.Add(dict);
                }
                obj["characters"] = arr;
            }

            string GetEqupped(string name)
            {
                return ToKey(!string.IsNullOrEmpty(TravelerName) && name == "Traveler" ? TravelerName : name);
            }

            if (weaponFile != null)
            {
                if (!weaponFile.Read(out var data))
                {
                    output = null;
                    return false;
                }
                JArray arr = new();
                foreach (var weapon in data)
                {
                    if (!weapon.IsValid()) continue;
                    JObject dict = new()
                    {
                        ["key"] = ToKey(weapon.Name),
                        ["level"] = weapon.Level,
                        ["ascension"] = weapon.Ascension,
                        ["refinement"] = weapon.Refinement,
                        ["location"] = GetEqupped(weapon.EquippedCharacter),
                        ["lock"] = weapon.Locked,
                    };
                    arr.Add(dict);
                }
                obj["weapons"] = arr;
            }

            if (artifactFile != null)
            {
                if (!artifactFile.Read(out var data))
                {
                    output = null;
                    return false;
                }
                JArray arr = new();
                foreach (var artifact in data)
                {
                    if (!artifact.IsValid()) continue;
                    JArray substats = new();
                    foreach (var substat in artifact.Substats)
                    {
                        JObject stat = new()
                        {
                            ["key"] = StatKeys[substat.Type],
                            ["value"] = substat.Value,
                        };
                        substats.Add(stat);
                    }
                    JObject dict = new()
                    {
                        ["setKey"] = ToKey(artifact.SetName),
                        ["slotKey"] = SlotKeys[artifact.Slot],
                        ["level"] = artifact.Level,
                        ["rarity"] = artifact.Rarity,
                        ["mainStatKey"] = StatKeys[artifact.MainStat],
                        ["location"] = GetEqupped(artifact.EquippedCharacter),
                        ["lock"] = artifact.Locked,
                        ["substats"] = substats,
                    };
                    arr.Add(dict);
                }
                obj["artifacts"] = arr;
            }

            output = obj.ToString();
            return true;
        }

        private static readonly Dictionary<ArtifactStatType, string> StatKeys = new()
        {
            [ArtifactStatType.HpFlat] = "hp",
            [ArtifactStatType.HpPercent] = "hp_",
            [ArtifactStatType.AtkFlat] = "atk",
            [ArtifactStatType.AtkPercent] = "atk_",
            [ArtifactStatType.DefFlat] = "def",
            [ArtifactStatType.DefPercent] = "def_",
            [ArtifactStatType.ElementalMastery] = "eleMas",
            [ArtifactStatType.EnergyRecharge] = "enerRech_",
            [ArtifactStatType.HealingBonus] = "heal_",
            [ArtifactStatType.CritRate] = "critRate_",
            [ArtifactStatType.CritDmg] = "critDMG_",
            [ArtifactStatType.PhysicalDmg] = "physical_dmg_",
            [ArtifactStatType.AnemoDmg] = "anemo_dmg_",
            [ArtifactStatType.GeoDmg] = "geo_dmg_",
            [ArtifactStatType.ElectroDmg] = "electro_dmg_",
            [ArtifactStatType.HydroDmg] = "hydro_dmg_",
            [ArtifactStatType.PyroDmg] = "pyro_dmg_",
            [ArtifactStatType.CryoDmg] = "cryo_dmg_",
            [ArtifactStatType.DendroDmg] = "dendro_dmg_",
        };

        private static readonly Dictionary<ArtifactSlot, string> SlotKeys = new()
        {
            [ArtifactSlot.Flower] = "flower",
            [ArtifactSlot.Plume] = "plume",
            [ArtifactSlot.Sands] = "sands",
            [ArtifactSlot.Goblet] = "goblet",
            [ArtifactSlot.Circlet] = "circlet",
        };

        private static readonly CultureInfo En = CultureInfo.GetCultureInfo("en");

        private static string ToKey(string input)
        {
            return Regex.Replace(En.TextInfo.ToTitleCase(input), @"\W", string.Empty);
        }
    }
}
