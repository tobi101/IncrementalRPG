using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [DisallowMultipleComponent]
    public class MainMenuGlowAnimator : MonoBehaviour
    {
        [SerializeField] private Graphic _targetGraphic;
        [SerializeField] private RectTransform _targetTransform;
        [SerializeField] private Graphic _shadowGraphic;
        [SerializeField, Range(0f, 1f)] private float _minAlpha = 0.45f;
        [SerializeField, Range(0f, 1f)] private float _maxAlpha = 0.65f;
        [SerializeField, Range(0f, 1f)] private float _shadowMinAlpha = 0.65f;
        [SerializeField, Range(0f, 1f)] private float _shadowMaxAlpha = 0.85f;
        [SerializeField] private bool _invertShadowPulse = true;
        [SerializeField, Min(0.01f)] private float _cycleDuration = 4f;
        [SerializeField, Min(0f)] private float _scalePulse = 0.03f;
        [SerializeField] private bool _useUnscaledTime = true;
        [SerializeField] private bool _randomizeStartPhase = true;

        private Color _baseColor;
        private Color _shadowBaseColor;
        private Vector3 _baseScale;
        private float _phaseOffset;

        private void Reset()
        {
            _targetGraphic = GetComponent<Graphic>();
            _targetTransform = transform as RectTransform;
        }

        private void Awake()
        {
            if (_targetGraphic == null)
                _targetGraphic = GetComponent<Graphic>();

            if (_targetTransform == null)
                _targetTransform = transform as RectTransform;

            if (_targetGraphic != null)
                _baseColor = _targetGraphic.color;

            if (_shadowGraphic != null)
                _shadowBaseColor = _shadowGraphic.color;

            if (_targetTransform != null)
                _baseScale = _targetTransform.localScale;

            _phaseOffset = _randomizeStartPhase ? Random.value : 0f;
        }

        private void OnEnable()
        {
            ApplyPulse();
        }

        private void Update()
        {
            ApplyPulse();
        }

        private void ApplyPulse()
        {
            float time = _useUnscaledTime ? Time.unscaledTime : Time.time;
            float normalizedTime = Mathf.Repeat(time / _cycleDuration + _phaseOffset, 1f);
            float pulse = (Mathf.Sin(normalizedTime * Mathf.PI * 2f - Mathf.PI * 0.5f) + 1f) * 0.5f;

            if (_targetGraphic != null)
            {
                Color color = _baseColor;
                color.a = Mathf.Lerp(_minAlpha, _maxAlpha, pulse);
                _targetGraphic.color = color;
            }

            if (_shadowGraphic != null)
            {
                float shadowPulse = _invertShadowPulse ? 1f - pulse : pulse;
                Color color = _shadowBaseColor;
                color.a = Mathf.Lerp(_shadowMinAlpha, _shadowMaxAlpha, shadowPulse);
                _shadowGraphic.color = color;
            }

            if (_targetTransform != null && _scalePulse > 0f)
            {
                float scaleMultiplier = 1f + _scalePulse * pulse;
                _targetTransform.localScale = _baseScale * scaleMultiplier;
            }
        }
    }
}
