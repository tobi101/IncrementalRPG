using TMPro;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class HubFeatureButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const float MarkerPulsePeriod = 1.8f;
        private const float MarkerMinimumAlpha = 0.7f;

        [SerializeField] private Button _button;
        [SerializeField] private Image _glowImage;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private GameObject _firstVisitMarker;
        [SerializeField] private SkeletonGraphic _hoverSkeleton;
        [SerializeField] private string _idleAnimationName = "idle";
        [SerializeField] private string _hoverAnimationName = "hover";
        [SerializeField] private bool _idleAnimationLoop = true;
        [SerializeField] private bool _hoverAnimationLoop = true;

        private CanvasRenderer _markerRenderer;

        public Button Button => _button;

        public void SetFirstVisitMarkerVisible(bool visible)
        {
            if (_firstVisitMarker != null)
                _firstVisitMarker.SetActive(visible);
        }

        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_firstVisitMarker != null)
                _markerRenderer = _firstVisitMarker.GetComponent<CanvasRenderer>();

            SetGlowVisible(false);
            PlaySkeletonAnimation(_idleAnimationName, _idleAnimationLoop);
        }

        private void OnEnable()
        {
            SetGlowVisible(false);
            PlaySkeletonAnimation(_idleAnimationName, _idleAnimationLoop);
        }

        private void OnDisable()
        {
            if (_markerRenderer != null)
                _markerRenderer.SetAlpha(1f);

            SetGlowVisible(false);
            PlaySkeletonAnimation(_idleAnimationName, _idleAnimationLoop);
        }

        private void Update()
        {
            if (_markerRenderer == null || !_firstVisitMarker.activeInHierarchy)
                return;

            var phase = (float)(Time.unscaledTimeAsDouble % MarkerPulsePeriod) / MarkerPulsePeriod;
            var pulse = 0.5f + 0.5f * Mathf.Cos(phase * 2f * Mathf.PI);
            _markerRenderer.SetAlpha(Mathf.Lerp(MarkerMinimumAlpha, 1f, pulse));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            SetGlowVisible(true);
            PlaySkeletonAnimation(_hoverAnimationName, _hoverAnimationLoop);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetGlowVisible(false);
            PlaySkeletonAnimation(_idleAnimationName, _idleAnimationLoop);
        }

        private void SetGlowVisible(bool visible)
        {
            if (_glowImage != null)
                _glowImage.enabled = visible;

            if (_text == null)
                return;

            // Toggle the whole label so its text and backdrop appear together.
            var label = _text.GetComponentInParent<HubLabelView>(true);
            if (label != null)
                label.gameObject.SetActive(visible);
            else
                _text.enabled = visible;
        }

        private void PlaySkeletonAnimation(string animationName, bool loop)
        {
            if (_hoverSkeleton == null || string.IsNullOrEmpty(animationName))
                return;

            if (!_hoverSkeleton.IsValid)
                _hoverSkeleton.Initialize(false);

            if (!_hoverSkeleton.IsValid || _hoverSkeleton.AnimationState == null)
                return;

            if (_hoverSkeleton.Skeleton.Data.FindAnimation(animationName) == null)
                return;

            _hoverSkeleton.AnimationState.SetAnimation(0, animationName, loop);
        }
    }
}
