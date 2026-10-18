using Core.Items;
using System;
using UDND.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.Inventory
{
    public sealed class EquipmentDropArea : DropAreaBase, IPointerClickHandler
    {
        private PlayerItemStorage _storage;
        private EquipmentSlot _slot;
        private Image _image;
        private Sprite _placeholder;
        private Color _placeholderColor;
        private Color _normalColor;
        private Vector2 _iconSize;
        private Action _onInventoryFull;
        private GameObject _pointer;

        public void Configure(PlayerItemStorage storage, EquipmentSlot slot, Image image, Action onInventoryFull = null)
        {
            _storage = storage;
            _onInventoryFull = onInventoryFull;
            _slot = slot;
            _image = image;
            _placeholder = image.sprite;
            _placeholderColor = image.color;
            _placeholderColor.a *= 128f / 255f;
            _iconSize = image.rectTransform.sizeDelta;
            // DropAreaBase toggles interactable between drags. Its default disabled
            // ColorTint must not dim equipped art; this view owns both visual states.
            transition = Transition.None;
            SetEquippedIcon(null);
            // DropAreaBase disables its own raycast between drags; keep a child target for unequipping.
            var pointer = new GameObject("EquipmentPointer", typeof(RectTransform), typeof(Image));
            _pointer = pointer;
            pointer.transform.SetParent(transform, false);
            var rect = (RectTransform)pointer.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            pointer.GetComponent<Image>().color = Color.clear;
        }

        public void EnableDragging(ItemCatalog catalog, PlayerInventoryView view) =>
            _pointer.AddComponent<EquipmentDragSlot>().Configure(_storage, catalog, _slot, view);

        public override bool CanAcceptDrop(DragContext context)
        {
            return context != null && context.Entries.Count == 1 && base.CanAcceptDrop(context);
        }

        protected override bool CanAcceptEntry(DragEntry entry)
        {
            return entry.Stack?.Count == 1 &&
                   entry.Stack.PrimaryAdapter is GameItemAdapter adapter &&
                   _storage != null && _storage.CanEquip(_slot, adapter.State.InstanceId);
        }

        public override DropResult ProcessDrop(DragContext context)
        {
            if (!CanAcceptDrop(context)) return DropResult.Failed("Invalid equipment drop");
            var adapter = (GameItemAdapter)context.Entries[0].Stack.PrimaryAdapter;
            if (!_storage.Equip(_slot, adapter.State.InstanceId)) return DropResult.Failed("Cannot equip item");
            SetEquippedIcon(adapter.Icon);
            return DropResult.Succeeded(adapter, 1);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
                if (_storage != null && !_storage.Equip(_slot, null)) _onInventoryFull?.Invoke();
        }

        public void SetEquippedIcon(Sprite icon)
        {
            _image.sprite = icon != null ? icon : _placeholder;
            _normalColor = icon != null ? Color.white : _placeholderColor;
            _image.color = _normalColor;
            // Cancel any Selectable tint started before Configure disabled transitions.
            _image.CrossFadeColor(Color.white, 0f, true, true);
            _image.preserveAspect = true;
            _image.rectTransform.sizeDelta = _iconSize * (icon != null ? 1f : .92f);
        }

        protected override void OnHighlightChanged(bool highlighted, bool canAccept)
        {
            if (_image == null) return;
            _image.color = highlighted
                ? canAccept
                    ? new Color(0.35f, 1f, 0.45f, _normalColor.a)
                    : new Color(1f, 0.3f, 0.3f, _normalColor.a)
                : _normalColor;
        }
    }
}
