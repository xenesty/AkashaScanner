using FuzzySharp;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AkashaScanner.Core
{
    public static class Utils
    {
        public static readonly string ExecutableDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        public static readonly Version AppVersion = Assembly.GetExecutingAssembly().GetName().Version!;

        public static void WaitFor(this Task task, int ms)
        {
            Task.WhenAll(task, Task.Delay(ms)).Wait();
        }

        public static async Task<T> WaitFor<T>(this Task<T> task, int ms)
        {
            await Task.WhenAll(task, Task.Delay(ms));
            var result = await task;
            return result;
        }

        public static bool TryParseFraction(this string text, out (int, int) output)
        {
            text = Regex.Replace(text, @":3", "8");
            text = Regex.Replace(text, @"\(\)", "0");
            text = Regex.Replace(text, @"\&", "8");
            text = Regex.Replace(text, @"A", "4");
            text = Regex.Replace(text, @"\s*/\s*", "/");
            var words = text.Split();
            foreach (var word in words)
            {
                if (word.Contains('/'))
                {
                    var t = Regex.Replace(word, @"[^0-9/]+", string.Empty);
                    var parts = t.Split('/', 2);
                    if (parts.Length == 2 && int.TryParse(parts[0], out int fst) && int.TryParse(parts[1], out int snd))
                    {
                        output = (fst, snd);
                        return true;
                    }
                }
            }
            output = default;
            return false;
        }

        public static (int, T?) Search<T>(this ICollection<T> list, Func<T, int> getScore) where T : class
        {
            if (list.Count == 0)
                return (0, null);

            var highestScore = 0;
            T highestItem = default!;
            foreach (var item in list)
            {
                var score = getScore(item);
                if (score > highestScore)
                {
                    highestScore = score;
                    highestItem = item;
                }
            }
            return (highestScore, highestItem);
        }

        public static (int, string?) FuzzySearch(this string value, ICollection<string> list)
        {
            if (list.Contains(value))
                return (100, value);

            return list.Search((item) => value.FuzzySearch(item));
        }

        public static int FuzzySearch(this string value, string target)
        {
            return Fuzz.Ratio(value, target);
        }

        public static (int, string?) PartialSearch(this string value, ICollection<string> list)
        {
            if (list.Contains(value))
                return (100, value);

            return list.Search((item) => value.PartialSearch(item));
        }

        public static int PartialSearch(this string value, string target)
        {
            return Fuzz.PartialRatio(value, target);
        }
    }
}
