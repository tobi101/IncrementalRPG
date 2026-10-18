using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private MainMenuButtonView[] _buttons;
        [SerializeField] private SettingsMenuController _settingsMenu;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _authorsPanel;
        [SerializeField] private Button _authorsCloseButton;
        [SerializeField] private Button _authorsBackdropButton;
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
            if (_authorsCloseButton != null)
            {
                _authorsCloseButton.onClick.AddListener(HidePanels);
                UIButtonAudio.EnsureOn(_authorsCloseButton, playHoverSound: true);
                UIButtonPressScaler.EnsureOn(_authorsCloseButton);
            }

            if (_authorsBackdropButton != null)
                _authorsBackdropButton.onClick.AddListener(HidePanels);

            HidePanels();
        }

        private void Update()
        {
            if (_authorsPanel != null && _authorsPanel.activeInHierarchy
                && Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                HidePanels();
            }
        }

        private void OnDestroy()
        {
            if (_authorsCloseButton != null)
                _authorsCloseButton.onClick.RemoveListener(HidePanels);

            if (_authorsBackdropButton != null)
                _authorsBackdropButton.onClick.RemoveListener(HidePanels);
        }

        public MainMenuButtonView GetButton(MainMenuAction action)
        {
            EnsureButtons();
            return _buttons.FirstOrDefault(button => button != null && button.Action == action);
        }

        public void SetContinueAvailable(bool available)
        {
            var continueButton = GetButton(MainMenuAction.Continue);
            if (continueButton == null)
                return;

            continueButton.gameObject.SetActive(true);
            if (continueButton.Button != null)
                continueButton.Button.interactable = available;

            var canvasGroup = continueButton.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                canvasGroup.alpha = available ? 1f : 0.5f;
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

        public void ShowAuthors()
        {
            ShowPanel(_authorsPanel, "Authors");

            if (_authorsCloseButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(_authorsCloseButton.gameObject);
        }

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
            bool authorsWereVisible = IsPanelVisible(_authorsPanel);
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

            if (authorsWereVisible && EventSystem.current != null)
            {
                var authorsButton = GetButton(MainMenuAction.Authors);
                if (authorsButton != null)
                    EventSystem.current.SetSelectedGameObject(authorsButton.gameObject);
            }
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
            SetMiscPanelsVisible(true);
            panel.SetActive(true);
        }
    }
}
