using System;
using Core.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class LootRewardPopupView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _composition;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _useNowButton;
        [SerializeField] private CanvasGroup _useNowGroup;
        [SerializeField] private RewardRevealEffect _revealEffect;

        public event Action ContinueClicked;
        public event Action UseNowClicked;

        private bool _isOpen;
        private bool _isPaused;
        private bool _canUse;
        private bool _submitted;

        private void Awake()
        {
            _continueButton.onClick.AddListener(HandleContinue);
            _useNowButton.onClick.AddListener(HandleUseNow);
            UIButtonAudio.InstallInChildren(this);
        }

        private void OnDestroy()
        {
            _continueButton.onClick.RemoveListener(HandleContinue);
            _useNowButton.onClick.RemoveListener(HandleUseNow);
        }

        private void OnDisable() => _isOpen = false;

        private void OnRectTransformDimensionsChange()
        {
            if (_composition == null)
                return;

            var size = ((RectTransform)transform).rect.size;
            var scale = Mathf.Min(size.x / 1920f, size.y / 1080f);
            _composition.localScale = Vector3.one * scale;
        }

        public void Show(LootReward reward, bool canUse, string status)
        {
            if (_isOpen)
                return;

            _title.text = reward.Definition.displayName.ToUpperInvariant();
            _icon.sprite = reward.Definition.icon;
            _status.text = status;
            _canUse = canUse;
            _submitted = false;
            _isOpen = true;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            OnRectTransformDimensionsChange();
            RefreshInteraction();
            _revealEffect.Play(_isPaused);
        }

        public void Hide()
        {
            _isOpen = false;
            _submitted = true;
            _revealEffect.Stop();
            gameObject.SetActive(false);
        }

        public void SetPaused(bool paused)
        {
            _isPaused = paused;
            _revealEffect.SetPaused(paused);
            RefreshInteraction();
        }

        public void ShowUseUnavailable(string status)
        {
            _canUse = false;
            _submitted = false;
            _status.text = status;
            RefreshInteraction();
        }

        private void RefreshInteraction()
        {
            var enabled = _isOpen && !_isPaused && !_submitted;
            _group.interactable = enabled;
            _continueButton.interactable = enabled;
            _useNowButton.interactable = enabled && _canUse;
            _useNowGroup.alpha = _canUse ? 1f : .4f;
        }

        private void HandleContinue()
        {
            if (!_isOpen || _isPaused || _submitted)
                return;

            _submitted = true;
            RefreshInteraction();
            ContinueClicked?.Invoke();
        }

        private void HandleUseNow()
        {
            if (!_isOpen || _isPaused || _submitted || !_canUse)
                return;

            _submitted = true;
            RefreshInteraction();
            UseNowClicked?.Invoke();
        }
    }
}
