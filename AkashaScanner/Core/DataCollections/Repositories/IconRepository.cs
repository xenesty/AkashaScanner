using System.Text.RegularExpressions;

namespace AkashaScanner.Core.DataCollections.Repositories
{
    public static class IconRepository
    {
        private static readonly string BasePath = Path.Combine("GenshinDatabase", "Icons");

        public static string GetPath(string type, string name, string ext)
        {
            return Path.Combine(BasePath, type, Regex.Replace(name, @"\W", string.Empty) + ext);
        }

        public static async Task SaveUrlAsIcon(HttpClient client, string iconUrl, string dest)
        {
            if (File.Exists(dest)) return;
            var resp = await client.GetAsync(iconUrl);
            resp.EnsureSuccessStatusCode();
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            using var fs = File.OpenWrite(dest);
            await resp.Content.CopyToAsync(fs);
        }
    }
}
