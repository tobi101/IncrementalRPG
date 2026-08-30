using UDND.Slots;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
    public sealed class InventoryGridSlot : BaseSlot
    {
        private static readonly Color HighlightColor = new(0.3f, 0.85f, 1f, 1f);

        private Image _background;
        private Color _normalColor;

        public void Configure(Image background)
        {
            _background = background;
            _normalColor = background.color;
        }

        protected override void RenderFilled()
        {
            if (!_isHighlighted)
                _background.color = _normalColor;
        }

        protected override void RenderEmpty()
        {
            if (!_isHighlighted)
                _background.color = _normalColor;
        }

        public override void Highlight(bool highlight)
        {
            _isHighlighted = highlight;
            _background.color = highlight ? HighlightColor : _normalColor;
        }
    }
}
