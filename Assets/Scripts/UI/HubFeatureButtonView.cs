using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class HubFeatureButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _glowImage;

        public Button Button => _button;

        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            SetGlowVisible(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            SetGlowVisible(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetGlowVisible(false);
        }

        private void SetGlowVisible(bool visible)
        {
            if (_glowImage == null) return;
            _glowImage.enabled = visible;
        }
    }
}
