using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Core.Settings;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace UI
{
    public sealed class SettingsMenuController : MonoBehaviour
    {
        private enum LocaleLabelMode
        {
            LocaleName,
            EnglishLanguageName,
            NativeLanguageName,
            LocaleCode
        }

        [Serializable]
        private struct LocaleLabelOverride
        {
            public string LocaleCode;
            public string Label;
        }

        [SerializeField] private SettingsMenuView _view;
        [SerializeField] private bool _hideOnAwake;
        [SerializeField] private LocaleLabelMode _localeLabelMode = LocaleLabelMode.LocaleName;
        [SerializeField] private LocaleLabelOverride[] _localeLabelOverrides;

        [Inject] private GameSettingsService _settingsService;

        private readonly List<Locale> _locales = new();
        private Coroutine _localesRefreshRoutine;
        private bool _isRefreshingView;

        public Button BackButton
        {
            get
            {
                EnsureDependencies();
                return _view != null ? _view.BackButton : null;
            }
        }

        private void Awake()
        {
            EnsureDependencies();
            UIButtonAudio.InstallInChildren(this);

            if (_hideOnAwake)
                _view?.Hide();
        }

        private void OnEnable()
        {
            EnsureDependencies();
            SubscribeView();
            SubscribeSettings();
            RefreshView();
        }

        private void OnDisable()
        {
            UnsubscribeView();
            UnsubscribeSettings();
            StopLocalesRefreshRoutine();
        }

        public void Open()
        {
            EnsureDependencies();
            _view?.Show();
            RefreshView();
        }

        public void Close()
        {
            EnsureDependencies();
            _view?.Hide();
        }

        public void Toggle()
        {
            EnsureDependencies();
            if (_view == null)
                return;

            if (_view.IsVisible())
                Close();
            else
                Open();
        }

        public bool IsVisible()
        {
            EnsureDependencies();
            return _view != null && _view.IsVisible();
        }

        private void SubscribeView()
        {
            if (_view == null)
                return;

            AddSliderListener(_view.MasterVolumeSlider, HandleMasterVolumeChanged);
            AddSliderListener(_view.MusicVolumeSlider, HandleMusicVolumeChanged);
            AddSliderListener(_view.SfxVolumeSlider, HandleSfxVolumeChanged);

            if (_view.LanguageDropdown != null)
            {
                _view.LanguageDropdown.onValueChanged.RemoveListener(HandleLanguageChanged);
                _view.LanguageDropdown.onValueChanged.AddListener(HandleLanguageChanged);
            }

            if (_view.BackButton != null)
            {
                _view.BackButton.onClick.RemoveListener(Close);
                _view.BackButton.onClick.AddListener(Close);
            }
        }

        private void UnsubscribeView()
        {
            if (_view == null)
                return;

            RemoveSliderListener(_view.MasterVolumeSlider, HandleMasterVolumeChanged);
            RemoveSliderListener(_view.MusicVolumeSlider, HandleMusicVolumeChanged);
            RemoveSliderListener(_view.SfxVolumeSlider, HandleSfxVolumeChanged);

            if (_view.LanguageDropdown != null)
                _view.LanguageDropdown.onValueChanged.RemoveListener(HandleLanguageChanged);

            if (_view.BackButton != null)
                _view.BackButton.onClick.RemoveListener(Close);
        }

        private void SubscribeSettings()
        {
            if (_settingsService == null)
                return;

            _settingsService.OnMasterVolumeChanged += RefreshMasterVolume;
            _settingsService.OnMusicVolumeChanged += RefreshMusicVolume;
            _settingsService.OnSfxVolumeChanged += RefreshSfxVolume;
            _settingsService.OnLocaleChanged += RefreshLanguageSelection;
        }

        private void UnsubscribeSettings()
        {
            if (_settingsService == null)
                return;

            _settingsService.OnMasterVolumeChanged -= RefreshMasterVolume;
            _settingsService.OnMusicVolumeChanged -= RefreshMusicVolume;
            _settingsService.OnSfxVolumeChanged -= RefreshSfxVolume;
            _settingsService.OnLocaleChanged -= RefreshLanguageSelection;
        }

        private void RefreshView()
        {
            if (_view == null || _settingsService == null)
                return;

            _isRefreshingView = true;
            RefreshMasterVolume(_settingsService.MasterVolume);
            RefreshMusicVolume(_settingsService.MusicVolume);
            RefreshSfxVolume(_settingsService.SfxVolume);
            _isRefreshingView = false;

            StartLocalesRefreshRoutine();
        }

        private void EnsureDependencies()
        {
            if (_view == null)
                _view = GetComponent<SettingsMenuView>();

            _settingsService ??= GameSettingsServiceLocator.Instance;
        }

        private void RefreshMasterVolume(float value)
        {
            SetSliderValue(_view?.MasterVolumeSlider, value);
        }

        private void RefreshMusicVolume(float value)
        {
            SetSliderValue(_view?.MusicVolumeSlider, value);
        }

        private void RefreshSfxVolume(float value)
        {
            SetSliderValue(_view?.SfxVolumeSlider, value);
        }

        private void RefreshLanguageSelection(string localeCode)
        {
            if (_view?.LanguageDropdown == null || _locales.Count == 0)
                return;

            var index = GetLocaleIndex(localeCode);
            if (index < 0)
                index = GetLocaleIndex(LocalizationSettings.SelectedLocale?.Identifier.Code);

            if (index >= 0)
                _view.LanguageDropdown.SetValueWithoutNotify(index);
        }

        private void StartLocalesRefreshRoutine()
        {
            StopLocalesRefreshRoutine();
            _localesRefreshRoutine = StartCoroutine(RefreshLocalesWhenReady());
        }

        private void StopLocalesRefreshRoutine()
        {
            if (_localesRefreshRoutine == null)
                return;

            StopCoroutine(_localesRefreshRoutine);
            _localesRefreshRoutine = null;
        }

        private IEnumerator RefreshLocalesWhenReady()
        {
            var initialization = LocalizationSettings.InitializationOperation;
            if (!initialization.IsDone)
                yield return initialization;

            RefreshLanguageDropdown();
            _localesRefreshRoutine = null;
        }

        private void RefreshLanguageDropdown()
        {
            var dropdown = _view?.LanguageDropdown;
            if (dropdown == null)
                return;

            _locales.Clear();
            dropdown.ClearOptions();

            var availableLocales = LocalizationSettings.AvailableLocales;
            if (availableLocales == null || availableLocales.Locales == null)
                return;

            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var locale in availableLocales.Locales)
            {
                if (locale == null)
                    continue;

                _locales.Add(locale);
                options.Add(new TMP_Dropdown.OptionData(GetLocaleLabel(locale)));
            }

            dropdown.AddOptions(options);
            RefreshLanguageSelection(_settingsService.LocaleCode);
        }

        private void HandleMasterVolumeChanged(float value)
        {
            if (!_isRefreshingView)
                _settingsService.SetMasterVolume(value);
        }

        private void HandleMusicVolumeChanged(float value)
        {
            if (!_isRefreshingView)
                _settingsService.SetMusicVolume(value);
        }

        private void HandleSfxVolumeChanged(float value)
        {
            if (!_isRefreshingView)
                _settingsService.SetSfxVolume(value);
        }

        private void HandleLanguageChanged(int index)
        {
            if (_isRefreshingView || index < 0 || index >= _locales.Count)
                return;

            _settingsService.SetLocale(_locales[index]);
        }

        private int GetLocaleIndex(string localeCode)
        {
            if (string.IsNullOrWhiteSpace(localeCode))
                return -1;

            for (var i = 0; i < _locales.Count; i++)
            {
                if (_locales[i] != null && _locales[i].Identifier.Code == localeCode)
                    return i;
            }

            return -1;
        }

        private static void AddSliderListener(Slider slider, UnityEngine.Events.UnityAction<float> handler)
        {
            if (slider == null)
                return;

            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.onValueChanged.RemoveListener(handler);
            slider.onValueChanged.AddListener(handler);
        }

        private static void RemoveSliderListener(Slider slider, UnityEngine.Events.UnityAction<float> handler)
        {
            if (slider != null)
                slider.onValueChanged.RemoveListener(handler);
        }

        private static void SetSliderValue(Slider slider, float value)
        {
            if (slider == null)
                return;

            slider.SetValueWithoutNotify(Mathf.Clamp01(value));
        }

        private string GetLocaleLabel(Locale locale)
        {
            if (locale == null)
                return string.Empty;

            var overrideLabel = GetLocaleLabelOverride(locale.Identifier.Code);
            if (!string.IsNullOrWhiteSpace(overrideLabel))
                return overrideLabel;

            var cultureInfo = locale.Identifier.CultureInfo;
            switch (_localeLabelMode)
            {
                case LocaleLabelMode.EnglishLanguageName:
                    return GetLanguageName(cultureInfo, nativeName: false, locale);
                case LocaleLabelMode.NativeLanguageName:
                    return GetLanguageName(cultureInfo, nativeName: true, locale);
                case LocaleLabelMode.LocaleCode:
                    return locale.Identifier.Code;
                case LocaleLabelMode.LocaleName:
                default:
                    return GetLocaleName(locale);
            }
        }

        private string GetLocaleLabelOverride(string localeCode)
        {
            if (_localeLabelOverrides == null || string.IsNullOrWhiteSpace(localeCode))
                return string.Empty;

            foreach (var labelOverride in _localeLabelOverrides)
            {
                if (labelOverride.LocaleCode == localeCode)
                    return labelOverride.Label;
            }

            return string.Empty;
        }

        private static string GetLanguageName(CultureInfo cultureInfo, bool nativeName, Locale fallbackLocale)
        {
            if (cultureInfo == null)
                return GetLocaleName(fallbackLocale);

            if (!nativeName && cultureInfo.TwoLetterISOLanguageName == "zh")
                return "Chinese";

            var languageCulture = GetLanguageCulture(cultureInfo);
            var label = nativeName ? languageCulture.NativeName : languageCulture.EnglishName;
            return string.IsNullOrWhiteSpace(label) ? GetLocaleName(fallbackLocale) : label;
        }

        private static CultureInfo GetLanguageCulture(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
                return null;

            if (cultureInfo.IsNeutralCulture)
                return cultureInfo;

            try
            {
                return CultureInfo.GetCultureInfo(cultureInfo.TwoLetterISOLanguageName);
            }
            catch (CultureNotFoundException)
            {
                return cultureInfo;
            }
        }

        private static string GetLocaleName(Locale locale)
        {
            if (locale == null)
                return string.Empty;

            return string.IsNullOrWhiteSpace(locale.LocaleName)
                ? locale.Identifier.Code
                : locale.LocaleName;
        }
    }
}
