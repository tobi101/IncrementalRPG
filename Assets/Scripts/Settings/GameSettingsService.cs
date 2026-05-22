using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Core.Settings
{
    public sealed class GameSettingsService
    {
        private readonly GameSettingsStorage _storage;
        private readonly GameSettingsData _data;
        private bool _waitingForLocalizationInitialization;

        public event Action<GameSettingsData> OnChanged;
        public event Action<float> OnMasterVolumeChanged;
        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSfxVolumeChanged;
        public event Action<string> OnLocaleChanged;

        public float MasterVolume => _data.MasterVolume;
        public float MusicVolume => _data.MusicVolume;
        public float SfxVolume => _data.SfxVolume;
        public string LocaleCode => _data.LocaleCode;

        public GameSettingsService() : this(new GameSettingsStorage())
        {
        }

        public GameSettingsService(GameSettingsStorage storage)
        {
            _storage = storage ?? new GameSettingsStorage();
            _data = _storage.LoadOrDefault();
            _data.Normalize();

            ApplyLocale();
        }

        public GameSettingsData GetSnapshot() => _data.Clone();

        public void SetMasterVolume(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(_data.MasterVolume, value))
                return;

            _data.MasterVolume = value;
            SaveAndNotify();
            OnMasterVolumeChanged?.Invoke(_data.MasterVolume);
        }

        public void SetMusicVolume(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(_data.MusicVolume, value))
                return;

            _data.MusicVolume = value;
            SaveAndNotify();
            OnMusicVolumeChanged?.Invoke(_data.MusicVolume);
        }

        public void SetSfxVolume(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(_data.SfxVolume, value))
                return;

            _data.SfxVolume = value;
            SaveAndNotify();
            OnSfxVolumeChanged?.Invoke(_data.SfxVolume);
        }

        public void SetLocale(Locale locale)
        {
            SetLocaleCode(locale != null ? locale.Identifier.Code : string.Empty);
        }

        public void SetLocaleCode(string localeCode)
        {
            localeCode ??= string.Empty;
            if (string.Equals(_data.LocaleCode, localeCode, StringComparison.Ordinal))
                return;

            _data.LocaleCode = localeCode;
            ApplyLocale();
            SaveAndNotify();
            OnLocaleChanged?.Invoke(_data.LocaleCode);
        }

        public void ApplyAll()
        {
            ApplyLocale();
            OnMasterVolumeChanged?.Invoke(_data.MasterVolume);
            OnMusicVolumeChanged?.Invoke(_data.MusicVolume);
            OnSfxVolumeChanged?.Invoke(_data.SfxVolume);
            OnLocaleChanged?.Invoke(_data.LocaleCode);
            OnChanged?.Invoke(GetSnapshot());
        }

        private void SaveAndNotify()
        {
            _data.Normalize();
            _storage.Write(_data);
            OnChanged?.Invoke(GetSnapshot());
        }

        private void ApplyLocale()
        {
            if (string.IsNullOrWhiteSpace(_data.LocaleCode))
                return;

            var initialization = LocalizationSettings.InitializationOperation;
            if (!initialization.IsDone)
            {
                if (_waitingForLocalizationInitialization)
                    return;

                _waitingForLocalizationInitialization = true;
                initialization.Completed += _ =>
                {
                    _waitingForLocalizationInitialization = false;
                    ApplyLocale();
                };
                return;
            }

            var locale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(_data.LocaleCode));
            if (locale == null)
            {
                Debug.LogWarning($"[GameSettingsService] Locale '{_data.LocaleCode}' is not available.");
                return;
            }

            if (LocalizationSettings.SelectedLocale != locale)
                LocalizationSettings.SelectedLocale = locale;
        }
    }
}
