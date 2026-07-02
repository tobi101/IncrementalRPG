using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private MainMenuButtonView[] _buttons;
        [SerializeField] private SettingsMenuController _settingsMenu;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _authorsPanel;
        [SerializeField] private GameObject _miscPanelsRoot;
        [SerializeField] private GameObject _attentionPanel;
        [SerializeField] private Button _newGameConfirmButton;
        [SerializeField] private Button _newGameCancelButton;

        public IReadOnlyList<MainMenuButtonView> Buttons
        {
            get
            {
                EnsureButtons();
                return _buttons;
            }
        }

        public Button NewGameConfirmButton
        {
            get
            {
                EnsureAttentionPanel();
                return _newGameConfirmButton;
            }
        }

        public Button NewGameCancelButton
        {
            get
            {
                EnsureAttentionPanel();
                return _newGameCancelButton;
            }
        }

        private void Awake()
        {
            EnsureButtons();
            EnsureSettingsPanel();
            EnsureAttentionPanel();
            EnsureMiscPanelsRoot();
            UIButtonAudio.InstallInChildren(this);
            InstallMainMenuButtonHoverAudio();
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
                SetMiscPanelsVisible(true);
                EnsureSettingsPanel();

                if (_settingsPanel != null)
                    _settingsPanel.SetActive(true);

                _settingsMenu.Open();
                return;
            }

            ShowPanel(_settingsPanel, "Settings");
        }

        public void ShowAuthors() => ShowPanel(_authorsPanel, "Authors");

        public void ShowAttention()
        {
            EnsureAttentionPanel();

            if (_attentionPanel == null)
            {
                Debug.LogWarning("[MainMenuView] Attention panel is not assigned.");
                return;
            }

            HidePanels();
            SetMiscPanelsVisible(true);
            _attentionPanel.SetActive(true);
        }

        public void HideAttention()
        {
            EnsureAttentionPanel();

            if (_attentionPanel != null)
                _attentionPanel.SetActive(false);

            HideMiscPanelsIfEmpty();
        }

        public void HidePanels()
        {
            EnsureSettingsPanel();
            EnsureAttentionPanel();
            EnsureMiscPanelsRoot();

            if (_settingsMenu != null)
                _settingsMenu.Close();

            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);

            if (_authorsPanel != null)
                _authorsPanel.SetActive(false);

            if (_attentionPanel != null)
                _attentionPanel.SetActive(false);

            SetMiscPanelsVisible(false);
        }

        private void EnsureButtons()
        {
            if (_buttons != null && _buttons.Any(button => button != null))
                return;

            _buttons = GetComponentsInChildren<MainMenuButtonView>(true);
        }

        private void EnsureSettingsPanel()
        {
            if (_settingsPanel == null && _settingsMenu != null)
                _settingsPanel = _settingsMenu.gameObject;
        }

        private void EnsureMiscPanelsRoot()
        {
            if (_miscPanelsRoot != null)
                return;

            if (_attentionPanel != null)
            {
                var canvas = _attentionPanel.GetComponentInParent<Canvas>(true);
                if (canvas != null)
                {
                    _miscPanelsRoot = canvas.gameObject;
                    return;
                }
            }

            if (_settingsPanel != null)
            {
                var canvas = _settingsPanel.GetComponentInParent<Canvas>(true);
                if (canvas != null)
                    _miscPanelsRoot = canvas.gameObject;
            }
        }

        private void EnsureAttentionPanel()
        {
            if (_attentionPanel == null)
            {
                var attentionTransform = GetComponentsInChildren<RectTransform>(true)
                    .FirstOrDefault(rectTransform => rectTransform.name == "AttentionPanel");

                if (attentionTransform != null)
                    _attentionPanel = attentionTransform.gameObject;
            }

            if (_attentionPanel == null)
                return;

            if (_newGameConfirmButton == null)
                _newGameConfirmButton = FindAttentionButton("YesButton");

            if (_newGameCancelButton == null)
                _newGameCancelButton = FindAttentionButton("NoButton");
        }

        private Button FindAttentionButton(string buttonName)
        {
            return _attentionPanel.GetComponentsInChildren<Button>(true)
                .FirstOrDefault(button => button.name == buttonName);
        }

        private void SetMiscPanelsVisible(bool visible)
        {
            EnsureMiscPanelsRoot();

            if (_miscPanelsRoot != null)
                _miscPanelsRoot.SetActive(visible);
        }

        private void HideMiscPanelsIfEmpty()
        {
            if (IsPanelVisible(_settingsPanel) || IsPanelVisible(_authorsPanel) || IsPanelVisible(_attentionPanel))
                return;

            SetMiscPanelsVisible(false);
        }

        private static bool IsPanelVisible(GameObject panel)
        {
            return panel != null && panel.activeSelf;
        }

        private void InstallMainMenuButtonHoverAudio()
        {
            foreach (var buttonView in _buttons)
            {
                if (buttonView != null)
                    UIButtonAudio.EnsureOn(buttonView.Button, playHoverSound: true, playClickSound: true);
            }
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
