using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.TestSkillTree.View
{
    // Place on a full-screen Image (Raycast Target = true) that wraps the Viewport.
    // Assign the Content RectTransform (the panel that holds nodes and connections).
    public class SkillTreePanZoomController : MonoBehaviour, IDragHandler, IScrollHandler
    {
        [SerializeField] private RectTransform _content;
        [SerializeField] private float _minZoom    = 0.3f;
        [SerializeField] private float _maxZoom    = 2f;
        [SerializeField] private float _zoomFactor = 1.12f; // multiplier per scroll step

        public void OnDrag(PointerEventData eventData)
        {
            _content.anchoredPosition += eventData.delta / _content.localScale.x;
        }

        public void OnScroll(PointerEventData eventData)
        {
            var oldScale = _content.localScale.x;
            var zoomDelta = eventData.scrollDelta.y > 0 ? _zoomFactor : 1f / _zoomFactor;
            var newScale  = Mathf.Clamp(oldScale * zoomDelta, _minZoom, _maxZoom);

            if (Mathf.Approximately(newScale, oldScale))
                return;

            // Keep the point under the cursor fixed after zoom.
            // Cursor in parent (viewport) local space:
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_content.parent,
                eventData.position,
                eventData.pressEventCamera,
                out var cursorInParent);

            var ratio = newScale / oldScale - 1f;
            _content.localScale      = Vector3.one * newScale;
            _content.anchoredPosition -= new Vector2(
                (cursorInParent.x - _content.anchoredPosition.x) * ratio,
                (cursorInParent.y - _content.anchoredPosition.y) * ratio);
        }
    }
}
