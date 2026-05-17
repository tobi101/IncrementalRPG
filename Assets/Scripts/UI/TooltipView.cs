using TMPro;
using UnityEngine;

namespace UI
{
    public class TooltipView : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Vector2 _offset = new(16f, -16f);

        private Canvas _canvas;

        private void Awake()
        {
            if (_root == null)
                _root = transform as RectTransform;

            _canvas = GetComponentInParent<Canvas>();
            Hide();
        }

        public void Show(string text, RectTransform anchor)
        {
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

        public void Hide()
        {
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
    }
}
