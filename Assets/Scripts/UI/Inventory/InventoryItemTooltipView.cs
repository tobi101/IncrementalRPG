using System;
using Core.Items;
using TMPro;
using UDND.Core;
using UDND.UI;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace UI.Inventory
{
    public sealed class InventoryItemTooltipView : BaseTooltipView
    {
        [SerializeField] private TMP_Text _itemName;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private Image _icon;
        [SerializeField] private RectTransform _divider;
        [SerializeField, Min(240f)] private float _preferredWidth = 440f;
        [SerializeField, Min(12f)] private float _padding = 24f;
        [SerializeField, Min(24f)] private float _iconSize = 64f;
        [SerializeField, Min(0f)] private float _contentGap = 16f;
        [SerializeField, Min(12f)] private float _bodyFontSize = 18f;
        [SerializeField, Min(14f)] private float _nameFontSize = 24f;
        private IItemAdapter _item;
        private Canvas _canvas;
        private Vector2Int _screenSize;
        private float _canvasScale;
        private readonly Vector3[] _corners = new Vector3[4];

        private void OnEnable() => LocalizationSettings.SelectedLocaleChanged += LocaleChanged;
        private void OnDisable() => LocalizationSettings.SelectedLocaleChanged -= LocaleChanged;
        private void LocaleChanged(Locale locale) { if (_item != null) SetContent(_item); }

        protected override void SetContent(IItemAdapter itemAdapter)
        {
            _item = itemAdapter;
            _itemName.text = itemAdapter.DisplayName;
            _description.text = itemAdapter is IDescribable describable ? describable.Description : string.Empty;
            _icon.sprite = itemAdapter.Icon;
            _icon.enabled = itemAdapter.Icon != null;
            _itemName.color = Color.white;
            if (itemAdapter is GameItemAdapter gameItem && gameItem.Definition.category == ItemCategory.Armor)
            {
                var color = EquipmentStats.RarityColor(gameItem.Rarity);
                var rarity = EquipmentStats.RarityName(gameItem.Rarity);
                _itemName.color = color;
                if (_description.text.StartsWith(rarity, StringComparison.Ordinal))
                    _description.text = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{rarity}</color>" +
                                        _description.text.Substring(rarity.Length);
            }
            RebuildLayout();
        }

        private void LateUpdate()
        {
            if (_item == null) return;
            if (_screenSize.x != Screen.width || _screenSize.y != Screen.height ||
                (_canvas != null && !Mathf.Approximately(_canvasScale, _canvas.scaleFactor)))
                RebuildLayout();
            ClampToScreen();
        }

        public override void UpdatePosition(Vector2 position)
        {
            base.UpdatePosition(position);
            ClampToScreen();
        }

        private void RebuildLayout()
        {
            _canvas ??= GetComponentInParent<Canvas>();
            _canvasScale = _canvas != null ? Mathf.Max(.01f, _canvas.scaleFactor) : 1f;
            _screenSize = new Vector2Int(Screen.width, Screen.height);
            var width = Mathf.Min(_preferredWidth, (Screen.width - 24f) / _canvasScale);
            var bodyWidth = width - _padding * 2f;
            var nameWidth = bodyWidth - _iconSize - _contentGap;
            _itemName.enableAutoSizing = _description.enableAutoSizing = false;
            _itemName.fontSize = _nameFontSize;
            _description.fontSize = _bodyFontSize;

            var headerHeight = Mathf.Max(_iconSize, _itemName.GetPreferredValues(_itemName.text, nameWidth, 10000f).y);
            var bodyHeight = _description.GetPreferredValues(_description.text, bodyWidth, 10000f).y;
            var availableHeight = (Screen.height - 24f) / _canvasScale;
            // Keep long, localized descriptions readable inside the viewport at smaller window sizes.
            while (_padding * 2f + headerHeight + _contentGap + bodyHeight > availableHeight && _description.fontSize > 14f)
            {
                _description.fontSize -= 1f;
                bodyHeight = _description.GetPreferredValues(_description.text, bodyWidth, 10000f).y;
            }

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _padding * 2f + headerHeight + _contentGap + bodyHeight);
            SetTopRect(_icon.rectTransform, _padding, _padding + (headerHeight - _iconSize) * .5f, _iconSize, _iconSize);
            SetTopRect(_itemName.rectTransform, _padding + _iconSize + _contentGap, _padding, nameWidth, headerHeight);
            SetTopRect(_description.rectTransform, _padding, _padding + headerHeight + _contentGap, bodyWidth, bodyHeight);
            if (_divider != null)
                SetTopRect(_divider, _padding, _padding + headerHeight + _contentGap * .5f, bodyWidth, 1f);
            _icon.preserveAspect = true;
            _itemName.ForceMeshUpdate();
            _description.ForceMeshUpdate();
        }

        private static void SetTopRect(RectTransform target, float left, float top, float width, float height)
        {
            target.anchorMin = target.anchorMax = target.pivot = new Vector2(0f, 1f);
            target.anchoredPosition = new Vector2(left, -top);
            target.sizeDelta = new Vector2(width, height);
        }

        private void ClampToScreen()
        {
            // This tooltip uses the inventory's Screen Space Overlay canvas.
            rectTransform.GetWorldCorners(_corners);
            var shift = Vector3.zero;
            if (_corners[0].x < 12f) shift.x = 12f - _corners[0].x;
            else if (_corners[2].x > Screen.width - 12f) shift.x = Screen.width - 12f - _corners[2].x;
            if (_corners[0].y < 12f) shift.y = 12f - _corners[0].y;
            else if (_corners[2].y > Screen.height - 12f) shift.y = Screen.height - 12f - _corners[2].y;
            rectTransform.position += shift;
        }
    }
}
