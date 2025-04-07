using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace RebuildUs.Localization;

internal static class Tr
{
    private static readonly Dictionary<string, Dictionary<SupportedLangs, string>> Translations = [];

    internal static SupportedLangs CurrentLanguage = SupportedLangs.English;

    internal static void Initialize()
    {
        for (var i = SupportedLangs.English; i <= SupportedLangs.Irish; i++)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"RebuildUs.Localization.Translations.{i}.json");
            var dic = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);

            foreach (var (key, value) in dic)
            {
                if (!Translations.TryGetValue(key, out var trans))
                {
                    trans = [];
                    Translations[key] = trans;
                }

                trans[i] = value;
            }
        }
    }

    internal static string Get(string key, params string[] args)
    {
        if (!Translations.TryGetValue(key, out var langDic))
        {
            return key;
        }

        if (!langDic.TryGetValue(CurrentLanguage, out var str))
        {
            if (!langDic.TryGetValue(SupportedLangs.English, out var enStr))
            {
                return key;
            }

            return string.Format(enStr, args);
        }

        return string.Format(str, args);
    }
}