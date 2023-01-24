using FuzzySharp;

namespace AkashaScanner.Core.Artifacts
{
    // https://docs.google.com/spreadsheets/d/1sYQrV5Yp_QTVEKMLWquMu0mDgHhOO_Rh2LfcWdS_Eno/edit#gid=1604940600
    public class ArtifactSubstatValues
    {
        public static readonly Dictionary<int, ArtifactSubstatValues> Values = new();

        public readonly Dictionary<ArtifactStatType, List<SubstatValue>> Substats = new();

        private static readonly Dictionary<ArtifactStatType, string> SubStatsMapping = new()
        {
            [ArtifactStatType.HpFlat] = "+{0:0}",
            [ArtifactStatType.HpPercent] = "+{0:0.0}%",
            [ArtifactStatType.AtkFlat] = "+{0:0}",
            [ArtifactStatType.AtkPercent] = "+{0:0.0}%",
            [ArtifactStatType.DefFlat] = "+{0:0}",
            [ArtifactStatType.DefPercent] = "+{0:0.0}%",
            [ArtifactStatType.EnergyRecharge] = "+{0:0.0}%",
            [ArtifactStatType.ElementalMastery] = "+{0:0}",
            [ArtifactStatType.CritRate] = "+{0:0.0}%",
            [ArtifactStatType.CritDmg] = "+{0:0.0}%",
        };

        static ArtifactSubstatValues()
        {
            Values[5] = new(6, new() { 1, 0.9, 0.8, 0.7 }, new()
            {
                [ArtifactStatType.HpFlat] = 298.75,
                [ArtifactStatType.HpPercent] = 5.83,
                [ArtifactStatType.AtkFlat] = 19.45,
                [ArtifactStatType.AtkPercent] = 5.83,
                [ArtifactStatType.DefFlat] = 23.15,
                [ArtifactStatType.DefPercent] = 7.29,
                [ArtifactStatType.CritRate] = 3.89,
                [ArtifactStatType.CritDmg] = 7.77,
                [ArtifactStatType.EnergyRecharge] = 6.48,
                [ArtifactStatType.ElementalMastery] = 23.31,
            });
            Values[4] = new(4, new() { 1, 0.9, 0.8, 0.7 }, new()
            {
                [ArtifactStatType.HpFlat] = 239,
                [ArtifactStatType.HpPercent] = 4.66,
                [ArtifactStatType.AtkFlat] = 15.56,
                [ArtifactStatType.AtkPercent] = 4.66,
                [ArtifactStatType.DefFlat] = 18.52,
                [ArtifactStatType.DefPercent] = 5.83,
                [ArtifactStatType.CritRate] = 3.11,
                [ArtifactStatType.CritDmg] = 6.22,
                [ArtifactStatType.EnergyRecharge] = 5.18,
                [ArtifactStatType.ElementalMastery] = 18.65,
            });
            Values[3] = new(2, new() { 1, 0.9, 0.8, 0.7 }, new()
            {
                [ArtifactStatType.HpFlat] = 143.4,
                [ArtifactStatType.HpPercent] = 3.5,
                [ArtifactStatType.AtkFlat] = 9.34,
                [ArtifactStatType.AtkPercent] = 3.5,
                [ArtifactStatType.DefFlat] = 11.11,
                [ArtifactStatType.DefPercent] = 4.37,
                [ArtifactStatType.CritRate] = 2.33,
                [ArtifactStatType.CritDmg] = 4.66,
                [ArtifactStatType.EnergyRecharge] = 3.89,
                [ArtifactStatType.ElementalMastery] = 13.99,
            });
            Values[2] = new(1, new() { 1, 0.85, 0.7 }, new()
            {
                [ArtifactStatType.HpFlat] = 71.7,
                [ArtifactStatType.HpPercent] = 2.33,
                [ArtifactStatType.AtkFlat] = 4.67,
                [ArtifactStatType.AtkPercent] = 2.33,
                [ArtifactStatType.DefFlat] = 5.56,
                [ArtifactStatType.DefPercent] = 2.91,
                [ArtifactStatType.CritRate] = 1.55,
                [ArtifactStatType.CritDmg] = 3.11,
                [ArtifactStatType.EnergyRecharge] = 2.59,
                [ArtifactStatType.ElementalMastery] = 9.33,
            });
            Values[1] = new(1, new() { 1, 0.8 }, new()
            {
                [ArtifactStatType.HpFlat] = 29.88,
                [ArtifactStatType.HpPercent] = 1.46,
                [ArtifactStatType.AtkFlat] = 1.95,
                [ArtifactStatType.AtkPercent] = 1.46,
                [ArtifactStatType.DefFlat] = 2.31,
                [ArtifactStatType.DefPercent] = 1.82,
                [ArtifactStatType.CritRate] = 0.97,
                [ArtifactStatType.CritDmg] = 1.94,
                [ArtifactStatType.EnergyRecharge] = 1.62,
                [ArtifactStatType.ElementalMastery] = 5.83,
            });
        }

