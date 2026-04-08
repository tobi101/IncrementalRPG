using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.TestSkillTree.View
{
    // Single instance on the Canvas (outside Content so it is unaffected by zoom).
    public class NodePopupView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _costText;

        private const float PopupGap = 15f;

        private SkillTreeService _service;
        private NodeDefinition   _current;
        private RectTransform    _rt;
        private Canvas           _canvas;

        private bool _nodeHovered;
        private bool _popupHovered;
        private bool _blocked;
        private Coroutine _hideCoroutine;

        public void Bind(SkillTreeService service)
        {
            _service = service;
            _rt      = (RectTransform)transform;
            _canvas  = GetComponentInParent<Canvas>();

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
            if (!gameObject.activeInHierarchy) return;
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

        public void Refresh(NodeDefinition definition)
        {
            _current = definition;
            Refresh();
        }

        private void Refresh()
        {
            _nameText.text = _current.displayName;

            var level      = _service.GetLevel(_current.id);
            var statsBlock = BuildEffectsText(_current, level);

            _descriptionText.text = string.IsNullOrEmpty(_current.description)
                ? statsBlock
                : $"{_current.description}\n\n{statsBlock}";

            if (_costText != null)
            {
                var cost = _service.GetUpgradeCost(_current.id);
                _costText.text = cost > 0 ? $"{cost}" : "";
            }
        }

        private static string BuildEffectsText(NodeDefinition def, int level)
        {
            var sb = new System.Text.StringBuilder();

            foreach (var effect in def.effects)
            {
                switch (effect.effectType)
                {
                    case NodeEffectType.Additive:
                    {
                        var current = SumValues(effect.valuesPerLevel, level);
                        var statName = GetStatName(effect.statType);

                        if (level >= def.maxLevel)
                        {
                            sb.AppendLine($"{statName}: +{current} (макс.)");
                        }
                        else
                        {
                            var next = current + GetValueAt(effect.valuesPerLevel, level);
                            sb.AppendLine(level == 0
                                ? $"{statName}: +{next}"
                                : $"{statName}: +{current} → +{next}");
                        }
                        break;
                    }
                    case NodeEffectType.Multiplicative:
                    {
                        var currentPct = Mathf.RoundToInt(SumValues(effect.valuesPerLevel, level) * 100f);
                        var statName   = GetStatName(effect.statType);

                        if (level >= def.maxLevel)
                        {
                            sb.AppendLine($"{statName}: +{currentPct}% (макс.)");
                        }
                        else
                        {
                            var nextPct = Mathf.RoundToInt((SumValues(effect.valuesPerLevel, level) + GetValueAt(effect.valuesPerLevel, level)) * 100f);
                            sb.AppendLine(level == 0
                                ? $"{statName}: +{nextPct}%"
                                : $"{statName}: +{currentPct}% → +{nextPct}%");
                        }
                        break;
                    }
                    case NodeEffectType.FeatureUnlock:
                        sb.AppendLine($"Разблокирует: {GetFeatureName(effect.feature)}");
                        break;
                }
            }

            return sb.ToString().TrimEnd();
        }

        private static float SumValues(float[] values, int level)
        {
            var sum = 0f;
            for (var i = 0; i < level && i < values.Length; i++)
                sum += values[i];
            return sum;
        }

        private static float GetValueAt(float[] values, int index)
            => index < values.Length ? values[index] : 0f;

        private static string GetStatName(StatType stat) => stat switch
        {
            StatType.ZoneRadius    => "Радиус зоны",
            StatType.ZoneDamage    => "Урон зоны",
            StatType.AttackSpeed   => "Скорость атаки",
            StatType.SpawnSpeed    => "Скорость спауна",
            StatType.SpawnCountMax => "Существ на старте",
            StatType.MapSize       => "Размер арены",
            _                      => stat.ToString(),
        };

        private static string GetFeatureName(GameFeature feature) => feature switch
        {
            GameFeature.Bombs => "Бомбы",
            _                 => feature.ToString(),
        };

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
    }
}
