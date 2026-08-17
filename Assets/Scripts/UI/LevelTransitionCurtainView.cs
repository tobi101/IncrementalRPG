using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public sealed class LevelTransitionCurtainView : MonoBehaviour
    {
        public event Action OpeningStarted;
        public event Action LampAnimationStarted;
        public event Action LampAnimationCompleted;

        [SerializeField, FormerlySerializedAs("_bottomCurtain")]
        private RectTransform _leftCurtain;

        [SerializeField, FormerlySerializedAs("_topCurtain")]
        private RectTransform _rightCurtain;

        [SerializeField] private RectTransform _curtainViewport;
        [SerializeField] private CanvasGroup _rootGroup;

        [SerializeField, FormerlySerializedAs("_messageGroup")]
        private CanvasGroup _revealGroup;

        [SerializeField] private LevelTransitionLampCounterView _lampCounter;
        [SerializeField] private AnimationCurve _movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField, Min(0f)] private float _revealFadeInDuration = 0.2f;
        [SerializeField, Min(0f)] private float _lampAnimationDelay = 0.2f;
        [SerializeField, Min(0f)] private float _revealFadeOutDuration = 0.2f;
        [SerializeField, Min(0f)] private float _offscreenPadding = 8f;
        [SerializeField, Min(0f)] private float _closedOverlap = 80f;
        [SerializeField] private float _seamOffset;
        [SerializeField] private bool _hideWhenIdle;

        private Coroutine _routine;
        private Vector2 _leftReferencePosition;
        private Vector2 _rightReferencePosition;
        private Vector2 _leftClosedPosition;
        private Vector2 _rightClosedPosition;
        private Vector2 _leftOpenPosition;
        private Vector2 _rightOpenPosition;
        private bool _referencePositionsCached;
        private bool _isPreparingGeometry;
        private bool _isTransitionActive;
        private bool _isPaused;

        private void Awake()
        {
            if (_rootGroup == null)
                _rootGroup = GetComponent<CanvasGroup>();

            if (_curtainViewport == null)
                _curtainViewport = transform as RectTransform;

            CacheReferencePositions();
            PrepareCurtainGeometry();
            SetRevealAlpha(0f);
            SetCurtainProgress(0f);
            SetRootVisible(false);

            if (_lampCounter != null)
            {
                _lampCounter.TurnOnAnimationStarted += HandleLampAnimationStarted;
                _lampCounter.TurnOnAnimationCompleted += HandleLampAnimationCompleted;
            }
        }

        private void OnDestroy()
        {
            if (_lampCounter == null)
                return;

            _lampCounter.TurnOnAnimationStarted -= HandleLampAnimationStarted;
            _lampCounter.TurnOnAnimationCompleted -= HandleLampAnimationCompleted;
        }

        private void OnDisable()
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = null;
            _isTransitionActive = false;
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!_referencePositionsCached || _isTransitionActive || _isPreparingGeometry)
                return;

            PrepareCurtainGeometry();
            SetCurtainProgress(0f);
        }

        public void Prepare(int levelCount, int newlyCompletedLevelIndex)
        {
            gameObject.SetActive(true);

            if (_routine != null)
                StopCoroutine(_routine);

            _routine = null;
            _isTransitionActive = true;
            CacheReferencePositions();
            PrepareCurtainGeometry();
            _lampCounter.Prepare(levelCount, newlyCompletedLevelIndex);
            SetRootVisible(true);
            SetRevealAlpha(0f);
            SetInteractionEnabled(false);
            SetCurtainProgress(0f);
        }

        public void PlayClose(float duration, Action completed)
        {
            StartPhase(CloseRoutine(Mathf.Max(0f, duration), completed));
        }

        public void PlayReveal()
        {
            StartPhase(RevealRoutine());
        }

        public void PlayOpen(float duration, Action completed)
        {
            SetInteractionEnabled(false);
            OpeningStarted?.Invoke();
            StartPhase(OpenRoutine(Mathf.Max(0f, duration), completed));
        }

        public void SetInteractionEnabled(bool enabled)
        {
            if (_rootGroup != null)
                _rootGroup.interactable = enabled;

            if (_revealGroup != null)
            {
                _revealGroup.interactable = enabled;
                _revealGroup.blocksRaycasts = enabled;
            }

        }

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;
            _lampCounter?.SetPaused(isPaused);
        }

        public void HideImmediately()
        {
            _isPaused = false;
            _isTransitionActive = false;
            _lampCounter?.SetPaused(false);

            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            CacheReferencePositions();
            PrepareCurtainGeometry();
            SetRevealAlpha(0f);
            SetInteractionEnabled(false);
            SetCurtainProgress(0f);
            SetRootVisible(false);

            if (_hideWhenIdle)
                gameObject.SetActive(false);
        }

        private void StartPhase(IEnumerator phase)
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(phase);
        }

        private IEnumerator CloseRoutine(float duration, Action completed)
        {
            yield return AnimateCurtains(0f, 1f, duration);
            _routine = null;
            completed?.Invoke();
        }

        private IEnumerator RevealRoutine()
        {
            yield return FadeReveal(0f, 1f, _revealFadeInDuration);

            var elapsed = 0f;
            while (elapsed < _lampAnimationDelay)
            {
                elapsed += GetGameplayDeltaTime();
                yield return null;
            }

            _routine = null;
            _lampCounter.PlayNewlyCompleted();
        }

        private IEnumerator OpenRoutine(float duration, Action completed)
        {
            yield return AnimateOpening(duration);
            SetRootVisible(false);
            _isTransitionActive = false;
            _routine = null;

            if (_hideWhenIdle)
                gameObject.SetActive(false);

            completed?.Invoke();
        }

        private IEnumerator AnimateCurtains(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                SetCurtainProgress(to);
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += GetGameplayDeltaTime();
                var t = Mathf.Clamp01(elapsed / duration);
                SetCurtainProgress(Mathf.Lerp(from, to, t));
                yield return null;
            }

            SetCurtainProgress(to);
        }

        private IEnumerator FadeReveal(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                SetRevealAlpha(to);
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += GetGameplayDeltaTime();
                SetRevealAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }

            SetRevealAlpha(to);
        }

        private IEnumerator AnimateOpening(float duration)
        {
            if (duration <= 0f)
            {
                SetRevealAlpha(0f);
                SetCurtainProgress(0f);
                yield break;
            }

            var fadeDuration = Mathf.Min(_revealFadeOutDuration, duration);
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += GetGameplayDeltaTime();
                var t = Mathf.Clamp01(elapsed / duration);
                SetCurtainProgress(Mathf.Lerp(1f, 0f, t));

                var revealAlpha = fadeDuration <= 0f
                    ? 0f
                    : 1f - Mathf.Clamp01(elapsed / fadeDuration);
                SetRevealAlpha(revealAlpha);
                yield return null;
            }

            SetRevealAlpha(0f);
            SetCurtainProgress(0f);
        }

        private void HandleLampAnimationStarted()
        {
            LampAnimationStarted?.Invoke();
        }

        private void HandleLampAnimationCompleted()
        {
            LampAnimationCompleted?.Invoke();
        }

        private float GetGameplayDeltaTime()
        {
            return _isPaused ? 0f : Time.deltaTime;
        }

        private void SetCurtainProgress(float progress)
        {
            var t = Mathf.Clamp01(progress);
            var eased = _movementCurve != null ? _movementCurve.Evaluate(t) : t;

            if (_leftCurtain != null)
            {
                _leftCurtain.anchoredPosition = Vector2.LerpUnclamped(
                    _leftOpenPosition,
                    _leftClosedPosition,
                    eased);
            }

            if (_rightCurtain != null)
            {
                _rightCurtain.anchoredPosition = Vector2.LerpUnclamped(
                    _rightOpenPosition,
                    _rightClosedPosition,
                    eased);
            }
        }

        private void CacheReferencePositions()
        {
            if (_referencePositionsCached)
                return;

            if (_leftCurtain != null)
                _leftReferencePosition = _leftCurtain.anchoredPosition;

            if (_rightCurtain != null)
                _rightReferencePosition = _rightCurtain.anchoredPosition;

            _referencePositionsCached = true;
        }

        private void PrepareCurtainGeometry()
        {
            if (!_referencePositionsCached || _curtainViewport == null || _isPreparingGeometry)
                return;

            _isPreparingGeometry = true;

            if (_leftCurtain != null)
                _leftCurtain.anchoredPosition = _leftReferencePosition;

            if (_rightCurtain != null)
                _rightCurtain.anchoredPosition = _rightReferencePosition;

            Canvas.ForceUpdateCanvases();
            CalculateClosedPositions();

            if (_leftCurtain != null)
                _leftCurtain.anchoredPosition = _leftClosedPosition;

            if (_rightCurtain != null)
                _rightCurtain.anchoredPosition = _rightClosedPosition;

            Canvas.ForceUpdateCanvases();
            _leftOpenPosition = GetOpenPosition(_leftCurtain, _leftClosedPosition, -1f);
            _rightOpenPosition = GetOpenPosition(_rightCurtain, _rightClosedPosition, 1f);
            _isPreparingGeometry = false;
        }

        private void CalculateClosedPositions()
        {
            var seamPosition = _curtainViewport.rect.center.x + _seamOffset;
            var halfOverlap = Mathf.Max(0f, _closedOverlap) * 0.5f;

            if (_leftCurtain != null)
            {
                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
                    _curtainViewport,
                    _leftCurtain);
                var offset = seamPosition + halfOverlap - bounds.max.x;
                _leftClosedPosition = _leftReferencePosition + Vector2.right * offset;
            }

            if (_rightCurtain != null)
            {
                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
                    _curtainViewport,
                    _rightCurtain);
                var offset = seamPosition - halfOverlap - bounds.min.x;
                _rightClosedPosition = _rightReferencePosition + Vector2.right * offset;
            }
        }

        private Vector2 GetOpenPosition(RectTransform curtain, Vector2 closedPosition, float direction)
        {
            if (curtain == null || _curtainViewport == null)
                return closedPosition;

            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
                _curtainViewport,
                curtain);

            var offset = direction < 0f
                ? _curtainViewport.rect.xMin - bounds.max.x - _offscreenPadding
                : _curtainViewport.rect.xMax - bounds.min.x + _offscreenPadding;

            return closedPosition + Vector2.right * offset;
        }

        private void SetRootVisible(bool visible)
        {
            if (_rootGroup == null)
                return;

            _rootGroup.alpha = visible ? 1f : 0f;
            _rootGroup.interactable = false;
            _rootGroup.blocksRaycasts = visible;
        }

        private void SetRevealAlpha(float alpha)
        {
            if (_revealGroup != null)
                _revealGroup.alpha = Mathf.Clamp01(alpha);
        }
    }
}
