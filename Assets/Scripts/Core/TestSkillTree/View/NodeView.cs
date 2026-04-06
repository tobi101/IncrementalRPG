using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.TestSkillTree.View
{
    // Prefab requirements: Image (_icon) + TextMeshProUGUI (_levelText) as children.
    public class NodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image             _icon;
        [SerializeField] private TextMeshProUGUI   _levelText;

        [Header("State Colors")]
        [SerializeField] private Color _colorAvailable = new Color(0.55f, 0.55f, 0.55f);
        [SerializeField] private Color _colorPartial   = new Color(1f,    0.85f, 0f   );
        [SerializeField] private Color _colorComplete  = new Color(0.2f,  0.8f,  0.2f );

        private SkillTreeService _service;
        private NodeDefinition   _definition;
        private NodePopupView    _popup;

        public void Bind(NodeDefinition definition, SkillTreeService service, NodePopupView popup)
        {
            _definition = definition;
            _service    = service;
            _popup      = popup;

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

            _icon.color = state switch
            {
                NodeState.Available => _colorAvailable,
                NodeState.Partial   => _colorPartial,
                NodeState.Complete  => _colorComplete,
                _                   => Color.white
            };

            _levelText.text = $"{_service.GetLevel(_definition.id)}/{_definition.maxLevel}";
        }

        public void OnPointerEnter(PointerEventData eventData) =>
            _popup.Show(_definition, (RectTransform)transform);

        public void OnPointerExit(PointerEventData eventData) =>
            _popup.OnNodeExit();
    }
}
