using TMPro;
using UI.Localization;
using UnityEngine;
using UnityEngine.Localization;
using Utils;

namespace UI
{
    public sealed class EnemyRewardTooltipView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _enemyName;
        [SerializeField] private TMP_Text _gold;
        [SerializeField] private TMP_Text _xp;
        [SerializeField] private float _gap = 4f;

        private LocalizedStringBinding _nameBinding;

        public void Show(LocalizedString enemyName, BigDouble gold, BigDouble xp, RectTransform icon)
        {
            _nameBinding ??= new LocalizedStringBinding(_enemyName);
            _nameBinding.Bind(enemyName);
            _gold.text = BigDoubleFormatter.Format(gold);
            _xp.text = BigDoubleFormatter.Format(xp);
            PositionAt(icon);
            gameObject.SetActive(true);
        }

        private void PositionAt(RectTransform icon)
        {
            var card = (RectTransform)transform;
            var panel = (RectTransform)card.parent;
            var left = panel.InverseTransformPoint(icon.TransformPoint(new Vector2(icon.rect.xMin, icon.rect.center.y)));
            var right = panel.InverseTransformPoint(icon.TransformPoint(new Vector2(icon.rect.xMax, icon.rect.center.y)));
            var halfWidth = card.rect.width * 0.5f;
            var halfHeight = card.rect.height * 0.5f;
            var moreRoomOnLeft = left.x - panel.rect.xMin >= panel.rect.xMax - right.x;
            var x = moreRoomOnLeft ? left.x - _gap - halfWidth : right.x + _gap + halfWidth;
            card.localPosition = new Vector3(
                Mathf.Clamp(x, panel.rect.xMin + halfWidth, panel.rect.xMax - halfWidth),
                Mathf.Clamp(left.y, panel.rect.yMin + halfHeight, panel.rect.yMax - halfHeight), 0f);
        }

        public void Hide() => gameObject.SetActive(false);

        private void OnDisable() => _nameBinding?.Clear();
    }
}
