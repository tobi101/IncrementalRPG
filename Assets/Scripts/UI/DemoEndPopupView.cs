using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class DemoEndPopupView : MonoBehaviour
    {
        [Header("Background Blur")]
        [SerializeField] private RawImage _blurBackground;
        [SerializeField] private GameObject _contentRoot;
        [SerializeField] private Material _blurMaterialTemplate;
        [SerializeField, Min(1)] private int _captureDownscale = 2;
        [SerializeField, Range(0f, 100f)] private float _blurIntensity = 12f;
        [SerializeField] private bool _useLowResBlur;

        [Header("Show Transition")]
        [SerializeField, Min(0f)] private float _showDuration = 0.6f;

        [Header("Buttons")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _wishlistButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private string _steamWishlistUrl;

        private Action _onContinueClicked;
        private Action _onMainMenuClicked;
        private RenderTexture _capturedTexture;
        private Material _runtimeBlurMaterial;
        private Coroutine _showRoutine;
        private CanvasGroup _canvasGroup;
        private bool _isInitialized;

        private void Awake()
        {
            UIButtonAudio.InstallInChildren(this);
            EnsureInitialized();
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_isInitialized)
            {
                RemoveListener(_continueButton, OnContinueButtonClicked);
                RemoveListener(_wishlistButton, OnWishlistButtonClicked);
                RemoveListener(_mainMenuButton, OnMainMenuButtonClicked);
            }

            ReleaseCapturedTexture();
            DestroyUnityObject(_runtimeBlurMaterial);
        }

        public void Show(Action onContinueClicked, Action onMainMenuClicked)
        {
            EnsureInitialized();

            _onContinueClicked = onContinueClicked;
            _onMainMenuClicked = onMainMenuClicked;

            gameObject.SetActive(true);
            SetPopupVisualsVisible(false);
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;

            if (_showRoutine != null)
                StopCoroutine(_showRoutine);

            _showRoutine = StartCoroutine(CaptureAndShow());
        }

        public void Hide()
        {
            if (_showRoutine != null)
            {
                StopCoroutine(_showRoutine);
                _showRoutine = null;
            }

            ReleaseCapturedTexture();
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
            }
            gameObject.SetActive(false);
            _onContinueClicked = null;
            _onMainMenuClicked = null;
        }

        private IEnumerator CaptureAndShow()
        {
            yield return new WaitForEndOfFrame();

            CaptureBackground();
            SetPopupVisualsVisible(true);

            var elapsed = 0f;
            while (elapsed < _showDuration)
            {
                var progress = Mathf.SmoothStep(0f, 1f, elapsed / _showDuration);
                _canvasGroup.alpha = progress;
                SetBlurIntensity(_blurIntensity * progress);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            SetBlurIntensity(_blurIntensity);
            _showRoutine = null;
        }

        private void CaptureBackground()
        {
            if (_blurBackground == null)
                return;

            ReleaseCapturedTexture();

            var downscale = Mathf.Max(1, _captureDownscale);
            var screenWidth = Mathf.Max(1, Screen.width);
            var screenHeight = Mathf.Max(1, Screen.height);
            var width = Mathf.Max(1, screenWidth / downscale);
            var height = Mathf.Max(1, screenHeight / downscale);

            _capturedTexture = new RenderTexture(width, height, 0, RenderTextureFormat.Default)
            {
                name = "DemoEndBlurBackground",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _capturedTexture.Create();

            // Capture the entire backbuffer before resizing; a smaller capture target can crop the frame.
            var fullResolutionCapture = RenderTexture.GetTemporary(screenWidth, screenHeight, 0, RenderTextureFormat.Default);
            var previousActive = RenderTexture.active;
            try
            {
                ScreenCapture.CaptureScreenshotIntoRenderTexture(fullResolutionCapture);
                Graphics.Blit(fullResolutionCapture, _capturedTexture);
            }
            finally
            {
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(fullResolutionCapture);
            }

            // Screen captures use the graphics API's vertical origin; UI UVs start at the bottom.
            _blurBackground.uvRect = SystemInfo.graphicsUVStartsAtTop
                ? new Rect(0f, 1f, 1f, -1f)
                : new Rect(0f, 0f, 1f, 1f);
            _blurBackground.texture = _capturedTexture;
            _blurBackground.material = GetBlurMaterial();
            SetBlurIntensity(0f);
        }

        private Material GetBlurMaterial()
        {
            if (_runtimeBlurMaterial == null)
            {
                if (_blurMaterialTemplate != null)
                {
                    _runtimeBlurMaterial = new Material(_blurMaterialTemplate);
                }
                else
                {
                    var shader = Shader.Find("AllIn1SpriteShader/AllIn1SpriteShaderUiMask")
                                 ?? Shader.Find("AllIn1SpriteShader/AllIn1SpriteShader");

                    if (shader != null)
                        _runtimeBlurMaterial = new Material(shader);
                }
            }

            if (_runtimeBlurMaterial == null)
                return null;

            _runtimeBlurMaterial.EnableKeyword("BLUR_ON");
            SetKeyword(_runtimeBlurMaterial, "BLURISHD_ON", _useLowResBlur);
            _runtimeBlurMaterial.SetFloat("_BlurHD", _useLowResBlur ? 1f : 0f);
            return _runtimeBlurMaterial;
        }

        private void SetBlurIntensity(float intensity)
        {
            if (_runtimeBlurMaterial != null)
                _runtimeBlurMaterial.SetFloat("_BlurIntensity", intensity);
        }

        private void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            AddListener(_continueButton, OnContinueButtonClicked);
            AddListener(_wishlistButton, OnWishlistButtonClicked);
            AddListener(_mainMenuButton, OnMainMenuButtonClicked);

            _isInitialized = true;
        }

        private void SetPopupVisualsVisible(bool visible)
        {
            if (_blurBackground != null)
                _blurBackground.gameObject.SetActive(visible);

            if (_contentRoot != null)
                _contentRoot.SetActive(visible);
        }

        private void OnContinueButtonClicked()
        {
            _onContinueClicked?.Invoke();
        }

        private void OnWishlistButtonClicked()
        {
            if (!string.IsNullOrWhiteSpace(_steamWishlistUrl))
                Application.OpenURL(_steamWishlistUrl);
        }

        private void OnMainMenuButtonClicked()
        {
            _onMainMenuClicked?.Invoke();
        }

        private void ReleaseCapturedTexture()
        {
            if (_blurBackground != null && _blurBackground.texture == _capturedTexture)
                _blurBackground.texture = null;

            if (_capturedTexture == null)
                return;

            _capturedTexture.Release();
            DestroyUnityObject(_capturedTexture);
            _capturedTexture = null;
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
            if (button == null)
                return;

            button.onClick.RemoveListener(listener);
        }

        private static void SetKeyword(Material material, string keyword, bool enabled)
        {
            if (enabled)
                material.EnableKeyword(keyword);
            else
                material.DisableKeyword(keyword);
        }

        private static void DestroyUnityObject(UnityEngine.Object unityObject)
        {
            if (unityObject == null)
                return;

            if (Application.isPlaying)
                Destroy(unityObject);
            else
                DestroyImmediate(unityObject);
        }
    }
}
