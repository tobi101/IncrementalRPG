using Core.Classes;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Classes
{
    public sealed class ClassAttackDropZone : MonoBehaviour, IDropHandler
    {
        public ClassesMenuView menu;
        public AttackSlot slot;
        public bool unequip;
        public void OnDrop(PointerEventData data)
        {
            var source = data.pointerDrag == null ? null : data.pointerDrag.GetComponent<ClassAttackItemView>();
            if (source == null || source.Attack == null || source.Attack.slot != slot) return;
            if (!unequip || source.IsEquipped) menu.SetAttack(slot, unequip ? null : source.Attack);
        }
    }
}