        public static Dictionary<ArtifactStatType, List<SubstatValue>>? GetValues(int rarity)
        {
            if (rarity >= 1 && rarity <= 5)
                return Values[rarity].Substats;
            return null;
        }

        private static IEnumerable<List<int>> GetCombinations(int n, int r)
        {
            var list = Enumerable.Repeat(0, r).ToList();
            yield return list;
            while (true)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    int k = list[i];
                    if (k == n - 1)
                    {
                        if (i == list.Count - 1)
                        {
                            yield break;
                        }
                        continue;
                    }
                    else
                    {
                        var x = ++list[i];
                        for (int j = 0; j < i; ++j)
                        {
                            list[j] = x;
                        }
                        yield return list;
                        break;
                    }
                }
            }
        }

        private ArtifactSubstatValues(int maxRoll, List<double> tiers, Dictionary<ArtifactStatType, double> values)
        {
            int numberOfTiers = tiers.Count;
            Dictionary<ArtifactStatType, List<double>> allValues = new();
            Dictionary<ArtifactStatType, Dictionary<string, SubstatValue>> substatMap = new();
            foreach (var (type, value) in values)
            {
                allValues[type] = tiers.Select(t => Math.Round(t * value, 2)).ToList();
                substatMap[type] = new();
            }
            foreach (var list in GetCombinations(maxRoll, numberOfTiers))
            {
                foreach (var (type, vals) in allValues)
                {
                    double sum = 0;
                    for (int i = 0; i < numberOfTiers; ++i)
                    {
                        int left = i == 0 ? maxRoll - 1 : list[i - 1];
                        int right = list[i];
                        sum += (left - right) * vals[i];
                    }
                    if (type.IsFlat())
                        sum = Math.Round(sum);
                    else
                        sum = Math.Round(sum, 1);
                    if (sum <= 0) continue;
                    var text = string.Format(SubStatsMapping[type], sum);
                    var map = substatMap[type]!;
                    if (map.ContainsKey(text)) continue;
                    var entry = new SubstatValue()
                    {
                        Text = text,
                        Value = Convert.ToDecimal(sum),
                    };
                    foreach (var (key, item) in map)
                    {
                        var distance = Levenshtein.EditDistance(key, text, 0);
                        if (distance <= 2)
                        {
                            entry.SimilarValues.Add(item);
                            item.SimilarValues.Add(entry);
                        }
                    }
                    map[text] = entry;
                }
            }
            foreach (var (stat, dict) in substatMap)
            {
                var list = dict.Values.ToList();
                list.Sort();
                foreach (var item in list)
                {
                    item.SimilarValues.Sort();
                }
                Substats[stat] = list;
            }
        }

        public record SubstatValue : IComparable<SubstatValue>
        {
            public string Text { get; init; } = default!;
            public decimal Value { get; init; }
            public List<SubstatValue> SimilarValues = new();

            public int CompareTo(SubstatValue? other)
            {
                if (other == null) return -1;
                if (ReferenceEquals(this, other)) return 0;
                if (Value < other.Value) return -1;
                return 1;
            }
        }
    }
}
