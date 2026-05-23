using System.Collections;
using UnityEngine;

namespace UI
{
    public sealed class LevelTransitionCurtainView : MonoBehaviour
    {
        [SerializeField] private RectTransform _topCurtain;
        [SerializeField] private RectTransform _bottomCurtain;
        [SerializeField] private CanvasGroup _rootGroup;
        [SerializeField] private CanvasGroup _messageGroup;
        [SerializeField] private AnimationCurve _movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool _hideWhenIdle;

        private Coroutine _routine;
        private Vector2 _topClosedPosition;
        private Vector2 _bottomClosedPosition;
        private bool _positionsCached;

        private void Awake()
        {
            if (_rootGroup == null)
                _rootGroup = GetComponent<CanvasGroup>();

            CacheClosedPositions();
            SetRootVisible(false);
            SetMessageVisible(true);
            SetCurtainProgress(0f);
        }

        private void OnDisable()
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = null;
        }

        public void Play(float closeDuration, float holdDuration, float openDuration)
        {
            gameObject.SetActive(true);
            CacheClosedPositions();

            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(PlayRoutine(
                Mathf.Max(0f, closeDuration),
                Mathf.Max(0f, holdDuration),
                Mathf.Max(0f, openDuration)));
        }

        public void HideImmediately()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            if (!_positionsCached)
                CacheClosedPositions();

            SetRootVisible(false);
            SetMessageVisible(true);
            SetCurtainProgress(0f);

            if (_hideWhenIdle)
                gameObject.SetActive(false);
        }

        private IEnumerator PlayRoutine(float closeDuration, float holdDuration, float openDuration)
        {
            SetRootVisible(true);
            SetMessageVisible(true);
            SetCurtainProgress(0f);

            yield return AnimateCurtains(0f, 1f, closeDuration);

            if (holdDuration > 0f)
                yield return new WaitForSeconds(holdDuration);

            yield return AnimateCurtains(1f, 0f, openDuration);

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
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                SetCurtainProgress(Mathf.Lerp(from, to, t));
                yield return null;
            }

            SetCurtainProgress(to);
        }

        private void SetCurtainProgress(float progress)
        {
            var t = Mathf.Clamp01(progress);
            var eased = _movementCurve != null ? _movementCurve.Evaluate(t) : t;

            if (_topCurtain != null)
            {
                var openPosition = GetOpenPosition(_topCurtain, _topClosedPosition, 1f);
                _topCurtain.anchoredPosition = Vector2.LerpUnclamped(openPosition, _topClosedPosition, eased);
            }

            if (_bottomCurtain != null)
            {
                var openPosition = GetOpenPosition(_bottomCurtain, _bottomClosedPosition, -1f);
                _bottomCurtain.anchoredPosition = Vector2.LerpUnclamped(openPosition, _bottomClosedPosition, eased);
            }
        }

        private void CacheClosedPositions()
        {
            if (_positionsCached)
                return;

            if (_topCurtain != null)
                _topClosedPosition = _topCurtain.anchoredPosition;

            if (_bottomCurtain != null)
                _bottomClosedPosition = _bottomCurtain.anchoredPosition;

            _positionsCached = true;
        }

        private Vector2 GetOpenPosition(RectTransform curtain, Vector2 closedPosition, float direction)
        {
            return closedPosition + Vector2.up * direction * GetCurtainTravelDistance(curtain);
        }

        private float GetCurtainTravelDistance(RectTransform curtain)
        {
            if (curtain.rect.height > 0f)
                return curtain.rect.height;

            var parent = curtain.parent as RectTransform;
            return parent != null ? parent.rect.height * 0.5f : 0f;
        }

        private void SetRootVisible(bool visible)
        {
            if (_rootGroup == null)
                return;

            _rootGroup.alpha = visible ? 1f : 0f;
            _rootGroup.interactable = false;
            _rootGroup.blocksRaycasts = visible;
        }

        private void SetMessageVisible(bool visible)
        {
            if (_messageGroup == null)
                return;

            _messageGroup.alpha = visible ? 1f : 0f;
            _messageGroup.interactable = false;
            _messageGroup.blocksRaycasts = false;
        }
    }
}
