using Core.Items;
using UDND.Selection;
using UDND.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
    // One backdrop follows the existing pooled placement, including its full rotated footprint.
    [DefaultExecutionOrder(1000)]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class InventoryRarityBackdrop : MaskableGraphic
    {
        [SerializeField, Range(0f, 1f)] private float _tintStrength = .16f;
        [SerializeField, Min(.5f)] private float _borderWidth = 1.5f;
        private PlacementOverlayItem _item;
        private PlacementOverlay _overlay;
        private bool _hasItem;

        public Color FillColor { get; private set; }
        public Color BorderColor { get; private set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            raycastTarget = false;
            _item = GetComponentInParent<PlacementOverlayItem>();
            _overlay = GetComponentInParent<PlacementOverlay>();
            _hasItem = false;
            SetVerticesDirty();
        }

        private void LateUpdate()
        {
            var placement = _item != null ? _item.CurrentPlacement : null;
            var adapter = placement?.Stack?.PrimaryAdapter as GameItemAdapter;
            var hasItem = adapter != null;
            var rarityColor = hasItem ? EquipmentStats.RarityColor(adapter.Rarity) : Color.clear;
            var highlighted = false;
            if (hasItem && _overlay != null)
            {
                rectTransform.localRotation = Quaternion.Euler(0f, 0f, _overlay.GetRotation(placement));
                foreach (var slot in _overlay.CollectCoveredSlots(placement))
                {
                    if (slot is InventoryGridSlot gridSlot && gridSlot.IsHighlighted ||
                        SelectionManager.IsInstanceExist && SelectionManager.AutoCreateInstance.CurrentContext.Contains(slot))
                        highlighted = true;
                }
            }

            // An opaque, dark fill hides internal cell seams without tinting the artwork above it.
            var fill = Color.Lerp(new Color(.055f, .055f, .06f), rarityColor, _tintStrength);
            var border = highlighted ? new Color(.3f, .85f, 1f) : rarityColor * new Color(.75f, .75f, .75f, 1f);
            if (_hasItem == hasItem && FillColor == fill && BorderColor == border)
                return;
            _hasItem = hasItem;
            FillColor = fill;
            BorderColor = border;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (!_hasItem) return;
            var r = rectTransform.rect;
            var w = Mathf.Min(_borderWidth, Mathf.Min(r.width, r.height) * .5f);
            Quad(vh, r, FillColor);
            Quad(vh, new Rect(r.xMin, r.yMin, r.width, w), BorderColor);
            Quad(vh, new Rect(r.xMin, r.yMax - w, r.width, w), BorderColor);
            Quad(vh, new Rect(r.xMin, r.yMin + w, w, r.height - w * 2f), BorderColor);
            Quad(vh, new Rect(r.xMax - w, r.yMin + w, w, r.height - w * 2f), BorderColor);
        }

        private static void Quad(VertexHelper vh, Rect r, Color color)
        {
            var i = vh.currentVertCount;
            vh.AddVert(new Vector3(r.xMin, r.yMin), color, Vector2.zero);
            vh.AddVert(new Vector3(r.xMin, r.yMax), color, Vector2.up);
            vh.AddVert(new Vector3(r.xMax, r.yMax), color, Vector2.one);
            vh.AddVert(new Vector3(r.xMax, r.yMin), color, Vector2.right);
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }
    }
}
