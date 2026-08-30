using System;
using UnityEngine;
using Utils;

namespace Core.Items
{
    public enum ItemCategory
    {
        Misc,
        Armor,
        Consumable,
        Scroll
    }

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
        public string statId;
        public float value;
    }

    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "RPG/Items/Item Definition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;
        public string displayName;
        [TextArea] public string description;
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

        [Header("Consumable")]
        public string effectId;
        public float effectValue;

        [Header("Scroll")]
        public string forgeModifierId;
        public float forgeModifierValue;

        private void OnValidate()
        {
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
