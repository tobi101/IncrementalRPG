using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class SettingsMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private TMP_Dropdown _languageDropdown;
        [SerializeField] private Button _backButton;

        public Slider MasterVolumeSlider => _masterVolumeSlider;
        public Slider MusicVolumeSlider => _musicVolumeSlider;
        public Slider SfxVolumeSlider => _sfxVolumeSlider;
        public TMP_Dropdown LanguageDropdown => _languageDropdown;
        public Button BackButton => _backButton;

        private GameObject Root => _root != null ? _root : gameObject;

        public void Show()
        {
            Root.SetActive(true);
        }

        public void Hide()
        {
            Root.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            Root.SetActive(visible);
        }

        public bool IsVisible()
        {
            return Root.activeSelf;
        }
    }
}
