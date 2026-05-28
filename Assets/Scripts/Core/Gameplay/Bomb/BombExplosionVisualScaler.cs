using UnityEngine;

namespace Core.Gameplay.Bomb
{
    public class BombExplosionVisualScaler : MonoBehaviour
    {
        [SerializeField] private Transform _explosionVisual;

        private Vector3 _baseScale;
        private bool _hasBaseScale;

        private void Awake()
        {
            CaptureBaseScale();
        }

        public void ScaleToRadius(float radius, float baseRadius)
        {
            CaptureBaseScale();
            if (_explosionVisual == null) return;

            var scaleMultiplier = baseRadius > 0f
                ? Mathf.Max(0f, radius / baseRadius)
                : 1f;

            _explosionVisual.localScale = new Vector3(
                _baseScale.x * scaleMultiplier,
                _baseScale.y * scaleMultiplier,
                _baseScale.z
            );
        }

        private void CaptureBaseScale()
        {
            if (_hasBaseScale) return;

            if (_explosionVisual == null)
                _explosionVisual = transform;

            _baseScale = _explosionVisual.localScale;
            _hasBaseScale = true;
        }
    }
}
