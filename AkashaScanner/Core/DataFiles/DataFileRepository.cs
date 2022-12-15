using System.Globalization;

namespace AkashaScanner.Core.DataFiles
{
    public class DataFileRepository<T> : IDataFileRepository<T>
    {
        private static readonly string OutputDir = Path.Combine(Utils.ExecutableDirectory, "ScannedData");
        private const int CountLength = 6;
        private const string TimeFormat = "yyyyMMddHHmmss";
        private const string Extension = ".json";
        private readonly string Prefix;
        private readonly string Pattern;
        public virtual int CurrentVersion => 1;

        public DataFileRepository(string prefix)
        {
            Prefix = prefix;
            Pattern = $"{prefix}_{new string('?', TimeFormat.Length)}_{new string('?', CountLength)}{Extension}";
        }

        public IDataFile<T> Create(int count)
        {
            var now = DateTime.Now;
            var time = now.ToString(TimeFormat);
            var fileName = $"{Prefix}_{time}_{count:D6}{Extension}";
            return new DataFile<T>(Path.Combine(OutputDir, fileName), now, count, CurrentVersion);
        }

        public List<IDataFile<T>> List()
        {
            var info = new DirectoryInfo(OutputDir);
            if (!info.Exists)
            {
                return new();
            }
            var list = info.GetFiles(Pattern).Select(info => GetFileFromName(info.FullName)).ToList();
            list.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
            return list;
        }

        private IDataFile<T> GetFileFromName(string name)
        {
            var timeStr = name[^(TimeFormat.Length + CountLength + 1 + Extension.Length)..^(CountLength + 1 + Extension.Length)];
            var time = DateTime.ParseExact(timeStr, TimeFormat, CultureInfo.InvariantCulture);
            var countStr = name[^(Extension.Length + CountLength)..^Extension.Length];
            var count = int.Parse(countStr);
            return new DataFile<T>(name, time, count, CurrentVersion);
        }
    }
}
