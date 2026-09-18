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

            var size = definition.InventorySize;
            var offsets = new List<Vector2Int>(size.x * size.y);
            for (var y = 0; y < size.y; y++)
            {
                for (var x = 0; x < size.x; x++)
                    offsets.Add(new Vector2Int(x, y));
            }

            _shape = new OffsetPlacementShape(offsets);
        }

        public PlayerItemInstanceState State { get; }
        public ItemDefinition Definition { get; }
        public string ItemId => Definition.IsStackable ? Definition.itemId : State.InstanceId;
        public Sprite Icon => Definition.GetIcon(State);
        public string DisplayName => Definition.GetDisplayName(State);
        public int MaxStackSize => Definition.IsStackable ? Definition.maxStackSize : 1;
        public IPlacementShape PlacementShape => _shape;
        public ItemRarity Rarity => State.HasRolledData ? State.Rarity : Definition.rarity;
        public BigDouble SellPrice => State.HasRolledData ? State.SellPrice : Definition.sellPrice;
        public IReadOnlyList<ItemStatState> Stats => State.HasRolledData ? State.Stats : Definition.CopyDefaultStats();
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
                    text.AppendLine(EquipmentStats.RarityName(Rarity));
                var description = Definition.GetDescription();
                if (!string.IsNullOrEmpty(description))
                    text.AppendLine(description);
                if (Definition.category == ItemCategory.Consumable)
                    text.AppendLine(ItemText.Get("potion.use.inventory_hint"));

                foreach (var stat in Stats)
                    if (stat != null)
                        text.AppendLine($"{EquipmentStats.Name(stat.StatId)}: {EquipmentStats.FormatPercent(stat.Value)}");

                if (Definition.category == ItemCategory.Armor)
                    text.AppendLine(ItemText.Get("equipment.inventory_hint"));

                text.Append(ItemText.Get("item.sell_price", BigDoubleFormatter.Format(SellPrice)));
                return text.ToString();
            }
        }

    }
}
