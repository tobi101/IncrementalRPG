using UnityEngine;

namespace UI.Classes
{
    [ExecuteAlways]
    public sealed class ClassMenuLayout : MonoBehaviour
    {
        public RectTransform composition;
        private void OnEnable() => Fit();
        private void OnRectTransformDimensionsChange() => Fit();
        private void Fit()
        {
            if (composition == null) return;
            var size = ((RectTransform)transform).rect.size;
            composition.localScale = Vector3.one * Mathf.Min(size.x / 1920f, size.y / 1080f);
        }
    }
}
