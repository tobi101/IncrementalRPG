using Core.Items;
using Model;
using UDND.Core;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Inventory
{
    public sealed class InventoryRecycleDropArea : DropAreaBase
    {
        private Player _player;
        private Graphic _graphic;
        private Color _normalColor;

        public void Configure(Player player, Graphic graphic)
        {
            _player = player;
            _graphic = graphic;
            _normalColor = graphic.color;
        }

        protected override bool CanAcceptEntry(DragEntry entry)
        {
            return entry.Stack?.PrimaryAdapter is GameItemAdapter;
        }

        protected override void OnProcessedEntry(ItemStack stack, DragEntry entry)
        {
            var salePrice = BigDouble.Zero;
            foreach (var adapter in stack.Adapters)
                salePrice += ((GameItemAdapter)adapter).SellPrice;

            _player.GoldTotal += salePrice;
        }

        protected override void OnHighlightChanged(bool highlighted, bool canAccept)
        {
            _graphic.color = highlighted && canAccept
                ? new Color(1f, 0.45f, 0.2f, _normalColor.a)
                : _normalColor;
        }
    }
}
