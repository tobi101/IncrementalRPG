using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class SideMenuButtonVisualState : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        ISelectHandler,
        IDeselectHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _glow;

        private bool _isHovered;
        private bool _isSelected;

        private void Reset()
        {
            _button = GetComponent<Button>();
            ResolveGlow();
        }

        private void Awake()
        {
            EnsureReferences();
            ResetVisualState();
        }

        private void OnEnable()
        {
            EnsureReferences();
            ResetVisualState();
        }

        private void OnDisable()
        {
            ResetVisualState();
        }

        public void Configure(GameObject glow)
        {
            _button = GetComponent<Button>();
            _glow = glow;
            ResetVisualState();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            RefreshGlow();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            RefreshGlow();
        }

        public void OnSelect(BaseEventData eventData)
        {
            _isSelected = true;
            RefreshGlow();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isSelected = false;
            RefreshGlow();
        }

        private void EnsureReferences()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            ResolveGlow();
        }

        private void ResolveGlow()
        {
            if (_glow == null)
                _glow = transform.Find("Glow")?.gameObject;
        }

        private bool CanInteract()
        {
            return _button == null || _button.interactable;
        }

        private void ResetVisualState()
        {
            _isHovered = false;
            _isSelected = false;
            SetGlowVisible(false);
        }

        private void RefreshGlow()
        {
            SetGlowVisible(CanInteract() && (_isHovered || _isSelected));
        }

        private void SetGlowVisible(bool visible)
        {
            if (_glow != null && _glow.activeSelf != visible)
                _glow.SetActive(visible);
        }
    }
}
