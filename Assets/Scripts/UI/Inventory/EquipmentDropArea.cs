using Core.Items;
using UDND.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
    public sealed class EquipmentDropArea : DropAreaBase
    {
        private PlayerItemStorage _storage;
        private EquipmentSlot _slot;
        private Image _image;
        private Sprite _placeholder;
        private Color _normalColor;

        public void Configure(PlayerItemStorage storage, EquipmentSlot slot, Image image)
        {
            _storage = storage;
            _slot = slot;
            _image = image;
            _placeholder = image.sprite;
            _normalColor = image.color;
        }

        public override bool CanAcceptDrop(DragContext context)
        {
            return context.Entries.Count == 1 && base.CanAcceptDrop(context);
        }

        protected override bool CanAcceptEntry(DragEntry entry)
        {
            return entry.Stack?.Count == 1 &&
                   entry.Stack.PrimaryAdapter is GameItemAdapter adapter &&
                   adapter.Definition.category == ItemCategory.Armor &&
                   adapter.Definition.equipmentSlot == _slot;
        }

        public override DropResult ProcessDrop(DragContext context)
        {
            var adapter = (GameItemAdapter)context.Entries[0].Stack.PrimaryAdapter;
            _storage.Equip(_slot, adapter.State.InstanceId);
            SetEquippedIcon(adapter.Icon);
            return DropResult.Succeeded(adapter, 1);
        }

        public void SetEquippedIcon(Sprite icon)
        {
            _image.sprite = icon != null ? icon : _placeholder;
        }

        protected override void OnHighlightChanged(bool highlighted, bool canAccept)
        {
            _image.color = highlighted
                ? canAccept
                    ? new Color(0.35f, 1f, 0.45f, _normalColor.a)
                    : new Color(1f, 0.3f, 0.3f, _normalColor.a)
                : _normalColor;
        }
    }
}
