using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;

namespace AkashaScanner.Core
{
    public static class AppUpdate
    {
        private const string LatestReleaseApiUrl = "https://api.github.com/repos/xenesty/AkashaScanner/releases/latest";
        private static readonly Version AppVersion = Assembly.GetExecutingAssembly().GetName().Version!;

        public static async Task<string?> Check()
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("curl", "7.75.0"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            var resp = await client.GetAsync(LatestReleaseApiUrl);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return null;
            var data = JsonConvert.DeserializeObject<Response>(body)!;
            var tag = data.tag_name;
            if (tag[0] != 'v')
                return null;

            var latestVersion = Version.Parse(tag[1..]);
            if (latestVersion > AppVersion)
            {
                return data.html_url;
            }
            return null;
        }

        private record Response
        {
            public string tag_name = string.Empty;
            public string html_url = string.Empty;
        }
    }
}
