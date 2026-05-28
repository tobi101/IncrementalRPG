using TMPro;
using UnityEngine;

namespace UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TmpGlowText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private TmpGlowStyle _glowStyle;
        [SerializeField] private Material _baseMaterialOverride;
        [SerializeField] private bool _reapplyWhenFontChanges = true;
        [SerializeField] private bool _warnIfShaderDoesNotSupportGlow = true;

        private TMP_FontAsset _lastAppliedFont;
        private TmpGlowStyle _lastAppliedStyle;
        private Material _capturedBaseMaterial;
        private Material _activeGlowMaterial;

        public TmpGlowStyle GlowStyle => _glowStyle;

        private void Reset()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void Awake()
        {
            if (_text == null)
                _text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            ApplyGlow();
        }

        private void LateUpdate()
        {
            if (!_reapplyWhenFontChanges || _text == null)
                return;

            if (_text.font != _lastAppliedFont ||
                _glowStyle != _lastAppliedStyle ||
                (_activeGlowMaterial != null && _text.fontSharedMaterial != _activeGlowMaterial))
            {
                ApplyGlow();
            }
        }

        public void SetGlowStyle(TmpGlowStyle glowStyle)
        {
            _glowStyle = glowStyle;

            if (_glowStyle == null)
                ClearGlow();
            else
                ApplyGlow();
        }

        public void ApplyGlow()
        {
            if (_text == null)
                _text = GetComponent<TMP_Text>();

            if (_text == null)
                return;

            if (_text.font == null)
            {
                _lastAppliedFont = _text.font;
                _lastAppliedStyle = _glowStyle;
                return;
            }

            if (_glowStyle == null)
            {
                ClearGlow();
                return;
            }

            var sourceMaterial = GetSourceMaterial();
            var material = TmpGlowMaterialCache.Get(sourceMaterial, _glowStyle);

            if (material == null)
            {
                WarnUnsupportedShader(sourceMaterial);
                _activeGlowMaterial = null;
                _lastAppliedFont = _text.font;
                _lastAppliedStyle = _glowStyle;
                return;
            }

            _text.fontSharedMaterial = material;
            _text.UpdateMeshPadding();
            _text.SetMaterialDirty();

            _activeGlowMaterial = material;
            _lastAppliedFont = _text.font;
            _lastAppliedStyle = _glowStyle;
        }

        public void ClearGlow()
        {
            if (_text == null)
                _text = GetComponent<TMP_Text>();

            if (_text == null || _text.font == null)
                return;

            _text.fontSharedMaterial = GetSourceMaterial();
            _text.UpdateMeshPadding();
            _text.SetMaterialDirty();

            _activeGlowMaterial = null;
            _lastAppliedFont = _text.font;
            _lastAppliedStyle = null;
        }

        private Material GetSourceMaterial()
        {
            if (MaterialMatchesCurrentFont(_baseMaterialOverride))
                return _baseMaterialOverride;

            var currentMaterial = _text.fontSharedMaterial;
            if (currentMaterial != null &&
                currentMaterial != _activeGlowMaterial &&
                !TmpGlowMaterialCache.IsRuntimeGlowMaterial(currentMaterial) &&
                MaterialMatchesCurrentFont(currentMaterial))
            {
                _capturedBaseMaterial = currentMaterial;
                return currentMaterial;
            }

            if (MaterialMatchesCurrentFont(_capturedBaseMaterial))
                return _capturedBaseMaterial;

            return _text.font != null ? _text.font.material : null;
        }

        private bool MaterialMatchesCurrentFont(Material material)
        {
            if (material == null || _text == null || _text.font == null || _text.font.atlasTexture == null)
                return false;

            if (!material.HasProperty(ShaderUtilities.ID_MainTex))
                return true;

            var materialAtlas = material.GetTexture(ShaderUtilities.ID_MainTex);
            return materialAtlas != null && materialAtlas.GetInstanceID() == _text.font.atlasTexture.GetInstanceID();
        }

        private void WarnUnsupportedShader(Material sourceMaterial)
        {
            if (!_warnIfShaderDoesNotSupportGlow || sourceMaterial == null || sourceMaterial.shader == null)
                return;

            Debug.LogWarning(
                $"{name}: TMP material '{sourceMaterial.name}' uses shader '{sourceMaterial.shader.name}', which does not support TMP Glow. Use a TextMeshPro Distance Field shader with Glow support.",
                this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_text == null)
                _text = GetComponent<TMP_Text>();

            if (Application.isPlaying && isActiveAndEnabled)
                ApplyGlow();
        }
#endif
    }
}
