using AmmoStatsInNames.Helpers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.Locales;
using System.Text.RegularExpressions;

namespace AmmoStatsInNames.CustomClasses;

[Injectable(InjectionType.Singleton)]
public class CustomLocales(
    CustomLogger logger,
    LocaleTable localeTable,
    LocaleService localeService
)
{
    public readonly Dictionary<string, Dictionary<string, string>> NewLocale = [];

    private readonly Dictionary<string, string> temp = [];

    private readonly Dictionary<string, Dictionary<string, string>> OriginalLocale = [];
    private HashSet<string> AllLangs { get; set; } = [];
    public void Initialize()
    {
        NewLocale.Clear();
        OriginalLocale.Clear();
        AllLangs = [.. localeTable.Global.Keys];

        foreach (var lang in AllLangs)
        {
            OriginalLocale.Add(lang, localeService.GetLocaleDb(lang));
            NewLocale.Add(lang, []);
        }
    }

    public void RegisterTag(string key, string value)
    {
        temp[key] = value;
    }
    public bool KeyExistsInDefaultLang(string key)
    {
        if (TryGetLocaleText("en", key) is not null) { return true; } else return false;
    }

    public bool DefaultLangLocaleContainsText(string key, string value)
    {
        NewLocale.TryGetValue("en", out var language);
        if (language != null)
        {
            language.TryGetValue(key, out var text);
            if (text != null)
            {
                return text.Contains(value);
            }
        }
        return false;
    }

    public void AddLocale(string localeKey, string key, bool clean = false)
    {
        foreach (var (lang, newLang) in NewLocale)
        {
            var text = TryGetLocaleText(lang, key);
            text ??= key;
            text = ReplaceTags(text, lang);
            newLang[localeKey] = clean ? StripHtml(text) : text;
        }
    }

    private string ReplaceTags(string text, string lang)
    {
        while (true)
        {
            var tags = ExtractTags(text);
            if (tags.Count == 0)
                break;

            foreach (var tag in tags)
            {
                var tagText = TryGetLocaleText(lang, tag);

                if (tagText is null)
                {
                    logger.Debug($"Tag not found: {tag}, Language: {lang}");
                    tagText = tag;
                }

                text = text.Replace($"{{{tag}}}", tagText);
            }
        }

        return text;
    }

    public void AddToExistingLocale(string localeKey, string key, bool clean = false)
    {
        AddLocale(localeKey, $"{{{localeKey}}}{key}", clean);
    }

    public void RegisterLocales()
    {
        foreach (var langId in AllLangs)
        {
            if (localeTable is not null && localeTable.Global.TryGetValue(langId, out var lazyloadedValue))
            {
                NewLocale.TryGetValue(langId, out var newLocaleToAdd);
                if (newLocaleToAdd is null)
                    NewLocale.TryGetValue("en", out newLocaleToAdd);

                if (newLocaleToAdd is null) continue;

                lazyloadedValue.AddTransformer(lazyloadedLocaleData =>
                {
                    if (lazyloadedLocaleData is null) return lazyloadedLocaleData;
                    foreach (var (key, value) in newLocaleToAdd)
                    {
                        lazyloadedLocaleData[key] = value;
                    }
                    return lazyloadedLocaleData;
                });
            }
        }
    }

    private string? TryGetLocaleText(string lang, string key)
    {
        var sources = new[]
        {
            temp,
            NewLocale.GetValueOrDefault(lang),
            OriginalLocale.GetValueOrDefault(lang),
            NewLocale.GetValueOrDefault("en"),
            OriginalLocale.GetValueOrDefault("en")
        };

        foreach (var source in sources)
        {
            if (source != null && source.TryGetValue(key, out var text) && text.Length > 0)
                return text;
        }

        return null;
    }
    public static List<string> ExtractTags(string input)
    {
        if (string.IsNullOrEmpty(input))
            return new List<string>();

        var matches = Regex.Matches(input, @"{([^{}]+)}");

        return matches
            .Select(m => m.Groups[1].Value.Trim())
            .Where(tag => tag.Length > 0)
            .Distinct()
            .ToList();
    }
    private static string StripHtml(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return Regex.Replace(input, "<.*?>", string.Empty);
    }
}
