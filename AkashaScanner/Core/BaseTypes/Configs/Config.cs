using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AkashaScanner.Core
{
    public record Config : IConfig
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter(),
            }
        };
        private static readonly string ConfigFile = Path.Combine(Utils.ExecutableDirectory, "config.json");

        [JsonIgnore]
        private bool Loaded;

        public double SuspendMultiplierMin { get; set; } = 1;
        public double SuspendMultiplierMax { get; set; } = 1.2;
        public string TravelerName { get; set; } = string.Empty;
        public int WeaponMinLevel { get; set; } = 1;
        public int WeaponMinRarity { get; set; } = 1;
        public int ArtifactMinLevel { get; set; } = 0;
        public int ArtifactMinRarity { get; set; } = 1;
        public int CharacterMinLevel { get; set; } = 1;
        public Dictionary<string, string> CharacterNameOverrides { get; set; } = new();
        public Dictionary<int, bool> AchievementOverrides { get; set; } = new();

        public async Task Load()
        {
            if (Loaded) return;
            Loaded = true;
            try
            {
                var text = await File.ReadAllTextAsync(ConfigFile);
                JsonConvert.PopulateObject(text, this, Settings);
            }
            catch (FileNotFoundException) { }
        }

        public async Task Save()
        {
            await File.WriteAllTextAsync(ConfigFile, JsonConvert.SerializeObject(this, Formatting.Indented, Settings));
        }
    }
}
