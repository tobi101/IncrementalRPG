using UnityEngine;

namespace UI.Classes
{
    [ExecuteAlways]
    public sealed class ClassMenuLayout : MonoBehaviour
    {
        public RectTransform composition;
        public RectTransform modalOverlays;
        public RectTransform classesViewport;
        private void OnEnable() => Fit();
        private void OnRectTransformDimensionsChange() => Fit();
        private void Fit()
        {
            if (composition == null) return;
            var size = ((RectTransform)transform).rect.size;
            var fitScale = Mathf.Min(size.x / 1920f, size.y / 1080f);
            if (fitScale <= 0f) return;
            var scale = Vector3.one * fitScale;
            composition.localScale = scale;
            if (modalOverlays != null) modalOverlays.localScale = scale;
            // Screen edges stay outside the centered board on wide/tall displays.
            if (classesViewport != null) classesViewport.sizeDelta = size / fitScale;
        }
    }
}
