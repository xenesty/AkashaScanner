using System.Diagnostics.CodeAnalysis;

namespace AkashaScanner.Core.DataFiles
{
    public interface IDataFile<T>
    {
        DateTime CreatedAt { get; }
        int Count { get; }
        bool Exists();
        bool Read([MaybeNullWhen(false)] out T data);
        void Write(T value);
    }
}
