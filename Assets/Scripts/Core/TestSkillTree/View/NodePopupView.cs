using TMPro;
using UI.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Core.TestSkillTree.View
{
    // Single instance on the Canvas (outside Content so it is unaffected by zoom).
    public class NodePopupView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _costText;

        [SerializeField] private Image _borderImage;
        [SerializeField] private Image _framePriceImage;
        [SerializeField] private Image _backGlowImage;

        [SerializeField] private NodeFramePriceSpriteConfig _framePriceSpriteConfig;
        [SerializeField] private NodeBackGlowSpriteConfig  _backGlowSpriteConfig;

        private NodeBorderColorConfig _borderColorConfig;

        private const float PopupGap = 15f;

        private SkillTreeService _service;
        private NodeDefinition   _current;
        private RectTransform    _rt;
        private Canvas           _canvas;
        private LocalizedStringBinding _nameBinding;
        private LocalizedStringBinding _descriptionBinding;

        private bool _blocked;

        public void Bind(SkillTreeService service, NodeBorderColorConfig borderColorConfig)
        {
            _service           = service;
            _borderColorConfig = borderColorConfig;
            _rt                = (RectTransform)transform;
            _canvas            = GetComponentInParent<Canvas>();
            _nameBinding        = new LocalizedStringBinding(_nameText);
            _descriptionBinding = new LocalizedStringBinding(_descriptionText);

            gameObject.SetActive(false);
        }

        public void Block()
        {
            _blocked = true;
            Hide();
        }

        public void Unblock() => _blocked = false;

        public void Show(NodeDefinition definition, RectTransform nodeTransform)
        {
            if (_blocked) return;

            _current = definition;
            gameObject.SetActive(true);
            Refresh();
            PositionNear(nodeTransform);
        }

        public void OnNodeExit() => Hide();

        public void Hide()
        {
            _nameBinding?.Clear();
            _descriptionBinding?.Clear();
            gameObject.SetActive(false);
            _current = null;
        }

        public void Refresh(NodeDefinition definition)
        {
            _current = definition;
            Refresh();
        }

        private void ApplyVisualState(NodeState state)
        {
            if (_borderColorConfig != null)
                _borderImage.color = _borderColorConfig.GetColor(state);

            if (_framePriceSpriteConfig != null && _framePriceImage != null)
                _framePriceImage.sprite = _framePriceSpriteConfig.GetSprite(state);

            if (_backGlowSpriteConfig != null && _backGlowImage != null)
                _backGlowImage.sprite = _backGlowSpriteConfig.GetSprite(state);
        }

        private void Refresh()
        {
            _nameBinding.Bind(_current.displayName);
            _descriptionBinding.Bind(_current.description);
            ApplyVisualState(_service.GetState(_current.id));

            if (_costText != null)
            {
                var cost = _service.GetUpgradeCost(_current.id);
                _costText.text = cost > 0 ? $"{cost}" : "";
            }
        }

        private void PositionNear(RectTransform nodeTransform)
        {
            var canvasRT   = (RectTransform)_canvas.transform;
            var canvasRect = canvasRT.rect;

            // Node center in canvas local space
            var screenCenter = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, nodeTransform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenCenter, _canvas.worldCamera, out var nodeCenter);

            // Node size in canvas local space (accounts for Content zoom)
            var corners = new Vector3[4];
            nodeTransform.GetWorldCorners(corners);
            var screenBL = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, corners[0]);
            var screenTR = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, corners[2]);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenBL, _canvas.worldCamera, out var localBL);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenTR, _canvas.worldCamera, out var localTR);

            float nodeHalfW  = (localTR.x - localBL.x) * 0.5f;
            float nodeHalfH  = (localTR.y - localBL.y) * 0.5f;
            float popupHalfW = _rt.rect.width  * 0.5f;
            float popupHalfH = _rt.rect.height * 0.5f;

            float nodeRightEdge  = nodeCenter.x + nodeHalfW;
            float nodeLeftEdge   = nodeCenter.x - nodeHalfW;
            float nodeTopEdge    = nodeCenter.y + nodeHalfH;
            float nodeBottomEdge = nodeCenter.y - nodeHalfH;

            float spaceRight  = canvasRect.xMax - nodeRightEdge;
            float spaceLeft   = nodeLeftEdge    - canvasRect.xMin;
            float spaceTop    = canvasRect.yMax - nodeTopEdge;
            float spaceBottom = nodeBottomEdge  - canvasRect.yMin;

            float bestHorizontal = Mathf.Max(spaceRight, spaceLeft);
            float bestVertical   = Mathf.Max(spaceTop,   spaceBottom);

            float x, y;
            if (bestHorizontal >= bestVertical)
            {
                // Place left or right; center vertically on node
                x = (spaceRight >= spaceLeft)
                    ? nodeRightEdge + PopupGap + popupHalfW
                    : nodeLeftEdge  - PopupGap - popupHalfW;
                y = Mathf.Clamp(nodeCenter.y, canvasRect.yMin + popupHalfH, canvasRect.yMax - popupHalfH);
            }
            else
            {
                // Place above or below; center horizontally on node
                y = (spaceTop >= spaceBottom)
                    ? nodeTopEdge    + PopupGap + popupHalfH
                    : nodeBottomEdge - PopupGap - popupHalfH;
                x = Mathf.Clamp(nodeCenter.x, canvasRect.xMin + popupHalfW, canvasRect.xMax - popupHalfW);
            }

            _rt.anchoredPosition = new Vector2(x, y);
        }

        private void OnUpgradeClicked()
        {
            if (_current == null) return;
            _service.Upgrade(_current.id);
            Refresh();
        }

        private void OnDestroy()
        {
            _nameBinding?.Dispose();
            _descriptionBinding?.Dispose();
        }
    }
}
