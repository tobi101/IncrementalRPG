using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class SideMenuFlyoutView : MonoBehaviour
    {
        [SerializeField] private Button _toggleButton;
        [SerializeField] private GameObject _listRoot;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _returnToHubButton;
        [SerializeField, Min(0f)] private float _animationDuration = 0.16f;
        [SerializeField, Min(0f)] private float _itemDelay = 0.04f;
        [SerializeField] private Vector2 _closedOffset = new Vector2(0f, 18f);

        private CanvasGroup _canvasGroup;
        private RectTransform _listTransform;
        private RectTransform[] _itemTransforms = Array.Empty<RectTransform>();
        private CanvasGroup[] _itemCanvasGroups = Array.Empty<CanvasGroup>();
        private Vector2[] _itemOpenPositions = Array.Empty<Vector2>();
        private Vector2 _openPosition;
        private Coroutine _animationRoutine;
        private bool _isOpen;
        private bool _isSubscribed;
        private bool _hasOpenPosition;

        public event Action<SideMenuFlyoutView> SettingsRequested;
        public event Action<SideMenuFlyoutView> MainMenuRequested;
        public event Action<SideMenuFlyoutView> ExitRequested;

        public bool IsOpen => _isOpen;
        public Button ReturnToHubButton => _returnToHubButton;

        private void Awake()
        {
            EnsureAnimationReferences();
            InstallButtonEffects();
            CloseImmediate();
        }

        private void OnEnable()
        {
            EnsureAnimationReferences();
            SubscribeButtons();
            CloseImmediate();
        }

        private void OnDisable()
        {
            UnsubscribeButtons();
            CloseImmediate();
        }

        public void Configure(
            Button toggleButton,
            GameObject listRoot,
            Button settingsButton,
            Button mainMenuButton,
            Button exitButton,
            Button returnToHubButton = null)
        {
            StopAnimation();
            RestoreCachedItemOpenPositions();

            var wasSubscribed = _isSubscribed;
            if (wasSubscribed)
                UnsubscribeButtons();

            _toggleButton = toggleButton;
            _listRoot = listRoot;
            _settingsButton = settingsButton;
            _mainMenuButton = mainMenuButton;
            _exitButton = exitButton;
            _returnToHubButton = returnToHubButton;
            _listTransform = null;
            _canvasGroup = null;
            _itemTransforms = Array.Empty<RectTransform>();
            _itemCanvasGroups = Array.Empty<CanvasGroup>();
            _itemOpenPositions = Array.Empty<Vector2>();
            _hasOpenPosition = false;

            EnsureAnimationReferences();

            SubscribeButtons();

            CloseImmediate();
        }

        public void Toggle()
        {
            if (_isOpen)
                Close();
            else
                Open();
        }

        public void Open()
        {
            if (_listRoot == null)
                return;

            StopAnimation();
            EnsureAnimationReferences();
            _isOpen = true;
            _listRoot.SetActive(true);
            SetInteractionEnabled(false);

            if (!CanAnimate())
            {
                ApplyOpenVisuals();
                return;
            }

            _animationRoutine = StartCoroutine(Animate(true));
        }

        public void Close()
        {
            if (_listRoot == null)
                return;

            StopAnimation();
            _isOpen = false;
            SetInteractionEnabled(false);

            if (!_listRoot.activeSelf || !CanAnimate())
            {
                CloseImmediate();
                return;
            }

            _animationRoutine = StartCoroutine(Animate(false));
        }

        public void CloseImmediate()
        {
            StopAnimation();
            _isOpen = false;

            if (_listTransform != null)
                _listTransform.anchoredPosition = _openPosition;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            ApplyClosedItemVisuals();

            if (_listRoot != null && _listRoot.activeSelf)
                _listRoot.SetActive(false);
        }

        private void HandleSettingsClicked()
        {
            Close();
            SettingsRequested?.Invoke(this);
        }

        private void HandleMainMenuClicked()
        {
            Close();
            MainMenuRequested?.Invoke(this);
        }

        private void HandleExitClicked()
        {
            Close();
            ExitRequested?.Invoke(this);
        }

        private void HandleReturnToHubClicked()
        {
            Close();
        }

        private void SubscribeButtons()
        {
            if (_isSubscribed)
                return;

            AddListener(_toggleButton, Toggle);
            AddListener(_settingsButton, HandleSettingsClicked);
            AddListener(_mainMenuButton, HandleMainMenuClicked);
            AddListener(_exitButton, HandleExitClicked);
            AddListener(_returnToHubButton, HandleReturnToHubClicked);
            _isSubscribed = true;
        }

        private void UnsubscribeButtons()
        {
            if (!_isSubscribed)
                return;

            RemoveListener(_toggleButton, Toggle);
            RemoveListener(_settingsButton, HandleSettingsClicked);
            RemoveListener(_mainMenuButton, HandleMainMenuClicked);
            RemoveListener(_exitButton, HandleExitClicked);
            RemoveListener(_returnToHubButton, HandleReturnToHubClicked);
            _isSubscribed = false;
        }

        private void EnsureAnimationReferences()
        {
            if (_listRoot == null)
                return;

            _listTransform = _listRoot.transform as RectTransform;
            if (_listTransform != null && !_hasOpenPosition)
            {
                _openPosition = _listTransform.anchoredPosition;
                _hasOpenPosition = true;
            }

            _canvasGroup = _listRoot.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = _listRoot.AddComponent<CanvasGroup>();

            EnsureItemReferences();
        }

        private void EnsureItemReferences()
        {
            if (_listTransform == null)
                return;

            var itemCount = _listTransform.childCount;
            if (_itemTransforms.Length == itemCount)
                return;

            RestoreCachedItemOpenPositions();

            _itemTransforms = new RectTransform[itemCount];
            _itemCanvasGroups = new CanvasGroup[itemCount];
            _itemOpenPositions = new Vector2[itemCount];

            for (var index = 0; index < itemCount; index++)
            {
                var itemTransform = _listTransform.GetChild(index) as RectTransform;
                _itemTransforms[index] = itemTransform;
                if (itemTransform == null)
                    continue;

                _itemOpenPositions[index] = itemTransform.anchoredPosition;
                var itemCanvasGroup = itemTransform.GetComponent<CanvasGroup>();
                if (itemCanvasGroup == null)
                    itemCanvasGroup = itemTransform.gameObject.AddComponent<CanvasGroup>();

                _itemCanvasGroups[index] = itemCanvasGroup;
            }
        }

        private void InstallButtonEffects()
        {
            UIButtonAudio.InstallInChildren(this);
            UIButtonPressScaler.InstallInChildren(this);
            UIButtonAudio.EnsureOn(_toggleButton);

            if (_toggleButton != null && _toggleButton.GetComponent<PauseButtonVisualState>() == null)
                UIButtonPressScaler.EnsureOn(_toggleButton);
        }

        private IEnumerator Animate(bool opening)
        {
            var elapsed = 0f;
            var itemCount = _itemTransforms.Length;
            var totalDuration = _animationDuration + Mathf.Max(0, itemCount - 1) * _itemDelay;
            var startVisibilities = new float[itemCount];

            for (var index = 0; index < itemCount; index++)
            {
                var itemCanvasGroup = _itemCanvasGroups[index];
                startVisibilities[index] = itemCanvasGroup != null
                    ? itemCanvasGroup.alpha
                    : opening ? 0f : 1f;
            }

            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;

            if (_listTransform != null)
                _listTransform.anchoredPosition = _openPosition;

            while (elapsed < totalDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                for (var index = 0; index < itemCount; index++)
                {
                    var cascadeIndex = opening ? index : itemCount - 1 - index;
                    var progress = SideMenuCascadeTiming.Evaluate(
                        elapsed,
                        cascadeIndex,
                        _animationDuration,
                        _itemDelay);
                    var eased = 1f - Mathf.Pow(1f - progress, 3f);
                    var targetVisibility = opening ? 1f : 0f;
                    var visibility = Mathf.Lerp(startVisibilities[index], targetVisibility, eased);
                    ApplyItemVisual(index, visibility);
                }

                yield return null;
            }

            _animationRoutine = null;

            if (opening)
            {
                ApplyOpenVisuals();
                yield break;
            }

            CloseImmediate();
        }

        private bool CanAnimate()
        {
            return _animationDuration > 0f && isActiveAndEnabled && gameObject.activeInHierarchy;
        }

        private void ApplyOpenVisuals()
        {
            if (_listTransform != null)
                _listTransform.anchoredPosition = _openPosition;

            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;

            ApplyOpenItemVisuals();
            SetInteractionEnabled(true);
        }

        private void ApplyClosedItemVisuals()
        {
            for (var index = 0; index < _itemTransforms.Length; index++)
                ApplyItemVisual(index, 0f);
        }

        private void ApplyOpenItemVisuals()
        {
            for (var index = 0; index < _itemTransforms.Length; index++)
                ApplyItemVisual(index, 1f);
        }

        private void RestoreCachedItemOpenPositions()
        {
            var itemCount = Mathf.Min(_itemTransforms.Length, _itemOpenPositions.Length);
            for (var index = 0; index < itemCount; index++)
            {
                if (_itemTransforms[index] != null)
                    _itemTransforms[index].anchoredPosition = _itemOpenPositions[index];
            }
        }

        private void ApplyItemVisual(int index, float visibility)
        {
            if (index < 0 || index >= _itemTransforms.Length)
                return;

            var itemTransform = _itemTransforms[index];
            if (itemTransform != null)
            {
                itemTransform.anchoredPosition = Vector2.LerpUnclamped(
                    _itemOpenPositions[index] + _closedOffset,
                    _itemOpenPositions[index],
                    visibility);
            }

            var itemCanvasGroup = _itemCanvasGroups[index];
            if (itemCanvasGroup != null)
                itemCanvasGroup.alpha = visibility;
        }

        private void SetInteractionEnabled(bool enabled)
        {
            if (_canvasGroup == null)
                return;

            _canvasGroup.interactable = enabled;
            _canvasGroup.blocksRaycasts = enabled;
        }

        private void StopAnimation()
        {
            if (_animationRoutine == null)
                return;

            StopCoroutine(_animationRoutine);
            _animationRoutine = null;
        }

        private static void AddListener(Button button, UnityEngine.Events.UnityAction listener)
        {
            if (button == null)
                return;

            button.onClick.RemoveListener(listener);
            button.onClick.AddListener(listener);
        }

        private static void RemoveListener(Button button, UnityEngine.Events.UnityAction listener)
        {
            if (button != null)
                button.onClick.RemoveListener(listener);
        }
    }
}
