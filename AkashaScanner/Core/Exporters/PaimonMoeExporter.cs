using AkashaScanner.Core.DataFiles;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace AkashaScanner.Core.Exporters
{
    public static class PaimonMoeExporter
    {
        public static bool Export(
            [MaybeNullWhen(false)] out string output,
            IDataFile<AchievementOutput> achievementFile,
            string Import,
            IDataFile<CharacterOutput>? characterFile = null)
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
            foreach (var (id, catId) in data)
            {
                var idStr = id.ToString();
                var catIdStr = catId.ToString();
                if (!dict.ContainsKey(catIdStr))
                {
                    dict[catIdStr] = new JObject();
                }
                ((JObject)dict[catIdStr]!)[idStr] = true;
            }
            obj["achievement"] = dict;

            if (characterFile != null && characterFile.Read(out var characterData) && characterData.Count > 0)
            {
                if (!obj.TryGetValue("characters", out var charsToken) || charsToken is not JObject chars)
                {
                    chars = new();
                    obj["characters"] = chars;
                }
                foreach (var character in characterData)
                {
                    var name = SnakeCase(character.Name);
                    if (!chars.TryGetValue(name, out var charToken) || charToken is not JObject charOb)
                    {
                        charOb = new();
                        chars[name] = charOb;
                        charOb["default"] = 0;
                        charOb["wish"] = 0;
                        charOb["manual"] = character.Constellation + 1;
                    }
                    else
                    {
                        var manual = character.Constellation + 1;
                        if (charOb.TryGetValue("default", out var defaultConsToken))
                        {
                            manual -= defaultConsToken.ToObject<int>();
                        }
                        if (charOb.TryGetValue("wish", out var wishConsToken))
                        {
                            manual -= wishConsToken.ToObject<int>();
                        }
                        charOb["manual"] = manual;
                    }
                }
            }

            output = obj.ToString();
            return true;
        }

        private static string SnakeCase(string input)
        {
            return Regex.Replace(input.ToLower(), @"\W+", "_").Trim('_');
        }
    }
}
