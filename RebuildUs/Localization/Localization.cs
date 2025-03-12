using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using GameCore;

namespace RebuildUs.Localization;

internal static class LocalizationManager
{
    private static readonly Dictionary<string, Dictionary<SupportedLangs, string>> Translations = [];

    internal static void Initialize()
    {
        for (var i = SupportedLangs.English; i <= SupportedLangs.Irish; i++)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"RebuildUs.Localization.Translations.{i}.json");
            var dic = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);

            foreach (var (key, value) in dic)
            {
                if (Translations[key] is null)
                {
                    Translations[key] = [];
                }

                Translations[key][i] = value;
            }
        }
    }

    internal static string GetString(string key, params string[] args)
    {
        var lang = TranslationController.InstanceExists ? TranslationController.Instance.currentLanguage.languageID : SupportedLangs.English;

        if (!Translations.TryGetValue(key, out var langDic))
        {
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