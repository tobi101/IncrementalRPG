using System.Linq;
using UDND;
using UDND.Core;
using UDND.Interaction;
using UDND.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.Inventory
{
    [DisallowMultipleComponent]
    public sealed class InventoryDragVisualAlignment : MonoBehaviour
    {
        private const float IconInset = 16f;

        private readonly Vector3[] _worldCorners = new Vector3[4];

        private Canvas _presentationCanvas;
        private RectTransform _activeVisual;
        private SourceSizedBaseDragVisual _equipmentVisual;

        private void Awake()
        {
            _presentationCanvas = GetComponent<Canvas>();
        }

        private void OnEnable()
        {
            UDNDEvents.OnDragStarted += HandleDragStarted;
            UDNDEvents.OnDragCancelled += HandleDragFinished;
            UDNDEvents.OnDropCompleted += HandleDragFinished;
        }

        private void OnDisable()
        {
            UDNDEvents.OnDragStarted -= HandleDragStarted;
            UDNDEvents.OnDragCancelled -= HandleDragFinished;
            UDNDEvents.OnDropCompleted -= HandleDragFinished;
        }

        private void LateUpdate()
        {
            if (_activeVisual != null)
                _activeVisual.position = ResolveScreenPosition();
        }

        private void HandleDragStarted(DragContext context)
        {
            var entry = context.Entries[0];
            SourceSizedBaseDragVisual dragVisual;
            if (entry.SourceInventory is EquipmentDragInventory source)
            {
                // UDND's per-inventory visual binder belongs to the bag component.
                // Equipment uses a transient source, so reuse the bag's visual prefab here.
                if (_equipmentVisual == null)
                {
                    var prefab = DragVisualPresenter.AutoCreateInstance.ResolveVisualPrefab(source.DataBinding.Inventory) as SourceSizedBaseDragVisual;
                    if (prefab == null) return;
                    _equipmentVisual = Instantiate(prefab, transform);
                }
                _equipmentVisual.Show(entry);
                dragVisual = _equipmentVisual;
            }
            else
            {
                dragVisual = GetComponentsInChildren<SourceSizedBaseDragVisual>(true)
                    .FirstOrDefault(v => v != _equipmentVisual && v.IsVisible);
            }
            if (dragVisual == null) return;
            _activeVisual = dragVisual.transform as RectTransform;

            var image = dragVisual.GetComponent<Image>();
            image.type = Image.Type.Simple;
            image.preserveAspect = true;

            var sourceRect = entry.SourceBaseSlot is EquipmentDragSlot equipment
                ? equipment.InventoryCell : entry.SourceBaseSlot.transform as RectTransform;
            sourceRect.GetWorldCorners(_worldCorners);

            var sourceCanvas = sourceRect.GetComponentInParent<Canvas>().rootCanvas;
            var sourceCamera = ResolveCanvasCamera(sourceCanvas);
            var min = RectTransformUtility.WorldToScreenPoint(sourceCamera, _worldCorners[0]);
            var max = min;
            for (var i = 1; i < _worldCorners.Length; i++)
            {
                var point = RectTransformUtility.WorldToScreenPoint(sourceCamera, _worldCorners[i]);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            var bounds = PlacementShapeUtility.GetBoundingSize(entry.Shape, 0, entry.OrientationTopology);
            var size = max - min;
            size = new Vector2(size.x * bounds.x, size.y * bounds.y) / _presentationCanvas.scaleFactor;
            size -= Vector2.one * IconInset;

            _activeVisual.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            _activeVisual.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            _activeVisual.position = ResolveScreenPosition();
        }

        private void HandleDragFinished(DragContext _)
        {
            _activeVisual = null;
            if (_equipmentVisual != null) _equipmentVisual.Hide();
        }

        private static Vector2 ResolveScreenPosition()
        {
            if (InputEventRouter.Instance.TryGetCurrentNavigationAnchor(out var selectedObject))
            {
                var selectedRect = selectedObject.transform as RectTransform;
                var selectedCanvas = selectedRect.GetComponentInParent<Canvas>().rootCanvas;
                return RectTransformUtility.WorldToScreenPoint(
                    ResolveCanvasCamera(selectedCanvas),
                    selectedRect.TransformPoint(selectedRect.rect.center));
            }

            return Mouse.current.position.ReadValue();
        }

        private static Camera ResolveCanvasCamera(Canvas canvas)
        {
            return canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;
        }
    }
}
