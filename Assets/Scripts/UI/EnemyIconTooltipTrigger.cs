using Entity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class EnemyIconTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TooltipView _tooltipView;

        private EntityConfig _config;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
        }

        public void Bind(EntityConfig config, TooltipView tooltipView)
        {
            _config = config;
            _tooltipView = tooltipView;

            if (_rectTransform == null)
                _rectTransform = transform as RectTransform;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_tooltipView == null || _config == null)
                return;

            _tooltipView.Show(_config.entityName, _rectTransform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _tooltipView?.Hide();
        }

        private void OnDisable()
        {
            _tooltipView?.Hide();
        }
    }
}
