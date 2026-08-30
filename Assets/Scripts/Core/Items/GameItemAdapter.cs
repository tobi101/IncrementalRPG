using System.Collections.Generic;
using System.Text;
using UDND.Core;
using UnityEngine;
using Utils;

namespace Core.Items
{
    public sealed class GameItemAdapter : IItemAdapter, IStackSizeLimitable,
        IItemPlacementShapeProvider, IDescribable, IFilterable, ISortable
    {
        private readonly OffsetPlacementShape _shape;

        public GameItemAdapter(PlayerItemInstanceState state, ItemDefinition definition)
        {
            State = state;
            Definition = definition;

            var offsets = new List<Vector2Int>(definition.width * definition.height);
            for (var y = 0; y < definition.height; y++)
            {
                for (var x = 0; x < definition.width; x++)
                    offsets.Add(new Vector2Int(x, y));
            }

            _shape = new OffsetPlacementShape(offsets);
        }

        public PlayerItemInstanceState State { get; }
        public ItemDefinition Definition { get; }
        public string ItemId => Definition.stackable ? Definition.itemId : State.InstanceId;
        public Sprite Icon => Definition.icon;
        public string DisplayName => Definition.displayName;
        public int MaxStackSize => Definition.stackable ? Definition.maxStackSize : 1;
        public IPlacementShape PlacementShape => _shape;
        public ItemRarity Rarity => State.HasRolledData ? State.Rarity : Definition.rarity;
        public BigDouble SellPrice => State.HasRolledData ? State.SellPrice : Definition.sellPrice;
        public IReadOnlyList<ItemStatState> Stats => State.HasRolledData ? State.Stats : GetDefaultStats();
        public string Category => Definition.category.ToString();
        public string Subcategory => Definition.equipmentSlot.ToString();
        int IFilterable.Rarity => (int)Rarity;
        public int SortValue => (int)Rarity;
        public string SortName => DisplayName;

        public string Description
        {
            get
            {
                var text = new StringBuilder();
                if (Definition.category == ItemCategory.Armor)
                    text.AppendLine(GetRarityName(Rarity));
                if (!string.IsNullOrEmpty(Definition.description))
                    text.AppendLine(Definition.description);
                if (Definition.category == ItemCategory.Consumable)
                    text.AppendLine("ПКМ — использовать");

                foreach (var stat in Stats)
                    text.AppendLine($"{stat.StatId}: {stat.Value:+0.##;-0.##;0}");

                text.Append($"Цена продажи: {BigDoubleFormatter.Format(SellPrice)}");
                return text.ToString();
            }
        }

        private IReadOnlyList<ItemStatState> GetDefaultStats()
        {
            var stats = new List<ItemStatState>(Definition.defaultStats.Length);
            foreach (var stat in Definition.defaultStats)
            {
                stats.Add(new ItemStatState
                {
                    StatId = stat.statId,
                    Value = stat.value
                });
            }

            return stats;
        }

        private static string GetRarityName(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => "Обычная",
                ItemRarity.Rare => "Редкая",
                ItemRarity.Unique => "Уникальная",
                ItemRarity.Legendary => "Легендарная",
                _ => string.Empty
            };
        }
    }
}
