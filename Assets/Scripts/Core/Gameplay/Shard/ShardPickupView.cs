using UnityEngine;

namespace Core.Gameplay.Shards
{
    public sealed class ShardPickupView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _icon;

        private static readonly int ShineLocation = Shader.PropertyToID("_ShineLocation");
        private static readonly int ShineGlow = Shader.PropertyToID("_ShineGlow");
        private const float LiftHeight = 0.06f;
        private const float LiftSpeed = 0.4f;
        private const float ShineIntensity = 0.3f;

        private MaterialPropertyBlock _properties;
        private Vector3 _iconBasePosition;
        private float _lift;

        public SpriteRenderer Icon => _icon;

        private void Awake()
        {
            _properties = new MaterialPropertyBlock();
            _iconBasePosition = _icon.transform.localPosition;
        }

        public void Prepare(Vector3 worldPosition)
        {
            transform.position = worldPosition;
            ResetForPool();
        }

        public void SetWorldPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public void SetCollectionProgress(float progress, float deltaTime)
        {
            _properties.SetFloat(ShineLocation, progress);
            _properties.SetFloat(ShineGlow, progress > 0f && progress < 1f ? ShineIntensity : 0f);
            _icon.SetPropertyBlock(_properties);

            _lift = Mathf.MoveTowards(_lift, Mathf.SmoothStep(0f, LiftHeight, progress), LiftSpeed * deltaTime);
            _icon.transform.localPosition = _iconBasePosition + Vector3.up * _lift;
        }

        public void ResetForPool()
        {
            _lift = 0f;
            _icon.transform.localPosition = _iconBasePosition;
            _properties.Clear();
            _icon.SetPropertyBlock(_properties);
        }
    }
}
