using TMPro;
using IncrementalRPG.Scripts.AudioManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.TestSkillTree.View
{
    // Prefab requirements: Image (_icon) + TextMeshProUGUI (_levelText) as children.
    public class NodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _additionalIcon;
        [SerializeField] private GameObject _additionalIconRoot;
        [SerializeField] private Image _borderIcon;
        [SerializeField] private TextMeshProUGUI _levelText;

        private SkillTreeService      _service;
        private NodeDefinition        _definition;
        private NodePopupView         _popup;
        private NodeBorderColorConfig _borderColorConfig;
        private AudioManager          _audioManager;

        public void Bind(NodeDefinition definition, SkillTreeService service, NodePopupView popup, NodeBorderColorConfig borderColorConfig, AudioManager audioManager)
        {
            _definition        = definition;
            _service           = service;
            _popup             = popup;
            _borderColorConfig = borderColorConfig;
            _audioManager      = audioManager;

            if (_icon != null && definition.icon != null)
                _icon.sprite = definition.icon;

            SetupAdditionalIcon(definition);
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

            var stateColor = _borderColorConfig != null
                ? _borderColorConfig.GetColor(state)
                : Color.white;

            _borderIcon.color = stateColor;

            if (_additionalIcon != null && _additionalIcon.gameObject.activeInHierarchy)
                _additionalIcon.color = stateColor;

            _levelText.text = $"{_service.GetLevel(_definition.id)}/{_definition.maxLevel}";
        }

        private void SetupAdditionalIcon(NodeDefinition definition)
        {
            if (_additionalIcon == null)
                return;

            var hasIcon = definition.additionalIcon != null;
            var iconRoot = _additionalIconRoot != null
                ? _additionalIconRoot
                : _additionalIcon.gameObject;

            iconRoot.SetActive(hasIcon);
            _additionalIcon.sprite = definition.additionalIcon;
            _additionalIcon.raycastTarget = false;
        }

        public void OnPointerEnter(PointerEventData eventData) =>
            _popup.Show(_definition, (RectTransform)transform);

        public void OnPointerExit(PointerEventData eventData) =>
            _popup.OnNodeExit();

        public void OnPointerClick(PointerEventData eventData)
        {
            var result = _service.TryUpgrade(_definition.id);
            PlayUpgradeResultSound(result);

            if (result == NodeUpgradeResult.Failed)
                return;

            _popup.Refresh(_definition);
        }

        private void PlayUpgradeResultSound(NodeUpgradeResult result)
        {
            switch (result)
            {
                case NodeUpgradeResult.Upgraded:
                    _audioManager?.PlaySkillUpgrade();
                    break;
                case NodeUpgradeResult.UpgradedToMax:
                    _audioManager?.PlaySkillMax();
                    break;
                case NodeUpgradeResult.Failed:
                default:
                    _audioManager?.PlaySkillError();
                    break;
            }
        }
    }
}
