using UnityEngine;

namespace Core.Gameplay.Shards
{
    public sealed class ShardPickupView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _icon;
        [SerializeField] private SpriteRenderer _glow;
        [SerializeField] private Color _collectedColor = Color.white;
        [SerializeField, Min(0f)] private float _glowPulseAmount = 0.08f;
        [SerializeField, Min(0f)] private float _glowPulseSpeed = 4f;

        private Color _iconBaseColor = Color.white;
        private Color _glowBaseColor = new Color(0.35f, 0.2f, 1f, 0.35f);
        private Vector3 _glowBaseScale = Vector3.one;

        private void Awake()
        {
            CacheBaseVisuals();
        }

        public void Prepare(Vector3 worldPosition)
        {
            CacheBaseVisuals();
            transform.position = worldPosition;
            SetVisualProgress(0f, 0f);
        }

        public void SetWorldPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public void SetVisualProgress(float collectionProgress, float elapsedLifetime)
        {
            collectionProgress = Mathf.Clamp01(collectionProgress);

            if (_icon != null)
                _icon.color = Color.Lerp(_iconBaseColor, _collectedColor, collectionProgress);

            if (_glow == null)
                return;

            var pulse = 1f + Mathf.Sin(elapsedLifetime * _glowPulseSpeed) * _glowPulseAmount;
            _glow.transform.localScale = _glowBaseScale * pulse;
            _glow.color = Color.Lerp(_glowBaseColor, Color.white, collectionProgress);
        }

        public void ResetForPool()
        {
            SetVisualProgress(0f, 0f);
        }

        private void CacheBaseVisuals()
        {
            if (_icon != null)
                _iconBaseColor = _icon.color;

            if (_glow != null)
            {
                _glowBaseColor = _glow.color;
                _glowBaseScale = _glow.transform.localScale;
            }
        }
    }
}
