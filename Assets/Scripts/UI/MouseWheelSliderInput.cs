using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public sealed class MouseWheelSliderInput : MonoBehaviour, IScrollHandler
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private float _step = 0.05f;

        private void Reset()
        {
            _slider = GetComponent<Slider>();
        }

        private void Awake()
        {
            if (_slider == null)
                _slider = GetComponent<Slider>();
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (_slider == null || !_slider.interactable)
                return;

            if (Mathf.Approximately(eventData.scrollDelta.y, 0f))
                return;

            var direction = eventData.scrollDelta.y >= 0f ? 1f : -1f;
            _slider.value = Mathf.Clamp01(_slider.value + direction * _step);
        }
    }
}
