using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core.DataCollections.Repositories
{
    public class WeaponsHoYoWikiRepository : HoYoWikiRepository<WeaponEntry>
    {
        public WeaponsHoYoWikiRepository(ILogger<WeaponsHoYoWikiRepository> logger)
        {
            Logger = logger;
        }

        public override async Task<List<WeaponEntry>?> Load()
        {
            using var client = CreateClient();
            Logger.LogInformation("Loading weapons");

            var resp = await LoadEntryPageList<Item>(client, "4");
            if (resp == null)
            {
                Logger.LogError("Fail to load weapons");
                return null;
            }

            var output = new List<WeaponEntry>();

            foreach (var item in resp)
            {
                if (string.IsNullOrEmpty(item.icon_url)) continue;
                var iconPath = IconRepository.GetPath("Weapons", item.name, Path.GetExtension(item.icon_url));
                await IconRepository.SaveUrlAsIcon(client, item.icon_url, iconPath);
                var entry = new WeaponEntry()
                {
                    Name = item.name.Trim(),
                    Icon = iconPath,
                    Rarity = int.Parse(item.filter_values.weapon_rarity.values[0][..1]),
                    Type = item.filter_values.weapon_type.values[0] switch
                    {
                        "Bow" => WeaponType.Bow,
                        "Catalyst" => WeaponType.Catalyst,
                        "Polearm" => WeaponType.Polearm,
                        "Sword" => WeaponType.Sword,
                        "Claymore" => WeaponType.Claymore,
                        _ => WeaponType.Invalid,
                    }
                };
                if (entry.Type != WeaponType.Invalid)
                    output.Add(entry);
            }
            Logger.LogInformation("Weapons loaded");

            return output;
        }
        private record Item
        {
            public string name = default!;
            public string icon_url = default!;
            public FilterValueList filter_values = default!;
        }

        private record FilterValueList
        {
            public FilterValue weapon_type = default!;
            public FilterValue weapon_rarity = default!;
        }

        private record FilterValue
        {
            public List<string> values = default!;
        }
    }
}
