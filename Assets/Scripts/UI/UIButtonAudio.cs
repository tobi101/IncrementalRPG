using IncrementalRPG.Scripts.AudioManager;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class UIButtonAudio : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private bool _playHoverSound = true;
        [SerializeField] private bool _playClickSound = true;

        [Inject] private AudioManager _audioManager;

        private bool _isClickSubscribed;

        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            SubscribeClick();
        }

        private void OnDestroy()
        {
            UnsubscribeClick();
        }

        public void Configure(Button button, bool playHoverSound, bool playClickSound)
        {
            if (button != null && button != _button)
            {
                UnsubscribeClick();
                _button = button;
            }

            if (_button == null)
                _button = GetComponent<Button>();

            _playHoverSound = playHoverSound;
            _playClickSound = playClickSound;
            SubscribeClick();
        }

        public static UIButtonAudio EnsureOn(Button button, bool playHoverSound = false, bool playClickSound = true)
        {
            if (button == null || IsExcluded(button))
                return null;

            var audio = button.GetComponent<UIButtonAudio>();
            if (audio == null)
                audio = button.gameObject.AddComponent<UIButtonAudio>();

            audio.Configure(button, playHoverSound, playClickSound);
            return audio;
        }

        public static void InstallInChildren(Component root, bool playHoverSound = false, bool playClickSound = true)
        {
            if (root == null)
                return;

            InstallInChildren(root.transform, playHoverSound, playClickSound);
        }

        public static void InstallInChildren(Transform root, bool playHoverSound = false, bool playClickSound = true)
        {
            if (root == null)
                return;

            var buttons = root.GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
                EnsureOn(button, playHoverSound, playClickSound);
        }

        public static void SetClickSoundEnabled(Button button, bool enabled)
        {
            if (button == null)
                return;

            var audio = button.GetComponent<UIButtonAudio>();
            if (audio != null)
                audio.Configure(button, audio._playHoverSound, enabled);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_playHoverSound) return;
            if (_button != null && !_button.interactable) return;

            ResolveAudioManager()?.PlayUiHover();
        }

        private void PlayClickSound()
        {
            if (!_playClickSound) return;
            ResolveAudioManager()?.PlayUiClick();
        }

        private void SubscribeClick()
        {
            if (_button == null || _isClickSubscribed)
                return;

            _button.onClick.AddListener(PlayClickSound);
            _isClickSubscribed = true;
        }

        private void UnsubscribeClick()
        {
            if (_button == null || !_isClickSubscribed)
                return;

            _button.onClick.RemoveListener(PlayClickSound);
            _isClickSubscribed = false;
        }

        private AudioManager ResolveAudioManager()
        {
            if (_audioManager == null)
                _audioManager = AudioManager.Resolve();

            return _audioManager;
        }

        private static bool IsExcluded(Button button)
        {
            return button.GetComponentInParent<HubFeatureButtonView>(true) != null;
        }
    }
}
