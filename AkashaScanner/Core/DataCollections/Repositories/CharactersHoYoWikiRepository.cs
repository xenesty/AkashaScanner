using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AkashaScanner.Core.DataCollections.Repositories
{
    public class CharactersHoYoWikiRepository : HoYoWikiRepository<CharacterEntry>
    {
        private static readonly Regex RemoveHtmlTags = new(@"<[^>]+>", RegexOptions.Compiled);
        private static readonly Regex FindTalentLevelUpConstellation = new(@"Increases\s+the\s+Level\s+of\s*(.*)\s*by\s+(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private const int TalentNameScore = 90;

        public CharactersHoYoWikiRepository(ILogger<CharactersHoYoWikiRepository> logger)
        {
            Logger = logger;
        }

        public override async Task<List<CharacterEntry>?> Load()
        {
            using var client = CreateClient();
            Logger.LogInformation("Loading characters");

            var resp = await LoadEntryPageList<Item>(client, "2");
            if (resp == null)
            {
                Logger.LogError("Fail to load characters");
                return null;
            }

            var output = new List<CharacterEntry>();

            foreach (var item in resp)
            {
                var name = item.name;
                Logger.LogInformation("Loading '{name}'", name);

                var result = await LoadEntryPage<Detail>(client, item.entry_page_id);
                if (result == null)
                {
                    Logger.LogError("Fail to load '{name}'", name);
                    return null;
                }

                var rarityData = result.filter_values?.character_rarity?.values?.FirstOrDefault();
                if (rarityData == null) continue;
                if (!int.TryParse(rarityData.Replace("-Star", string.Empty), out int rarity)) continue;

                var elementData = result.filter_values?.character_vision?.values?.FirstOrDefault();
                if (elementData == null) continue;
                Element element = elementData switch
                {
                    "Anemo" => Element.Anemo,
                    "Pyro" => Element.Pyro,
                    "Cryo" => Element.Cryo,
                    "Electro" => Element.Electro,
                    "Hydro" => Element.Hydro,
                    "Geo" => Element.Geo,
                    "Dendro" => Element.Dendro,
                    _ => Element.Invalid,
                };
                if (element == Element.Invalid) continue;

                var weaponData = result.filter_values?.character_weapon?.values?.FirstOrDefault();
                if (weaponData == null) continue;
                var weaponType = weaponData switch
                {
                    "Bow" => WeaponType.Bow,
                    "Catalyst" => WeaponType.Catalyst,
                    "Polearm" => WeaponType.Polearm,
                    "Sword" => WeaponType.Sword,
                    "Claymore" => WeaponType.Claymore,
                    _ => WeaponType.Invalid,
                };
                if (weaponType == WeaponType.Invalid) continue;

                var consComp = result.modules.Find((m) => m.name == "Constellation")?.components.Find((c) => c.component_id == "summaryList");
                if (string.IsNullOrEmpty(consComp?.data)) continue;
                var constellationsData = JsonConvert.DeserializeObject<ConstellationData>(consComp.data)!.list;
                if (constellationsData.Count == 0) continue; // Skip Traveler without element

                var talentsComp = result.modules.Find((m) => m.name == "Talents")?.components.Find((c) => c.component_id == "talent");
                if (string.IsNullOrEmpty(talentsComp?.data)) continue;
                var talentsData = JsonConvert.DeserializeObject<TalentData>(talentsComp.data)!.list;

                var talents = talentsData.Select(data =>
                {
                    var name = data.title.Trim();
                    if (data.attributes != null)
                    {
                        var levelAttr = data.attributes.Find(a => a.key == "Level" || a.key == "");
                        if (levelAttr != null && levelAttr.values.Count >= 10)
                        {
                            // Assume all normal attack starts with the word 'Normal Attack'
                            if (name.StartsWith("Normal Attack"))
                            {
                                return new CharacterEntry.Talent()
                                {
                                    Name = name,
                                    Type = TalentType.Attack,
                                };
                            }
                            // Assume all bursts have an energy cost
                            if (data.attributes.Find(a => a.key.Trim() == "Energy Cost") != null)
                            {
                                return new CharacterEntry.Talent()
                                {
                                    Name = name,
                                    Type = TalentType.Burst,
                                };
                            }
                            // Assume anything that can be leveled and is not a nomral attack/burst is a skill
                            return new CharacterEntry.Talent()
                            {
                                Name = name,
                                Type = TalentType.Skill,
                            };
                        }
                    }
                    return new CharacterEntry.Talent()
                    {
                        Name = name,
                        Type = TalentType.Other,
                    };
                }).ToList();

                var skillName = talents.Find(t => t.Type == TalentType.Skill)!.Name;
                var burstName = talents.Find(t => t.Type == TalentType.Burst)!.Name;

                var iconPath = IconRepository.GetPath("Characters", item.name, Path.GetExtension(item.icon_url));
                await IconRepository.SaveUrlAsIcon(client, item.icon_url, iconPath);

                var constellations = constellationsData.Select(data =>
                {
                    var name = data.name.Trim();
                    var match = FindTalentLevelUpConstellation.Match(RemoveHtmlTags.Replace(data.desc, string.Empty));
                    if (match.Success && int.TryParse(match.Groups[2].Value, out int levelUp))
                    {
                        var talentName = match.Groups[1].Value.Trim();
                        var skillScore = talentName.FuzzySearch(skillName);
                        var burstScore = talentName.FuzzySearch(burstName);

                        if (skillScore >= TalentNameScore && skillScore > burstScore)
                        {
                            return new CharacterEntry.Constellation()
                            {
                                Name = name,
                                SkillLevel = levelUp,
                            };
                        }
                        else if (burstScore >= TalentNameScore)
                        {
                            return new CharacterEntry.Constellation()
                            {
                                Name = name,
                                BurstLevel = levelUp,
                            };
                        }
                    }
                    return new CharacterEntry.Constellation()
                    {
                        Name = name,
                    };
                });

                output.Add(new CharacterEntry()
                {
                    Name = item.name.Trim(),
                    Rarity = rarity,
                    Element = element,
                    WeaponType = weaponType,
                    IsTraveler = item.name.Contains("Traveler"),
                    Icon = iconPath,
                    Talents = talents,
                    Constellations = constellations.ToList(),
                });
            }
            Logger.LogInformation("Characters loaded");

            return output;
        }
        private record Item
        {
            public string entry_page_id = default!;
            public string name = default!;
            public string icon_url = default!;
        }

        private record Detail
        {
            public List<Module> modules = default!;
            public FilterValues filter_values = default!;
        }

        public record FilterValues
        {
            public FilterValue character_vision = default!;
            public FilterValue character_weapon = default!;
            public FilterValue character_rarity = default!;
        }

        public record FilterValue
        {
            public List<string> values = default!;
        }

        private record Module
        {
            public string name = default!;
            public List<Component> components = default!;
        }

        private record Component
        {
            public string component_id = default!;
            public string data = default!;
        }

        private record ConstellationData
        {
            public List<ConstellationDataContent> list = default!;
        }

        public record ConstellationDataContent
        {
            public string name = default!;
            public string desc = default!;
        }

        private record TalentData
        {
            public List<TalentDataContent> list = default!;
        }

        public record TalentDataContent
        {
            public string title = default!;
            public List<TalentAttribute>? attributes = default!;
        }

        public record TalentAttribute
        {
            public string key = default!;
            public List<string> values = default!;
        }
    }
}
