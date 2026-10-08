using Core.Classes;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Classes
{
    public sealed class ClassAttackItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Image icon;
        public Image tint;
        public Image selectionBorder;
        public LocalizeStringEvent title;
        public AttackDefinition Attack { get; private set; }
        public bool IsEquipped { get; private set; }
        private ClassesMenuView _menu;
        public void Bind(ClassesMenuView menu, AttackDefinition attack, bool equipped)
        {
            _menu = menu; Attack = attack;
            icon.sprite = attack.icon;
            title.StringReference = attack.displayName;
            SetEquipped(equipped);
        }

        public void SetEquipped(bool equipped)
        {
            IsEquipped = equipped;
            tint.gameObject.SetActive(equipped);
            selectionBorder.gameObject.SetActive(equipped);
        }

        public void OnPointerEnter(PointerEventData data) => _menu.ShowTooltip(Attack, (RectTransform)transform);
        public void OnPointerExit(PointerEventData data) => _menu.HideTooltip();
        public void OnPointerClick(PointerEventData data)
        {
            if (!data.dragging && data.button == PointerEventData.InputButton.Left)
                _menu.SetAttack(Attack.slot, IsEquipped ? null : Attack);
        }
    }
}
