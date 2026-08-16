using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public sealed class LevelTransitionCurtainView : MonoBehaviour
    {
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
        }

        private void OnDisable()
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = null;
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!_referencePositionsCached || _routine != null || _isPreparingGeometry)
                return;

            PrepareCurtainGeometry();
            SetCurtainProgress(0f);
        }

        public void Play(float closeDuration, float holdDuration, float openDuration,
            int levelCount, int newlyCompletedLevelIndex)
        {
            gameObject.SetActive(true);

            if (_routine != null)
                StopCoroutine(_routine);

            CacheReferencePositions();
            PrepareCurtainGeometry();
            _lampCounter?.Prepare(levelCount, newlyCompletedLevelIndex);

            _routine = StartCoroutine(PlayRoutine(
                Mathf.Max(0f, closeDuration),
                Mathf.Max(0f, holdDuration),
                Mathf.Max(0f, openDuration)));
        }

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;
            _lampCounter?.SetPaused(isPaused);
        }

        public void HideImmediately()
        {
            _isPaused = false;
            _lampCounter?.SetPaused(false);

            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            CacheReferencePositions();
            PrepareCurtainGeometry();
            SetRevealAlpha(0f);
            SetCurtainProgress(0f);
            SetRootVisible(false);

            if (_hideWhenIdle)
                gameObject.SetActive(false);
        }

        private IEnumerator PlayRoutine(float closeDuration, float holdDuration, float openDuration)
        {
            SetRootVisible(true);
            SetRevealAlpha(0f);
            SetCurtainProgress(0f);

            yield return AnimateCurtains(0f, 1f, closeDuration);
            yield return PlayClosedPhase(holdDuration);
            yield return AnimateOpening(openDuration);

            SetRootVisible(false);
            _routine = null;

            if (_hideWhenIdle)
                gameObject.SetActive(false);
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

        private IEnumerator PlayClosedPhase(float duration)
        {
            if (duration <= 0f)
            {
                SetRevealAlpha(1f);
                _lampCounter?.PlayNewlyCompleted();
                yield break;
            }

            var fadeDuration = Mathf.Min(_revealFadeInDuration, duration);
            var lampStartTime = Mathf.Min(duration, fadeDuration + _lampAnimationDelay);
            var lampStarted = false;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += GetGameplayDeltaTime();

                var revealAlpha = fadeDuration <= 0f
                    ? 1f
                    : Mathf.Clamp01(elapsed / fadeDuration);
                SetRevealAlpha(revealAlpha);

                if (!lampStarted && elapsed >= lampStartTime)
                {
                    _lampCounter?.PlayNewlyCompleted();
                    lampStarted = true;
                }

                yield return null;
            }

            SetRevealAlpha(1f);

            if (!lampStarted)
                _lampCounter?.PlayNewlyCompleted();
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
            if (_revealGroup == null)
                return;

            _revealGroup.alpha = Mathf.Clamp01(alpha);
            _revealGroup.interactable = false;
            _revealGroup.blocksRaycasts = false;
        }
    }
}
