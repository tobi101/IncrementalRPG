using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.TestSkillTree.View
{
    // Prefab requirements: Image (_icon) + TextMeshProUGUI (_levelText) as children.
    public class NodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _borderIcon;
        [SerializeField] private TextMeshProUGUI _levelText;

        private SkillTreeService      _service;
        private NodeDefinition        _definition;
        private NodePopupView         _popup;
        private NodeBorderColorConfig _borderColorConfig;

        public void Bind(NodeDefinition definition, SkillTreeService service, NodePopupView popup, NodeBorderColorConfig borderColorConfig)
        {
            _definition        = definition;
            _service           = service;
            _popup             = popup;
            _borderColorConfig = borderColorConfig;

            if (_icon != null && definition.icon != null)
                _icon.sprite = definition.icon;

            Refresh();
        }

        public void Refresh()
        {
            var state = _service.GetState(_definition.id);

            if (state == NodeState.Hidden)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            _borderIcon.color = _borderColorConfig != null
                ? _borderColorConfig.GetColor(state)
                : Color.white;

            _levelText.text = $"{_service.GetLevel(_definition.id)}/{_definition.maxLevel}";
        }

        public void OnPointerEnter(PointerEventData eventData) =>
            _popup.Show(_definition, (RectTransform)transform);

        public void OnPointerExit(PointerEventData eventData) =>
            _popup.OnNodeExit();

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_service.CanUpgrade(_definition.id)) return;
            _service.Upgrade(_definition.id);
            _popup.Refresh(_definition);
        }
    }
}
