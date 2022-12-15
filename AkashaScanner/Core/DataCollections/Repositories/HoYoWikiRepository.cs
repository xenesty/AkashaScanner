using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace AkashaScanner.Core.DataCollections.Repositories
{
    public abstract class HoYoWikiRepository<E> : IRepository<List<E>> where E : class, IEntry
    {
        private const string EntryListUrl = "https://sg-wiki-api.hoyolab.com/hoyowiki/wapi/get_entry_page_list";
        private const string EntryUrl = "https://sg-wiki-api-static.hoyolab.com/hoyowiki/wapi/entry_page?entry_page_id=";
        private static readonly List<string> EmptyList = new();
        private const int PageSize = 30;

        protected ILogger Logger { get; init; } = default!;
        public abstract Task<List<E>?> Load();

        protected Task<List<T>?> LoadEntryPageList<T>(HttpClient client, string MenuId) => LoadEntryPageList<T>(client, MenuId, EmptyList);

        protected async Task<List<T>?> LoadEntryPageList<T>(HttpClient client, string MenuId, List<string> Filters)
        {
            var req = new GetEntryListRequest()
            {
                menu_id = MenuId,
                filters = Filters,
            };
            var resp = await InternalLoadList<T>(client, req);
            if (resp == null) return null;
            var data = resp.data;
            var output = data.list;
            if (!int.TryParse(data.total, out int total)) return output;
            for (int i = 2; total > output.Count; ++i)
            {
                resp = await InternalLoadList<T>(client, req with { page_num = i });
                if (resp == null) return null;
                var list = resp.data.list;
                output.AddRange(list);
            }
            return output;
        }

        private async Task<ListResponse<T>?> InternalLoadList<T>(HttpClient client, GetEntryListRequest reqBody)
        {
            var requestBody = JsonConvert.SerializeObject(reqBody);

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            using HttpRequestMessage request = new()
            {
                RequestUri = new Uri(EntryListUrl),
                Method = HttpMethod.Post,
                Content = content,
            };
            var resp = await client.SendAsync(request);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ListResponse<T>>(body)!;
        }

        protected static async Task<T?> LoadEntryPage<T>(HttpClient client, string entryId)
        {
            var resp = await client.GetAsync(EntryUrl + entryId);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<EntryResponse<T>>(body)!;
            return data.data.page;
        }

        protected HttpClient CreateClient()
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(Windows NT 10.0; Win64; x64)"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AppleWebKit", "537.36"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(KHTML, like Gecko)"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "106.0.0.0"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Safari", "537.36"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US"));
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en", 0.5));
            client.DefaultRequestHeaders.Referrer = new Uri("https://wiki.hoyolab.com/");
            client.DefaultRequestHeaders.Add("x-rpc-language", "en-us");
            client.DefaultRequestHeaders.Add("cookie", "mi18nLang=en-us");
            return client;
        }

        private record GetEntryListRequest
        {
            public List<string> filters = new();
            public string menu_id = default!;
            public int page_num = 1;
            public int page_size = PageSize;
            public bool use_es = true;
        }
        private record ListData<T>
        {
            public List<T> list = default!;
            public string total = default!;
        }
        private record ListResponse<T>
        {
            public ListData<T> data = default!;
        }
        private record EntryResponse<T>
        {
            public EntryData<T> data = default!;
        }

        private record EntryData<T>
        {
            public T page = default!;
        }
    }
}
