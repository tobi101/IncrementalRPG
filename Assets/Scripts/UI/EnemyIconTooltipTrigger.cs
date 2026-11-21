using Entity;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace UI
{
    public class EnemyIconTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private EnemyRewardTooltipView _tooltipView;

        private EntityConfig _config;
        private BigDouble _gold;
        private BigDouble _xp;

        public void Bind(EntityConfig config, BigDouble gold, BigDouble xp, EnemyRewardTooltipView tooltipView)
        {
            _config = config;
            _gold = gold;
            _xp = xp;
            _tooltipView = tooltipView;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_tooltipView == null || _config == null)
                return;

            _tooltipView.Show(_config.entityName, _gold, _xp, (RectTransform)transform);
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
