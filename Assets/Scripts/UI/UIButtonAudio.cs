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

        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_button != null)
                _button.onClick.AddListener(PlayClickSound);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(PlayClickSound);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_playHoverSound) return;
            if (_button != null && !_button.interactable) return;

            _audioManager?.PlayUiHover();
        }

        private void PlayClickSound()
        {
            if (!_playClickSound) return;
            _audioManager?.PlayUiClick();
        }
    }
}
