using UnityEngine.Localization;

namespace Core.Items
{
    public static class ItemText
    {
        public const string Table = "LocalesTable";

        public static string Get(string key, params object[] arguments) =>
            new LocalizedString(Table, key).GetLocalizedString(arguments);

        public static string UseResultKey(ConsumableUseResult result) => result switch
        {
            ConsumableUseResult.Success => string.Empty,
            ConsumableUseResult.FamilyOccupied => "potion.use.family_occupied",
            ConsumableUseResult.MissingItem => "potion.use.missing",
            _ => "potion.use.unsupported"
        };

        public static string EffectKey(string effectId) => effectId switch
        {
            RunConsumableService.Wealth => "potion.effect.wealth",
            RunConsumableService.Rage => "potion.effect.rage",
            RunConsumableService.Concentration => "potion.effect.concentration",
            _ => string.Empty
        };
    }
}
