using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private MainMenuButtonView[] _buttons;
        [SerializeField] private SettingsMenuController _settingsMenu;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _authorsPanel;

        public IReadOnlyList<MainMenuButtonView> Buttons
        {
            get
            {
                EnsureButtons();
                return _buttons;
            }
        }

        private void Awake()
        {
            EnsureButtons();
            HidePanels();
        }

        public MainMenuButtonView GetButton(MainMenuAction action)
        {
            EnsureButtons();
            return _buttons.FirstOrDefault(button => button != null && button.Action == action);
        }

        public void SetContinueVisible(bool visible)
        {
            var continueButton = GetButton(MainMenuAction.Continue);
            if (continueButton != null)
                continueButton.gameObject.SetActive(visible);
        }

        public void SetButtonsInteractable(bool interactable)
        {
            EnsureButtons();

            foreach (var buttonView in _buttons)
            {
                if (buttonView != null && buttonView.Button != null)
                    buttonView.Button.interactable = interactable;
            }
        }

        public void ShowSettings()
        {
            if (_settingsMenu != null)
            {
                HidePanels();
                _settingsMenu.Open();
                return;
            }

            ShowPanel(_settingsPanel, "Settings");
        }

        public void ShowAuthors() => ShowPanel(_authorsPanel, "Authors");

        public void HidePanels()
        {
            if (_settingsMenu != null)
                _settingsMenu.Close();
            else if (_settingsPanel != null)
                _settingsPanel.SetActive(false);

            if (_authorsPanel != null)
                _authorsPanel.SetActive(false);
        }

        private void EnsureButtons()
        {
            if (_buttons != null && _buttons.Any(button => button != null))
                return;

            _buttons = GetComponentsInChildren<MainMenuButtonView>(true);
        }

        private void ShowPanel(GameObject panel, string panelName)
        {
            if (panel == null)
            {
                Debug.LogWarning($"[MainMenuView] {panelName} panel is not assigned.");
                return;
            }

            HidePanels();
            panel.SetActive(true);
        }
    }
}
