using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Items
{
    [Serializable]
    public sealed class EquipmentAppearance
    {
        public string id;
        public EquipmentSlot slot;
        public ItemRarity rarity;
        public Sprite icon;
        public string[] nameKeys = Array.Empty<string>();
    }

    [CreateAssetMenu(fileName = "EquipmentAppearances", menuName = "RPG/Items/Equipment Appearances")]
    public sealed class EquipmentAppearanceCatalog : ScriptableObject
    {
        public EquipmentAppearance[] appearances = Array.Empty<EquipmentAppearance>();

        public bool TryGet(string id, out EquipmentAppearance appearance)
        {
            appearance = null;
            if (string.IsNullOrEmpty(id)) return false;
            foreach (var entry in appearances)
                if (entry != null && entry.id == id) { appearance = entry; return true; }
            return false;
        }

        public EquipmentAppearance Roll(EquipmentSlot slot, ItemRarity rarity)
        {
            EquipmentAppearance selected = null;
            var count = 0;
            foreach (var entry in appearances)
                if (entry.slot == slot && entry.rarity == rarity && UnityEngine.Random.Range(0, ++count) == 0)
                    selected = entry;
            return selected ?? throw new InvalidOperationException($"No appearance for {slot}/{rarity}.");
        }

        public bool IsValid()
        {
            if (appearances == null || appearances.Length == 0) return false;
            var ids = new HashSet<string>();
            var names = new HashSet<string>();
            var pools = new HashSet<(EquipmentSlot, ItemRarity)>();
            foreach (var entry in appearances)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.id) || !ids.Add(entry.id) || entry.icon == null ||
                    !EquipmentStats.IsSupportedSlot(entry.slot) || !Enum.IsDefined(typeof(ItemRarity), entry.rarity) ||
                    entry.nameKeys == null || entry.nameKeys.Length == 0) return false;
                foreach (var key in entry.nameKeys)
                    if (string.IsNullOrWhiteSpace(key) || !names.Add(key)) return false;
                pools.Add((entry.slot, entry.rarity));
            }
            return pools.Count == 12;
        }
    }
}
