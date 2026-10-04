using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace UI.Classes
{
    // The supplied class banner contains both a medallion and a horizontal nameplate.
    // Sample only its central element icon; the outer medallion is assembled from UI art layers.
    public sealed class ClassEmblemImage : Image
    {
        public bool sampleClassBanner = true;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (sprite == null || !sampleClassBanner) { base.OnPopulateMesh(vh); return; }
            const int segments = 64;
            var rect = GetPixelAdjustedRect();
            var uv = DataUtility.GetOuterUV(sprite);
            var centre = new Vector2(185f / 355, 185f / 369);
            var radius = new Vector2(80f / 355, 80f / 369);
            vh.Clear();
            Add(vh, new Vector2(0.5f, 0.5f), centre, rect, uv);
            for (var i = 0; i < segments; i++)
            {
                var angle = i * Mathf.PI * 2 / segments;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Add(vh, new Vector2(0.5f, 0.5f) + direction * 0.5f,
                    centre + Vector2.Scale(direction, radius), rect, uv);
            }
            for (var i = 0; i < segments; i++) vh.AddTriangle(0, i + 1, (i + 1) % segments + 1);
        }
        private void Add(VertexHelper vh, Vector2 p, Vector2 sample, Rect rect, Vector4 uv)
        {
            vh.AddVert(new Vector3(rect.xMin + p.x * rect.width, rect.yMin + p.y * rect.height), color,
                new Vector2(Mathf.Lerp(uv.x, uv.z, sample.x), Mathf.Lerp(uv.y, uv.w, sample.y)));
        }
    }
}
