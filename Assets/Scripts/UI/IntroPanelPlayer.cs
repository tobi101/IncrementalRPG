using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UI
{
    public class IntroPanelPlayer : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private GameObject _videoDisplay;
        [SerializeField] private Button _skipButton;
        [SerializeField] private bool _hidePanelOnAwake = true;
        [SerializeField] private bool _hidePanelOnComplete;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;

        [Header("Media")]
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _audioClip;

        [Header("Playback")]
        [SerializeField] private bool _allowInputSkip = true;
        [SerializeField] [Min(0f)] private float _prepareTimeout = 5f;
        [SerializeField] [Min(0.1f)] private float _firstFrameTimeout = 5f;
        [SerializeField] [Min(0.1f)] private float _playbackStallTimeout = 5f;

        private enum PlaybackPhase
        {
            Idle,
            Preparing,
            WaitingForFirstFrame,
            Playing
        }

        private InputAction _skipAction;
        private Action _onComplete;
        private bool _isPlaying;
        private bool _playRequested;
        private PlaybackPhase _phase;
        private float _phaseElapsed;
        private float _secondsWithoutFrame;
        private long _lastReceivedFrame = -1;
        private bool _hasFirstFrame;
        private string _playbackError;
        private bool _applicationPaused;
        private bool _resetWatchdogTimers;

        public bool IsPlaying => _isPlaying;

        private void Reset()
        {
            _panel = gameObject;
            _videoPlayer = GetComponentInChildren<VideoPlayer>(true);
            _audioSource = GetComponentInChildren<AudioSource>(true);
            _aspectRatioFitter = GetComponentInChildren<AspectRatioFitter>(true);
            _videoDisplay = _aspectRatioFitter != null ? _aspectRatioFitter.gameObject : null;
        }

        private void Awake()
        {
            if (_panel == null)
                _panel = gameObject;

            if (_videoPlayer == null)
                _videoPlayer = GetComponentInChildren<VideoPlayer>(true);

            if (_audioSource == null)
                _audioSource = GetComponentInChildren<AudioSource>(true);

            if (_aspectRatioFitter == null)
                _aspectRatioFitter = GetComponentInChildren<AspectRatioFitter>(true);

            if (_videoDisplay == null && _aspectRatioFitter != null)
                _videoDisplay = _aspectRatioFitter.gameObject;

            InitializeSkipAction();
            ConfigureMedia();
            SetVideoDisplayVisible(false);

            if (_hidePanelOnAwake && !_playRequested)
                SetPanelVisible(false);
        }

        private void OnEnable()
        {
            if (_skipButton == null)
            {
                SubscribeSkipAction();
                return;
            }

            _skipButton.onClick.RemoveListener(Skip);
            _skipButton.onClick.AddListener(Skip);
            SubscribeSkipAction();
        }

        private void OnDisable()
        {
            if (_skipButton != null)
                _skipButton.onClick.RemoveListener(Skip);

            UnsubscribeSkipAction();
            CancelPlayback();
        }

        private void OnDestroy()
        {
            _skipAction?.Dispose();
        }

        public void Play(Action onComplete)
        {
            CancelPlayback();
            _playRequested = true;

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[IntroPanelPlayer] Cannot play intro while the player object hierarchy is inactive.");
                _playRequested = false;
                onComplete?.Invoke();
                return;
            }

            _onComplete = onComplete;
            _isPlaying = true;
            _phase = PlaybackPhase.Preparing;
            _phaseElapsed = 0f;
            _secondsWithoutFrame = 0f;
            _lastReceivedFrame = -1;
            _hasFirstFrame = false;
            _playbackError = null;
            SetInputSkipEnabled(true);
            SetVideoDisplayVisible(false);
            SetPanelVisible(true);
            ConfigureMedia();

            if (_videoPlayer == null)
            {
                FailPlayback("VideoPlayer is not assigned.");
                return;
            }

            SubscribeVideoEvents();
            _videoPlayer.sendFrameReadyEvents = true;
            _videoPlayer.Prepare();
        }

        public void Skip()
        {
            if (!_isPlaying)
                return;

            Complete();
        }

        private void OnApplicationPause(bool paused)
        {
            _applicationPaused = paused;
            _resetWatchdogTimers = true;
        }

        private void OnApplicationFocus(bool focused)
        {
            if (focused)
                _resetWatchdogTimers = true;
        }

        private void Update()
        {
            if (!_isPlaying || _applicationPaused || (!Application.isFocused && !Application.runInBackground))
                return;

            var deltaTime = Time.unscaledDeltaTime;
            if (_resetWatchdogTimers)
            {
                // Time spent with the application suspended is not a decoder stall.
                _phaseElapsed = 0f;
                _secondsWithoutFrame = 0f;
                _resetWatchdogTimers = false;
                deltaTime = 0f;
            }

            TickPlayback(deltaTime);
        }

        private void TickPlayback(float deltaTime)
        {
            if (!_isPlaying)
                return;

            if (_playbackError != null)
            {
                FailPlayback($"VideoPlayer error: {_playbackError}");
                return;
            }

            if (_videoPlayer == null)
            {
                FailPlayback("VideoPlayer became unavailable during playback.");
                return;
            }

            _phaseElapsed += deltaTime;
            switch (_phase)
            {
                case PlaybackPhase.Preparing:
                    if (_videoPlayer.isPrepared)
                    {
                        ApplyVideoAspectRatio();
                        _phase = PlaybackPhase.WaitingForFirstFrame;
                        _phaseElapsed = 0f;
                        _videoPlayer.Play();
                    }
                    else if (_phaseElapsed >= _prepareTimeout)
                    {
                        FailPlayback("VideoPlayer prepare timed out.");
                    }
                    break;

                case PlaybackPhase.WaitingForFirstFrame:
                    if (_hasFirstFrame)
                    {
                        _phase = PlaybackPhase.Playing;
                        _phaseElapsed = 0f;
                        _secondsWithoutFrame = 0f;
                        SetVideoDisplayVisible(true);

                        if (_audioSource != null && _audioSource.clip != null)
                            _audioSource.Play();
                    }
                    else if (_phaseElapsed >= _firstFrameTimeout)
                    {
                        FailPlayback("VideoPlayer first frame timed out.");
                    }
                    break;

                case PlaybackPhase.Playing:
                    if (!_videoPlayer.isPlaying && HasVideoReachedEnd())
                    {
                        Complete();
                        return;
                    }

                    _secondsWithoutFrame += deltaTime;
                    if (_secondsWithoutFrame >= _playbackStallTimeout)
                        FailPlayback("VideoPlayer playback stalled: no new video frames.");
                    break;
            }
        }

        private void SubscribeVideoEvents()
        {
            _videoPlayer.loopPointReached -= HandleVideoFinished;
            _videoPlayer.loopPointReached += HandleVideoFinished;
            _videoPlayer.errorReceived -= HandleVideoError;
            _videoPlayer.errorReceived += HandleVideoError;
            _videoPlayer.frameReady -= HandleFrameReady;
            _videoPlayer.frameReady += HandleFrameReady;
        }

        private void HandleVideoError(VideoPlayer source, string message)
        {
            if (_isPlaying && source == _videoPlayer)
                _playbackError = message ?? "Unknown video error.";
        }

        private void HandleFrameReady(VideoPlayer source, long frameIndex)
        {
            if (!_isPlaying || source != _videoPlayer || _phase == PlaybackPhase.Preparing)
                return;

            _hasFirstFrame = true;
            if (frameIndex == _lastReceivedFrame)
                return;

            _lastReceivedFrame = frameIndex;
            _secondsWithoutFrame = 0f;
        }

        private void FailPlayback(string reason)
        {
            if (!_isPlaying)
                return;

            var media = _videoPlayer == null ? "unassigned" :
                _videoPlayer.clip != null ? _videoPlayer.clip.name : _videoPlayer.url;
            Debug.LogWarning($"[IntroPanelPlayer] {reason} Source='{media}', phase={_phase}, " +
                $"lastFrame={_lastReceivedFrame}, waiting={_phaseElapsed:F2}s, stalled={_secondsWithoutFrame:F2}s.", this);
            Complete();
        }

        private void ConfigureMedia()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.playOnAwake = false;
                _videoPlayer.isLooping = false;
                _videoPlayer.waitForFirstFrame = true;
                _videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            }

            if (_audioSource == null)
                return;

            _audioSource.playOnAwake = false;
            _audioSource.loop = false;

            if (_audioClip != null)
                _audioSource.clip = _audioClip;
        }

        private void InitializeSkipAction()
        {
            if (_skipAction != null)
                return;

            _skipAction = new InputAction("SkipIntro", InputActionType.Button);
            _skipAction.AddBinding("<Keyboard>/escape");
            _skipAction.AddBinding("<Keyboard>/space");
            _skipAction.AddBinding("<Mouse>/leftButton");
            _skipAction.AddBinding("<Mouse>/rightButton");
            _skipAction.AddBinding("<Mouse>/middleButton");
            _skipAction.AddBinding("<Mouse>/backButton");
            _skipAction.AddBinding("<Mouse>/forwardButton");
            _skipAction.AddBinding("<Gamepad>/start");
            _skipAction.AddBinding("<Gamepad>/buttonSouth");
        }

        private void SubscribeSkipAction()
        {
            InitializeSkipAction();
            _skipAction.performed -= HandleSkipPerformed;
            _skipAction.performed += HandleSkipPerformed;
        }

        private void UnsubscribeSkipAction()
        {
            if (_skipAction == null)
                return;

            _skipAction.performed -= HandleSkipPerformed;
            _skipAction.Disable();
        }

        private void SetInputSkipEnabled(bool enabled)
        {
            if (_skipAction == null)
                return;

            if (enabled && _allowInputSkip)
                _skipAction.Enable();
            else
                _skipAction.Disable();
        }

        private void HandleSkipPerformed(InputAction.CallbackContext context)
        {
            Skip();
        }

        private void ApplyVideoAspectRatio()
        {
            if (_aspectRatioFitter == null || _videoPlayer == null || _videoPlayer.width == 0 || _videoPlayer.height == 0)
                return;

            _aspectRatioFitter.aspectRatio = (float)_videoPlayer.width / _videoPlayer.height;
        }

        private bool HasVideoReachedEnd()
        {
            if (_videoPlayer == null || _videoPlayer.isLooping || _videoPlayer.length <= 0d)
                return false;

            return _videoPlayer.time >= _videoPlayer.length - 0.05d;
        }

        private void HandleVideoFinished(VideoPlayer source)
        {
            Complete();
        }

        private void Complete()
        {
            if (!_isPlaying)
                return;

            var onComplete = _onComplete;
            CancelPlayback();
            SetVideoDisplayVisible(false);

            if (_hidePanelOnComplete)
                SetPanelVisible(false);

            onComplete?.Invoke();
        }

        private void CancelPlayback()
        {
            _isPlaying = false;
            _playRequested = false;
            _phase = PlaybackPhase.Idle;
            _playbackError = null;
            _onComplete = null;
            SetInputSkipEnabled(false);
            StopMedia();
        }

        private void StopMedia()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.loopPointReached -= HandleVideoFinished;
                _videoPlayer.errorReceived -= HandleVideoError;
                _videoPlayer.frameReady -= HandleFrameReady;
                _videoPlayer.sendFrameReadyEvents = false;
                _videoPlayer.Stop();
            }

            if (_audioSource != null)
                _audioSource.Stop();
        }

        private void SetPanelVisible(bool visible)
        {
            if (_panel != null)
                _panel.SetActive(visible);
        }

        private void SetVideoDisplayVisible(bool visible)
        {
            if (_videoDisplay != null && _videoDisplay != gameObject)
                _videoDisplay.SetActive(visible);
        }
    }
}
