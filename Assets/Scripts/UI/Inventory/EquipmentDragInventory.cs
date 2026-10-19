using System;
using System.Collections.Generic;
using System.Linq;
using Core.Items;
using UDND.Core;
using UDND.DataBinding;
using UDND.Inventories;
using UDND.Slots;
using UnityEngine;

namespace UI.Inventory
{
    // A transactional view of a worn item for UDND. Splitting/removal only changes this
    // temporary stack; PlayerItemStorage changes when a bag transfer or recycling commits.
    public sealed class EquipmentDragInventory : IInventory, IInventorySnapshotProvider
    {
        private readonly PlayerItemStorage _storage;
        private readonly ItemCatalog _catalog;
        private readonly BaseSlot _slot;
        private ItemStack _stack = ItemStack.Empty();
        public EquipmentSlot EquipmentSlot { get; }
        public InventoryDataBindingBase DataBinding { get; }
        public IReadOnlyList<BaseSlot> Slots { get; }
        public int SlotCount => 1;

        public EquipmentDragInventory(BaseSlot slot, PlayerItemStorage storage, ItemCatalog catalog, EquipmentSlot equipmentSlot,
            PlayerInventoryView view)
        {
            _slot = slot; _storage = storage; _catalog = catalog; EquipmentSlot = equipmentSlot; DataBinding = view;
            Slots = Array.AsReadOnly(new[] { slot });
        }

        public void Refresh()
        {
            _stack = ItemStack.Empty();
            if (_storage.TryGet(_storage.GetEquipped(EquipmentSlot), out var item) && _catalog.TryGet(item.ItemDefinitionId, out var definition))
                ItemStack.TryCreate(new[] { new GameItemAdapter(item, definition) }, out _stack);
        }

        public bool IsCurrentItem(GameItemAdapter adapter) => adapter != null &&
            _storage.GetEquipped(EquipmentSlot) == adapter.State.InstanceId;

        public bool TryRecycle(GameItemAdapter adapter)
        {
            if (!IsCurrentItem(adapter) || _stack.Count != 1 ||
                !ReferenceEquals(_stack.PrimaryAdapter, adapter) || !_storage.Consume(adapter.State.InstanceId))
                return false;

            _stack = ItemStack.Empty();
            return true;
        }

        public BaseSlot GetSlot(int index) => index == 0 ? _slot : null;
        public int GetDragAmount(BaseSlot slot, DragAmount? overrideAmount = null, int? overrideCustom = null) =>
            ReferenceEquals(slot, _slot) ? _stack.Count : 0;
        public bool Contains(IItemAdapter item) => _stack.Adapters.Contains(item);
        public void UpdateAllVisuals() => _slot.UpdateVisuals();
        public bool TryGetStackForSlot(BaseSlot slot, out IReadOnlyItemStack stack)
        { stack = _stack; return ReferenceEquals(slot, _slot); }
        public bool TryGetPlacementAt(BaseSlot slot, out Placement placement) { placement = null; return false; }
        public Vector2Int GetGrabOffset(Placement placement, BaseSlot slot) => Vector2Int.zero;

        public bool TrySplitFromSlot(BaseSlot slot, int amount, out ItemStack split)
        {
            split = null;
            if (!ReferenceEquals(slot, _slot) || amount != 1 || _stack.Count != 1 ||
                !IsCurrentItem(_stack.PrimaryAdapter as GameItemAdapter)) return false;
            split = _stack.CreateCopy();
            _stack = ItemStack.Empty();
            return true;
        }

        public bool TryRemoveFromSlot(BaseSlot slot, IReadOnlyList<IItemAdapter> adapters, out int removed)
        {
            removed = 0;
            if (adapters?.Count != 1 || _stack.Count != 1 || !ReferenceEquals(adapters[0], _stack.PrimaryAdapter)) return false;
            if (!TrySplitFromSlot(slot, 1, out _)) return false;
            removed = 1;
            return true;
        }

        public InventorySnapshot CaptureSnapshot() => new(1, _stack.IsEmpty ? Array.Empty<InventoryPlacementState>() :
            new[] { new InventoryPlacementState(0, _stack.Adapters, 0, Vector2Int.one) });
        public void RestoreSnapshot(InventorySnapshot snapshot)
        {
            _stack = ItemStack.Empty();
            if (snapshot.Placements.Count > 0) ItemStack.TryCreate(snapshot.Placements[0].Adapters, out _stack);
        }

        // This view cannot receive items. EquipmentDropArea handles bag -> equipment.
        public int GetAcceptableCount(InventoryAcceptanceRequest request) => 0;
        public bool TryAddStack(ItemStack stack, int targetSlotIndex = -1) => false;
        public bool TryAddStackQuiet(ItemStack stack, int targetSlotIndex = -1) => false;
        public bool TryAddToSlot(ItemStack stack, BaseSlot target, IInventory sourceInventory = null, int sourceSlotIndex = -1) => false;
        public bool TrySetStackForSlot(BaseSlot slot, ItemStack stack) => false;
        public bool TryClearSlot(BaseSlot slot) => false;
        public bool TryAddToSlotStack(BaseSlot slot, ItemStack stack) => false;
        public int RemoveItemsFromSlot(BaseSlot source, ItemStack stack, IInventory targetInventory = null, BaseSlot target = null) => 0;
    }
}
