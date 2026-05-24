using System;
using System.Collections;
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

        private InputAction _skipAction;
        private Coroutine _playRoutine;
        private Action _onComplete;
        private bool _isPlaying;
        private bool _completionRequested;
        private bool _playRequested;

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

            if (_videoPlayer != null)
                _videoPlayer.loopPointReached -= HandleVideoFinished;
        }

        private void OnDestroy()
        {
            _skipAction?.Dispose();
        }

        public void Play(Action onComplete)
        {
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

            if (_playRoutine != null)
                StopCoroutine(_playRoutine);

            _onComplete = onComplete;
            _playRoutine = StartCoroutine(PlayRoutine());
        }

        public void Skip()
        {
            if (!_isPlaying)
                return;

            Complete();
        }

        private IEnumerator PlayRoutine()
        {
            _isPlaying = true;
            _completionRequested = false;
            SetInputSkipEnabled(true);
            SetVideoDisplayVisible(false);

            SetPanelVisible(true);
            ConfigureMedia();

            if (_videoPlayer == null)
            {
                Debug.LogWarning("[IntroPanelPlayer] VideoPlayer is not assigned.");
                Complete();
                yield break;
            }

            _videoPlayer.loopPointReached -= HandleVideoFinished;
            _videoPlayer.loopPointReached += HandleVideoFinished;

            _videoPlayer.Stop();

            if (_audioSource != null)
                _audioSource.Stop();

            _videoPlayer.Prepare();

            var elapsed = 0f;
            while (!_videoPlayer.isPrepared && elapsed < _prepareTimeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!_videoPlayer.isPrepared)
            {
                Debug.LogWarning("[IntroPanelPlayer] VideoPlayer prepare timed out.");
                Complete();
                yield break;
            }

            ApplyVideoAspectRatio();

            if (_completionRequested)
                yield break;

            if (_audioSource != null && _audioSource.clip != null)
                _audioSource.Play();

            _videoPlayer.Play();
            SetVideoDisplayVisible(true);

            while (!_completionRequested)
            {
                if (!_videoPlayer.isPlaying && HasVideoReachedEnd())
                    Complete();

                yield return null;
            }
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

            _isPlaying = false;
            _completionRequested = true;
            _playRequested = false;
            SetInputSkipEnabled(false);

            StopMedia();
            SetVideoDisplayVisible(false);

            if (_hidePanelOnComplete)
                SetPanelVisible(false);

            if (_playRoutine != null)
            {
                StopCoroutine(_playRoutine);
                _playRoutine = null;
            }

            var onComplete = _onComplete;
            _onComplete = null;
            onComplete?.Invoke();
        }

        private void StopMedia()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.loopPointReached -= HandleVideoFinished;
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
