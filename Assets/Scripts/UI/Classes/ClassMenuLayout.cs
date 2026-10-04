using UnityEngine;

namespace UI.Classes
{
    [ExecuteAlways]
    public sealed class ClassMenuLayout : MonoBehaviour
    {
        public RectTransform composition;
        public RectTransform modalOverlays;
        private void OnEnable() => Fit();
        private void OnRectTransformDimensionsChange() => Fit();
        private void Fit()
        {
            if (composition == null) return;
            var size = ((RectTransform)transform).rect.size;
            var scale = Vector3.one * Mathf.Min(size.x / 1920f, size.y / 1080f);
            composition.localScale = scale;
            if (modalOverlays != null) modalOverlays.localScale = scale;
        }
    }
}
