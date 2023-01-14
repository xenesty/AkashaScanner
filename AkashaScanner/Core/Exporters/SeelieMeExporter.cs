using AkashaScanner.Core.DataFiles;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;

namespace AkashaScanner.Core.Exporters
{
    public static class SeelieMeExporter
    {
        public static bool Export(
            [MaybeNullWhen(false)] out string output,
            IDataFile<AchievementOutput> achievementFile,
            string Import)
        {
            JObject obj = new();
            if (!string.IsNullOrEmpty(Import))
            {
                var parsed = JToken.Parse(Import);
                if (parsed is JObject ob)
                {
                    obj = ob;
                }
            }
            if (!achievementFile.Read(out var data))
            {
                output = null;
                return false;
            }
            JObject dict = new();
            foreach (var (id, _) in data)
            {
                dict[id.ToString()] = new JObject
                {
                    ["done"] = true
                };
            }
            obj["achievements"] = dict;
            output = obj.ToString();
            return true;
        }
    }
}
