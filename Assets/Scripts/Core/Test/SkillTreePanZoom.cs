using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Test
{
    // Вешается на полноэкранную прозрачную панель поверх дерева.
    // Панель должна иметь Image (alpha = 0) для перехвата событий.
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class SkillTreePanZoom : MonoBehaviour, IDragHandler, IScrollHandler
    {
        [SerializeField] private RectTransform _container;
        [SerializeField] private float _minZoom = 0.5f;
        [SerializeField] private float _maxZoom = 2f;
        [SerializeField] private float _zoomSpeed = 0.1f;

        public void OnDrag(PointerEventData eventData)
        {
            // Делим на scale, чтобы скорость пана была постоянной при любом зуме
            _container.anchoredPosition += eventData.delta / _container.localScale.x;
        }

        public void OnScroll(PointerEventData eventData)
        {
            float current = _container.localScale.x;
            float next = Mathf.Clamp(current + eventData.scrollDelta.y * _zoomSpeed, _minZoom, _maxZoom);
            _container.localScale = Vector3.one * next;
        }
    }
}
