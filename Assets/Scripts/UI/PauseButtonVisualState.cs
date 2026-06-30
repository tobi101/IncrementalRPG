using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class PauseButtonVisualState : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _glassOff;
        [SerializeField] private GameObject _glassOn;
        [SerializeField] private GameObject _backLight;
        [SerializeField, Min(0f)] private float _pressedScale = 0.94f;

        private bool _isHovered;
        private bool _isPressed;
        private bool _wasInteractable;
        private RectTransform _glassOffTransform;
        private RectTransform _glassOnTransform;
        private Vector3 _glassOffBaseScale = Vector3.one;
        private Vector3 _glassOnBaseScale = Vector3.one;

        private void Reset()
        {
            _button = GetComponent<Button>();
            ResolveNamedVisuals();
        }

        private void Awake()
        {
            EnsureReferences();
            DisableRootPressScaler();
            CaptureBaseScales();
            _wasInteractable = CanInteract();
            ApplyVisuals();
        }

        private void OnEnable()
        {
            EnsureReferences();
            DisableRootPressScaler();
            CaptureBaseScales();
            ResetInputState();
            _wasInteractable = CanInteract();
            ApplyVisuals();
        }

        private void OnDisable()
        {
            ResetInputState();
            ApplyVisuals();
        }

        private void Update()
        {
            var canInteract = CanInteract();
            if (canInteract == _wasInteractable)
                return;

            _wasInteractable = canInteract;
            if (!canInteract)
                ResetInputState();

            ApplyVisuals();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanInteract())
                return;

            _isHovered = true;
            ApplyVisuals();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ResetInputState();
            ApplyVisuals();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CanInteract())
                return;

            _isPressed = true;
            ApplyVisuals();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            ApplyVisuals();
        }

        private void EnsureReferences()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            ResolveNamedVisuals();
        }

        private void ResolveNamedVisuals()
        {
            if (_glassOff == null)
                _glassOff = transform.Find("GlassOff")?.gameObject;

            if (_glassOn == null)
                _glassOn = transform.Find("GlassOn")?.gameObject;

            if (_backLight == null)
                _backLight = transform.Find("BackLight")?.gameObject;
        }

        private void DisableRootPressScaler()
        {
            var rootPressScaler = GetComponent<UIButtonPressScaler>();
            if (rootPressScaler != null)
                rootPressScaler.enabled = false;
        }

        private void ResetInputState()
        {
            _isHovered = false;
            _isPressed = false;
        }

        private void ApplyVisuals()
        {
            var canInteract = CanInteract();
            var pressed = canInteract && _isPressed;
            var highlighted = canInteract && (_isHovered || _isPressed);
            var glassOnVisible = highlighted;

            SetActive(_glassOff, !glassOnVisible);
            SetActive(_glassOn, glassOnVisible);
            SetActive(_backLight, highlighted);
            ApplyPressedScale(pressed);
        }

        private void CaptureBaseScales()
        {
            _glassOffTransform = _glassOff != null ? _glassOff.transform as RectTransform : null;
            _glassOnTransform = _glassOn != null ? _glassOn.transform as RectTransform : null;

            if (_glassOffTransform != null)
                _glassOffBaseScale = _glassOffTransform.localScale;

            if (_glassOnTransform != null)
                _glassOnBaseScale = _glassOnTransform.localScale;
        }

        private void ApplyPressedScale(bool pressed)
        {
            SetScale(_glassOffTransform, _glassOffBaseScale, pressed);
            SetScale(_glassOnTransform, _glassOnBaseScale, pressed);
        }

        private bool CanInteract()
        {
            return _button == null || _button.interactable;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
                target.SetActive(active);
        }

        private void SetScale(RectTransform target, Vector3 baseScale, bool pressed)
        {
            if (target != null)
                target.localScale = pressed ? baseScale * _pressedScale : baseScale;
        }
    }
}
