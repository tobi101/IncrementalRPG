using UnityEngine;
using UnityEngine.Tilemaps;

namespace Core.Gameplay
{
    [DisallowMultipleComponent]
    public class TilemapCameraAutoFitter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Tilemap targetTilemap;
        [SerializeField] private Camera targetCamera;

        [Header("Lava Animation")]
        [Tooltip("Extra distance below the camera bottom edge where lava starts.")]
        [SerializeField] private float lavaOffScreenMargin = 1f;

        [Header("Lava Decoration")]
        [SerializeField] private Transform lavaRoot;
        [Tooltip("Camera orthographic size at which lava looks correct (set this after running Generate at reference map size).")]
        [SerializeField] private float lavaReferenceOrthoSize = 5f;
        [Tooltip("Constant vertical offset in world units. Does not change with map size.")]
        [SerializeField] private float lavaVerticalOffsetFixed = 0f;
        [Tooltip("Vertical offset multiplied by the scale factor. Compensates for lava pivot not being at visual center.")]
        [SerializeField] private float lavaVerticalOffsetScaled = 0f;

        [Header("Fit")]
        [Tooltip("How much of the viewport (0..1) the generated map should occupy. 1 = tight fit, 0.8 = more margins.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float viewportFill = 0.85f;
        [Min(0.01f)]
        [SerializeField] private float minOrthographicSize = 2f;
        [Min(0.01f)]
        [SerializeField] private float maxOrthographicSize = 100f;
        [SerializeField] private bool keepCurrentCameraZ = true;
        [Tooltip("Vertical offset as a fraction of the tilemap height. Negative = shift camera down (map appears lower in viewport).")]
        [Range(-0.5f, 0.5f)]
        [SerializeField] private float normalizedVerticalOffset = 0f;

        private float _lavaStartY;
        private float _lavaEndY;

        [ContextMenu("Fit Camera To Tilemap")]
        public void FitToTilemap()
        {
            if (targetTilemap == null)
            {
                Debug.LogError("[TilemapCameraAutoFitter] Target Tilemap is not assigned.");
                return;
            }

            var cameraToFit = targetCamera != null ? targetCamera : Camera.main;
            if (cameraToFit == null)
            {
                Debug.LogError("[TilemapCameraAutoFitter] Target Camera is not assigned and Camera.main was not found.");
                return;
            }

            if (!cameraToFit.orthographic)
            {
                Debug.LogError("[TilemapCameraAutoFitter] Camera must be Orthographic.");
                return;
            }

            if (!TryGetWorldBounds(out var worldBounds))
            {
                Debug.LogWarning("[TilemapCameraAutoFitter] Tilemap bounds are empty. Nothing to fit.");
                return;
            }

            var safeAspect = Mathf.Max(0.0001f, cameraToFit.aspect);
            var baseSizeByHeight = worldBounds.extents.y;
            var baseSizeByWidth = worldBounds.extents.x / safeAspect;
            var orthographicSize = Mathf.Max(baseSizeByHeight, baseSizeByWidth);
            orthographicSize /= Mathf.Max(0.01f, viewportFill);
            orthographicSize = Mathf.Clamp(orthographicSize, minOrthographicSize, maxOrthographicSize);

            var nextPosition = worldBounds.center;
            nextPosition.y += orthographicSize * normalizedVerticalOffset;
            if (keepCurrentCameraZ)
            {
                nextPosition.z = cameraToFit.transform.position.z;
            }

            cameraToFit.transform.position = nextPosition;
            cameraToFit.orthographicSize = orthographicSize;

            FitLavaDecoration(worldBounds, orthographicSize);

            Debug.Log($"[TilemapCameraAutoFitter] Camera fitted. Size: {orthographicSize:F2}, Fill: {viewportFill:F2}, Center: {worldBounds.center}.");
        }

        private void FitLavaDecoration(Bounds worldBounds, float orthographicSize)
        {
            if (lavaRoot == null || lavaReferenceOrthoSize < 0.01f)
                return;

            var scale = orthographicSize / lavaReferenceOrthoSize;
            lavaRoot.localScale = Vector3.one * scale;
            var verticalOffset = lavaVerticalOffsetFixed + lavaVerticalOffsetScaled * scale;
            lavaRoot.position = new Vector3(
                worldBounds.center.x,
                worldBounds.center.y + verticalOffset,
                lavaRoot.position.z);
        }

        public void PrepareLavaAnimation()
        {
            if (lavaRoot == null) return;

            var cam = targetCamera != null ? targetCamera : Camera.main;
            _lavaEndY = lavaRoot.position.y;
            _lavaStartY = cam.transform.position.y - cam.orthographicSize - lavaOffScreenMargin;

            lavaRoot.position = new Vector3(lavaRoot.position.x, _lavaStartY, lavaRoot.position.z);
        }

        public void AnimateLava(float progress)
        {
            if (lavaRoot == null) return;

            var y = Mathf.Lerp(_lavaStartY, _lavaEndY, progress);
            lavaRoot.position = new Vector3(lavaRoot.position.x, y, lavaRoot.position.z);
        }

        private bool TryGetWorldBounds(out Bounds bounds)
        {
            var tilemapRenderer = targetTilemap.GetComponent<TilemapRenderer>();
            if (tilemapRenderer != null)
            {
                bounds = tilemapRenderer.bounds;
                if (bounds.size.sqrMagnitude > 0f)
                {
                    return true;
                }
            }

            var cellBounds = targetTilemap.cellBounds;
            if (cellBounds.size.x <= 0 || cellBounds.size.y <= 0)
            {
                bounds = default;
                return false;
            }

            var min = targetTilemap.CellToWorld(new Vector3Int(cellBounds.xMin, cellBounds.yMin, 0));
            var max = targetTilemap.CellToWorld(new Vector3Int(cellBounds.xMax, cellBounds.yMax, 0));
            bounds = new Bounds((min + max) * 0.5f, new Vector3(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y), 0f));

            return bounds.size.sqrMagnitude > 0f;
        }
    }
}