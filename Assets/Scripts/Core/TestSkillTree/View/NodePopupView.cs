using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.TestSkillTree.View
{
    // Single instance on the Canvas (outside Content so it is unaffected by zoom).
    public class NodePopupView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Button          _upgradeButton;
        [SerializeField] private Vector2         _offset = new Vector2(16f, 0f);

        private SkillTreeService _service;
        private NodeDefinition   _current;
        private RectTransform    _rt;
        private Canvas           _canvas;

        public void Bind(SkillTreeService service)
        {
            _service = service;
            _rt      = (RectTransform)transform;
            _canvas  = GetComponentInParent<Canvas>();

            _upgradeButton.onClick.AddListener(OnUpgradeClicked);
            gameObject.SetActive(false);
        }

        public void Show(NodeDefinition definition, RectTransform nodeTransform)
        {
            _current = definition;
            gameObject.SetActive(true);
            Refresh();
            PositionNear(nodeTransform);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _current = null;
        }

        private void Refresh()
        {
            _nameText.text        = _current.displayName;
            _descriptionText.text = _current.description;
            _levelText.text       = $"{_service.GetLevel(_current.id)} / {_current.maxLevel}";
            _upgradeButton.interactable = _service.CanUpgrade(_current.id);
        }

        private void PositionNear(RectTransform nodeTransform)
        {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(
                _canvas.worldCamera, nodeTransform.position);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_canvas.transform,
                screenPoint,
                _canvas.worldCamera,
                out var localPoint);

            _rt.anchoredPosition = localPoint + _offset + Vector2.right * (_rt.rect.width * 0.5f);
        }

        private void OnUpgradeClicked()
        {
            if (_current == null) return;
            _service.Upgrade(_current.id);
            Refresh();
        }

        private void OnDestroy()
        {
            if (_upgradeButton != null)
                _upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
        }
    }
}
