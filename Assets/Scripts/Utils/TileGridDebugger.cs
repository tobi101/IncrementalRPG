using UnityEngine;
using UnityEngine.Tilemaps;

namespace Utils
{
    public class TileGridDebugger : MonoBehaviour
    {
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private int _previewSize = 4;
        [SerializeField] private float _sphereRadius = 0.05f;

        private void OnDrawGizmos()
        {
            if (_tilemap == null) return;

            for (var x = 0; x < _previewSize; x++)
            for (var y = 0; y < _previewSize; y++)
            {
                var corner = _tilemap.GetCellCenterWorld(new Vector3Int(x, y, 0));

                var p00 = _tilemap.GetCellCenterWorld(new Vector3Int(x, y, 0));
                var p11 = _tilemap.GetCellCenterWorld(new Vector3Int(x + 1, y + 1, 0));
                var center = (p00 + p11) * 0.5f;

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(corner, _sphereRadius);

                Gizmos.color = Color.green;
                Gizmos.DrawSphere(center, _sphereRadius);
            }
        }
    }
}
