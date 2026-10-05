using Core.Classes;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Classes
{
    public sealed class ClassAttackItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public Image icon;
        public Image tint;
        public LocalizeStringEvent title;
        public AttackDefinition Attack { get; private set; }
        public bool IsEquipped { get; private set; }
        private ClassesMenuView _menu;
        private bool _dragging;
        public void Bind(ClassesMenuView menu, AttackDefinition attack, bool equipped, Color color)
        {
            _menu = menu; Attack = attack; IsEquipped = equipped;
            icon.sprite = attack.icon;
            tint.color = color;
            title.StringReference = attack.displayName;
        }
        public void OnPointerEnter(PointerEventData data) { if (!_dragging) _menu.ShowTooltip(Attack, (RectTransform)transform); }
        public void OnPointerExit(PointerEventData data) => _menu.HideTooltip();
        public void OnPointerClick(PointerEventData data)
        {
            if (!_dragging && !data.dragging && data.button == PointerEventData.InputButton.Left)
                _menu.SetAttack(Attack.slot, IsEquipped ? null : Attack);
        }
        public void OnBeginDrag(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left) return;
            _dragging = true;
            _menu.BeginAttackDrag(Attack, data);
        }
        public void OnDrag(PointerEventData data) { if (_dragging) _menu.MoveAttackDrag(data); }
        public void OnEndDrag(PointerEventData data) { _menu.EndAttackDrag(); _dragging = false; }
        private void OnDisable() { if (_dragging && _menu != null) _menu.EndAttackDrag(); _dragging = false; }
    }
}
