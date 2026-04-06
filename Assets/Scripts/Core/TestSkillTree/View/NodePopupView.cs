using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.TestSkillTree.View
{
    // Single instance on the Canvas (outside Content so it is unaffected by zoom).
    public class NodePopupView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private Vector2         _offset = new Vector2(16f, 0f);

        private SkillTreeService _service;
        private NodeDefinition   _current;
        private RectTransform    _rt;
        private Canvas           _canvas;

        private bool _nodeHovered;
        private bool _popupHovered;
        private Coroutine _hideCoroutine;

        public void Bind(SkillTreeService service)
        {
            _service = service;
            _rt      = (RectTransform)transform;
            _canvas  = GetComponentInParent<Canvas>();

            gameObject.SetActive(false);
        }

        public void Show(NodeDefinition definition, RectTransform nodeTransform)
        {
            _current      = definition;
            _nodeHovered  = true;
            gameObject.SetActive(true);
            Refresh();
            PositionNear(nodeTransform);
        }

        public void OnNodeExit()
        {
            _nodeHovered = false;
            TryHide();
        }

        public void OnPointerEnter(PointerEventData eventData) => _popupHovered = true;

        public void OnPointerExit(PointerEventData eventData)
        {
            _popupHovered = false;
            TryHide();
        }

        private void TryHide()
        {
            if (_nodeHovered || _popupHovered) return;
            if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
            _hideCoroutine = StartCoroutine(HideDelayed());
        }

        private System.Collections.IEnumerator HideDelayed()
        {
            yield return null;
            if (!_nodeHovered && !_popupHovered)
            {
                gameObject.SetActive(false);
                _current = null;
            }
            _hideCoroutine = null;
        }

        public void Hide()
        {
            if (_hideCoroutine != null) { StopCoroutine(_hideCoroutine); _hideCoroutine = null; }
            _nodeHovered  = false;
            _popupHovered = false;
            gameObject.SetActive(false);
            _current = null;
        }

        private void Refresh()
        {
            _nameText.text        = _current.displayName;
            _descriptionText.text = _current.description;

            if (_costText != null)
            {
                var cost = _service.GetUpgradeCost(_current.id);
                _costText.text = cost > 0 ? $"{cost} Gold" : "";
            }
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
    }
}
