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
            gameObject.SetActive(false);
            _onContinueClicked = null;
            _onMainMenuClicked = null;
        }

        private IEnumerator CaptureAndShow()
        {
            yield return new WaitForEndOfFrame();

            CaptureBackground();
            SetPopupVisualsVisible(true);
            _showRoutine = null;
        }

        private void CaptureBackground()
        {
            if (_blurBackground == null)
                return;

            ReleaseCapturedTexture();

            var downscale = Mathf.Max(1, _captureDownscale);
            var width = Mathf.Max(1, Screen.width / downscale);
            var height = Mathf.Max(1, Screen.height / downscale);

            _capturedTexture = new RenderTexture(width, height, 0, RenderTextureFormat.Default)
            {
                name = "DemoEndBlurBackground"
            };
            _capturedTexture.Create();

            ScreenCapture.CaptureScreenshotIntoRenderTexture(_capturedTexture);
            _blurBackground.texture = _capturedTexture;
            _blurBackground.material = GetBlurMaterial();
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
            _runtimeBlurMaterial.SetFloat("_BlurIntensity", _blurIntensity);
            _runtimeBlurMaterial.SetFloat("_BlurHD", _useLowResBlur ? 1f : 0f);
            return _runtimeBlurMaterial;
        }

        private void EnsureInitialized()
        {
            if (_isInitialized)
                return;

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
