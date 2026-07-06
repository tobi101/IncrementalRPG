using TMPro;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class HubFeatureButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _glowImage;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private SkeletonGraphic _hoverSkeleton;
        [SerializeField] private string _idleAnimationName = "idle";
        [SerializeField] private string _hoverAnimationName = "hover";
        [SerializeField] private bool _idleAnimationLoop = true;
        [SerializeField] private bool _hoverAnimationLoop = true;

        public Button Button => _button;

        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

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
            SetGlowVisible(false);
            PlaySkeletonAnimation(_idleAnimationName, _idleAnimationLoop);
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
            if (_glowImage == null) return;
            if (_text == null) return;
            
            _glowImage.enabled = visible;
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
