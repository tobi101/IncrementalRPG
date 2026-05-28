using TMPro;
using UnityEngine;

namespace UI
{
    [CreateAssetMenu(fileName = "TmpGlowStyle", menuName = "UI/TMP Glow Style")]
    public sealed class TmpGlowStyle : ScriptableObject
    {
        [SerializeField] private Color _color = new(1f, 0.82f, 0.25f, 0.75f);
        [SerializeField, Range(-1f, 1f)] private float _offset;
        [SerializeField, Range(0f, 1f)] private float _inner = 0.05f;
        [SerializeField, Range(0f, 1f)] private float _outer = 0.4f;
        [SerializeField, Range(0f, 1f)] private float _power = 0.75f;

        public Color Color => _color;
        public float Offset => _offset;
        public float Inner => _inner;
        public float Outer => _outer;
        public float Power => _power;

        public bool ApplyTo(Material material)
        {
            if (material == null || !SupportsGlow(material))
                return false;

            material.EnableKeyword(ShaderUtilities.Keyword_Glow);
            material.SetColor(ShaderUtilities.ID_GlowColor, _color);
            material.SetFloat(ShaderUtilities.ID_GlowOffset, _offset);
            material.SetFloat(ShaderUtilities.ID_GlowInner, _inner);
            material.SetFloat(ShaderUtilities.ID_GlowOuter, _outer);
            material.SetFloat(ShaderUtilities.ID_GlowPower, _power);
            ShaderUtilities.UpdateShaderRatios(material);

            return true;
        }

        public static bool SupportsGlow(Material material)
        {
            return material != null &&
                   material.HasProperty(ShaderUtilities.ID_GlowColor) &&
                   material.HasProperty(ShaderUtilities.ID_GlowOffset) &&
                   material.HasProperty(ShaderUtilities.ID_GlowInner) &&
                   material.HasProperty(ShaderUtilities.ID_GlowOuter) &&
                   material.HasProperty(ShaderUtilities.ID_GlowPower);
        }
    }
}
