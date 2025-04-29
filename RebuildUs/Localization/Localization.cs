using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using RebuildUs.Utilities;

namespace RebuildUs.Localization;

public static class Tr
{
    private static readonly Dictionary<string, Dictionary<SupportedLangs, string>> Translations = [];

    public static void Initialize()
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

    public static string Get(string key, params string[] args)
    {
        var lang = TranslationController.InstanceExists ? FastDestroyableSingleton<TranslationController>.Instance.currentLanguage.languageID : SupportedLangs.English;

        if (!Translations.TryGetValue(key, out var langDic))
        {
            Plugin.Instance.Logger.LogWarning($"There are no translation data. key: {key}");
            return key;
        }

        if (!langDic.TryGetValue(lang, out var str))
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