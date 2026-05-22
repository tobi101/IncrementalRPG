using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace IncrementalRPG.Scripts.AudioManager
{
    public enum MusicTrack
    {
        None,
        MainMenu,
        Hub,
        Gameplay
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Lifecycle")]
        [SerializeField] private bool _registerAsGlobalInstance = true;
        [SerializeField] private bool _dontDestroyOnLoad;
        [SerializeField] private bool _destroyDuplicateInstances;

        [Header("Sources")]
        [FormerlySerializedAs("_audioSource")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _loopingSfxSource;
        [SerializeField] private bool _createMissingSfxSource = true;
        [SerializeField] private bool _createMissingMusicSource = true;
        [SerializeField] private bool _createMissingLoopingSfxSource = true;
        [SerializeField] private AudioMixerGroup _sfxMixerGroup;
        [SerializeField] private AudioMixerGroup _musicMixerGroup;

        [Header("SFX")]
        [SerializeField] private AudioClip _hitAudioClip;
        [SerializeField] private AudioClip _waveAudioClip;
        [SerializeField] private AudioClip _uiHoverAudioClip;
        [SerializeField] private AudioClip _uiClickAudioClip;
        [SerializeField] private AudioClip _skillUpgradeAudioClip;
        [SerializeField] private AudioClip _skillMaxAudioClip;
        [SerializeField] private AudioClip _skillErrorAudioClip;

        [Header("Looping SFX")]
        [SerializeField] private AudioClip _lavaLoopClip;
        [SerializeField] [Range(0f, 1f)] private float _loopingSfxSourceVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float _lavaLoopMinVolume = 0f;
        [SerializeField] [Range(0f, 1f)] private float _lavaLoopMaxVolume = 1f;
        [SerializeField] [Min(0f)] private float _loopingSfxFadeDuration = 0.25f;

        [Header("Music")]
        [SerializeField] private AudioClip _mainMenuMusicClip;
        [SerializeField] private AudioClip _hubMusicClip;
        [SerializeField] private AudioClip _gameplayMusicClip;
        [SerializeField] [Range(0f, 1f)] private float _musicSourceVolume = 1f;
        [SerializeField] [Min(0f)] private float _musicFadeDuration = 0.5f;

        private Coroutine _musicFadeRoutine;
        private Coroutine _loopingSfxFadeRoutine;
        private MusicTrack _currentMusicTrack = MusicTrack.None;
        private bool _warnedAboutMissingSfxSource;
        private bool _warnedAboutMissingMusicSource;
        private bool _warnedAboutMissingLoopingSfxSource;
        private bool _warnedAboutMissingLavaLoopClip;
        private bool _sourcesConfigured;

        public static AudioManager Resolve(AudioManager fallback = null)
        {
            if (Instance != null)
                return Instance;

            if (fallback != null)
                return fallback;

            return FindFirstObjectByType<AudioManager>();
        }

        private void Awake()
        {
            RegisterGlobalInstance();
            InitializeSources();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void PlayHitAudio(float delay = 0f)
        {
            if (delay <= 0f)
                PlayImmediate();
            else
                StartCoroutine(PlayDelayed(delay));
        }

        public void PlayWaveAudio()
        {
            PlaySfxOneShot(_waveAudioClip, 1f);
        }

        public void PlayUiHover()
        {
            PlaySfxOneShot(_uiHoverAudioClip, 1f);
        }

        public void PlayUiClick()
        {
            PlaySfxOneShot(_uiClickAudioClip, 1f);
        }

        public void PlaySkillUpgrade()
        {
            PlaySfxOneShot(_skillUpgradeAudioClip, 1f);
        }

        public void PlaySkillMax()
        {
            PlaySfxOneShot(_skillMaxAudioClip, 1f);
        }

        public void PlaySkillError()
        {
            PlaySfxOneShot(_skillErrorAudioClip, 1f);
        }

        public void PlaySfx(AudioClip clip, float pitch = 1f)
        {
            PlaySfxOneShot(clip, pitch);
        }

        public void PlayRandomSfx(AudioClip[] clips, float pitch = 1f)
        {
            var clip = PickRandomClip(clips);
            if (clip != null)
                PlaySfxOneShot(clip, pitch);
        }

        public void PlayLavaLoop(bool immediate = false)
        {
            if (_lavaLoopClip == null)
            {
                if (!_warnedAboutMissingLavaLoopClip)
                {
                    Debug.LogWarning("[AudioManager] Lava loop clip is not assigned.");
                    _warnedAboutMissingLavaLoopClip = true;
                }

                return;
            }

            PlayLoopingSfx(_lavaLoopClip, GetLavaLoopVolume(0f), immediate);
        }

        public void StopLavaLoop(bool immediate = false)
        {
            StopLoopingSfx(immediate);
        }

        public void SetLavaLoopProgress(float progress)
        {
            SetLoopingSfxVolume(GetLavaLoopVolume(progress));
        }

        public void PlayLoopingSfx(AudioClip clip, bool immediate = false)
        {
            PlayLoopingSfx(clip, _loopingSfxSourceVolume, immediate);
        }

        private void PlayLoopingSfx(AudioClip clip, float targetVolume, bool immediate)
        {
            if (clip == null)
                return;

            targetVolume = Mathf.Clamp01(targetVolume);
            InitializeSources();
            if (!HasLoopingSfxSource())
                return;

            if (_loopingSfxFadeRoutine != null)
            {
                StopCoroutine(_loopingSfxFadeRoutine);
                _loopingSfxFadeRoutine = null;
            }

            if (_loopingSfxSource.clip == clip && _loopingSfxSource.isPlaying)
            {
                _loopingSfxSource.volume = targetVolume;
                return;
            }

            if (immediate || _loopingSfxFadeDuration <= 0f)
            {
                PlayLoopingSfxImmediate(clip, targetVolume);
                return;
            }

            _loopingSfxFadeRoutine = StartCoroutine(FadeToLoopingSfx(clip, targetVolume));
        }

        public void StopLoopingSfx(bool immediate = false)
        {
            InitializeSources();
            if (!HasLoopingSfxSource())
                return;

            if (_loopingSfxFadeRoutine != null)
            {
                StopCoroutine(_loopingSfxFadeRoutine);
                _loopingSfxFadeRoutine = null;
            }

            if (!_loopingSfxSource.isPlaying && _loopingSfxSource.clip == null)
                return;

            if (immediate || _loopingSfxFadeDuration <= 0f)
            {
                _loopingSfxSource.Stop();
                _loopingSfxSource.clip = null;
                _loopingSfxSource.volume = _loopingSfxSourceVolume;
                return;
            }

            _loopingSfxFadeRoutine = StartCoroutine(FadeOutLoopingSfx());
        }

        private void SetLoopingSfxVolume(float volume)
        {
            InitializeSources();
            if (!HasLoopingSfxSource())
                return;

            if (_loopingSfxFadeRoutine != null)
            {
                StopCoroutine(_loopingSfxFadeRoutine);
                _loopingSfxFadeRoutine = null;
            }

            _loopingSfxSource.volume = Mathf.Clamp01(volume);
        }

        public void PlayMusic(MusicTrack track, bool immediate = false)
        {
            InitializeSources();

            var clip = GetMusicClip(track);
            if (track == MusicTrack.None || clip == null)
            {
                StopMusic(immediate);
                return;
            }

            if (!HasMusicSource())
                return;

            if (_currentMusicTrack == track && _musicSource.clip == clip && _musicSource.isPlaying)
                return;

            if (_musicFadeRoutine != null)
                StopCoroutine(_musicFadeRoutine);

            if (immediate || _musicFadeDuration <= 0f)
            {
                PlayMusicImmediate(track, clip);
                return;
            }

            _musicFadeRoutine = StartCoroutine(FadeToMusic(track, clip));
        }

        public void StopMusic(bool immediate = false)
        {
            InitializeSources();

            if (!HasMusicSource())
                return;

            if (_musicFadeRoutine != null)
                StopCoroutine(_musicFadeRoutine);

            if (immediate || _musicFadeDuration <= 0f)
            {
                _musicSource.Stop();
                _musicSource.clip = null;
                _musicSource.volume = _musicSourceVolume;
                _currentMusicTrack = MusicTrack.None;
                return;
            }

            _musicFadeRoutine = StartCoroutine(FadeOutMusic());
        }

        private IEnumerator PlayDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayImmediate();
        }

        private void PlayImmediate()
        {
            PlaySfxOneShot(_hitAudioClip, Random.Range(0.85f, 1.2f));
        }

        private void PlaySfxOneShot(AudioClip clip, float pitch)
        {
            if (clip == null) return;

            InitializeSources();
            if (!HasSfxSource()) return;

            _sfxSource.pitch = pitch;
            _sfxSource.PlayOneShot(clip);
        }

        private static AudioClip PickRandomClip(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
                return null;

            var validCount = 0;
            for (var i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null)
                    validCount++;
            }

            if (validCount == 0)
                return null;

            var targetIndex = Random.Range(0, validCount);
            for (var i = 0; i < clips.Length; i++)
            {
                if (clips[i] == null)
                    continue;

                if (targetIndex == 0)
                    return clips[i];

                targetIndex--;
            }

            return null;
        }

        private void RegisterGlobalInstance()
        {
            if (!_registerAsGlobalInstance)
                return;

            if (Instance != null && Instance != this)
            {
                if (_destroyDuplicateInstances)
                    Destroy(gameObject);

                return;
            }

            Instance = this;

            if (_dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }

        private void InitializeSources()
        {
            if (_sourcesConfigured)
                return;

            EnsureSources();
            ConfigureSources();
            _sourcesConfigured = true;
        }

        private void EnsureSources()
        {
            if (_sfxSource == null)
                _sfxSource = GetComponent<AudioSource>();

            if (_sfxSource == null && _createMissingSfxSource)
                _sfxSource = gameObject.AddComponent<AudioSource>();

            if (_musicSource == null && _createMissingMusicSource)
                _musicSource = gameObject.AddComponent<AudioSource>();

            if (_loopingSfxSource == null && _createMissingLoopingSfxSource)
                _loopingSfxSource = gameObject.AddComponent<AudioSource>();
        }

        private void ConfigureSources()
        {
            if (_sfxSource != null)
            {
                _sfxSource.playOnAwake = false;

                if (_sfxMixerGroup != null)
                    _sfxSource.outputAudioMixerGroup = _sfxMixerGroup;
            }

            if (_loopingSfxSource != null)
            {
                _loopingSfxSource.playOnAwake = false;
                _loopingSfxSource.loop = true;
                _loopingSfxSource.pitch = 1f;
                _loopingSfxSource.volume = _loopingSfxSourceVolume;
                _loopingSfxSource.spatialBlend = 0f;

                if (_sfxMixerGroup != null)
                    _loopingSfxSource.outputAudioMixerGroup = _sfxMixerGroup;
            }

            if (_musicSource == null)
                return;

            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.pitch = 1f;
            _musicSource.volume = _musicSourceVolume;

            if (_musicMixerGroup != null)
                _musicSource.outputAudioMixerGroup = _musicMixerGroup;
        }

        private AudioClip GetMusicClip(MusicTrack track)
        {
            switch (track)
            {
                case MusicTrack.MainMenu:
                    return _mainMenuMusicClip;
                case MusicTrack.Hub:
                    return _hubMusicClip;
                case MusicTrack.Gameplay:
                    return _gameplayMusicClip;
                case MusicTrack.None:
                default:
                    return null;
            }
        }

        private void PlayMusicImmediate(MusicTrack track, AudioClip clip)
        {
            _musicSource.clip = clip;
            _musicSource.loop = true;
            _musicSource.pitch = 1f;
            _musicSource.volume = _musicSourceVolume;
            _musicSource.Play();
            _currentMusicTrack = track;
        }

        private float GetLavaLoopVolume(float progress)
        {
            var min = Mathf.Clamp01(_lavaLoopMinVolume);
            var max = Mathf.Clamp01(_lavaLoopMaxVolume);

            if (max < min)
                (min, max) = (max, min);

            return Mathf.Lerp(min, max, Mathf.Clamp01(progress));
        }

        private void PlayLoopingSfxImmediate(AudioClip clip, float targetVolume)
        {
            _loopingSfxSource.clip = clip;
            _loopingSfxSource.loop = true;
            _loopingSfxSource.pitch = 1f;
            _loopingSfxSource.volume = targetVolume;
            _loopingSfxSource.spatialBlend = 0f;
            _loopingSfxSource.Play();
        }

        private IEnumerator FadeToMusic(MusicTrack track, AudioClip clip)
        {
            if (_musicSource.isPlaying && _musicSource.clip != null)
                yield return FadeMusicVolume(_musicSource.volume, 0f, _musicFadeDuration * 0.5f);

            _musicSource.clip = clip;
            _musicSource.loop = true;
            _musicSource.pitch = 1f;
            _musicSource.volume = 0f;
            _musicSource.Play();
            _currentMusicTrack = track;

            yield return FadeMusicVolume(0f, _musicSourceVolume, _musicFadeDuration * 0.5f);
            _musicFadeRoutine = null;
        }

        private IEnumerator FadeToLoopingSfx(AudioClip clip, float targetVolume)
        {
            if (_loopingSfxSource.isPlaying && _loopingSfxSource.clip != null)
                yield return FadeLoopingSfxVolume(_loopingSfxSource.volume, 0f, _loopingSfxFadeDuration * 0.5f);

            _loopingSfxSource.clip = clip;
            _loopingSfxSource.loop = true;
            _loopingSfxSource.pitch = 1f;
            _loopingSfxSource.volume = 0f;
            _loopingSfxSource.spatialBlend = 0f;
            _loopingSfxSource.Play();

            yield return FadeLoopingSfxVolume(0f, targetVolume, _loopingSfxFadeDuration * 0.5f);
            _loopingSfxFadeRoutine = null;
        }

        private IEnumerator FadeOutMusic()
        {
            yield return FadeMusicVolume(_musicSource.volume, 0f, _musicFadeDuration);
            _musicSource.Stop();
            _musicSource.clip = null;
            _musicSource.volume = _musicSourceVolume;
            _currentMusicTrack = MusicTrack.None;
            _musicFadeRoutine = null;
        }

        private IEnumerator FadeOutLoopingSfx()
        {
            yield return FadeLoopingSfxVolume(_loopingSfxSource.volume, 0f, _loopingSfxFadeDuration);
            _loopingSfxSource.Stop();
            _loopingSfxSource.clip = null;
            _loopingSfxSource.volume = _loopingSfxSourceVolume;
            _loopingSfxFadeRoutine = null;
        }

        private IEnumerator FadeMusicVolume(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                _musicSource.volume = to;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _musicSource.volume = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            _musicSource.volume = to;
        }

        private IEnumerator FadeLoopingSfxVolume(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                _loopingSfxSource.volume = to;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _loopingSfxSource.volume = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            _loopingSfxSource.volume = to;
        }

        private bool HasSfxSource()
        {
            if (_sfxSource != null)
                return true;

            if (!_warnedAboutMissingSfxSource)
            {
                Debug.LogWarning("[AudioManager] SFX AudioSource is not assigned.");
                _warnedAboutMissingSfxSource = true;
            }

            return false;
        }

        private bool HasLoopingSfxSource()
        {
            if (_loopingSfxSource != null)
                return true;

            if (!_warnedAboutMissingLoopingSfxSource)
            {
                Debug.LogWarning("[AudioManager] Looping SFX AudioSource is not assigned.");
                _warnedAboutMissingLoopingSfxSource = true;
            }

            return false;
        }

        private bool HasMusicSource()
        {
            if (_musicSource != null)
                return true;

            if (!_warnedAboutMissingMusicSource)
            {
                Debug.LogWarning("[AudioManager] Music AudioSource is not assigned.");
                _warnedAboutMissingMusicSource = true;
            }

            return false;
        }
    }
}
