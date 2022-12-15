using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace AkashaScanner.Core.DataCollections.Repositories
{
    public class AchievementRepository : IRepository<List<AchievementCategoryEntry>>
    {
        private const string AchievementDataUrl = "https://raw.githubusercontent.com/MadeBaruna/paimon-moe/main/src/data/achievement/en.json";

        private readonly ILogger Logger;

        public AchievementRepository(ILogger<AchievementRepository> logger)
        {
            Logger = logger;
        }

        public async Task<List<AchievementCategoryEntry>?> Load()
        {
            using var client = CreateClient();
            Logger.LogInformation("Loading achievements");
            var resp = await client.GetAsync(AchievementDataUrl);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<IDictionary<string, AchievementCategory>>(body)!;

            var categories = new List<AchievementCategoryEntry>();
            foreach (var (catIdStr, cat) in data)
            {
                if (!int.TryParse(catIdStr, out int catId) || string.IsNullOrEmpty(cat.name))
                {
                    Logger.LogError("Fail to load achievement category {id}", catIdStr);
                    return null;
                }
                var category = new AchievementCategoryEntry()
                {
                    Id = catId,
                    Name = cat.name,
                    Order = cat.order,
                    Achievements = new(),
                };
                foreach (var achievement in cat.achievements)
                {
                    var item = ParseAchievement(category, achievement);
                    if (item != null)
                        category.Achievements.Add(item);
                }
                categories.Add(category);
            }
            categories.Sort();
            return categories;
        }

        private AchievementEntry? ParseAchievement(AchievementCategoryEntry category, JToken achievement)
        {
            if (achievement is JObject obj)
            {
                if (
                    obj.TryGetValue("id", out var idToken) && idToken.Type == JTokenType.Integer &&
                    obj.TryGetValue("name", out var nameToken) && nameToken.Type == JTokenType.String
                )
                {
                    return new()
                    {
                        Ids = new() { idToken.ToObject<int>() },
                        Name = nameToken.ToObject<string>()!,
                        Category = category,
                    };
                }
            }
            else if (achievement is JArray arr)
            {
                List<int> ids = new();
                string name = default!;
                foreach (var token in arr)
                {
                    if (token is JObject ob)
                    {
                        if (ob.TryGetValue("id", out var idToken) && idToken.Type == JTokenType.Integer)
                        {
                            ids.Add(idToken.ToObject<int>());
                        }
                        else
                        {
                            Logger.LogWarning("Cannot get id of achievement");
                            return null;
                        }
                        if (string.IsNullOrEmpty(name))
                        {
                            if (ob.TryGetValue("name", out var nameToken) && nameToken.Type == JTokenType.String)
                            {
                                name = nameToken.ToObject<string>()!;
                            }
                            else
                            {
                                Logger.LogWarning("Cannot get name of achievement");
                                return null;
                            }
                        }
                    }
                }
                return new()
                {
                    Ids = ids,
                    Name = name,
                    Category = category,
                };
            }
            return null;
        }

        private HttpClient CreateClient()
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(Windows NT 10.0; Win64; x64)"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AppleWebKit", "537.36"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(KHTML, like Gecko)"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "106.0.0.0"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Safari", "537.36"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private record AchievementCategory
        {
            public string name = default!;
            public int order;
            public List<JToken> achievements = default!;
        }
    }
}
