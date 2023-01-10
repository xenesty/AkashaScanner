using AkashaScanner.Core.DataFiles;
using Microsoft.Extensions.Logging;

namespace AkashaScanner.Core.ResultHandler
{
    public abstract class ListResultHandler<R, U> : IResultHandler<R> where U : List<R>, new()
    {
        protected IDataFileRepository<U> DataFileRepository { get; init; } = default!;
        protected ILogger Logger { get; init; } = default!;
        protected readonly List<(int, R)> Items = new();

        public void Init()
        {
            Items.Clear();
        }

        public void Add(R item, int order)
        {
            Items.Add((order, item));
        }

        public void Save()
        {
            Items.Sort();
            var file = DataFileRepository.Create(Items.Count);
            U output = new();
            output.AddRange(Items.Select(t => t.Item2));
            file.Write(output);
        }
    }
}
