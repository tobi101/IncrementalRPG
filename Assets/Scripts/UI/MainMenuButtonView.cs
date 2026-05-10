using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public enum MainMenuAction
    {
        NewGame,
        Continue,
        Settings,
        Authors,
        Exit
    }

    public class MainMenuButtonView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private MainMenuAction _action;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject[] _pressedVisuals;

        public MainMenuAction Action => _action;
        public Button Button => _button;

        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            SetPressedVisualsVisible(false);
        }

        private void OnEnable()
        {
            SetPressedVisualsVisible(false);
        }

        private void OnDisable()
        {
            SetPressedVisualsVisible(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable)
                return;

            SetPressedVisualsVisible(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SetPressedVisualsVisible(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetPressedVisualsVisible(false);
        }

        private void SetPressedVisualsVisible(bool visible)
        {
            if (_pressedVisuals == null)
                return;

            foreach (var visual in _pressedVisuals)
            {
                if (visual != null)
                    visual.SetActive(visible);
            }
        }
    }
}
