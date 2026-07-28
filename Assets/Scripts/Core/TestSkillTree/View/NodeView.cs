using System.Collections;
using IncrementalRPG.Scripts.AudioManager;
using Spine.Unity;
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
        [SerializeField] private NodeLevelCounterView _levelCounter;
        [SerializeField] private SkeletonGraphic _lockedSkeleton;
        [SerializeField] private string _lockedIdleAnimationName = "idle";
        [SerializeField] private string _lockedOpenAnimationName = "open";
        [SerializeField] private string _lockedCancelAnimationName = "cancel";

        private SkillTreeService      _service;
        private NodeDefinition        _definition;
        private NodePopupView         _popup;
        private NodeCircleSpriteConfig _circleSpriteConfig;
        private AudioManager          _audioManager;
        private Coroutine             _revealRoutine;
        private Vector3               _revealBaseScale;
        private bool                  _hasRevealBaseScale;
        private NodeState             _lastVisibleState;
        private bool                  _hasLastVisibleState;
        private bool                  _lockedOpenAnimationPlaying;
        private int                   _lockedAnimationVersion;

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

            _levelCounter?.Initialize(definition.maxLevel, service.GetLevel(definition.id));

            CacheRevealBaseScale(true);
            Refresh();
        }

        public void Refresh()
        {
            Refresh(_service.GetState(_definition.id));
        }

        public void Refresh(NodeState state)
        {
            StopRevealRoutine(true);

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

        public void PrepareReveal(NodeState state, float startScale)
        {
            StopRevealRoutine(false);
            CacheRevealBaseScale();

            gameObject.SetActive(true);

            ApplyStateCircle(state);
            ApplyLockedVisualState(state);

            var target = GetRevealRoot();
            if (target != null)
                target.localScale = _revealBaseScale * Mathf.Max(0f, startScale);
        }

        public void PlayReveal(float duration, float startScale)
        {
            var target = GetRevealRoot();
            if (target == null)
                return;

            CacheRevealBaseScale();
            StopRevealRoutine(false);
            target.localScale = _revealBaseScale * Mathf.Max(0f, startScale);

            if (!gameObject.activeInHierarchy)
            {
                ResetRevealTransform();
                return;
            }

            _revealRoutine = StartCoroutine(PlayRevealRoutine(target, duration));
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
                PlayLockedOpenFeedback();
            else if (!_lockedOpenAnimationPlaying)
                HideLockedVisualImmediate();

            _lastVisibleState = state;
            _hasLastVisibleState = true;
        }

        public void PlayLevelUpgrade(int newLevel) =>
            _levelCounter?.PlayUpgrade(newLevel);

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

        private RectTransform GetRevealRoot() =>
            (RectTransform)transform;

        private void CacheRevealBaseScale(bool force = false)
        {
            var target = GetRevealRoot();
            if (target == null)
                return;

            if (!force && _hasRevealBaseScale)
                return;

            _revealBaseScale = target.localScale;
            _hasRevealBaseScale = true;
        }

        private void StopRevealRoutine(bool resetTransform)
        {
            if (_revealRoutine != null)
            {
                StopCoroutine(_revealRoutine);
                _revealRoutine = null;
            }

            if (resetTransform)
                ResetRevealTransform();
        }

        private IEnumerator PlayRevealRoutine(RectTransform target, float duration)
        {
            if (target == null)
                yield break;

            if (duration <= 0f)
            {
                ResetRevealTransform();
                _revealRoutine = null;
                yield break;
            }

            var fromScale = target.localScale;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (target == null)
                    yield break;

                var t = Mathf.Clamp01(elapsed / duration);
                var easedT = Mathf.SmoothStep(0f, 1f, t);
                target.localScale = Vector3.Lerp(fromScale, _revealBaseScale, easedT);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            ResetRevealTransform();
            _revealRoutine = null;
        }

        private void ResetRevealTransform()
        {
            if (!_hasRevealBaseScale)
                return;

            var target = GetRevealRoot();
            if (target != null)
                target.localScale = _revealBaseScale;
        }

        private void ShowLockedVisual()
        {
            PlayLockedIdleAnimation();
        }

        private void HideLockedVisualImmediate()
        {
            _lockedAnimationVersion++;
            _lockedOpenAnimationPlaying = false;
            ResetLockedSkeletonPose();
            SetActive(_lockedSkeleton, false);
        }

        private void PlayLockedOpenFeedback()
        {
            if (_lockedOpenAnimationPlaying)
                return;

            _lockedOpenAnimationPlaying = true;

            var version = ++_lockedAnimationVersion;
            var entry = PlayLockedSkeletonAnimation(_lockedOpenAnimationName, false, true);
            if (entry == null)
            {
                HideLockedVisualImmediate();
                return;
            }

            entry.Complete += _ =>
            {
                if (version != _lockedAnimationVersion)
                    return;

                HideLockedVisualImmediate();
            };
        }

        private void PlayLockedClickFeedback()
        {
            if (_lockedSkeleton == null || !gameObject.activeInHierarchy)
                return;

            _lockedOpenAnimationPlaying = false;

            var version = ++_lockedAnimationVersion;
            var entry = PlayLockedSkeletonAnimation(_lockedCancelAnimationName, false, true);
            if (entry == null)
            {
                PlayLockedIdleAnimation();
                return;
            }

            entry.Complete += _ =>
            {
                if (version != _lockedAnimationVersion)
                    return;

                PlayLockedIdleAnimation();
            };
        }

        private void PlayLockedIdleAnimation()
        {
            _lockedOpenAnimationPlaying = false;
            _lockedAnimationVersion++;

            if (PlayLockedSkeletonAnimation(_lockedIdleAnimationName, true, true) == null)
                HideLockedVisualImmediate();
        }

        private Spine.TrackEntry PlayLockedSkeletonAnimation(string animationName, bool loop, bool resetPose)
        {
            if (string.IsNullOrEmpty(animationName))
                return null;

            if (!EnsureLockedSkeletonReady())
                return null;

            if (_lockedSkeleton.Skeleton.Data.FindAnimation(animationName) == null)
                return null;

            if (resetPose)
                ResetLockedSkeletonPose();

            var entry = _lockedSkeleton.AnimationState.SetAnimation(0, animationName, loop);
            if (entry != null)
                entry.MixDuration = 0f;

            return entry;
        }

        private bool EnsureLockedSkeletonReady()
        {
            if (_lockedSkeleton == null)
                return false;

            SetActive(_lockedSkeleton, true);

            if (!_lockedSkeleton.IsValid)
                _lockedSkeleton.Initialize(false);

            return _lockedSkeleton.IsValid &&
                   _lockedSkeleton.Skeleton != null &&
                   _lockedSkeleton.AnimationState != null;
        }

        private void ResetLockedSkeletonPose()
        {
            if (_lockedSkeleton == null || !_lockedSkeleton.IsValid)
                return;

            _lockedSkeleton.AnimationState?.ClearTracks();
            _lockedSkeleton.Skeleton?.SetToSetupPose();
        }

        private static void SetActive(Component target, bool active)
        {
            if (target != null)
                target.gameObject.SetActive(active);
        }

        private void OnDisable()
        {
            StopRevealRoutine(true);

            _lockedAnimationVersion++;
            _lockedOpenAnimationPlaying = false;

            if (_hasLastVisibleState && _lastVisibleState == NodeState.Locked)
                PlayLockedIdleAnimation();
            else
                HideLockedVisualImmediate();
        }
    }
}
