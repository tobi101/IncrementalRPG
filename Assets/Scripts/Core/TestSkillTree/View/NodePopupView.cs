using TMPro;
using IncrementalRPG.Scripts.AudioManager;
using UI.Localization;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Core.TestSkillTree.View
{
    // Single instance on the Canvas (outside Content so it is unaffected by zoom).
    public class NodePopupView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _costText;
        
        [SerializeField] private Image _framePriceImage;
        [SerializeField] private Image _backGlowImage;

        [SerializeField] private NodeFramePriceSpriteConfig _framePriceSpriteConfig;
        [SerializeField] private NodeBackGlowSpriteConfig  _backGlowSpriteConfig;
        [SerializeField] private RectTransform _positionRoot;

        private NodeBorderColorConfig _borderColorConfig;

        private const float PopupGap = 30f;
        private const float NodeGapScale = 0.25f;

        private SkillTreeService _service;
        private AudioManager     _audioManager;
        private NodeDefinition   _current;
        private RectTransform    _rt;
        private LocalizedStringBinding _nameBinding;
        private LocalizedStringBinding _descriptionBinding;

        private bool _blocked;

        public void Bind(SkillTreeService service, NodeBorderColorConfig borderColorConfig, AudioManager audioManager)
        {
            _service           = service;
            _borderColorConfig = borderColorConfig;
            _audioManager      = audioManager;
            _rt                = (RectTransform)transform;
            _positionRoot      = _positionRoot != null ? _positionRoot : _rt.parent as RectTransform;
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
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rt);
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
                _costText.text = cost > 0 ? BigDoubleFormatter.FormatFloor(cost) : "";
            }
        }

        private void PositionNear(RectTransform nodeTransform)
        {
            if (_positionRoot == null || nodeTransform == null)
                return;

            var boundsRect = _positionRoot.rect;
            var nodeRect = GetRectInPositionRoot(nodeTransform);
            var popupSize = _rt.rect.size;
            var popupHalfSize = popupSize * 0.5f;
            var gap = Mathf.Max(PopupGap, Mathf.Min(nodeRect.width, nodeRect.height) * NodeGapScale);
            var preferredClearRect = Expand(nodeRect, gap);

            var rightCenter  = new Vector2(nodeRect.xMax + gap + popupHalfSize.x, nodeRect.center.y);
            var leftCenter   = new Vector2(nodeRect.xMin - gap - popupHalfSize.x, nodeRect.center.y);
            var topCenter    = new Vector2(nodeRect.center.x, nodeRect.yMax + gap + popupHalfSize.y);
            var bottomCenter = new Vector2(nodeRect.center.x, nodeRect.yMin - gap - popupHalfSize.y);

            var bestCenter = rightCenter;
            var bestScore = float.MaxValue;

            TryCandidate(rightCenter);
            TryCandidate(leftCenter);
            TryCandidate(topCenter);
            TryCandidate(bottomCenter);

            var pivotPosition = CenterToPivotPosition(bestCenter, popupSize);
            _rt.anchoredPosition = LocalPointToAnchoredPosition(pivotPosition);

            void TryCandidate(Vector2 rawCenter)
            {
                var center = ClampPopupCenter(rawCenter, popupHalfSize, boundsRect);
                var popupRect = RectFromCenter(center, popupSize);
                var score = ScoreCandidate(popupRect, nodeRect, preferredClearRect, boundsRect, rawCenter);

                if (score >= bestScore)
                    return;

                bestScore = score;
                bestCenter = center;
            }
        }

        private Rect GetRectInPositionRoot(RectTransform target)
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);

            var local = (Vector2)_positionRoot.InverseTransformPoint(corners[0]);
            var min = local;
            var max = local;

            for (var i = 1; i < corners.Length; i++)
            {
                local = _positionRoot.InverseTransformPoint(corners[i]);
                min = Vector2.Min(min, local);
                max = Vector2.Max(max, local);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private Vector2 CenterToPivotPosition(Vector2 center, Vector2 size) =>
            center + new Vector2((_rt.pivot.x - 0.5f) * size.x, (_rt.pivot.y - 0.5f) * size.y);

        private Vector2 LocalPointToAnchoredPosition(Vector2 localPoint)
        {
            var parentRect = _positionRoot.rect;
            var anchorCenter = (_rt.anchorMin + _rt.anchorMax) * 0.5f;
            var anchorPosition = new Vector2(
                Mathf.Lerp(parentRect.xMin, parentRect.xMax, anchorCenter.x),
                Mathf.Lerp(parentRect.yMin, parentRect.yMax, anchorCenter.y));

            return localPoint - anchorPosition;
        }

        private static Vector2 ClampPopupCenter(Vector2 center, Vector2 halfSize, Rect bounds)
        {
            center.x = bounds.width <= halfSize.x * 2f
                ? bounds.center.x
                : Mathf.Clamp(center.x, bounds.xMin + halfSize.x, bounds.xMax - halfSize.x);

            center.y = bounds.height <= halfSize.y * 2f
                ? bounds.center.y
                : Mathf.Clamp(center.y, bounds.yMin + halfSize.y, bounds.yMax - halfSize.y);

            return center;
        }

        private static Rect RectFromCenter(Vector2 center, Vector2 size) =>
            new Rect(center - size * 0.5f, size);

        private static Rect Expand(Rect rect, float amount)
        {
            rect.xMin -= amount;
            rect.xMax += amount;
            rect.yMin -= amount;
            rect.yMax += amount;
            return rect;
        }

        private static float ScoreCandidate(Rect popupRect, Rect nodeRect, Rect preferredClearRect, Rect boundsRect, Vector2 rawCenter)
        {
            var actualNodeOverlap = OverlapArea(popupRect, nodeRect);
            var preferredGapOverlap = OverlapArea(popupRect, preferredClearRect);
            var overflow = OverflowDistance(popupRect, boundsRect);
            var displacement = ((Vector2)popupRect.center - rawCenter).sqrMagnitude;

            return actualNodeOverlap * 1000000f
                   + preferredGapOverlap * 10000f
                   + overflow * 1000f
                   + displacement;
        }

        private static float OverlapArea(Rect a, Rect b)
        {
            var width = Mathf.Max(0f, Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin));
            var height = Mathf.Max(0f, Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin));
            return width * height;
        }

        private static float OverflowDistance(Rect rect, Rect bounds)
        {
            var left = Mathf.Max(0f, bounds.xMin - rect.xMin);
            var right = Mathf.Max(0f, rect.xMax - bounds.xMax);
            var bottom = Mathf.Max(0f, bounds.yMin - rect.yMin);
            var top = Mathf.Max(0f, rect.yMax - bounds.yMax);
            return left + right + bottom + top;
        }

        private void OnUpgradeClicked()
        {
            if (_current == null) return;

            var result = _service.TryUpgrade(_current.id);
            PlayUpgradeResultSound(result);
            Refresh();
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

        private void OnDestroy()
        {
            _nameBinding?.Dispose();
            _descriptionBinding?.Dispose();
        }
    }
}
