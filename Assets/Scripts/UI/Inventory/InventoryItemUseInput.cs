using Core.Items;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Inventory
{
    public sealed class InventoryItemUseInput : MonoBehaviour, IPointerClickHandler
    {
        private InventoryGridSlot _slot;
        private RunConsumableService _consumables;
        private PlayerItemStorage _storage;
        private Action<ConsumableUseResult> _feedback;
        private Action _onInventoryFull;

        public void Configure(InventoryGridSlot slot, RunConsumableService consumables, Action<ConsumableUseResult> feedback, PlayerItemStorage storage, Action onInventoryFull = null)
        {
            _slot = slot;
            _consumables = consumables;
            _feedback = feedback;
            _storage = storage;
            _onInventoryFull = onInventoryFull;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right || _slot.IsEmpty)
                return;

            if (_slot.Stack.PrimaryAdapter is GameItemAdapter adapter &&
                adapter.Definition.category == ItemCategory.Consumable)
            {
                _consumables.TryUse(adapter.State.InstanceId, out var result);
                _feedback?.Invoke(result);
            }
            else if (_slot.Stack.PrimaryAdapter is GameItemAdapter equipment &&
                     equipment.Definition.category == ItemCategory.Armor)
            {
                var slot = equipment.Definition.equipmentSlot;
                if (!_storage.Equip(slot, equipment.State.InstanceId)) _onInventoryFull?.Invoke();
            }
        }
    }
}
