using AmmoStatsInNames.CustomClasses;
using AmmoStatsInNames.DataStructures;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using System.Globalization;

namespace AmmoStatsInNames.Helpers;

[Injectable(InjectionType.Singleton)]
public class BulletNamesHelper(
    CustomLocales customLocales,
    ConfigData config,
    CustomLogger logger
)
{
    private readonly HashSet<string> invalidTags = [];

    public Dictionary<MongoId, string> CreatedStats { get; } = [];
    public Dictionary<string, string> CustomNames { get; } = [];
    public Dictionary<string, List<string>> Tags { get; } = [];
    public int ItemsModified { get; private set; }

    public void ResolveConfigNaming()
    {
        CustomNames["bullet"] = config.StatsToAdd ?? string.Empty;
        CustomNames["buckshot"] = config.StatsToAddBuckshot ?? string.Empty;
        CustomNames["grenade"] = config.StatsToAddGrenadeRound ?? string.Empty;
        foreach (var (type, name) in CustomNames)
        {
            Tags[type] = CustomLocales.ExtractTags(name);
        }
    }

    public void AddStatsToItem(TemplateItem item, TemplateItem? bullet = null)
    {
        if (bullet == null)
            bullet = item;
        else
        {
            CreatedStats.TryGetValue(bullet.Id, out var cachedStat);
            if (cachedStat is null) return;
            customLocales.AddLocale($"{item.Id} Name", config.StatsBeforeName ? $"{cachedStat} {{{item.Id} Name}}" : $"{{{item.Id} Name}} {cachedStat}");
            ItemsModified++;
            return;
        }

        if (bullet.Properties is not { } properties
            || properties.AmmoType is not { } ammoType
            || !CustomNames.TryGetValue(ammoType, out var statTemplate)
            || !Tags.TryGetValue(ammoType, out var tags))
            return;

        var createdStat = statTemplate;

        if (string.IsNullOrEmpty(bullet.Name)
            || bullet.Name.Contains("patron_rsp", StringComparison.OrdinalIgnoreCase)
            || bullet.Name.Contains("patron_26x75", StringComparison.OrdinalIgnoreCase))
            return;

        foreach (var tag in tags)
        {
            var property = properties.GetType().GetProperty(tag);
            if (property is null)
            {
                if (invalidTags.Add(tag))
                    logger.Warning($"Unknown ammo property in configuration: {tag}");
                continue;
            }

            var value = property.GetValue(properties);
            if (value == null) continue;
            if (tag is "PenetrationPower" or "Damage")
                value = Math.Round(Convert.ToDouble(value), MidpointRounding.AwayFromZero);

            var text = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (text is null) continue;
            createdStat = createdStat.Replace($"{{{tag}}}", text.PadLeft(Math.Max(config.PaddingLength, 0), '0'));
        }
        CreatedStats[bullet.Id] = createdStat;

        ItemsModified++;
        customLocales.AddLocale($"{item.Id} Name", config.StatsBeforeName ? $"{createdStat} {{{item.Id} Name}}" : $"{{{item.Id} Name}} {createdStat}");
    }
}
