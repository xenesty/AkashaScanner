using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics.CodeAnalysis;

namespace AkashaScanner.Core.DataFiles
{
    public record DataFile<T> : IDataFile<T>
    {
        private static readonly JsonConverter stringEnumConverter = new StringEnumConverter();
        private readonly string FilePath;
        private readonly DateTime CreatedAt;
        private readonly int Count;
        private readonly int ExpectedVersion;

        DateTime IDataFile<T>.CreatedAt => CreatedAt;
        int IDataFile<T>.Count => Count;

        public DataFile(string filePath, DateTime createdAt, int count, int expectedVersion)
        {
            FilePath = filePath;
            CreatedAt = createdAt;
            Count = count;
            ExpectedVersion = expectedVersion;
        }

        public bool Exists()
        {
            return File.Exists(FilePath);
        }

        public bool Read([MaybeNullWhen(false)] out T data)
        {
            var text = File.ReadAllText(FilePath);
            var content = JsonConvert.DeserializeObject<Content>(text, stringEnumConverter)!;
            if (content.Version == ExpectedVersion)
            {
                data = content.Data;
                return true;
            }
            else
            {
                data = default!;
                return false;
            }

        }

        public void Write(T value)
        {
            var content = new Content() { Data = value, Version = ExpectedVersion };
            var text = JsonConvert.SerializeObject(content, Formatting.Indented, stringEnumConverter);
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, text);
        }

        private record Content
        {
            public int Version;
            public T Data = default!;
        }
    }
}
