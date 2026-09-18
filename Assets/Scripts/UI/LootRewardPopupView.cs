using System;
using Core.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Utils;

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
        private LootReward _reward;
        private string _unavailableKey;
        private Vector2? _continueButtonPosition;
        private RectTransform _continueButtonGlow;

        private void Awake()
        {
            _continueButton.onClick.AddListener(HandleContinue);
            _useNowButton.onClick.AddListener(HandleUseNow);
            UIButtonAudio.InstallInChildren(this);
            LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
        }

        private void OnDestroy()
        {
            _continueButton.onClick.RemoveListener(HandleContinue);
            _useNowButton.onClick.RemoveListener(HandleUseNow);
            LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
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

            _reward = reward;
            _unavailableKey = status;
            _icon.sprite = reward.Definition.icon;
            RefreshTexts();
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
            _unavailableKey = status;
            RefreshTexts();
            RefreshInteraction();
        }

        private void HandleLocaleChanged(Locale locale)
        {
            if (_isOpen)
                RefreshTexts();
        }

        private void RefreshTexts()
        {
            var definition = _reward.Definition;
            var name = definition.GetDisplayName();
            _title.text = (definition.category == ItemCategory.Currency
                ? ItemText.Get("loot.currency_amount", name, BigDoubleFormatter.Format(_reward.CurrencyAmount))
                : name).ToUpperInvariant();
            var showUse = definition.category is ItemCategory.Consumable or ItemCategory.Armor;
            _useNowGroup.gameObject.SetActive(showUse);
            var continueRect = (RectTransform)_continueButton.transform;
            _continueButtonPosition ??= continueRect.anchoredPosition;
            continueRect.anchoredPosition = showUse ? _continueButtonPosition.Value
                : new Vector2(0f, _continueButtonPosition.Value.y);
            _continueButtonGlow ??= _composition.Find("ContinueButtonGlow") as RectTransform;
            if (_continueButtonGlow != null)
                _continueButtonGlow.anchoredPosition = continueRect.anchoredPosition;
            var status = _reward.IsPendingPlacement ? ItemText.Get("loot.inventory_full") : string.Empty;
            if (!string.IsNullOrEmpty(_unavailableKey))
                status += (status.Length > 0 ? "\n" : string.Empty) + ItemText.Get(_unavailableKey);
            else if (definition.category is ItemCategory.Consumable or ItemCategory.Scroll or ItemCategory.Currency)
                status += (status.Length > 0 ? "\n" : string.Empty) + definition.GetDescription();
            _status.text = status;
            _continueButton.GetComponentInChildren<TMP_Text>(true).text = ItemText.Get("loot.continue");
            _useNowButton.GetComponentInChildren<TMP_Text>(true).text = ItemText.Get("loot.use_now");
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
