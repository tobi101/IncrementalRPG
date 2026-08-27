using Spine.Unity;
using UnityEngine;

namespace Entity
{
    public sealed class SkeletonTint : MonoBehaviour
    {
        [SerializeField] private SkeletonAnimation _skeletonAnimation;
        [SerializeField] private Color _color = Color.white;

        public Color Color => _color;

        private void Awake()
        {
            ApplyColor();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ApplyColor();
        }
#endif

        public void SetColor(Color color)
        {
            _color = color;
            ApplyColor();
        }

        private void ApplyColor()
        {
            _skeletonAnimation.Initialize(false);
            _skeletonAnimation.Skeleton.SetColor(_color);
        }
    }
}
