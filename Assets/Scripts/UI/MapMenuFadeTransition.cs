using System;
using System.Collections;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class MapMenuFadeTransition : MonoBehaviour
    {
        private static readonly int AlphaPropertyId = Shader.PropertyToID("_Alpha");
        private static readonly int FadeAmountPropertyId = Shader.PropertyToID("_FadeAmount");

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField, Min(0f)] private float _fadeInDuration = 0.35f;
        [SerializeField, Min(0f)] private float _burnDuration = 0.65f;
        [SerializeField] private float _fadeAmountStart = -0.1f;
        [SerializeField] private float _fadeAmountEnd = 1f;
        [SerializeField] private bool _useUnscaledTime = true;

        private Material _material;
        private Coroutine _transitionCoroutine;

        public bool IsPlaying => _transitionCoroutine != null;

        private void Reset()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Awake()
        {
            Initialize();
            SetHiddenState();
        }

        public void Play(Action onCovered, Action onFinished = null)
        {
            if (IsPlaying)
                return;

            Initialize();

            if (_spriteRenderer == null)
            {
                onCovered?.Invoke();
                onFinished?.Invoke();
                return;
            }

            _transitionCoroutine = StartCoroutine(PlayRoutine(onCovered, onFinished));
        }

        private void Initialize()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_spriteRenderer != null && _material == null)
                _material = _spriteRenderer.material;
        }

        private IEnumerator PlayRoutine(Action onCovered, Action onFinished)
        {
            _spriteRenderer.enabled = true;
            SetAlpha(0f);
            SetFadeAmount(_fadeAmountStart);

            yield return AnimateAlpha(0f, 1f, _fadeInDuration);

            onCovered?.Invoke();

            yield return AnimateFadeAmount(_fadeAmountStart, _fadeAmountEnd, _burnDuration);

            SetHiddenState();
            _transitionCoroutine = null;
            onFinished?.Invoke();
        }

        private IEnumerator AnimateAlpha(float from, float to, float duration)
        {
            yield return AnimateValue(from, to, duration, SetAlpha);
        }

        private IEnumerator AnimateFadeAmount(float from, float to, float duration)
        {
            yield return AnimateValue(from, to, duration, SetFadeAmount);
        }

        private IEnumerator AnimateValue(float from, float to, float duration, Action<float> setter)
        {
            if (duration <= 0f)
            {
                setter(to);
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += GetDeltaTime();
                var progress = Mathf.Clamp01(elapsed / duration);
                setter(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, progress)));
                yield return null;
            }

            setter(to);
        }

        private float GetDeltaTime()
        {
            return _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        private void SetHiddenState()
        {
            SetAlpha(0f);
            SetFadeAmount(_fadeAmountStart);

            if (_spriteRenderer != null)
                _spriteRenderer.enabled = false;
        }

        private void SetAlpha(float value)
        {
            if (_material != null && _material.HasProperty(AlphaPropertyId))
            {
                _material.SetFloat(AlphaPropertyId, value);
                return;
            }

            if (_spriteRenderer == null)
                return;

            var color = _spriteRenderer.color;
            color.a = value;
            _spriteRenderer.color = color;
        }

        private void SetFadeAmount(float value)
        {
            if (_material != null && _material.HasProperty(FadeAmountPropertyId))
                _material.SetFloat(FadeAmountPropertyId, value);
        }

        private void OnDestroy()
        {
            if (_material != null)
                Destroy(_material);
        }
    }
}
