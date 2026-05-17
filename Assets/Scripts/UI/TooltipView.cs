using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace UI
{
    public class TooltipView : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Vector2 _offset = new(16f, -16f);

        private Canvas _canvas;
        private RectTransform _anchor;
        private LocalizedString _localizedText;
        private LocalizedString.ChangeHandler _localizedTextChanged;

        private void Awake()
        {
            _localizedTextChanged = HandleLocalizedTextChanged;

            if (_root == null)
                _root = transform as RectTransform;

            _canvas = GetComponentInParent<Canvas>();
            Hide();
        }

        public void Show(string text, RectTransform anchor)
        {
            ClearLocalizedText();
            _anchor = anchor;

            if (string.IsNullOrEmpty(text))
            {
                Hide();
                return;
            }

            if (_text != null)
                _text.text = text;

            SetVisible(true);
            PositionNear(anchor);
        }

        public void Show(LocalizedString text, RectTransform anchor)
        {
            ClearLocalizedText();
            _anchor = anchor;

            if (text == null || text.IsEmpty)
            {
                Hide();
                return;
            }

            _localizedText = text;
            _localizedText.StringChanged += _localizedTextChanged;
        }

        public void Hide()
        {
            ClearLocalizedText();
            _anchor = null;
            SetVisible(false);
        }

        private void PositionNear(RectTransform anchor)
        {
            if (_root == null || anchor == null)
                return;

            var parent = _root.parent as RectTransform;
            if (parent == null)
                return;

            var camera = GetEventCamera();
            var corners = new Vector3[4];
            anchor.GetWorldCorners(corners);

            var screenPoint = RectTransformUtility.WorldToScreenPoint(camera, corners[2]) + _offset;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, camera, out var localPoint))
                _root.anchoredPosition = localPoint;
        }

        private Camera GetEventCamera()
        {
            if (_canvas == null)
                _canvas = GetComponentInParent<Canvas>();

            return _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? _canvas.worldCamera
                : null;
        }

        private void SetVisible(bool visible)
        {
            var target = _root != null ? _root.gameObject : gameObject;
            if (target.activeSelf != visible)
                target.SetActive(visible);
        }

        private void HandleLocalizedTextChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                SetVisible(false);
                return;
            }

            if (_text != null)
                _text.text = value;

            SetVisible(true);
            PositionNear(_anchor);
        }

        private void ClearLocalizedText()
        {
            if (_localizedText != null)
                _localizedText.StringChanged -= _localizedTextChanged;

            _localizedText = null;
        }

        private void OnDestroy()
        {
            ClearLocalizedText();
        }
    }
}
