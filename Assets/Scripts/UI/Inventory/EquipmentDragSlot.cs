using Core.Items;
using UDND;
using UDND.Slots;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Inventory
{
    // A drag source only. The parent EquipmentDropArea still owns equipping/unequipping by click.
    public sealed class EquipmentDragSlot : BaseSlot, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private EquipmentDragInventory _source;
        public RectTransform InventoryCell { get; private set; }

        public void Configure(PlayerItemStorage storage, ItemCatalog catalog, EquipmentSlot slot, PlayerInventoryView view)
        {
            _source = new EquipmentDragInventory(this, storage, catalog, slot, view);
            InventoryCell = view.Inventory.GetSlot(0).transform as RectTransform;
            Initialize(0, _source);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || _source == null) return;
            _source.Refresh();
            if (!IsEmpty) DragAndDropManager.AutoCreateInstance.StartDrag(this);
        }

        public void OnDrag(PointerEventData eventData) { } // The shared drag visual follows the pointer.

        public void OnEndDrag(PointerEventData eventData)
        {
            if (OwnsCurrentDrag()) DragAndDropManager.AutoCreateInstance.CompleteDrag();
        }

        private void OnDisable()
        {
            if (OwnsCurrentDrag()) DragAndDropManager.AutoCreateInstance.CancelDrag();
        }

        private bool OwnsCurrentDrag() => DragAndDropManager.IsInstanceExist &&
            DragAndDropManager.AutoCreateInstance.CurrentContext?.Entries.Count == 1 &&
            ReferenceEquals(DragAndDropManager.AutoCreateInstance.CurrentContext.Entries[0].SourceBaseSlot, this);
    }
}
