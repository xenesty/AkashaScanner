using AkashaScanner.Core.DataCollections.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AkashaScanner.Core.DataCollections
{
    public abstract class BaseCollection<D> : IDataCollection where D : class
    {
        private static readonly JsonConverter stringEnumConverter = new StringEnumConverter();
        private static readonly string BaseDir = Path.Combine(Utils.ExecutableDirectory, "GenshinDatabase");

        protected abstract int CurrentVersion { get; }
        protected ILogger Logger { get; init; } = default!;
        protected IRepository<D> Repository { get; init; } = default!;

        private CollectionData? _data = null;

        protected CollectionData Data
        {
            get => _data ?? throw new ArgumentNullException("Database has not been loaded yet");
            set
            {
                _data = value;
            }
        }

        protected abstract string LocalFile { get; }

        private string LocalPath { get => Path.Combine(BaseDir, LocalFile); }

        public DateTime GetLastUpdate()
        {
            return Data.LastUpdate;
        }

        public bool IsLoaded()
        {
            return _data != null;
        }

        public bool HasLocal()
        {
            return File.Exists(LocalPath);
        }

        public virtual async Task LoadLocal()
        {
            Logger.LogDebug("Loading from local database.");
            try
            {
                var text = await File.ReadAllTextAsync(LocalPath);
                var data = JsonConvert.DeserializeObject<CollectionData>(text, stringEnumConverter)!;
                if (data.Version == CurrentVersion)
                {
                    Data = data;
                    Logger.LogDebug("Local database loaded.");
                }
                else
                {
                    Logger.LogWarning("Local database outdated.");
                }
                return;
            }
            catch (DirectoryNotFoundException) { }
            catch (FileNotFoundException) { }
            Logger.LogWarning("Local database not found.");
        }

        public virtual async Task LoadRemote()
        {
            var output = await Repository.Load();
            if (output == null) return;
            var data = new CollectionData()
            {
                Version = CurrentVersion,
                LastUpdate = DateTime.Now,
                Data = output,
            };
            Data = data;
            WriteLocal(data);
        }

        private void WriteLocal(CollectionData data)
        {
            Logger.LogInformation("Writing to local database.");
            Directory.CreateDirectory(BaseDir);
            var text = JsonConvert.SerializeObject(data, Formatting.Indented, stringEnumConverter);
            File.WriteAllText(LocalPath, text);
        }

        protected void NotifyFail(string text)
        {
            Logger.LogWarning("Fail to identify `{text}`", text);
        }

        protected void NotifySuccess(string text, string name, int score)
        {
            Logger.LogDebug("Identify `{text}` as `{result}` with {score}/100 confidence", text, name, score);
        }

        public class CollectionData
        {
            public int Version = 1;
            public DateTime LastUpdate = DateTime.MinValue;
            public D Data = default!;
        }
    }
}
