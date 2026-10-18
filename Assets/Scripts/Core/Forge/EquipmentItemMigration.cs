using System;
using System.Linq;
using Core.Items;

namespace Core.Forge
{
    public static class EquipmentItemMigration
    {
        public static bool Migrate(PlayerItemInstanceState item, ItemDefinition definition, ForgeConfig config,
            bool assignAppearance = true)
        {
            if (item == null || !string.IsNullOrEmpty(item.AppearanceId) || definition == null ||
                definition.category != ItemCategory.Armor || !EquipmentStats.IsSupportedSlot(definition.equipmentSlot) ||
                definition.appearanceCatalog == null || !Enum.IsDefined(typeof(ItemRarity), item.Rarity)) return false;

            var budget = config.GetLevel(1).statBudgets[(int)item.Rarity];
            // Unrolled prototypes have no saved bonus types to preserve.
            item.Stats = item.Stats == null || item.Stats.Count == 0
                ? ForgeStatRoller.Roll(budget, (int)item.Rarity + 2)
                : ForgeStatRoller.Rebalance(item.Stats, budget);
            item.HasRolledData = true;
            item.SellPrice = budget * (double)config.goldPerBonusPercent;
            item.RolledIconIndex = -1;
            item.RolledNameKey = null;
            if (assignAppearance)
            {
                var pool = definition.appearanceCatalog.appearances
                    .Where(a => a.slot == definition.equipmentSlot && a.rarity == item.Rarity)
                    .OrderBy(a => a.id, StringComparer.Ordinal).ToArray();
                // Stable across reloads, catalog ordering, platforms and unrelated random rolls.
                var hash = StableHash(item.InstanceId);
                var appearance = pool[hash % (uint)pool.Length];
                item.AppearanceId = appearance.id;
                item.RolledNameKey = appearance.nameKeys[(hash / (uint)pool.Length) % (uint)appearance.nameKeys.Length];
            }
            return true;
        }

        private static uint StableHash(string value)
        {
            var hash = 2166136261u;
            foreach (var c in value ?? string.Empty) hash = unchecked((hash ^ c) * 16777619u);
            return hash;
        }
    }
}
