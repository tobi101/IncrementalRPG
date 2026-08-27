using UnityEngine;

namespace Entity
{
    public sealed class ShineSweepAnimator : MonoBehaviour
    {
        private static readonly int ShineLocationId = Shader.PropertyToID("_ShineLocation");

        private const float HiddenLocation = -1f;
        private const float StartLocation = -0.05f;
        private const float EndLocation = 1.05f;

        [SerializeField] private Renderer _renderer;
        [SerializeField] private float _sweepDuration = 0.6f;
        [SerializeField] private float _pauseDuration = 3.5f;
        [SerializeField] private float _initialDelay;

        private MaterialPropertyBlock _propertyBlock;
        private float _elapsedTime;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            _elapsedTime = -_initialDelay;
            SetShineLocation(HiddenLocation);
        }

        private void Update()
        {
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime < 0f)
                return;

            var cycleTime = Mathf.Repeat(_elapsedTime, _sweepDuration + _pauseDuration);
            var shineLocation = cycleTime < _sweepDuration
                ? Mathf.Lerp(StartLocation, EndLocation, cycleTime / _sweepDuration)
                : HiddenLocation;

            SetShineLocation(shineLocation);
        }

        private void SetShineLocation(float location)
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(ShineLocationId, location);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
