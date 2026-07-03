using System.Collections;
using IncrementalRPG.Scripts.AudioManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Core.TestSkillTree.View
{
    // Prefab requirements: icon Image and state circle Image as children.
    public class NodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField, FormerlySerializedAs("_borderIcon")] private Image _stateCircleImage;
        [SerializeField] private float _stateCircleRotationDegreesPerSecond = 18f;
        [SerializeField] private RectTransform _feedbackRoot;
        [SerializeField] private float _feedbackScale = 0.9f;
        [SerializeField] private float _feedbackClockwiseRotationDegrees = 12f;
        [SerializeField] private float _feedbackShrinkDuration = 0.08f;
        [SerializeField] private float _feedbackReturnDuration = 0.14f;
        [SerializeField] private RectTransform _chainBase;
        [SerializeField] private RectTransform _leftChain;
        [SerializeField] private RectTransform _rightChain;
        [SerializeField] private RectTransform _lockTransform;
        [SerializeField] private float _chainExitDistance = 24f;
        [SerializeField] private float _chainExitScale = 0.75f;
        [SerializeField] private float _chainExitDuration = 0.18f;
        [SerializeField] private float _lockedClickShakeDistance = 4f;
        [SerializeField] private float _lockedClickShakeDuration = 0.2f;

        private SkillTreeService      _service;
        private NodeDefinition        _definition;
        private NodePopupView         _popup;
        private NodeCircleSpriteConfig _circleSpriteConfig;
        private AudioManager          _audioManager;
        private RectTransform         _cachedFeedbackRoot;
        private Vector3               _feedbackBaseScale;
        private Quaternion            _feedbackBaseRotation;
        private Coroutine             _feedbackRoutine;
        private bool                  _hasFeedbackBaseTransform;
        private Vector2               _leftChainBasePosition;
        private Vector2               _rightChainBasePosition;
        private Vector2               _lockBasePosition;
        private Vector3               _leftChainBaseScale;
        private Vector3               _rightChainBaseScale;
        private Coroutine             _chainExitRoutine;
        private Coroutine             _lockShakeRoutine;
        private NodeState             _lastVisibleState;
        private bool                  _hasLastVisibleState;
        private bool                  _hasLockedVisualBaseTransform;

        public string NodeId => _definition != null ? _definition.id : string.Empty;

        public void Bind(NodeDefinition definition, SkillTreeService service, NodePopupView popup, NodeCircleSpriteConfig circleSpriteConfig, AudioManager audioManager)
        {
            _definition        = definition;
            _service           = service;
            _popup             = popup;
            _circleSpriteConfig = circleSpriteConfig;
            _audioManager      = audioManager;

            if (_icon != null && definition.icon != null)
                _icon.sprite = definition.icon;

            CacheFeedbackBaseTransform(GetFeedbackRoot(), true);
            CacheLockedVisualBaseTransform();
            Refresh();
        }

        public void Refresh()
        {
            var state = _service.GetState(_definition.id);

            if (state == NodeState.Hidden)
            {
                _hasLastVisibleState = false;
                HideLockedVisualImmediate();
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            ApplyStateCircle(state);
            ApplyLockedVisualState(state);
        }

        private void Update()
        {
            if (_stateCircleImage == null || Mathf.Approximately(_stateCircleRotationDegreesPerSecond, 0f))
                return;

            _stateCircleImage.rectTransform.Rotate(
                0f,
                0f,
                -_stateCircleRotationDegreesPerSecond * Time.unscaledDeltaTime);
        }

        private void ApplyStateCircle(NodeState state)
        {
            if (_stateCircleImage == null)
                return;

            var sprite = _circleSpriteConfig != null
                ? _circleSpriteConfig.GetSprite(state)
                : null;

            if (sprite != null)
                _stateCircleImage.sprite = sprite;

            _stateCircleImage.color = Color.white;
        }

        private void ApplyLockedVisualState(NodeState state)
        {
            var wasLocked = _hasLastVisibleState && _lastVisibleState == NodeState.Locked;
            var isLocked = state == NodeState.Locked;

            if (isLocked)
                ShowLockedVisual();
            else if (wasLocked)
                PlayChainsExitFeedback();
            else if (_chainExitRoutine == null)
                HideLockedVisualImmediate();

            _lastVisibleState = state;
            _hasLastVisibleState = true;
        }

        public void PlayUpgradeFeedback()
        {
            var target = GetFeedbackRoot();
            if (target == null)
                return;

            CacheFeedbackBaseTransform(target);

            if (_feedbackRoutine != null)
            {
                StopCoroutine(_feedbackRoutine);
                _feedbackRoutine = null;
                ResetFeedbackTransform();
            }

            if (!gameObject.activeInHierarchy)
                return;

            _feedbackRoutine = StartCoroutine(PlayUpgradeFeedbackRoutine(target));
        }

        public void OnPointerEnter(PointerEventData eventData) =>
            _popup.Show(_definition, (RectTransform)transform);

        public void OnPointerExit(PointerEventData eventData) =>
            _popup.OnNodeExit();

        public void OnPointerClick(PointerEventData eventData)
        {
            var state = _service.GetState(_definition.id);
            if (state == NodeState.Locked)
                PlayLockedClickFeedback();

            var result = _service.TryUpgrade(_definition.id);
            PlayUpgradeResultSound(result);

            if (result == NodeUpgradeResult.Failed)
                return;

            _popup.Refresh(_definition);
        }

        private void PlayUpgradeResultSound(NodeUpgradeResult result)
        {
            switch (result)
            {
                case NodeUpgradeResult.Upgraded:
                    _audioManager?.PlaySkillUpgrade();
                    break;
                case NodeUpgradeResult.UpgradedToMax:
                    _audioManager?.PlaySkillMax();
                    break;
                case NodeUpgradeResult.Failed:
                default:
                    _audioManager?.PlaySkillError();
                    break;
            }
        }

        private RectTransform GetFeedbackRoot() =>
            _feedbackRoot != null ? _feedbackRoot : (RectTransform)transform;

        private void CacheFeedbackBaseTransform(RectTransform target, bool force = false)
        {
            if (target == null)
                return;

            if (!force && _hasFeedbackBaseTransform && _cachedFeedbackRoot == target)
                return;

            _cachedFeedbackRoot = target;
            _feedbackBaseScale = target.localScale;
            _feedbackBaseRotation = target.localRotation;
            _hasFeedbackBaseTransform = true;
        }

        private IEnumerator PlayUpgradeFeedbackRoutine(RectTransform target)
        {
            ResetFeedbackTransform();

            var targetScale = _feedbackBaseScale * _feedbackScale;
            var targetRotation = _feedbackBaseRotation * Quaternion.Euler(0f, 0f, -_feedbackClockwiseRotationDegrees);

            yield return AnimateFeedbackTransform(
                target,
                _feedbackBaseScale,
                _feedbackBaseRotation,
                targetScale,
                targetRotation,
                _feedbackShrinkDuration);

            yield return AnimateFeedbackTransform(
                target,
                targetScale,
                targetRotation,
                _feedbackBaseScale,
                _feedbackBaseRotation,
                _feedbackReturnDuration);

            ResetFeedbackTransform();
            _feedbackRoutine = null;
        }

        private IEnumerator AnimateFeedbackTransform(
            RectTransform target,
            Vector3 fromScale,
            Quaternion fromRotation,
            Vector3 toScale,
            Quaternion toRotation,
            float duration)
        {
            if (target == null)
                yield break;

            if (duration <= 0f)
            {
                target.localScale = toScale;
                target.localRotation = toRotation;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (target == null)
                    yield break;

                var t = Mathf.Clamp01(elapsed / duration);
                var easedT = Mathf.SmoothStep(0f, 1f, t);

                target.localScale = Vector3.Lerp(fromScale, toScale, easedT);
                target.localRotation = Quaternion.Slerp(fromRotation, toRotation, easedT);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            target.localScale = toScale;
            target.localRotation = toRotation;
        }

        private void ResetFeedbackTransform()
        {
            if (!_hasFeedbackBaseTransform || _cachedFeedbackRoot == null)
                return;

            _cachedFeedbackRoot.localScale = _feedbackBaseScale;
            _cachedFeedbackRoot.localRotation = _feedbackBaseRotation;
        }

        private void CacheLockedVisualBaseTransform()
        {
            if (_hasLockedVisualBaseTransform)
                return;

            if (_leftChain != null)
            {
                _leftChainBasePosition = _leftChain.anchoredPosition;
                _leftChainBaseScale = _leftChain.localScale;
            }

            if (_rightChain != null)
            {
                _rightChainBasePosition = _rightChain.anchoredPosition;
                _rightChainBaseScale = _rightChain.localScale;
            }

            if (_lockTransform != null)
                _lockBasePosition = _lockTransform.anchoredPosition;

            _hasLockedVisualBaseTransform = true;
        }

        private void ShowLockedVisual()
        {
            CacheLockedVisualBaseTransform();

            if (_chainExitRoutine != null)
            {
                StopCoroutine(_chainExitRoutine);
                _chainExitRoutine = null;
            }

            SetActive(_chainBase, true);
            SetActive(_leftChain, true);
            SetActive(_rightChain, true);
            SetActive(_lockTransform, true);
            ResetChainTransforms();
        }

        private void HideLockedVisualImmediate()
        {
            StopLockedVisualCoroutines();
            ResetLockedVisualTransforms();
            SetActive(_lockTransform, false);
            SetActive(_leftChain, false);
            SetActive(_rightChain, false);
            SetActive(_chainBase, false);
        }

        private void PlayChainsExitFeedback()
        {
            if (_chainExitRoutine != null)
                return;

            if (!gameObject.activeInHierarchy || _leftChain == null || _rightChain == null)
            {
                HideLockedVisualImmediate();
                return;
            }

            CacheLockedVisualBaseTransform();

            if (_lockShakeRoutine != null)
            {
                StopCoroutine(_lockShakeRoutine);
                _lockShakeRoutine = null;
            }

            ResetLockedVisualTransforms();
            SetActive(_chainBase, true);
            SetActive(_leftChain, true);
            SetActive(_rightChain, true);
            SetActive(_lockTransform, false);

            _chainExitRoutine = StartCoroutine(PlayChainsExitRoutine());
        }

        private void PlayLockedClickFeedback()
        {
            if (_lockTransform == null || !gameObject.activeInHierarchy)
                return;

            CacheLockedVisualBaseTransform();

            if (_lockShakeRoutine != null)
            {
                StopCoroutine(_lockShakeRoutine);
                _lockShakeRoutine = null;
            }

            _lockTransform.anchoredPosition = _lockBasePosition;
            SetActive(_chainBase, true);
            SetActive(_lockTransform, true);

            _lockShakeRoutine = StartCoroutine(PlayLockedClickFeedbackRoutine());
        }

        private IEnumerator PlayChainsExitRoutine()
        {
            var leftStartPosition = _leftChainBasePosition;
            var rightStartPosition = _rightChainBasePosition;
            var leftEndPosition = leftStartPosition + Vector2.left * _chainExitDistance;
            var rightEndPosition = rightStartPosition + Vector2.right * _chainExitDistance;
            var leftEndScale = _leftChainBaseScale * _chainExitScale;
            var rightEndScale = _rightChainBaseScale * _chainExitScale;

            yield return AnimateChains(
                leftStartPosition,
                rightStartPosition,
                leftEndPosition,
                rightEndPosition,
                _leftChainBaseScale,
                _rightChainBaseScale,
                leftEndScale,
                rightEndScale,
                _chainExitDuration);

            ResetChainTransforms();
            SetActive(_leftChain, false);
            SetActive(_rightChain, false);
            SetActive(_chainBase, false);
            _chainExitRoutine = null;
        }

        private IEnumerator AnimateChains(
            Vector2 leftStartPosition,
            Vector2 rightStartPosition,
            Vector2 leftEndPosition,
            Vector2 rightEndPosition,
            Vector3 leftStartScale,
            Vector3 rightStartScale,
            Vector3 leftEndScale,
            Vector3 rightEndScale,
            float duration)
        {
            if (duration <= 0f)
            {
                _leftChain.anchoredPosition = leftEndPosition;
                _rightChain.anchoredPosition = rightEndPosition;
                _leftChain.localScale = leftEndScale;
                _rightChain.localScale = rightEndScale;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                var t = Mathf.Clamp01(elapsed / duration);
                var easedT = Mathf.SmoothStep(0f, 1f, t);

                _leftChain.anchoredPosition = Vector2.Lerp(leftStartPosition, leftEndPosition, easedT);
                _rightChain.anchoredPosition = Vector2.Lerp(rightStartPosition, rightEndPosition, easedT);
                _leftChain.localScale = Vector3.Lerp(leftStartScale, leftEndScale, easedT);
                _rightChain.localScale = Vector3.Lerp(rightStartScale, rightEndScale, easedT);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _leftChain.anchoredPosition = leftEndPosition;
            _rightChain.anchoredPosition = rightEndPosition;
            _leftChain.localScale = leftEndScale;
            _rightChain.localScale = rightEndScale;
        }

        private IEnumerator PlayLockedClickFeedbackRoutine()
        {
            if (_lockedClickShakeDuration <= 0f)
            {
                _lockTransform.anchoredPosition = _lockBasePosition;
                _lockShakeRoutine = null;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < _lockedClickShakeDuration)
            {
                var t = Mathf.Clamp01(elapsed / _lockedClickShakeDuration);
                var damping = 1f - t;
                var offset = Mathf.Sin(t * Mathf.PI * 8f) * _lockedClickShakeDistance * damping;

                _lockTransform.anchoredPosition = _lockBasePosition + Vector2.right * offset;

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _lockTransform.anchoredPosition = _lockBasePosition;
            _lockShakeRoutine = null;
        }

        private void StopLockedVisualCoroutines()
        {
            if (_chainExitRoutine != null)
            {
                StopCoroutine(_chainExitRoutine);
                _chainExitRoutine = null;
            }

            if (_lockShakeRoutine != null)
            {
                StopCoroutine(_lockShakeRoutine);
                _lockShakeRoutine = null;
            }
        }

        private void ResetLockedVisualTransforms()
        {
            CacheLockedVisualBaseTransform();
            ResetChainTransforms();

            if (_lockTransform != null)
                _lockTransform.anchoredPosition = _lockBasePosition;
        }

        private void ResetChainTransforms()
        {
            if (_leftChain != null)
            {
                _leftChain.anchoredPosition = _leftChainBasePosition;
                _leftChain.localScale = _leftChainBaseScale;
            }

            if (_rightChain != null)
            {
                _rightChain.anchoredPosition = _rightChainBasePosition;
                _rightChain.localScale = _rightChainBaseScale;
            }
        }

        private static void SetActive(Component target, bool active)
        {
            if (target != null)
                target.gameObject.SetActive(active);
        }

        private void OnDisable()
        {
            if (_feedbackRoutine != null)
            {
                StopCoroutine(_feedbackRoutine);
                _feedbackRoutine = null;
            }

            ResetFeedbackTransform();
            StopLockedVisualCoroutines();
            ResetLockedVisualTransforms();
        }
    }
}
