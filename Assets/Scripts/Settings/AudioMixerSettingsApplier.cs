using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Settings
{
    public sealed class AudioMixerSettingsApplier : MonoBehaviour
    {
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private string _masterVolumeParameter = "MasterVolume";
        [SerializeField] private string _musicVolumeParameter = "MusicVolume";
        [SerializeField] private string _sfxVolumeParameter = "SfxVolume";
        [SerializeField] private bool _useAudioListenerAsMasterFallback = true;

        [Inject] private GameSettingsService _settingsService;

        private bool _warnedAboutMissingMixer;

        private void Awake()
        {
            _settingsService ??= GameSettingsServiceLocator.Instance;
        }

        private void OnEnable()
        {
            _settingsService.OnMasterVolumeChanged += ApplyMasterVolume;
            _settingsService.OnMusicVolumeChanged += ApplyMusicVolume;
            _settingsService.OnSfxVolumeChanged += ApplySfxVolume;

            ApplyMasterVolume(_settingsService.MasterVolume);
            ApplyMusicVolume(_settingsService.MusicVolume);
            ApplySfxVolume(_settingsService.SfxVolume);
        }

        private void OnDisable()
        {
            if (_settingsService == null)
                return;

            _settingsService.OnMasterVolumeChanged -= ApplyMasterVolume;
            _settingsService.OnMusicVolumeChanged -= ApplyMusicVolume;
            _settingsService.OnSfxVolumeChanged -= ApplySfxVolume;
        }

        private void ApplyMasterVolume(float value)
        {
            if (!TrySetVolume(_masterVolumeParameter, value) && _useAudioListenerAsMasterFallback)
                AudioListener.volume = Mathf.Clamp01(value);
        }

        private void ApplyMusicVolume(float value)
        {
            TrySetVolume(_musicVolumeParameter, value);
        }

        private void ApplySfxVolume(float value)
        {
            TrySetVolume(_sfxVolumeParameter, value);
        }

        private bool TrySetVolume(string parameterName, float normalizedValue)
        {
            if (_audioMixer == null)
            {
                if (!_warnedAboutMissingMixer)
                {
                    Debug.LogWarning("[AudioMixerSettingsApplier] AudioMixer is not assigned. Only master AudioListener fallback can be applied.");
                    _warnedAboutMissingMixer = true;
                }

                return false;
            }

            if (string.IsNullOrWhiteSpace(parameterName))
                return false;

            var decibels = NormalizedToDecibels(normalizedValue);
            if (!_audioMixer.SetFloat(parameterName, decibels))
            {
                Debug.LogWarning($"[AudioMixerSettingsApplier] AudioMixer parameter '{parameterName}' is not exposed.");
                return false;
            }

            return true;
        }

        private static float NormalizedToDecibels(float value)
        {
            value = Mathf.Clamp01(value);
            return value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
        }
    }
}
