using System;
using UnityEngine;
using UnityEngine.Localization;
using Utils;

namespace Core.Items
{
    public enum ItemCategory
    {
        Misc,
        Armor,
        Consumable,
        Scroll,
        Currency
    }

    public enum RewardCurrency { Gold, Shards }

    public enum ItemRarity
    {
        Common,
        Rare,
        Unique,
        Legendary
    }

    public enum EquipmentSlot
    {
        None,
        Helmet,
        Chest,
        Weapon,
        Boots
    }

    [Serializable]
    public sealed class ItemStatDefinition
    {
        [EquipmentStatId] public string statId;
        [Tooltip("Percentage bonus as a fraction: 0.10 = +10%. Same stat on equipped items is added together.")]
        public float value;
    }

    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "RPG/Items/Item Definition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;
        public string displayName;
        [TextArea] public string description;
        public LocalizedString localizedName = new();
        public LocalizedString localizedDescription = new();
        public Sprite icon;
        public ItemCategory category;

        [Header("Inventory")]
        public bool stackable;
        [Min(1)] public int maxStackSize = 1;
        [Min(1)] public int width = 1;
        [Min(1)] public int height = 1;
        public BigDouble sellPrice = BigDouble.Zero;

        [Header("Armor")]
        public ItemRarity rarity;
        public EquipmentSlot equipmentSlot;
        public ItemStatDefinition[] defaultStats = Array.Empty<ItemStatDefinition>();

        [Header("Forge appearance (chosen independently)")]
        public string[] forgeNameKeys = Array.Empty<string>();
        public Sprite[] forgeIcons = Array.Empty<Sprite>();

        [Header("Consumable")]
        public string effectId;
        public float effectValue;

        [Header("Scroll")]
        public string forgeModifierId;
        public float forgeModifierValue;

        [Header("Currency reward")]
        public RewardCurrency currency;

        public Vector2Int InventorySize => category == ItemCategory.Armor ? equipmentSlot switch
        {
            EquipmentSlot.Helmet => Vector2Int.one,
            EquipmentSlot.Chest => new Vector2Int(2, 2),
            EquipmentSlot.Weapon => new Vector2Int(1, 2),
            _ => new Vector2Int(width, height)
        } : new Vector2Int(width, height);

        public bool IsStackable => category != ItemCategory.Armor && stackable;

        public System.Collections.Generic.List<ItemStatState> CopyDefaultStats()
        {
            var result = new System.Collections.Generic.List<ItemStatState>();
            foreach (var stat in defaultStats ?? Array.Empty<ItemStatDefinition>())
                result.Add(stat == null ? null : new ItemStatState { StatId = stat.statId, Value = stat.value });
            return result;
        }

        public string GetDisplayName() => localizedName == null || localizedName.IsEmpty
            ? displayName : localizedName.GetLocalizedString();

        public string GetDisplayName(PlayerItemInstanceState item) => string.IsNullOrEmpty(item?.RolledNameKey)
            ? GetDisplayName() : ItemText.Get(item.RolledNameKey);

        public Sprite GetIcon(PlayerItemInstanceState item) => item != null && item.RolledIconIndex >= 0 &&
            item.RolledIconIndex < forgeIcons.Length && forgeIcons[item.RolledIconIndex] != null
                ? forgeIcons[item.RolledIconIndex] : icon;

        public string GetDescription(float? value = null) => localizedDescription == null || localizedDescription.IsEmpty
            ? description : localizedDescription.GetLocalizedString(value ??
                (category == ItemCategory.Scroll ? forgeModifierValue : effectValue));

        private void OnValidate()
        {
            if (category == ItemCategory.Armor)
            {
                stackable = false;
                var size = InventorySize;
                width = size.x;
                height = size.y;
                if (!EquipmentStats.IsSupportedSlot(equipmentSlot))
                    Debug.LogWarning($"[{name}] Armor requires Helmet, Chest or Weapon slot.", this);
                if (!EquipmentStats.Validate(CopyDefaultStats(), out var error))
                    Debug.LogWarning($"[{name}] {error}", this);
            }
            if (!stackable)
                maxStackSize = 1;
            else
            {
                maxStackSize = Mathf.Max(1, maxStackSize);
                width = 1;
                height = 1;
            }

            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
        }
    }
}
