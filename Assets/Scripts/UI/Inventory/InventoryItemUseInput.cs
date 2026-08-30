using Core.Items;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Inventory
{
    public sealed class InventoryItemUseInput : MonoBehaviour, IPointerClickHandler
    {
        private InventoryGridSlot _slot;
        private RunConsumableService _consumables;

        public void Configure(InventoryGridSlot slot, RunConsumableService consumables)
        {
            _slot = slot;
            _consumables = consumables;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right || _slot.IsEmpty)
                return;

            if (_slot.Stack.PrimaryAdapter is GameItemAdapter adapter &&
                adapter.Definition.category == ItemCategory.Consumable)
            {
                _consumables.TryUse(adapter.State.InstanceId);
            }
        }
    }
}
