using System.Collections;
using TMPro;
using IncrementalRPG.Scripts.AudioManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.TestSkillTree.View
{
    // Prefab requirements: Image (_icon) + TextMeshProUGUI (_levelText) as children.
    public class NodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _additionalIcon;
        [SerializeField] private GameObject _additionalIconRoot;
        [SerializeField] private Image _borderIcon;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private RectTransform _feedbackRoot;
        [SerializeField] private float _feedbackScale = 0.9f;
        [SerializeField] private float _feedbackClockwiseRotationDegrees = 12f;
        [SerializeField] private float _feedbackShrinkDuration = 0.08f;
        [SerializeField] private float _feedbackReturnDuration = 0.14f;

        private SkillTreeService      _service;
        private NodeDefinition        _definition;
        private NodePopupView         _popup;
        private NodeBorderColorConfig _borderColorConfig;
        private AudioManager          _audioManager;
        private RectTransform         _cachedFeedbackRoot;
        private Vector3               _feedbackBaseScale;
        private Quaternion            _feedbackBaseRotation;
        private Coroutine             _feedbackRoutine;
        private bool                  _hasFeedbackBaseTransform;

        public string NodeId => _definition != null ? _definition.id : string.Empty;

        public void Bind(NodeDefinition definition, SkillTreeService service, NodePopupView popup, NodeBorderColorConfig borderColorConfig, AudioManager audioManager)
        {
            _definition        = definition;
            _service           = service;
            _popup             = popup;
            _borderColorConfig = borderColorConfig;
            _audioManager      = audioManager;

            if (_icon != null && definition.icon != null)
                _icon.sprite = definition.icon;

            SetupAdditionalIcon(definition);
            CacheFeedbackBaseTransform(GetFeedbackRoot(), true);
            Refresh();
        }

        public void Refresh()
        {
            var state = _service.GetState(_definition.id);

            if (state == NodeState.Hidden)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            var stateColor = _borderColorConfig != null
                ? _borderColorConfig.GetColor(state)
                : Color.white;

            _borderIcon.color = stateColor;

            if (_additionalIcon != null && _additionalIcon.gameObject.activeInHierarchy)
                _additionalIcon.color = stateColor;

            _levelText.text = $"{_service.GetLevel(_definition.id)}/{_definition.maxLevel}";
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

        private void SetupAdditionalIcon(NodeDefinition definition)
        {
            if (_additionalIcon == null)
                return;

            var hasIcon = definition.additionalIcon != null;
            var iconRoot = _additionalIconRoot != null
                ? _additionalIconRoot
                : _additionalIcon.gameObject;

            iconRoot.SetActive(hasIcon);
            _additionalIcon.sprite = definition.additionalIcon;
            _additionalIcon.raycastTarget = false;
        }

        public void OnPointerEnter(PointerEventData eventData) =>
            _popup.Show(_definition, (RectTransform)transform);

        public void OnPointerExit(PointerEventData eventData) =>
            _popup.OnNodeExit();

        public void OnPointerClick(PointerEventData eventData)
        {
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

        private void OnDisable()
        {
            if (_feedbackRoutine != null)
            {
                StopCoroutine(_feedbackRoutine);
                _feedbackRoutine = null;
            }

            ResetFeedbackTransform();
        }
    }
}
