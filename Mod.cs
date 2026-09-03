using AmmoStatsInNames.CustomClasses;
using AmmoStatsInNames.Helpers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace AmmoStatsInNames;

[Injectable(TypePriority = OnLoadOrder.PresetCallbacks + 5)]
public class AmmoStatsInNamesMod(
    ItemHelper itemHelper,
    TemplateTable templateTable,
    CustomLocales customLocales,
    BulletNamesHelper bulletNamesHelper,
    CustomLogger logger
) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        customLocales.Initialize();
        bulletNamesHelper.ResolveConfigNaming();

        var allAmmoItems = itemHelper.GetItemTplsOfBaseType("5485a8684bdc2da71d8b4567").ToArray();
        var bulletTypes = new HashSet<string>() { "bullet", "buckshot", "grenade" };
        foreach (var itemId in allAmmoItems)
        {
            cancellationToken.ThrowIfCancellationRequested();
            templateTable.Items.TryGetValue(itemId, out var item);
            if (item is null || item.Properties is null) continue;

            if (item.Properties.AmmoType is not { } ammoType || !bulletTypes.Contains(ammoType)) continue;
            bulletNamesHelper.AddStatsToItem(item);
        }

        var allAmmoBoxes = itemHelper.GetItemTplsOfBaseType("543be5cb4bdc2deb348b4568").ToArray();
        foreach (var itemId in allAmmoBoxes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            templateTable.Items.TryGetValue(itemId, out var item);
            if (item is null || item.Properties is null) continue;

            if (item.Properties.StackSlots?.Count() != 1) continue;

            var bulletId = item.Properties.StackSlots?.FirstOrDefault()?.Properties?.Filters?.FirstOrDefault()?.Filter?.FirstOrDefault();
            if (bulletId == null) continue;

            templateTable.Items.TryGetValue((MongoId)bulletId, out var bullet);
            if (bullet is null || bullet.Properties is null) continue;

            bulletNamesHelper.AddStatsToItem(item, bullet);
        }
        customLocales.RegisterLocales();

        logger.Ok($"Modified {bulletNamesHelper.ItemsModified} ammo and ammobox names");
        return Task.CompletedTask;
    }
}
