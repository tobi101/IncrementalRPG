using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Classes
{
    public sealed class ClassHoverMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RectTransform emblem;
        public CanvasGroup glow;
        private bool _hovered;
        private float _amount;
        public void OnPointerEnter(PointerEventData data) => _hovered = GetComponent<Button>().interactable;
        public void OnPointerExit(PointerEventData data) => _hovered = false;
        private void OnDisable()
        {
            _hovered = false;
            _amount = 0;
            if (emblem != null) { emblem.localScale = Vector3.one; emblem.localRotation = Quaternion.identity; }
            if (glow != null) glow.alpha = 0;
        }
        private void Update()
        {
            _amount = Mathf.MoveTowards(_amount, _hovered ? 1 : 0, Time.unscaledDeltaTime * 6);
            if (glow != null) glow.alpha = _amount * 0.75f;
            if (emblem == null) return;
            emblem.localScale = Vector3.one * (1 + _amount * 0.045f);
            emblem.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.unscaledTime * 2) * _amount * 2);
        }
    }
}
