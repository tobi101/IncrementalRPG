using UnityEngine;

namespace Core.Gameplay.Bomb
{
    public class BombExplosionDebugView : MonoBehaviour
    {
        private const int Segments = 32;

        public void Show(Vector3 center, float radiusX, float radiusY, float duration)
        {
            var lr = gameObject.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = Segments;
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.red;
            lr.endColor = Color.red;

            for (var i = 0; i < Segments; i++)
            {
                var angle = 2f * Mathf.PI * i / Segments;
                var x = center.x + radiusX * Mathf.Cos(angle);
                var y = center.y + radiusY * Mathf.Sin(angle);
                lr.SetPosition(i, new Vector3(x, y, center.z));
            }

            Destroy(gameObject, duration);
        }
    }
}
