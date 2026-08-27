using UnityEngine;

namespace Entity
{
    public sealed class SkeletonTint : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _color = Color.white;

        private MaterialPropertyBlock _propertyBlock;

        public Color Color => _color;

        private void OnEnable()
        {
            _propertyBlock = new MaterialPropertyBlock();
            ApplyColor(_propertyBlock);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ApplyColor(new MaterialPropertyBlock());
        }
#endif

        public void SetColor(Color color)
        {
            _color = color;
            ApplyColor(_propertyBlock);
        }

        private void ApplyColor(MaterialPropertyBlock propertyBlock)
        {
            _renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(ColorId, _color);
            _renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
