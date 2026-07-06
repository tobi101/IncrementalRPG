using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.TestSkillTree.View
{
    public class SkillTreePanZoomController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        [SerializeField] private RectTransform _content;
        [SerializeField] private NodePopupView _popupView;
        [SerializeField] private float _minZoom          = 0.3f;
        [SerializeField] private float _maxZoom          = 2f;
        [SerializeField] private float _zoomFactor       = 1.12f; // multiplier per scroll step
        [SerializeField] private float _rubberBandDamping = 0.3f; // 0 = wall, 1 = no resistance
        [SerializeField] private float _snapDuration      = 0.35f;

        private Vector2   _grabPoint;
        private Coroutine _snapCoroutine;

        public void FocusOnContentPoint(Vector2 contentPoint, float zoom)
        {
            if (_content == null)
                return;

            StopSnap();

            var clampedZoom = Mathf.Clamp(zoom, _minZoom, _maxZoom);
            _content.localScale = Vector3.one * clampedZoom;
            _content.anchoredPosition = ClampedPosition(-contentPoint * clampedZoom);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _popupView.Block();

            StopSnap();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _content,
                eventData.position,
                eventData.pressEventCamera,
                out _grabPoint);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _popupView.Unblock();

            var clamped = ClampedPosition(_content.anchoredPosition);
            if (_content.anchoredPosition != clamped)
                _snapCoroutine = StartCoroutine(SnapTo(clamped));
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_content.parent,
                eventData.position,
                eventData.pressEventCamera,
                out var cursorInParent);

            var raw      = cursorInParent - _grabPoint * _content.localScale.x;
            var clamped  = ClampedPosition(raw);
            var overflow = raw - clamped;

            _content.anchoredPosition = clamped + overflow * _rubberBandDamping;
        }

        public void OnScroll(PointerEventData eventData)
        {
            var oldScale = _content.localScale.x;
            var zoomDelta = eventData.scrollDelta.y > 0 ? _zoomFactor : 1f / _zoomFactor;
            var newScale  = Mathf.Clamp(oldScale * zoomDelta, _minZoom, _maxZoom);

            if (Mathf.Approximately(newScale, oldScale))
                return;

            // Keep the point under the cursor fixed after zoom.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_content.parent,
                eventData.position,
                eventData.pressEventCamera,
                out var cursorInParent);

            var ratio = newScale / oldScale - 1f;
            _content.localScale       = Vector3.one * newScale;
            _content.anchoredPosition -= new Vector2(
                (cursorInParent.x - _content.anchoredPosition.x) * ratio,
                (cursorInParent.y - _content.anchoredPosition.y) * ratio);

            _content.anchoredPosition = ClampedPosition(_content.anchoredPosition);
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private Vector2 ClampedPosition(Vector2 position)
        {
            var viewportSize = ((RectTransform)_content.parent).rect.size;
            var boundsSize = _content.rect.size * _content.localScale.x;

            // With pivot 0.5/0.5 the content center is at anchoredPosition.
            // Allowed travel = half the excess size on each axis.
            var xLimit = Mathf.Max(0f, (boundsSize.x - viewportSize.x) * 0.5f);
            var yLimit = Mathf.Max(0f, (boundsSize.y - viewportSize.y) * 0.5f);

            return new Vector2(
                Mathf.Clamp(position.x, -xLimit, xLimit),
                Mathf.Clamp(position.y, -yLimit, yLimit));
        }

        private IEnumerator SnapTo(Vector2 target)
        {
            var start   = _content.anchoredPosition;
            var elapsed = 0f;

            while (elapsed < _snapDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / _snapDuration;
                t = 1f - (1f - t) * (1f - t); // ease-out quad
                _content.anchoredPosition = Vector2.Lerp(start, target, t);
                yield return null;
            }

            _content.anchoredPosition = target;
            _snapCoroutine = null;
        }

        private void StopSnap()
        {
            if (_snapCoroutine == null)
                return;

            StopCoroutine(_snapCoroutine);
            _snapCoroutine = null;
        }
    }
}
