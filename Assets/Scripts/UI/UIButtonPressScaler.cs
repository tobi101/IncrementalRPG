using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class UIButtonPressScaler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _target;
        [SerializeField, Min(0f)] private float _pressedScale = 0.96f;

        private Vector3 _baseScale = Vector3.one;

        private void Reset()
        {
            _button = GetComponent<Button>();
            _target = transform as RectTransform;
        }

        private void Awake()
        {
            EnsureReferences();
            CaptureBaseScale();
            ResetScale();
        }

        private void OnEnable()
        {
            EnsureReferences();
            CaptureBaseScale();
            ResetScale();
        }

        private void OnDisable()
        {
            ResetScale();
        }

        public void Configure(Button button, RectTransform target = null, float pressedScale = 0.94f)
        {
            if (button != null)
                _button = button;

            _target = target != null ? target : ResolveDefaultTarget();
            _pressedScale = pressedScale;

            CaptureBaseScale();
            ResetScale();
        }

        public static UIButtonPressScaler EnsureOn(Button button, float pressedScale = 0.94f)
        {
            if (button == null)
                return null;

            var scaler = button.GetComponent<UIButtonPressScaler>();
            if (scaler == null)
                scaler = button.gameObject.AddComponent<UIButtonPressScaler>();

            scaler.Configure(button, pressedScale: pressedScale);
            return scaler;
        }

        public static void InstallInChildren(Component root, float pressedScale = 0.94f)
        {
            if (root == null)
                return;

            InstallInChildren(root.transform, pressedScale);
        }

        public static void InstallInChildren(Transform root, float pressedScale = 0.94f)
        {
            if (root == null)
                return;

            var buttons = root.GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
                EnsureOn(button, pressedScale);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CanInteract() || _target == null)
                return;

            _target.localScale = _baseScale * _pressedScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetScale();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ResetScale();
        }

        private void EnsureReferences()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_target == null)
                _target = ResolveDefaultTarget();
        }

        private RectTransform ResolveDefaultTarget()
        {
            if (_target != null)
                return _target;

            if (_button != null)
                return _button.transform as RectTransform;

            return transform as RectTransform;
        }

        private void CaptureBaseScale()
        {
            if (_target != null)
                _baseScale = _target.localScale;
        }

        private void ResetScale()
        {
            if (_target != null)
                _target.localScale = _baseScale;
        }

        private bool CanInteract()
        {
            return _button == null || _button.interactable;
        }
    }
}
